using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace PmSoft.Caching
{
    /// <summary>
    /// 默认提供的缓存服务
    /// </summary>
    [Serializable]
    public class DefaultCacheService : ICacheService
    {
        private ICache cache;
        private readonly Dictionary<CachingExpirationType, TimeSpan> cachingExpirationDictionary;
        private ICache localCache;
        private readonly CacheServiceOptions options;
        private readonly ILogger<DefaultCacheService> logger;

        /// <summary>
        /// 默认缓存服务
        /// </summary>
        /// <param name="optionsAccessor">设置访问器</param>
        /// <param name="distributedCache">分布式缓存器</param>
        /// <param name="localCache">本地内存缓存器</param>
        /// <param name="logger">日志</param>
        public DefaultCacheService(IOptions<CacheServiceOptions> optionsAccessor,
            IDistributedCache distributedCache,
            IMemoryCache localCache,
            ILogger<DefaultCacheService> logger)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }
            this.logger = logger;
            this.options = optionsAccessor.Value;
            if (options.EnableDistributedCache && distributedCache != null)
                this.cache = new DistributedMemoryCache(distributedCache);
            else
                this.cache = new RuntimeMemoryCache(localCache);
            this.localCache = new RuntimeMemoryCache(localCache);
            this.EnableDistributedCache = options.EnableDistributedCache;
            this.cachingExpirationDictionary = new Dictionary<CachingExpirationType, TimeSpan>();
            this.cachingExpirationDictionary.Add(CachingExpirationType.Invariable, new TimeSpan(0, 0, (int)(86400f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.Stable, new TimeSpan(0, 0, (int)(28800f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.RelativelyStable, new TimeSpan(0, 0, (int)(7200f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.UsualSingleObject, new TimeSpan(0, 0, (int)(600f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.UsualObjectCollection, new TimeSpan(0, 0, (int)(300f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.SingleObject, new TimeSpan(0, 0, (int)(180f * options.CacheExpirationFactor)));
            this.cachingExpirationDictionary.Add(CachingExpirationType.ObjectCollection, new TimeSpan(0, 0, (int)(180f * options.CacheExpirationFactor)));

            this.logger.LogInformation($"DefaultCacheService::Init,EnableDistributedCache:{options.EnableDistributedCache},CacheExpirationFactor:{options.CacheExpirationFactor}.");
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void Add(string cacheKey, object value, TimeSpan timeSpan)
        {
            this.cache.Add(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public async Task AddAsync(string cacheKey, object value, TimeSpan timeSpan)
        {
            await this.cache.AddAsync(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void Add(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            this.Add(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 添加到缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public async Task AddAsync(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            await this.AddAsync(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public async Task ClearAsync()
        {
            await this.cache.ClearAsync();
        }

        /// <summary>
        /// 从缓存获取  
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns></returns>
        public object Get(string cacheKey)
        {
            object obj = null;
            if (this.EnableDistributedCache)
            {
                obj = this.localCache.Get(cacheKey);
            }
            if (obj == null)
            {
                obj = this.cache.Get(cacheKey);
                if (this.EnableDistributedCache && obj != null)
                {
                    this.localCache.Add(cacheKey, obj, this.cachingExpirationDictionary[CachingExpirationType.SingleObject]);
                }
            }
            return obj;
        }

        /// <summary>
        /// 从缓存获取  
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns></returns>
        public async Task<object> GetAsync(string cacheKey)
        {
            object obj = null;
            if (this.EnableDistributedCache)
            {
                obj = await this.localCache.GetAsync(cacheKey);
            }
            if (obj == null)
            {
                obj = await this.cache.GetAsync(cacheKey);
                if (this.EnableDistributedCache && obj != null)
                {
                    await this.localCache.AddAsync(cacheKey, obj, this.cachingExpirationDictionary[CachingExpirationType.SingleObject]);
                }
            }
            return obj;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        public T Get<T>(string cacheKey) where T : class
        {
            object obj = this.Get(cacheKey);
            if (obj != null)
            {
                return (obj as T);
            }
            return default(T);
        }

        public async Task<T> GetAsync<T>(string cacheKey) where T : class
        {
            object obj = await this.GetAsync(cacheKey);
            if (obj != null)
            {
                return (obj as T);
            }
            return default(T);
        }


        public bool TryGetValue<T>(string cacheKey, out T value)
        {
            object obj = Get(cacheKey);
            if (obj == null)
            {
                value = default(T);
                return false;
            }
            value = (T)obj;
            return true;
        }

        /// <summary>
        /// 从一层缓存获取缓存项;在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public T GetFromFirstLevel<T>(string cacheKey) where T : class
        {
            object fromFirstLevel = this.GetFromFirstLevel(cacheKey);
            if (fromFirstLevel != null)
            {
                return (fromFirstLevel as T);
            }
            return default(T);
        }

        /// <summary>
        /// 从一层缓存获取缓存项;在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <typeparam name="T">缓存项类型</typeparam>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public async Task<T> GetFromFirstLevelAsync<T>(string cacheKey) where T : class
        {
            object fromFirstLevel = await this.GetFromFirstLevelAsync(cacheKey);
            if (fromFirstLevel != null)
            {
                return (fromFirstLevel as T);
            }
            return default(T);
        }

        /// <summary>
        /// 从一层缓存获取缓存项;在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public object GetFromFirstLevel(string cacheKey)
        {
            return this.cache.Get(cacheKey);
        }

        /// <summary>
        /// 从一层缓存获取缓存项;在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <returns>返回cacheKey对应的缓存项，如果不存在则返回null</returns>
        public async Task<object> GetFromFirstLevelAsync(string cacheKey)
        {
            return await this.cache.GetAsync(cacheKey);
        }

        /// <summary>
        /// 标识为删除
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="entity">缓存的实体</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void MarkDeletion(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType)
        {
            if (entity is IDelEntity)
            {
                (entity as IDelEntity).IsDeletedInDatabase = true;
            }
            this.Remove(cacheKey);
        }

        /// <summary>
        /// 标识为删除
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="entity">缓存的实体</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public async Task MarkDeletionAsync(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType)
        {
            if (entity is IDelEntity)
            {
                (entity as IDelEntity).IsDeletedInDatabase = true;
            }
            await this.RemoveAsync(cacheKey);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        public void Remove(string cacheKey)
        {
            this.cache.Remove(cacheKey);
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        public async Task RemoveAsync(string cacheKey)
        {
            await this.cache.RemoveAsync(cacheKey);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public void Set(string cacheKey, object value, TimeSpan timeSpan)
        {
            this.cache.Set(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间间隔</param>
        public async Task SetAsync(string cacheKey, object value, TimeSpan timeSpan)
        {
            await this.cache.SetAsync(cacheKey, value, timeSpan);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public void Set(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            this.Set(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 添加或更新缓存
        /// </summary>
        /// <param name="cacheKey">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="cachingExpirationType">缓存期限类型</param>
        public async Task SetAsync(string cacheKey, object value, CachingExpirationType cachingExpirationType)
        {
            await this.SetAsync(cacheKey, value, this.cachingExpirationDictionary[cachingExpirationType]);
        }

        /// <summary>
        /// 是否启用了分布式缓存
        /// </summary>
        public bool EnableDistributedCache { get; private set; }
    }
}
