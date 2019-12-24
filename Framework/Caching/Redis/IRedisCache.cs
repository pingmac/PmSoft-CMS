using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PmSoft.Caching.Redis
{
    public interface IRedisCache
    {
        IDatabase Database { get; }

        #region Key & Value

        T Get<T>(string key);

        Task<T> GetAsync<T>(string key, CancellationToken token = default(CancellationToken));

        void Add<T>(string key, T value, TimeSpan expiresIn);

        Task AddAsync<T>(string key, T value, TimeSpan expiresIn, CancellationToken token = default(CancellationToken));

        bool SetKeyExpire(string key, TimeSpan expiry);

        #endregion

        #region DELETE

        void Remove(string key);

        Task RemoveAsync(string key, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// 清空数据库
        /// </summary>
        void Flush();

        /// <summary>
        /// 清空数据库
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task FlushAsync(CancellationToken token = default(CancellationToken));

        #endregion

        #region Hash

        bool HashSet<T>(string hashKey, string hashField, T value, bool nx = false);

        Task<bool> HashSetAsync<T>(string hashKey, string hashField, T value, bool nx = false, CancellationToken token = default(CancellationToken));

        long HashDelete(string hashKey, IEnumerable<string> hashFields);

        Task<long> HashDeleteAsync(string hashKey, IEnumerable<string> hashFields, CancellationToken token = default(CancellationToken));

        bool HashDelete(string hashKey, string hashField);

        Task<bool> HashDeleteAsync(string hashKey, string hashField, CancellationToken token = default(CancellationToken));

        IEnumerable<T> HashValues<T>(string hashKey);

        Task<IEnumerable<T>> HashValuesAsync<T>(string hashKey, CancellationToken token = default(CancellationToken));

        long HashLength(string hashKey);

        Task<long> HashLengthAsync(string hashKey, CancellationToken token = default(CancellationToken));

        bool HashExists(string hashKey, string hashField);

        Task<bool> HashExistsAsync(string hashKey, string hashField, CancellationToken token = default(CancellationToken));

        #endregion

        #region Lock

        /// <summary>
        /// 锁查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string LockQuery(string key);
        /// <summary>
        /// 释放锁
        /// </summary>
        /// <param name="key">锁的名称</param>
        /// <param name="value">用来标识谁拥有该锁并用来释放锁</param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> LockReleaseAsync(string key, string value, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 获取锁
        /// </summary>
        /// <param name="key">锁的名称</param>
        /// <param name="value">用来标识谁拥有该锁并用来释放锁</param>
        /// <param name="expiry"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> LockTakeAsync(string key, string value, TimeSpan expiry, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 等待使用锁
        /// </summary>
        /// <param name="key">锁的名称</param>
        /// <param name="value">用来标识谁拥有该锁并用来释放锁</param>
        /// <param name="maxWait">最大等待时间</param>
        /// <param name="lockExpiry">锁过期时间</param>
        /// <returns></returns>
        Task<bool> WaitUseLockAsync(string key, string value, TimeSpan maxWait, TimeSpan lockExpiry, CancellationToken token = default(CancellationToken));
        /// <summary>
        /// 使用锁
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">锁的名称</param>
        /// <param name="value">用来标识谁拥有该锁并用来释放锁</param>
        /// <param name="maxWait">最大等待时间</param>
        /// <param name="lockExpiry">锁过期时间</param>
        /// <param name="success">锁成功执行带返回参数方法</param>
        /// <param name="fail">锁失败</param>
        /// <returns>事务返回结果</returns>
        Task<T> UseLockAsync<T>(string key, string value, TimeSpan maxWait, TimeSpan lockExpiry, Func<T> success, Func<T> fail = null, CancellationToken token = default(CancellationToken));

        #endregion
    }
}
