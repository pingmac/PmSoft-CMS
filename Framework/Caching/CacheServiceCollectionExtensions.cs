using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PmSoft.Caching
{
    public static class CacheServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultCacheService(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            //添加本地缓存
            services.AddMemoryCache();

            services.AddOptions();
            services.TryAdd(ServiceDescriptor.Singleton<ICacheService, DefaultCacheService>());

            return services;
        }


        public static IServiceCollection AddDefaultCacheService(this IServiceCollection services, Action<CacheServiceOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddDefaultCacheService();
            services.Configure(setupAction);

            return services;
        }

    }
}
