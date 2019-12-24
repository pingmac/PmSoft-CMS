using System;

namespace PmSoft.Caching
{
    /// <summary>
    /// 用于列表缓存设置接口
    /// </summary>
    public interface IListCacheSetting
    {
        // Properties
        /// <summary>
        /// 缓存分区字段名称
        /// </summary>
        string AreaCachePropertyName { get; set; }
        /// <summary>
        /// 缓存分区字段值
        /// </summary>
        object AreaCachePropertyValue { get; set; }
        /// <summary>
        /// 列表缓存版本设置 
        /// </summary>
        CacheVersionType CacheVersionType { get; }
    }
}
