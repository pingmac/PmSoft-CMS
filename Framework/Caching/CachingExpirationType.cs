using System;

namespace PmSoft.Caching
{
    /// <summary>
    /// 缓存期限类型
    /// </summary>
    public enum CachingExpirationType
    {
        /// <summary>
        /// 永久不变的 缓存时间:86400秒*时间因子
        /// </summary>
        Invariable,
        /// <summary>
        /// 稳定数据 缓存时间:28800秒*时间因子
        /// </summary>
        Stable,
        /// <summary>
        /// 相对稳定 缓存时间:7200秒*时间因子
        /// </summary>
        RelativelyStable,
        /// <summary>
        /// 常用的单个对象 缓存时间:600秒*时间因子
        /// </summary>
        UsualSingleObject,
        /// <summary>
        /// 常用的对象集合 缓存时间:300秒*时间因子
        /// </summary>
        UsualObjectCollection,
        /// <summary>
        /// 单个对象 缓存时间:180秒*时间因子
        /// </summary>
        SingleObject,
        /// <summary>
        /// 对象集合 缓存时间:180秒*时间因子
        /// </summary>
        ObjectCollection
    }

}
