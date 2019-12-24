namespace PmSoft.Tasks
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// 任务执行控制器 - 需要从DI容器中获取注册的tasks
    /// </summary>
    public interface ITaskScheduler
    {
        /// <summary>
        /// 获取单个任务 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        TaskDetail GetTask(int Id);
        /// <summary>
        /// 重启所有任务
        /// </summary>
        void ResumeAll();
        /// <summary>
        /// 执行单个任务
        /// </summary>
        /// <param name="Id"></param>
        Task Run(int Id);
        /// <summary>
        /// 保存任务状态 - 将当前需要需要ResumeContinue为true的任务记录，以便应用程序重启后检查是否需要立即执行
        /// </summary>
        Task SaveTaskStatusAsync();
        /// <summary>
        /// 开始执行任务
        /// </summary>
        void Start(RunAtServer? runAtServer);
        /// <summary>
        /// 停止任务
        /// </summary>
        void Stop();
        /// <summary>
        /// 更新任务在调度器中的状态 
        /// </summary>
        /// <param name="task"></param>
        void Update(TaskDetail task);
    }
}

