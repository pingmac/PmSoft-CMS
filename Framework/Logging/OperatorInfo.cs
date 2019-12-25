using System;

namespace PmSoft.Logging
{
    /// <summary>
    /// 操作者信息
    /// </summary>
    [Serializable]
    public class OperatorInfo
    {
        /// <summary>
        /// 操作访问的url 
        /// </summary>
        public string AccessUrl { get; set; }
        /// <summary>
        /// 操作者名称 
        /// </summary>
        public string Operator { get; set; }
        /// <summary>
        /// 操作者IP 
        /// </summary>
        public string OperatorIP { get; set; } = "";
        /// <summary>
        /// 操作者UserId 
        /// </summary>
        public int OperatorUserId { get; set; }
        /// <summary>
        /// 操作者单位ID
        /// </summary>
        public int? UnitId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>
        public string UnitName { get; set; }
        /// <summary>
        /// 单位组织代码
        /// </summary>
        public string UnitCode { get; set; }
    }
}

