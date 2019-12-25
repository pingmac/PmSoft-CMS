using System.IO;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using MySql.Data.MySqlClient;
using PetaPoco;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis.Extensions.Core.Configuration;
using PmSoft.Caching;
using PmSoft.Repositories;
using PmSoft.Tasks;
using PmSoft.Tasks.Quartz;
using PmSoft.DBContext;
using PmSoft.Log4Net;
using PmSoft.Events;

namespace PmSoft
{
    public static class Builder
    {
        /// <summary>
        /// 注入基础组件
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddPmSoftFrameWork(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            //用户请求上下文访问器
            services.AddHttpContextAccessor();

            //控制器方法上下文访问器
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //添加Redis客户端
            services.AddRedisCacheClient(() => configuration.GetSection("Redis").Get<RedisConfiguration>());

            //注册默认缓存服务
            services.Configure<CacheServiceOptions>(configuration.GetSection("CacheService"));
            services.AddDefaultCacheService();

            //注册数据库上下文调度器
            services.AddPetaPocoDbContextScheduler(options =>
            {
                options.MaxSize = configuration.GetValue<int>("DbContextMaxSize", 5);
            });

            // 注册默认的IStoreProvider
            //services.AddSingleton<IStoreProvider>(c => new DefaultStoreProvider(Path.Combine(hostingEnvironment.WebRootPath, "Uploads"), "~/Uploads"));

            //注册任务调度器
            services.AddSingleton<ITaskScheduler, QuartzTaskScheduler>();

            //仓库注册
            services.AddSingleton(typeof(IRepository<>), typeof(Repository<>));

            //日志
            services.AddLogging(builder =>
            {
                builder
                .AddFilter("Microsoft", LogLevel.Warning)
                //.AddConsole()
                .AddDebug()
                .AddLog4Net();
            });

            //注册事件总线
            //services.AddSingleton<IEventBus, LocalEventBus>();
            services.AddSingleton<IEventBus, RedisEventBus>();

            //注册定时任务业务逻辑
            services.AddSingleton<TaskService>();

            //注册CAP
            //services.Configure<DotNetCore.CAP.RabbitMQOptions>(configuration.GetSection("RabbitMQ"));
            //services.AddCap(options =>
            //{
            //    options.UseSqlServer(configuration.GetConnectionString("ManagerDB")); //储存计数统计
            //    options.UseRabbitMQ(config => { });
            //});

        }
    }
}
