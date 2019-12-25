using PmSoft.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PmSoft.Tasks.Quartz
{

    /// <summary>
    /// 用以管理Quartz任务调度相关的操作 
    /// </summary>
    [Serializable]
    public class QuartzTaskScheduler : ITaskScheduler
    {
        /// <summary>
        /// 任务列表
        /// </summary>
        private List<TaskDetail> taskDetailList;

        private readonly TaskService taskService;

        private RunAtServer? runAtServer;

        /// <summary>
        /// 构造函数
        /// </summary>
        public QuartzTaskScheduler(TaskService taskService)
        {
            this.taskService = taskService;
        }


        /// <summary>
        /// 获取单个任务 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public TaskDetail GetTask(int Id)
        {
            return taskDetailList.FirstOrDefault(m => m.Id == Id);
        }

        /// <summary>
        /// 移除任务
        /// </summary>
        /// <param name="name"></param>
        private async void Remove(string name)
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.DeleteJob(new JobKey(name));
        }

        /// <summary>
        /// 重启所有任务
        /// </summary>
        public void ResumeAll()
        {
            this.Stop();
            this.Start(runAtServer);
        }

        /// <summary>
        /// 运行单个任务 
        /// </summary>
        /// <param name="Id"></param>
        public async Task Run(int Id)
        {
            TaskDetail task = this.GetTask(Id);
            await this.Run(task);
        }

        /// <summary>
        /// 运行单个任务
        /// </summary>
        /// <param name="task"></param>
        public async Task Run(TaskDetail taskDetail)
        {
            if (taskDetail != null)
            {
                Type type = Type.GetType(taskDetail.ClassType);
                if (type == null)
                {
                    LoggerLocator.GetLogger<QuartzTaskScheduler>().LogWarning($"任务： {taskDetail.Name} 的taskType为空。");
                }
                else
                {
                    ITask task = (ITask)Activator.CreateInstance(type);
                    if (task != null && !taskDetail.IsRunning)
                    {
                        try
                        {
                            await task.ExecuteAsync(null);
                        }
                        catch (Exception exception)
                        {
                            LoggerLocator.GetLogger<QuartzTaskScheduler>().LogError(exception, string.Format("执行任务： {0} 出现异常。", taskDetail.Name));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保存任务状态 - 将当前需要需要ResumeContinue为true的任务记录，以便应用程序重启后检查是否需要立即执行
        /// </summary>
        public async Task SaveTaskStatusAsync()
        {
            foreach (TaskDetail detail in taskDetailList)
            {
                await taskService.SaveTaskStatusAsync(detail);
            }
        }

        /// <summary>
        /// 启动任务
        /// </summary>
        public async void Start(RunAtServer? runAtServer)
        {
            this.runAtServer = runAtServer;
            var tasks = await taskService.GetAllAsync();
            taskDetailList = (runAtServer.HasValue ?
                tasks.Where(m => m.RunAtServer == runAtServer) : tasks)
                .ToList();

            if (taskDetailList.Count() != 0)
            {
                new TaskService();
                ISchedulerFactory factory = new StdSchedulerFactory();
                IScheduler scheduler = await factory.GetScheduler();
                foreach (TaskDetail detail in taskDetailList)
                {
                    if (detail.Enabled)
                    {
                        Type type = Type.GetType(detail.ClassType);
                        if (type != null)
                        {
                            string name = type.Name + "_trigger";
                            IJobDetail jobDetail = JobBuilder.Create(typeof(QuartzTask)).WithIdentity(type.Name).Build();
                            jobDetail.JobDataMap.Add(new KeyValuePair<string, object>("Id", detail.Id));
                            TriggerBuilder builder = TriggerBuilder.Create().WithIdentity(name).WithCronSchedule(detail.TaskRule);
                            /* 
                             *      withMisfireHandlingInstructionDoNothing
                                      ——不触发立即执行
                                      ——等待下次Cron触发频率到达时刻开始按照Cron频率依次执行

                                      withMisfireHandlingInstructionIgnoreMisfires
                                      ——以错过的第一个频率时间立刻开始执行
                                      ——重做错过的所有频率周期后
                                      ——当下一次触发频率发生时间大于当前时间后，再按照正常的Cron频率依次执行

                                      withMisfireHandlingInstructionFireAndProceed(默认)
                                      ——以当前时间为触发频率立刻触发一次执行
                                      ——然后按照Cron频率依次执行
                             */
                            if (detail.StartDate > DateTime.MinValue)
                            {
                                builder.StartAt(new DateTimeOffset(detail.StartDate));
                            }
                            DateTime? endDate = detail.EndDate;
                            DateTime startDate = detail.StartDate;
                            if (endDate.HasValue ? (endDate.GetValueOrDefault() > startDate) : false)
                            {
                                DateTime? nullable2 = detail.EndDate;
                                builder.EndAt(nullable2.HasValue ? new DateTimeOffset?(nullable2.GetValueOrDefault()) : null);
                            }
                            ICronTrigger trigger = (ICronTrigger)builder.Build();
                            DateTimeOffset timeOffset = await scheduler.ScheduleJob(jobDetail, trigger);
                            DateTime dateTime = timeOffset.DateTime;
                            detail.NextStart = new DateTime?(TimeZoneInfo.ConvertTime(dateTime, trigger.TimeZone));
                        }
                    }
                }
                await scheduler.Start();
            }
        }

        /// <summary>
        /// 停止任务
        /// </summary>
        public async void Stop()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Shutdown(true);
        }

        /// <summary>
        /// 更新任务在调度器中的状态 
        /// </summary>
        /// <param name="task"></param>
        public async void Update(TaskDetail task)
        {
            if (task != null)
            {
                int num = taskDetailList.FindIndex(n => n.Id == task.Id);
                if (taskDetailList[num] != null)
                {
                    task.ClassType = taskDetailList[num].ClassType;
                    task.LastEnd = taskDetailList[num].LastEnd;
                    task.LastStart = taskDetailList[num].LastStart;
                    task.LastIsSuccess = taskDetailList[num].LastIsSuccess;
                    taskDetailList[num] = task;
                    Type type = Type.GetType(task.ClassType);
                    if (type != null)
                    {
                        this.Remove(type.Name);
                        if (task.Enabled)
                        {
                            string name = type.Name + "_trigger";
                            ISchedulerFactory factory = new StdSchedulerFactory();
                            IScheduler scheduler = await factory.GetScheduler();
                            IJobDetail jobDetail = JobBuilder.Create(typeof(QuartzTask)).WithIdentity(type.Name).Build();
                            jobDetail.JobDataMap.Add(new KeyValuePair<string, object>("Id", task.Id));
                            TriggerBuilder builder = TriggerBuilder.Create().WithIdentity(name).WithCronSchedule(task.TaskRule);
                            if (task.StartDate > DateTime.MinValue)
                            {
                                builder.StartAt(new DateTimeOffset(task.StartDate));
                            }
                            if (task.EndDate.HasValue)
                            {
                                DateTime? endDate = task.EndDate;
                                DateTime startDate = task.StartDate;
                                if (endDate.HasValue ? (endDate.GetValueOrDefault() > startDate) : false)
                                {
                                    DateTime? nullable3 = task.EndDate;
                                    builder.EndAt(nullable3.HasValue ? new DateTimeOffset?(nullable3.GetValueOrDefault()) : null);
                                }
                            }
                            ICronTrigger trigger = (ICronTrigger)builder.Build();
                            DateTimeOffset timeOffset = await scheduler.ScheduleJob(jobDetail, trigger);
                            DateTime dateTime = timeOffset.DateTime;
                            task.NextStart = new DateTime?(TimeZoneInfo.ConvertTime(dateTime, trigger.TimeZone));
                        }
                    }
                }
            }

        }

    }
}

