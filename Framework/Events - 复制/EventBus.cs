using System;
using System.Collections.Generic;
using System.Text;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件总线
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public class EventBus<TEventArgs>
    {
        private static readonly object lockObj;
        private static volatile EventBus<TEventArgs> instance;

        static EventBus()
        {
            instance = null;
            lockObj = new object();
        }

        /// <summary>
        /// 获取实例 
        /// </summary>
        /// <returns></returns>
        public static EventBus<TEventArgs> Instance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new EventBus<TEventArgs>();
                    }
                }
            }
            return instance;
        }
    }
}
