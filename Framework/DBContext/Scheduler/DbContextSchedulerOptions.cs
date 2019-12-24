using Microsoft.Extensions.Options;

namespace PmSoft.DBContext
{
    public class DbContextSchedulerOptions : IOptions<DbContextSchedulerOptions>
    {
        public DbContextSchedulerOptions() { MaxSize = 5; }

        public int MaxSize { get; set; }

        DbContextSchedulerOptions IOptions<DbContextSchedulerOptions>.Value
        {
            get { return this; }
        }
    }
}
