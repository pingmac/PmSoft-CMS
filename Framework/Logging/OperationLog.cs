using PetaPoco;
using System;
using System.Runtime.CompilerServices;
using PmSoft;
using PmSoft.Caching;

namespace PmSoft.Logging
{
    /// <summary>
    /// 操作日志实体
    /// </summary>
    [Serializable, CacheSetting(true), TableName("tb_Operationlogs"), PrimaryKey("Id", AutoIncrement = true)]
    public class OperationLog : IEntity
    {
        /// <summary>
        /// 无参构造函数 
        /// </summary>
        public OperationLog()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="operatorInfo"></param>
        public OperationLog(OperatorInfo operatorInfo)
        {
            this.UnitId = operatorInfo.UnitId;
            this.OrganizationName = operatorInfo.UnitName;
            this.OrganizationCode = operatorInfo.UnitCode;
            this.OperatorUserId = operatorInfo.OperatorUserId;
            this.OperatorIP = operatorInfo.OperatorIP;
            this.Operator = operatorInfo.Operator;
            this.AccessUrl = operatorInfo.AccessUrl;
            this.CreateDate = DateTime.Now;
        }

        /// <summary>
        /// Id
        /// </summary>	
        [Column(Name = "Id")]
        public long LogId { get; set; }
        /// <summary>
        /// 操作模块名称
        /// </summary>		
        public string ModuleName { get; set; }
        /// <summary>
        /// 操作类型
        /// </summary>		
        public int? OperationType { get; set; }
        /// <summary>
        /// 操作对象名称
        /// </summary>		
        public string OperationObjectName { get; set; }
        /// <summary>
        /// 操作对象ID
        /// </summary>		
        public long OperationObjectId { get; set; }
        /// <summary>
        /// 日志描述
        /// </summary>		
        public string Description { get; set; }
        /// <summary>
        /// 单位ID
        /// </summary>		
        public int? UnitId { get; set; }
        /// <summary>
        /// 单位名称
        /// </summary>		
        public string OrganizationName { get; set; }
        /// <summary>
        /// 组织代码
        /// </summary>		
        public string OrganizationCode { get; set; }
        /// <summary>
        /// 操作用户ID
        /// </summary>		
        public long OperatorUserId { get; set; }
        /// <summary>
        /// 操作者显示名称
        /// </summary>		
        public string Operator { get; set; }
        /// <summary>
        /// 操作者IP
        /// </summary>		
        public string OperatorIP { get; set; }
        /// <summary>
        /// 访问URL
        /// </summary>		
        public string AccessUrl { get; set; }
        /// <summary>
        /// 查询SQL条件
        /// </summary>		
        public string SqlCondition { get; set; }
        /// <summary>
        /// 错误代码
        /// </summary>		
        public string ErrorCode { get; set; }
        /// <summary>
        /// 操作是否成功
        /// </summary>		
        public bool IsSucceed { get; set; } = true;
        /// <summary>
        /// 创建时间
        /// </summary>		
        public DateTime CreateDate { get; set; }

        [Ignore]
        object IEntity.EntityId
        {
            get
            {
                return this.LogId;
            }
        }


    }
}

