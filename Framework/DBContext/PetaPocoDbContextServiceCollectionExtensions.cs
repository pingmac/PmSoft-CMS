using Microsoft.Extensions.DependencyInjection;
using System;

namespace PmSoft.DBContext
{
    public static class PetaPocoDbContextServiceCollectionExtensions
    {
        //public static IServiceCollection AddPetaPocoDbContextPool(this IServiceCollection services, Action<DbContextPoolOptions> setupAction)
        //{
        //    if (null == services)
        //    {
        //        throw new ArgumentNullException(nameof(services));
        //    }
        //    if (setupAction == null)
        //    {
        //        throw new ArgumentNullException(nameof(setupAction));
        //    }

        //    services.AddOptions();
        //    services.Configure(setupAction);

        //    services.AddSingleton(typeof(IDbContextPool<>), typeof(DefaultDbContextPool<>));
        //    services.AddScoped(typeof(IDbContextHolder<>), typeof(TransientDbContextHolder<>));

        //    return services;
        //}

        public static IServiceCollection AddPetaPocoDbContextScheduler(this IServiceCollection services, Action<DbContextSchedulerOptions> setupAction)
        {
            if (null == services)
            {
                throw new ArgumentNullException(nameof(services));
            }
            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            services.Configure(setupAction);
            services.AddSingleton(typeof(IDbContextScheduler<>), typeof(DefaultDbContextScheduler<>));

            return services;
        }

    }
}
