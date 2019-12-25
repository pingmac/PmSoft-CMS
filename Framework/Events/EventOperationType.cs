using System;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件操作类型
    /// </summary>
    public class EventOperationType
    {
        private static volatile EventOperationType operationType = null;
        private static readonly object lockObj = new object();

        private EventOperationType()
        {
        }

        /// <summary>
        /// 获取实例
        /// </summary>
        /// <returns></returns>
        public static EventOperationType Instance()
        {
            if (operationType == null)
            {
                lock (lockObj)
                {
                    if (operationType == null)
                    {
                        operationType = new EventOperationType();
                    }
                }
            }
            return operationType;
        }

        /// <summary>
        /// 通过审核
        /// </summary>
        /// <returns></returns>
        public string Approved()
        {
            return "Approved";
        }

        /// <summary>
        /// 取消审核
        /// </summary>
        /// <returns></returns>
        public string Disapproved()
        {
            return "Disapproved";
        }


        /// <summary>
        /// 创建 
        /// </summary>
        /// <returns></returns>
        public string Create()
        {
            return "Create";
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <returns></returns>
        public string Update()
        {
            return "Update";
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public string Delete()
        {
            return "Delete";
        }


        /// <summary>
        /// 启用
        /// </summary>
        /// <returns></returns>
        public string Enable()
        {
            return "Enable";
        }

        /// <summary>
        /// 禁用
        /// </summary>
        /// <returns></returns>
        public string Disable()
        {
            return "Disable";
        }
    }
}
