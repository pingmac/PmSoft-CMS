using PetaPoco;
using Microsoft.Extensions.Options;

namespace PmSoft.DBContext
{
    /// <summary>
    /// PetaPoco数据库上下文
    /// </summary>
    public abstract class PetaPocoDbContext : Database
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="optionsAccessor"></param>
        protected PetaPocoDbContext(PetaPocoDbContextOptions optionsAccessor)
            : base(optionsAccessor.ConnectionString, optionsAccessor.ProviderName)
        {
            KeepConnectionAlive = true; //是否保持长连接
        }
    }
}
