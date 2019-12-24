using System;

namespace PmSoft.Tasks
{
    /// <summary>
    /// 任务规则组成部分
    /// </summary>
    public enum RulePart
    {
        /// <summary>
        /// 秒域
        /// </summary>
        seconds,
        /// <summary>
        /// 分钟域
        /// </summary>
        minutes,
        /// <summary>
        /// 小时域 
        /// </summary>
        hours,
        /// <summary>
        /// 日期域
        /// </summary>
        day,
        /// <summary>
        /// 规则月部分 
        /// </summary>
        mouth,
        /// <summary>
        /// 星期域
        /// </summary>
        dayofweek
    }

    /// <summary>
    /// 任务频率
    /// </summary>
    public enum TaskFrequency
    {
        /// <summary>
        /// 每周
        /// </summary>
        Weekly,
        /// <summary>
        /// 每月
        /// </summary>
        PerMonth,
        /// <summary>
        /// 每天
        /// </summary>
        EveryDay
    }

    /// <summary>
    /// 任务在哪台服务器上运行
    /// </summary>
    public enum RunAtServer
    {
        /// <summary>
        /// 主平台
        /// </summary>
        MasterHost = 101,
        /// <summary>
        /// 服务
        /// </summary>
        ServiceHost = 201
    }
}
