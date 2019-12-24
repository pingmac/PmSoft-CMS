using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace PmSoft.DBContext
{
    /// <summary>
    /// 项目默认数据库
    /// </summary>
    public class DefaultDbContext : PetaPocoDbContext
    {
        public DefaultDbContext() : this(ServiceLocator.GetService<IConfiguration>()) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor"></param>
        public DefaultDbContext(IConfiguration configuration)
            : base(new PetaPocoDbContextOptions
            {
                ConnectionString = configuration.GetConnectionString("ManagerDB"),
                ProviderName = "SqlServer"
            })
        {
        }
    }
}
