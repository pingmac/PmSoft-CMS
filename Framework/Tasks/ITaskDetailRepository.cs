namespace PmSoft.Tasks
{
    using System;
    using System.Threading.Tasks;
    using PmSoft.Repositories;

    public interface ITaskDetailRepository : IRepository<TaskDetail>
    {
        /// <summary>
        /// 保存任务状态 
        /// </summary>
        /// <param name="taskDetail"></param>
        Task SaveTaskStatusAsync(TaskDetail taskDetail);
    }
}

