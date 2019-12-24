using System;
using System.Threading.Tasks;

namespace PmSoft.Caching
{
    /// <summary>
    /// 缓存服务接口 
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// 添加到缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        void Add(string cacheKey, object value, TimeSpan timeSpan);
        /// <summary>
        /// 添加到缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        Task AddAsync(string cacheKey, object value, TimeSpan timeSpan);
        /// <summary>
        /// 添加到缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="cachingExpirationType"></param>
        void Add(string cacheKey, object value, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 添加到缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="cachingExpirationType"></param>
        Task AddAsync(string cacheKey, object value, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 清空缓存 
        /// </summary>
        void Clear();
        /// <summary>
        /// 清空缓存 
        /// </summary>
        Task ClearAsync();
        /// <summary>
        /// 从缓存获取 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        object Get(string cacheKey);
        /// <summary>
        /// 从缓存获取 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<object> GetAsync(string cacheKey);
        /// <summary>
        /// 从缓存获取(缓存项必须是引用类型) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T Get<T>(string cacheKey) where T : class;
        /// <summary>
        /// 从缓存获取(缓存项必须是引用类型) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string cacheKey) where T : class;
        /// <summary>
        /// 尝试获取一个值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue<T>(string cacheKey, out T value);
        /// <summary>
        /// 从一层缓存获取(缓存项必须是引用类型) 
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        T GetFromFirstLevel<T>(string cacheKey) where T : class;
        /// <summary>
        /// 从一层缓存获取(缓存项必须是引用类型) 
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<T> GetFromFirstLevelAsync<T>(string cacheKey) where T : class;
        /// <summary>
        /// 从一层缓存获取 
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        object GetFromFirstLevel(string cacheKey);
        /// <summary>
        /// 从一层缓存获取 
        /// 在启用分布式缓存的情况下，指穿透二级缓存从一层缓存（分布式缓存）读取
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <returns></returns>
        Task<object> GetFromFirstLevelAsync(string cacheKey);
        /// <summary>
        /// 标识为删除 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        /// <param name="cachingExpirationType"></param>
        void MarkDeletion(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 标识为删除 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="entity"></param>
        /// <param name="cachingExpirationType"></param>
        Task MarkDeletionAsync(string cacheKey, IEntity entity, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 移除缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        void Remove(string cacheKey);
        /// <summary>
        /// 移除缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        Task RemoveAsync(string cacheKey);
        /// <summary>
        /// 添加或更新缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        void Set(string cacheKey, object value, TimeSpan timeSpan);
        /// <summary>
        /// 添加或更新缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="timeSpan"></param>
        Task SetAsync(string cacheKey, object value, TimeSpan timeSpan);
        /// <summary>
        /// 添加或更新缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="cachingExpirationType"></param>
        void Set(string cacheKey, object value, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 添加或更新缓存 
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="value"></param>
        /// <param name="cachingExpirationType"></param>
        Task SetAsync(string cacheKey, object value, CachingExpirationType cachingExpirationType);
        /// <summary>
        /// 是否启用分布式缓存
        /// </summary>
        bool EnableDistributedCache { get; }
    }

}
