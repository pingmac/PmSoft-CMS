using System;
using System.Threading.Tasks;
using PetaPoco;
using PmSoft;
using PmSoft.Logging;
using PmSoft.Repositories;
using PmSoft.Utilities;

namespace PmSoft.Logging.Repositories
{


    /// <summary>
    /// OperationLog仓储接口 
    /// </summary>
    public class OperationLogRepository : Repository<OperationLog>, IOperationLogRepository, IRepository<OperationLog>
    {
        public async Task<int> CleanAsync(DateTime? startDate, DateTime? endDate)
        {
            Sql builder = Sql.Builder;
            builder.Append("delete from tb_Operationlogs", new object[0]);
            if (startDate.HasValue)
            {
                builder.Where("CreateDate >= @0", new object[] { startDate.Value });
            }
            if (endDate.HasValue)
            {
                builder.Where("CreateDate <= @0", new object[] { endDate.Value });
            }
            return await CreateDAO().ExecuteAsync(builder);
        }

        public async Task<PagedList<OperationLog>> GetLogsAsync(OperationLogQuery query, int pageSize, int pageIndex)
        {
            Sql builder = Sql.Builder;
            if (!string.IsNullOrEmpty(query.Description))
            {
                builder.Where("OperationObjectName like @0 or Description like @0", new object[] { '%' + query.Description + '%' });
            }
            if (!string.IsNullOrEmpty(query.OperationType))
            {
                builder.Where("OperationType = @0", new object[] { query.OperationType });
            }
            if (!string.IsNullOrEmpty(query.Operator))
            {
                builder.Where("Operator like @0", new object[] { "%" + query.Operator + "%" });
            }
            if (query.StartDateTime.HasValue)
            {
                builder.Where("CreateDate >= @0", new object[] { ValueUtility.GetSafeSqlDateTime(query.StartDateTime.Value) });
            }
            if (query.EndDateTime.HasValue)
            {
                builder.Where("CreateDate <= @0", new object[] { ValueUtility.GetSafeSqlDateTime(query.EndDateTime.Value) });
            }
            if (query.OperatorUserId.HasValue)
            {
                builder.Where("OperatorUserId = @0", new object[] { query.OperatorUserId.Value });
            }
            if (!string.IsNullOrEmpty(query.Source))
            {
                builder.Where("Source like @0", new object[] { "%" + query.Source + "%" });
            }
            builder.OrderBy(new object[] { "Id desc" });
            return await GetPagingEntitiesAsync(pageSize, pageIndex, builder);
        }
    }
}

