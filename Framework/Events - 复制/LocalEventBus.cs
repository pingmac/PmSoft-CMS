using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PmSoft.Logging;

namespace PmSoft.Events
{
    /// <summary>
    /// 本地事件总线（用于订阅事件、发布事件）
    /// </summary>>
    /// <typeparam name="TEventArgs">通用事件参数</typeparam>
    public class LocalEventBus<TEventArgs> : IEventBus<TEventArgs> where TEventArgs : CommonEventArgs
    {
        private static volatile LocalEventBus<TEventArgs> instance;
        private static ConcurrentBag<Type> handlers;
        private static readonly object lockObj;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static LocalEventBus()
        {
            instance = null;
            lockObj = new object();
            handlers = new ConcurrentBag<Type>();
        }

        /// <summary>
        /// 获取实例 
        /// </summary>
        /// <returns></returns>
        public static LocalEventBus<TEventArgs> Instance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new LocalEventBus<TEventArgs>();
                    }
                }
            }
            return instance;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventHandler">事件处理类</typeparam>
        public void Subscribe<TEventHandler>() where TEventHandler : IEventHandler<CommonEventArgs>
        {
            handlers.Add(typeof(TEventHandler));
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventArgs">事件参数</param>
        /// <returns></returns>
        public async Task<bool> PublishAsync(TEventArgs eventArgs)
        {
            foreach (Type handlerType in handlers)
            {
                var handler = ServiceLocator.GetService<IEventHandler<TEventArgs>>(handlerType);
                if (null != handler)
                {
                    await handler.HandleAsync(eventArgs).ConfigureAwait(false);
                }
            }
            return true;
        }

    }
}
