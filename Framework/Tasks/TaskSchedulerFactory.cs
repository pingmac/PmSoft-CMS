namespace PmSoft.Tasks
{
    using System;

    /// <summary>
    /// 任务工厂 
    /// </summary>
    public static class TaskSchedulerFactory
    {
        private static ITaskScheduler taskScheduler;

        /// <summary>
        /// 获取任务调度器 
        /// </summary>
        /// <returns></returns>
        public static ITaskScheduler GetScheduler()
        {
            if (taskScheduler == null)
            {
                taskScheduler = ServiceLocator.GetService<ITaskScheduler>();
            }
            return taskScheduler;
        }
    }
}

