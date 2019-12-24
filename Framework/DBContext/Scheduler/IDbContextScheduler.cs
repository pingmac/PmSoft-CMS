using System;
using System.Collections.Generic;
using System.Text;

namespace PmSoft.DBContext
{
    public interface IDbContextScheduler<TDbContext>
    {
        TDbContext GetDbContext();
    }
}
