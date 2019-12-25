using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PmSoft;
using PmSoft.Events;

namespace ManagementWebHost
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddPmSoftFrameWork(Configuration);

            services.AddControllersWithViews();

            //��ȡweb���õ�����Soft��ͷ�ĳ���
            AssemblyName[] assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies().Where(n => n.Name.StartsWith("PmSoft")).ToArray();
            List<Assembly> assemblyList = assemblyNames.Select(n => Assembly.Load(n)).ToList();

            //��ȡ��Ŀ¼�µ�ָ�����ƿ�ͷ�ĵĳ���
            IEnumerable<string> files = Directory.EnumerateFiles(AppContext.BaseDirectory, "ManagementWebHost.dll");
            assemblyList.AddRange(files.Select(n => Assembly.Load(AssemblyName.GetAssemblyName(n))));
            Assembly[] assemblies = assemblyList.ToArray();

            //����ע�����е�EventMoudle
            services.AddSingletonByAssembly<IEventMoudle>(assemblies);

            //����ע�����е�EventHandler
            services.AddSingletonByAssembly(assemblies, "EventHandler");

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            ServiceLocator.Instance = app.ApplicationServices;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            //ע���¼��������
            IEnumerable<IEventMoudle> eventMoudles = app.ApplicationServices.GetServices<IEventMoudle>();
            foreach (var eventMoudle in eventMoudles)
            {
                eventMoudle.RegisterEventHandler();
            }
        }
    }
}
