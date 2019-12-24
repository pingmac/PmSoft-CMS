using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PmSoft.Logging;
using Quartz;

namespace PmSoft.Tasks.Quartz
{
    /// <summary>
    /// Quartz任务实现 
    /// </summary>
   // [DisallowConcurrentExecution]
    public class QuartzTask : IJob
    {
        /// <summary>
        /// 执行任务 - 外部不需调用，仅用于任务调度组建内部
        /// </summary>
        /// <param name="context">Quartz任务运行环境</param>
        public async Task Execute(IJobExecutionContext context)
        {

            int Id = context.JobDetail.JobDataMap.GetInt("Id");
            TaskDetail task = TaskSchedulerFactory.GetScheduler().GetTask(Id);
            if (task == null)
            {
                throw new ArgumentException("Not found task ：" + task.Name);
            }
            TaskService service = new TaskService();
            task.IsRunning = true;
            DateTime dt = DateTime.Now;
            try
            {
                await ((ITask)Activator.CreateInstance(Type.GetType(task.ClassType))).ExecuteAsync(task);
                task.LastIsSuccess = true;
            }
            catch (Exception exception)
            {
                LogFactory.GetLogger<QuartzTask>().LogError(exception, $"Exception while running job {context.JobDetail.Key} of type {context.JobDetail.JobType}");
                task.LastIsSuccess = false;
            }
            task.IsRunning = false;
            task.LastStart = new DateTime?(dt);
            if (context.NextFireTimeUtc.HasValue)
            {
                task.NextStart = new DateTime?(context.NextFireTimeUtc.Value.LocalDateTime);
            }
            else
            {
                task.NextStart = null;
            }
            task.LastEnd = new DateTime?(DateTime.Now);
            await service.SaveTaskStatusAsync(task);
        }
    }
}

