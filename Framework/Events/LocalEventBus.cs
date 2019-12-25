using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Linq;
using PmSoft.Logging;

namespace PmSoft.Events
{
    /// <summary>
    /// 本地事件总线（用于订阅事件、发布事件）
    /// </summary>>
    public class LocalEventBus : IEventBus
    {
        private static ConcurrentDictionary<string, List<Type>> handlers;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        public LocalEventBus()
        {
            handlers = new ConcurrentDictionary<string, List<Type>>();
        }

        /// <summary>
        /// 获取事件KEY
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <returns></returns>
        private string GetEventKey<TSender, TEventArgs>()
        {
            return typeof(TSender).FullName + "-" + typeof(TEventArgs).FullName;
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TSender, TEventArgs, TEventHandler>()
            where TEventArgs : CommonEventArgs
            where TEventHandler : IEventHandler<TSender, TEventArgs>
        {
            string eventKey = GetEventKey<TSender, TEventArgs>();
            Type handlerType = typeof(TEventHandler);
            List<Type> handlerTypes;
            if (handlers.ContainsKey(eventKey) && handlers.TryGetValue(eventKey, out handlerTypes))
            {
                handlerTypes.Add(handlerType);
                handlers[eventKey] = handlerTypes;
            }
            else
            {
                handlerTypes = new List<Type> { handlerType };
                handlers.TryAdd(eventKey, handlerTypes);
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task<bool> PublishAsync<TSender, TEventArgs>(TSender sender, TEventArgs eventArgs)
            where TEventArgs : CommonEventArgs
        {
            string eventKey = GetEventKey<TSender, TEventArgs>();

            if (!handlers.ContainsKey(eventKey))
                return false;

            var handlerTypes = handlers[eventKey];

            foreach (Type handlerType in handlerTypes)
            {
                var handler = ServiceLocator.GetService<IEventHandler<TSender, TEventArgs>>(handlerType);
                if (null != handler)
                {
                    await handler.HandleAsync(sender, eventArgs).ContinueWith(task =>
                    {
                        task.Exception?.Handle(exception =>
                        {
                            LoggerLocator.GetLogger<LocalEventBus>().LogError(exception, "执行触发操作事件时发生异常");
                            return true;
                        });
                    }).ConfigureAwait(false);
                }
            }
            return true;
        }

    }
}
