using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace PmSoft.Caching
{
    public class RuntimeMemoryCache : ICache
    {
        private readonly IMemoryCache cache;

        public RuntimeMemoryCache(IMemoryCache memoryCache)
        {
            this.cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        /// <summary>
        /// 加入缓存项 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
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
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan">缓存失效时间</param>
        public async Task AddAsync(string key, object value, TimeSpan timeSpan)
        {
            await Task.Run(() => Add(key, value, timeSpan));
        }


        /// <summary>
        /// 从缓存中清除所有缓存项
        /// </summary>
        public void Clear()
        {
        }

        /// <summary>
        /// 从缓存中清除所有缓存项
        /// </summary>
        public async Task ClearAsync()
        {
            await Task.Run(() => Clear());
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

            return this.cache.Get(key);
        }

        /// <summary>
        /// 获取缓存项
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            return await Task.Run(() => Get(key));
        }

        /// <summary>
        /// 移除指定的缓存项
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            this.cache.Remove(key);
        }

        /// <summary>
        /// 移除指定的缓存项
        /// </summary>
        /// <param name="key"></param>
        public async Task RemoveAsync(string key)
        {
            await Task.Run(() => Remove(key));
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
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

            if (!string.IsNullOrEmpty(key) && (value != null))
            {
                this.cache.Set(key, value, timeSpan);
            }
        }

        /// <summary>
        /// 如果不存在缓存项则添加，否则更新
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        public async Task SetAsync(string key, object value, TimeSpan timeSpan)
        {
            await Task.Run(() => Set(key, value, timeSpan));
        }

    }
}
