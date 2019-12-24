using System;

namespace PmSoft.Caching
{
    /// <summary>
    /// 实体缓存期限类型
    /// </summary>
    public enum EntityCacheExpirationPolicies
    {
        /// <summary>
        /// 单个实体 缓存时间:180秒*时间因子
        /// </summary>
        Normal = 5,
        /// <summary>
        /// 稳定数据 缓存时间:28800秒*时间因子
        /// </summary>
        Stable = 1,
        /// <summary>
        /// 常用的单个实体 缓存时间:600秒*时间因子
        /// </summary>
        Usual = 3
    }
}
