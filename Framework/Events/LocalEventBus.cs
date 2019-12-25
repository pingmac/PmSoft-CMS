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
        private static ConcurrentDictionary<Type, List<Type>> handlers;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        public LocalEventBus()
        {
            handlers = new ConcurrentDictionary<Type, List<Type>>();
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        public void Subscribe<TEventArgs, TEventHandler>()
            where TEventArgs : CommonEventArgs
            where TEventHandler : IEventHandler<CommonEventArgs>
        {
            Type eventArgsType = typeof(TEventArgs);
            Type handlerType = typeof(TEventHandler);
            List<Type> handlerTypes;
            if (handlers.ContainsKey(eventArgsType) && handlers.TryGetValue(eventArgsType, out handlerTypes))
            {
                handlerTypes.Add(handlerType);
                handlers[eventArgsType] = handlerTypes;
            }
            else
            {
                handlerTypes = new List<Type> { handlerType };
                handlers.TryAdd(eventArgsType, handlerTypes);
            }
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        public async Task<bool> PublishAsync<TEventArgs>(TEventArgs eventArgs)
            where TEventArgs : CommonEventArgs
        {
            if (!handlers.ContainsKey(typeof(TEventArgs)))
                return false;

            var handlerTypes = handlers[typeof(TEventArgs)];

            foreach (Type handlerType in handlerTypes)
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
