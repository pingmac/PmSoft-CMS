using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PmSoft.DBContext;

namespace PmSoft
{
    /// <summary>
    /// 服务定位器
    /// </summary>
    public class ServiceLocator
    {
        public static IServiceProvider Instance { get; set; }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetService<TService>()
        {
            return Instance.GetService<TService>();
        }

        /// <summary>
        /// 获取服务集合
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static IEnumerable<TService> GetServices<TService>()
        {
            return Instance.GetServices<TService>();
        }

        /// <summary>
        /// 创建一个作用域
        /// </summary>
        /// <returns></returns>
        public static IServiceScope CreateScope()
        {
            return Instance.CreateScope();
        }

        /// <summary>
        /// 获取HTTP上下文作用域的服务
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public static TService GetHttpContextScopeService<TService>()
        {
            IHttpContextAccessor contextAccessor = GetService<IHttpContextAccessor>();

            if (contextAccessor?.HttpContext == null)
                return default(TService);

            return contextAccessor.HttpContext.RequestServices.GetService<TService>();
        }

        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetDBContext<T>() where T : PetaPocoDbContext
        {
            return GetService<IDbContextScheduler<T>>().GetDbContext();
            //return GetService<IDbContextPool<T>>().ContextPool.Get();
        }
    }

    public static class ServiceCollectionServiceExtensions
    {
        public static void AddSingletonByAssembly(this IServiceCollection services, Assembly[] assemblies, string endsWithName)
        {
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract);
                foreach (Type type in types)
                {
                    if (type.Name.EndsWith(endsWithName))
                        services.AddSingleton(type);
                }
            }
        }

        public static void AddSingletonByAssembly<TService>(this IServiceCollection services, Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                IEnumerable<Type> types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface);
                foreach (Type type in types)
                {

                    if (type.GetInterfaces().Any(m => m == typeof(TService)))
                        services.AddSingleton(typeof(TService), type);
                }
            }
        }
    }
}
