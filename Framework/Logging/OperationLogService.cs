namespace PmSoft.Logging
{
    using System;
    using System.Threading.Tasks;
    using PmSoft;
    using PmSoft.Logging.Repositories;
    using PmSoft.Utilities;

    /// <summary>
    /// 操作日志业务逻辑类
    /// </summary>
    public class OperationLogService
    {
        private readonly IOperationLogRepository repository;

        /// <summary>
        /// 构造函数 
        /// </summary>
        /// <param name="repository"></param>
        public OperationLogService(IOperationLogRepository repository)
        {
            this.repository = repository;
        }

        /// <summary>
        /// 删除指定时间段内的日志列表 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<int> CleanAsync(DateTime? startDate, DateTime? endDate)
        {
            return await repository.CleanAsync(startDate, endDate);
        }

        /// <summary>
        /// 创建操作日志
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public async Task<long> CreateAsync(OperationLog entry)
        {
            if (!entry.OperationType.HasValue || string.IsNullOrEmpty(entry.Operator))
                return 0;

            await repository.InsertAsync(entry);
            return entry.LogId;
        }

        /// <summary>
        /// 创建操作日志
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="operationType"></param>
        /// <returns></returns>
        public async Task<OperationLog> CreateAsync<TEntity>(TEntity entity, string operationType) where TEntity : class
        {
            return await CreateAsync(entity, operationType, default(TEntity));
        }

        /// <summary>
        /// 创建操作日志 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entity"></param>
        /// <param name="operationType"></param>
        /// <param name="historyData"></param>
        /// <returns></returns>
        public async Task<OperationLog> CreateAsync<TEntity>(TEntity entity, string operationType, TEntity historyData) where TEntity : class
        {
            IOperatorInfoGetter getter = ServiceLocator.GetService<IOperatorInfoGetter>();
            if (getter == null)
            {
                throw new ApplicationException("IOperatorInfoGetter not registered to DIContainer");
            }
            OperationLog operationLog = new OperationLog(getter.GetOperatorInfo());
            await repository.InsertAsync(operationLog);
            return operationLog;
        }

        /// <summary>
        /// 删除entryId相应的操作日志 
        /// </summary>
        /// <param name="entryId"></param>
        public async Task DeleteAsync(long entryId)
        {
            await repository.DeleteByEntityIdAsync(entryId);
        }

        /// <summary>
        /// 根据DiscussQuestionQuery查询获取可分页的数据集合 
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<PagedList<OperationLog>> GetLogsAsync(OperationLogQuery query, int pageSize, int pageIndex)
        {
            return await repository.GetLogsAsync(query, pageSize, pageIndex);
        }
    }
}

