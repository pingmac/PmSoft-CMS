using System;
using System.Runtime.CompilerServices;
using PetaPoco;
using PmSoft.Caching;
using PmSoft.Utilities;

namespace PmSoft.Tasks
{

    /// <summary>
    /// 任务详细信息实体 
    /// </summary>
    [Serializable,
        CacheSetting(false),
        TableName("Task_Details"),
        PrimaryKey("Id", AutoIncrement = true)]
    public class TaskDetail : IEntity
    {
        /// <summary>
        /// 获取规则指定部分 
        /// </summary>
        /// <param name="rulePart">规则组成部分</param>
        /// <returns></returns>
        public string GetRulePart(RulePart rulePart)
        {
            return "";
        }

        /// <summary>
        /// 实例化实体 - 实例化实体时先根据taskName从数据库中获取，如果取不到则创建新实例
        /// </summary>
        /// <returns></returns>
        public static TaskDetail New()
        {
            return new TaskDetail();
        }

        /// <summary>
        /// 任务类型
        /// </summary>
        public string ClassType { get; set; }

        /// <summary>
        /// 是否启用 
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// 上次执行结束时间
        /// </summary>
        public DateTime? LastEnd { get; set; }

        /// <summary>
        /// 上次任务执行状态 
        /// </summary>
        public bool? LastIsSuccess { get; set; }

        /// <summary>
        /// 上次执行开始时间
        /// </summary>
        public DateTime? LastStart { get; set; }

        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextStart { get; set; }

        /// <summary>
        /// 程序重启后立即执行 - 在应用程序池重启后,是否检查此任务上次被执行过，如果没有执行则立即执行
        /// </summary>
        public bool RunAtRestart { get; set; }

        /// <summary>
        /// 任务在哪台服务器上运行
        /// </summary>
        public RunAtServer RunAtServer { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 任务的执行时间规则
        /// </summary>
        public string TaskRule { get; set; }

        [Ignore]
        object IEntity.EntityId
        {
            get
            {
                return this.Id;
            }
        }


    }
}

