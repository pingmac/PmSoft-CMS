using System;
using PmSoft.Logging;

namespace PmSoft.Events
{
    /// <summary>
    /// 通用事件参数
    /// </summary>
    [Serializable]
    public class CommonEventArgs : EventArgs
    {
        /// <summary>
        /// 构造函数 建议使用EventOperationType协助输入，例如：EventOperationType.Instance().Create() 
        /// </summary>
        /// <param name="eventOperationType">事件操作类型</param>
        public CommonEventArgs(string eventOperationType)
        {
            //IOperatorInfoGetter getter = ServiceLocator.GetService<IOperatorInfoGetter>();
            //if (getter == null)
            //{
            //    throw new ApplicationException("IOperatorInfoGetter not registered to DIContainer");
            //}
            //this.EventOperationType = eventOperationType;
            //this.OperatorInfo = getter.GetOperatorInfo();
        }

        public CommonEventArgs(string eventOperationType, OperatorInfo operatorInfo)
        {
            this.EventOperationType = eventOperationType;
            this.OperatorInfo = operatorInfo;
        }

        /// <summary>
        /// 事件操作类型
        /// </summary>
        public string EventOperationType { get; }

        /// <summary>
        /// 操作人信息
        /// </summary>
        public OperatorInfo OperatorInfo { get; }
    }
}
