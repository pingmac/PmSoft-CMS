using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Core.Abstractions;
using StackExchange.Redis.Extensions.Core.Implementations;
using StackExchange.Redis.Extensions.Binary;
using StackExchange.Redis.Extensions.Core.Configuration;

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
            services.AddOptions<CacheServiceOptions>();
            services.AddSingleton<ICacheService, DefaultCacheService>();

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

            services.Configure(setupAction);
            services.AddDefaultCacheService();

            return services;
        }

        public static IServiceCollection AddRedisCacheClient(this IServiceCollection services, Func<RedisConfiguration> setupfunc)
        {
            services.AddSingleton(setupfunc());
            services.AddSingleton<IRedisCacheClient, RedisCacheClient>();
            services.AddSingleton<IRedisCacheConnectionPoolManager, RedisCacheConnectionPoolManager>();
            services.AddSingleton<IRedisDefaultCacheClient, RedisDefaultCacheClient>();
            services.AddSingleton<ISerializer, BinarySerializer>();

            return services;
        }

    }
}
