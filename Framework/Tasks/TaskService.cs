using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmSoft.Tasks
{

    /// <summary>
    /// 任务业务逻辑 
    /// </summary>
    public class TaskService
    {
        private ITaskDetailRepository taskDetailRepository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TaskService() : this(new TaskDetailRepository())
        {
        }

        /// <summary>
        /// 可设置repository的构造函数（主要用于测试用例） 
        /// </summary>
        /// <param name="taskDetailRepository"></param>
        public TaskService(ITaskDetailRepository taskDetailRepository)
        {
            this.taskDetailRepository = taskDetailRepository;
        }

        /// <summary>
        /// 依据TaskName获取任务 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<TaskDetail> GetAsync(int Id)
        {
            return await taskDetailRepository.GetAsync(Id);
        }

        /// <summary>
        /// 获取所用任务
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TaskDetail>> GetAllAsync()
        {
            return await taskDetailRepository.GetAllAsync();
        }

        /// <summary>
        /// 应用程序关闭时保存任务当前状态
        /// </summary>
        /// <param name="entity"></param>
        public async Task SaveTaskStatusAsync(TaskDetail entity)
        {
            await taskDetailRepository.SaveTaskStatusAsync(entity);
        }

        /// <summary>
        /// 更新任务相关信息
        /// </summary>
        /// <param name="entity"></param>
        public async Task UpdateAsync(TaskDetail entity)
        {
            await taskDetailRepository.UpdateAsync(entity);
            ServiceLocator.GetService<ITaskScheduler>().Update(entity);
        }
    }
}

