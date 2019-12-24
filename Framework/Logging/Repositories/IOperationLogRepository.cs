namespace PmSoft.Logging.Repositories
{
    using System;
    using System.Threading.Tasks;
    using PmSoft;
    using PmSoft.Logging;
    using PmSoft.Repositories;

    /// <summary>
    /// OperationLog仓储接口 
    /// </summary>
    public interface IOperationLogRepository : IRepository<OperationLog>
    {
        /// <summary>
        /// 删除指定时间段内的日志列表 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<int> CleanAsync(DateTime? startDate, DateTime? endDate);
        /// <summary>
        /// 根据DiscussQuestionQuery查询获取可分页的数据集合 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        Task<PagedList<OperationLog>> GetLogsAsync(OperationLogQuery query, int pageSize, int pageIndex);
    }
}

