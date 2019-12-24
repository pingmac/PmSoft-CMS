using Microsoft.Extensions.Options;

namespace PmSoft.Caching
{
    public class CacheServiceOptions : IOptions<CacheServiceOptions>
    {
        /// <summary>
        /// 缓存过期时间因子
        /// </summary>
        public float CacheExpirationFactor { get; set; } = 1.0F;

        /// <summary>
        /// 是否启用分布式缓存
        /// </summary>
        public bool EnableDistributedCache { get; set; } = false;

        CacheServiceOptions IOptions<CacheServiceOptions>.Value
        {
            get { return this; }
        }
    }
}
