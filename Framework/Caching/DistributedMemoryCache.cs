using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis.Extensions.Core.Abstractions;
using Newtonsoft.Json;

namespace PmSoft.Caching
{
    /// <summary>
    /// 分布式缓存
    /// </summary>
    public class DistributedMemoryCache : ICache
    {
        private readonly IRedisCacheClient cache;

        public DistributedMemoryCache(IRedisCacheClient cache)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// 加入缓存项
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            this.Set(key, value, timeSpan);
        }


        /// <summary>
        /// 加入缓存项
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public async Task AddAsync(string key, object value, TimeSpan timeSpan)
        {
            await this.SetAsync(key, value, timeSpan);
        }


        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {
            ClearAsync().Wait();
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public async Task ClearAsync()
        {
            await this.cache.Db0.FlushDbAsync();
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            return GetAsync(key).Result;
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.ToLower();

            return await this.cache.Db0.GetAsync<object>(key);
        }

        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.ToLower();
            RemoveAsync(key).Wait();
        }

        /// <summary>
        /// 移除缓存项
        /// </summary>
        /// <param name="key"></param>
        public async Task RemoveAsync(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.ToLower();
            await this.cache.Db0.RemoveAsync(key);
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Set(string key, object value, TimeSpan timeSpan)
        {
            SetAsync(key, value, timeSpan).Wait();
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public async Task SetAsync(string key, object value, TimeSpan timeSpan)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (timeSpan == null)
            {
                throw new ArgumentNullException(nameof(timeSpan));
            }

            key = key.ToLower();
            if (value != null)
            {
                IRedisDatabase database = this.cache.Db0;

                if (database == null)
                {
                    throw new NullReferenceException(nameof(database));
                }
                await database.AddAsync(key, value, DateTimeOffset.Now.AddMinutes(10));
            }
        }
    }
}
