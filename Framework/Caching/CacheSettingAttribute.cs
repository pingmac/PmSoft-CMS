using System;
using System.Runtime.CompilerServices;

namespace PmSoft.Caching
{
    /// <summary>
    /// 实体缓存设置属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheSettingAttribute : Attribute
    {
        private EntityCacheExpirationPolicies expirationPolicy = EntityCacheExpirationPolicies.Normal;

        public CacheSettingAttribute(bool enableCache)
        {
            this.EnableCache = enableCache;
        }

        /// <summary>
        /// 是否启用缓存
        /// </summary>
        public bool EnableCache { get; private set; }

        /// <summary>
        /// 缓存过期策略
        /// </summary>
        public EntityCacheExpirationPolicies ExpirationPolicy
        {
            get
            {
                return this.expirationPolicy;
            }
            set
            {
                this.expirationPolicy = value;
            }
        }

        /// <summary>
        /// 实体正文缓存对应的属性名称,（如果不需单独存储实体正文缓存，则不要设置）
        /// </summary>
        public string PropertyNameOfBody { get; set; }
        /// <summary>
        /// 缓存分区的属性名称（多个属性用“,”分割）
        /// </summary>
        public string PropertyNamesOfArea { get; set; }
    }
}
