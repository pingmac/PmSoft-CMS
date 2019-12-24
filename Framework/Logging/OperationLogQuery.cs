namespace PmSoft.Logging
{
    using System;
 
    /// <summary>
    /// OperationLog查询对象
    /// </summary>
    [Serializable]
    public class OperationLogQuery
    {
        /// <summary>
        /// 截止时间 
        /// </summary>
        public DateTime? EndDateTime { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// 操作类型 
        /// </summary>
        public string OperationType { get; set; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartDateTime { get; set; }
        /// <summary>
        /// 操作人ID
        /// </summary>
        public long? OperatorUserId { get; set; }
        /// <summary>
        /// 日志来源，一般为应用模块名称
        /// </summary>
        public string Source { get; set; }
    }
}

