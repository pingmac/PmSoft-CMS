using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace PmSoft.Caching
{
    /// <summary>
    /// 用于连接Memcached的分布式缓存
    /// </summary>
    public class DistributedMemoryCache : ICache
    {
        private readonly IDistributedCache cache;
        public ISerializer serializer;

        public DistributedMemoryCache(IDistributedCache distributedCache)
        {
            this.cache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            this.serializer = new BinarySerializer();
        }

        /// <summary>
        /// 加入缓存项
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Add(string key, object value, TimeSpan timeSpan)
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

            await this.SetAsync(key, value, timeSpan);
        }


        /// <summary>
        /// 清空缓存
        /// </summary>
        public void Clear()
        {

        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public Task ClearAsync()
        {
            return Task.Run(() => Clear());
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            key = key.ToLower();

            byte[] data = this.cache.Get(key);

            if (data == null) return null;

            object result = this.serializer.Deserialize(data);

            return result;
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

            byte[] data = await this.cache.GetAsync(key);

            if (data == null) return null;

            object result = await this.serializer.DeserializeAsync(data);

            return result;
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
            this.cache.Remove(key);
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
            await this.cache.RemoveAsync(key);
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key">缓存项标识</param>
        /// <param name="value">缓存项</param>
        /// <param name="timeSpan">缓存失效时间</param>
        public void Set(string key, object value, TimeSpan timeSpan)
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
                DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();

                byte[] bytes = this.serializer.Serialize(value);

                this.cache.Set(key, bytes, distributedCacheEntryOptions.SetSlidingExpiration(timeSpan));
            }
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
                DistributedCacheEntryOptions distributedCacheEntryOptions = new DistributedCacheEntryOptions();

                byte[] bytes = await this.serializer.SerializeAsync(value);

                await this.cache.SetAsync(key, bytes, distributedCacheEntryOptions.SetSlidingExpiration(timeSpan));
            }
        }
    }
}
