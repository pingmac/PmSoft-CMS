using System;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Options;

namespace PmSoft.DBContext
{
    /// <summary>
    /// 默认数据库上下文调度器
    /// 实现原理：初始化指定数量的数据库上下文实例(数据库持久连接)，优先获取不繁忙的实例
    /// </summary>
    public class DefaultDbContextScheduler<TDbContext> : IDbContextScheduler<TDbContext> where TDbContext : PetaPocoDbContext, new()
    {
        private readonly TDbContext[] dbContexts;
        private int _num;

        public DefaultDbContextScheduler(IOptions<DbContextSchedulerOptions> optionsAccessor)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            Options = optionsAccessor.Value;

            dbContexts = new TDbContext[Options.MaxSize];
        }

        public DbContextSchedulerOptions Options { get; private set; }

        public TDbContext GetDbContext()
        {

            int cnt = Interlocked.Increment(ref _num);
            if (cnt > Options.MaxSize)
            {
                Interlocked.Decrement(ref _num);

                var dbContext = dbContexts.Where(m => !m.IsBustling).
                    OrderByDescending(m => Guid.NewGuid())
                    .FirstOrDefault();

                if (dbContext != null)
                    return dbContext;

                Random random = new Random(Guid.NewGuid().GetHashCode());

                return dbContexts[random.Next(0, Options.MaxSize)];
            }

            TDbContext context = new TDbContext();
            dbContexts[cnt - 1] = context;

            return context;

        }


    }
}
