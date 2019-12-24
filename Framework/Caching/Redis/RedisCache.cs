// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace PmSoft.Caching.Redis
{
    public class RedisCache : IDistributedCache, IRedisCache, IDisposable
    {
        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] = data - byte[]
        // this order should not change LUA script depends on it
        private const string SetScript = (@"
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4])
                if ARGV[3] ~= '-1' then
                  redis.call('EXPIRE', KEYS[1], ARGV[3])
                end
                return 1");
        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const long NotPresent = -1;

        private volatile ConnectionMultiplexer _connection;
        private IDatabase _cache;

        private readonly RedisCacheOptions _options;
        private readonly string _instance;
        public ISerializer _serializer;

        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(initialCount: 1, maxCount: 1);

        public RedisCache(IOptions<RedisCacheOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            _serializer = new NewtonsoftSerializer();

            _options = optionsAccessor.Value;

            // This allows partitioning a single backend cache for use with multiple apps/services.
            _instance = _options.InstanceName ?? string.Empty;
        }

        public IDatabase Database
        {
            get
            {
                Connect();
                return _cache;
            }
        }

        #region Get

        public T Get<T>(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var valueBytes = Get(key);

            if (valueBytes == null)
            {
                return default(T);
            }
            return _serializer.Deserialize<T>(valueBytes);
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var valueBytes = await GetAsync(key);

            if (valueBytes == null)
            {
                return default(T);
            }
            return await _serializer.DeserializeAsync<T>(valueBytes);
        }

        public byte[] Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return GetAndRefresh(key, getData: true);
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            return await GetAndRefreshAsync(key, getData: true, token: token);
        }

        #endregion

        #region Set


        public void Add<T>(string key, T value, TimeSpan expiresIn)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var entryBytes = _serializer.Serialize(value);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(expiresIn);
            Set(key, entryBytes, options);
        }

        public async Task AddAsync<T>(string key, T value, TimeSpan expiresIn, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var entryBytes = await _serializer.SerializeAsync(value);

            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions();
            options.SetSlidingExpiration(expiresIn);
            await SetAsync(key, entryBytes, options);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Connect();

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            var result = _cache.ScriptEvaluate(SetScript, new RedisKey[] { _instance + key },
                new RedisValue[]
                {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                });
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token);

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = GetAbsoluteExpiration(creationTime, options);

            await _cache.ScriptEvaluateAsync(SetScript, new RedisKey[] { _instance + key },
                new RedisValue[]
                {
                        absoluteExpiration?.Ticks ?? NotPresent,
                        options.SlidingExpiration?.Ticks ?? NotPresent,
                        GetExpirationInSeconds(creationTime, absoluteExpiration, options) ?? NotPresent,
                        value
                });
        }
        #endregion

        #region Refresh

        public void Refresh(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            GetAndRefresh(key, getData: false);
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await GetAndRefreshAsync(key, getData: false, token: token);
        }
        #endregion

        #region Privates

        private void Connect()
        {
            if (_connection != null)
            {
                return;
            }

            _connectionLock.Wait();
            try
            {
                if (_connection == null)
                {
                    _connection = ConnectionMultiplexer.Connect(_options.Configuration);
                    _cache = _connection.GetDatabase();
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task ConnectAsync(CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            if (_connection != null)
            {
                return;
            }

            await _connectionLock.WaitAsync();
            try
            {
                if (_connection == null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);

                    _cache = _connection.GetDatabase(0);
                }
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private byte[] GetAndRefresh(string key, bool getData)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = _cache.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = _cache.HashMemberGet(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                Refresh(key, absExpr, sldExpr);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }

        private async Task<byte[]> GetAndRefreshAsync(string key, bool getData, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            await ConnectAsync(token);

            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            if (getData)
            {
                results = await _cache.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey, DataKey);
            }
            else
            {
                results = await _cache.HashMemberGetAsync(_instance + key, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                MapMetadata(results, out DateTimeOffset? absExpr, out TimeSpan? sldExpr);
                await RefreshAsync(key, absExpr, sldExpr, token);
            }

            if (results.Length >= 3 && results[2].HasValue)
            {
                return results[2];
            }

            return null;
        }
        #endregion

        #region Remove

        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            _cache.KeyDelete(_instance + key);
            // TODO: Error handling
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);

            await _cache.KeyDeleteAsync(_instance + key);
            // TODO: Error handling
        }

        /// <summary>
        /// 清空数据库
        /// </summary>
        public void Flush()
        {
            Connect();

            foreach (var endPoint in _connection.GetEndPoints())
                _connection.GetServer(endPoint).FlushAllDatabases();
        }

        /// <summary>
        /// 清空数据库
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task FlushAsync(CancellationToken token = default(CancellationToken))
        {

            await ConnectAsync(token);

            foreach (var endPoint in _connection.GetEndPoints())
                await _connection.GetServer(endPoint).FlushAllDatabasesAsync();
        }

        #endregion

        #region Hash

        public long HashDelete(string hashKey, IEnumerable<string> hashFields)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashFields == null)
            {
                throw new ArgumentNullException(nameof(hashFields));
            }

            Connect();

            return _cache.HashDelete(hashKey, hashFields.Select(x => (RedisValue)x).ToArray());
        }

        public async Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> hashFields, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashFields == null)
            {
                throw new ArgumentNullException(nameof(hashFields));
            }

            await ConnectAsync(token);

            return await _cache.HashDeleteAsync(hashKey, hashFields.Select(x => (RedisValue)x).ToArray());
        }

        public bool HashDelete(string hashKey, string hashField)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashField == null)
            {
                throw new ArgumentNullException(nameof(hashField));
            }

            Connect();

            return _cache.HashDelete(hashKey, hashField);
        }

        public async Task<bool> HashDeleteAsync(string hashKey, string hashField, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashField == null)
            {
                throw new ArgumentNullException(nameof(hashField));
            }

            await ConnectAsync(token);

            return await _cache.HashDeleteAsync(hashKey, hashField);
        }

        public bool HashSet<T>(string hashKey, string hashField, T value, bool nx = false)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashField == null)
            {
                throw new ArgumentNullException(nameof(hashField));
            }

            Connect();

            return _cache.HashSet(hashKey, hashField, _serializer.Serialize(value), nx ? When.NotExists : When.Always);
        }

        public async Task<bool> HashSetAsync<T>(string hashKey, string key, T value, bool nx = false, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);

            return await _cache.HashSetAsync(hashKey, key, _serializer.Serialize(value), nx ? When.NotExists : When.Always);
        }

        public IEnumerable<T> HashValues<T>(string hashKey)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            Connect();

            return _cache.HashValues(hashKey).Select(x => _serializer.Deserialize<T>(x));
        }

        public async Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            await ConnectAsync(token);

            var hashValues = await _cache.HashValuesAsync(hashKey);

            return hashValues.Select(x => _serializer.Deserialize<T>(x));
        }

        public long HashLength(string hashKey)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            Connect();

            return _cache.HashLength(hashKey);
        }

        public async Task<long> HashLengthAsync(string hashKey, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            await ConnectAsync(token);

            return await _cache.HashLengthAsync(hashKey);
        }

        public bool HashExists(string hashKey, string hashField)
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashField == null)
            {
                throw new ArgumentNullException(nameof(hashField));
            }

            Connect();

            return _cache.HashExists(hashKey, hashField);
        }

        public async Task<bool> HashExistsAsync(string hashKey, string hashField, CancellationToken token = default(CancellationToken))
        {
            if (hashKey == null)
            {
                throw new ArgumentNullException(nameof(hashKey));
            }

            if (hashField == null)
            {
                throw new ArgumentNullException(nameof(hashField));
            }

            await ConnectAsync(token);

            return await _cache.HashExistsAsync(hashKey, hashField);
        }

        public bool SetKeyExpire(string key, TimeSpan expiry)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            return _cache.KeyExpire(key, expiry);
        }

        #endregion

        #region Lock

        public string LockQuery(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            Connect();

            return _cache.LockQuery(key);
        }


        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> LockReleaseAsync(string key, string value, CancellationToken token)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token).ConfigureAwait(false); ;

            return await _cache.LockReleaseAsync(key, value).ConfigureAwait(false);
        }

        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="token"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, CancellationToken token)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            await ConnectAsync(token);

            return await _cache.LockTakeAsync(key, value, expiry).ConfigureAwait(false);
        }

        public async Task<bool> WaitUseLockAsync(string key, string value, TimeSpan maxWait, TimeSpan lockExpiry, CancellationToken token)
        {
            var totalTime = TimeSpan.Zero;
            var sleepTime = TimeSpan.FromMilliseconds(100);
            var lockAchieved = false;
            while (!lockAchieved && totalTime < maxWait)
            {
                lockAchieved = await LockTakeAsync(key, value, lockExpiry, token).ConfigureAwait(false); ;
                if (lockAchieved)
                {
                    break;
                }
                await Task.Delay(sleepTime);
                totalTime += sleepTime;
            }
            return lockAchieved;
        }

        public async Task<T> UseLockAsync<T>(string key, string value, TimeSpan maxWait, TimeSpan lockExpiry, Func<T> success, Func<T> fail = null, CancellationToken token = default(CancellationToken))
        {
            bool isLocked = await WaitUseLockAsync(key, value, maxWait, lockExpiry, token).ConfigureAwait(false); ;
            if (isLocked)
            {
                try
                {
                    return success();
                }
                finally
                {
                    await LockReleaseAsync(key, value, token).ConfigureAwait(false); ;//释放锁
                }
            }
            else if (fail != null)
                return fail();
            else
                return default(T);
        }
        #endregion

        private void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        private void Refresh(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                _cache.KeyExpire(_instance + key, expr);
                // TODO: Error handling
            }
        }

        private async Task RefreshAsync(string key, DateTimeOffset? absExpr, TimeSpan? sldExpr, CancellationToken token = default(CancellationToken))
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            token.ThrowIfCancellationRequested();

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                await _cache.KeyExpireAsync(_instance + key, expr);
                // TODO: Error handling
            }
        }

        private static long? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                return (long)Math.Min(
                    (absoluteExpiration.Value - creationTime).TotalSeconds,
                    options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                return (long)(absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                return (long)options.SlidingExpiration.Value.TotalSeconds;
            }
            return null;
        }

        private static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(DistributedCacheEntryOptions.AbsoluteExpiration),
                    options.AbsoluteExpiration.Value,
                    "The absolute expiration value must be in the future.");
            }
            var absoluteExpiration = options.AbsoluteExpiration;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Close();
            }
        }
    }
}