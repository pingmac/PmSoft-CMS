using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using PmSoft.Logging;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件总线（用于定义事件、触发事件）
    /// 分布式项目不适应 by AK
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    //public class EventBus<S> : EventBus<S, CommonEventArgs> { }

    /// <summary>
    /// 事件总线（用于定义事件、触发事件）
    /// </summary>
    /// <typeparam name="S">触发事件的对象类型</typeparam>
    /// <typeparam name="T">通用事件参数</typeparam>
    public class EventBus<S, T> : IEventBus<S, T> where T : CommonEventArgs
    {
        private static volatile EventBus<S, T> instance;
        private static ConcurrentBag<Func<S, T, Task>> handlers;
        private static ConcurrentBag<Func<IEnumerable<S>, T, Task>> batchHandlers;
        private static readonly object lockObj;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static EventBus()
        {
            instance = null;
            lockObj = new object();
            handlers = new ConcurrentBag<Func<S, T, Task>>();
            batchHandlers = new ConcurrentBag<Func<IEnumerable<S>, T, Task>>();
        }

        /// <summary>
        /// 获取实例 
        /// </summary>
        /// <returns></returns>
        public static EventBus<S, T> Instance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new EventBus<S, T>();
                    }
                }
            }
            return instance;
        }

        #region 订阅

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Func<S, T, Task> handler)
        {
            handlers.Add(handler);
        }

        /// <summary>
        /// 订阅
        /// </summary>
        /// <param name="handler"></param>
        public void Subscribe(Func<IEnumerable<S>, T, Task> handler)
        {
            batchHandlers.Add(handler);
        }


        #endregion

        #region 触发

        /// <summary>
        /// 触发操作事件(异步执行)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        public void Publish(S sender, T eventArgs)
        {
            foreach (Func<S, T, Task> handler in handlers)
            {
                Task.Run(() => handler(sender, eventArgs)).ContinueWith(task =>
                 {
                     task.Exception?.Handle(exception =>
                     {
                         LoggerLocator.GetLogger<EventBus<S, T>>().LogError(exception, "执行触发操作事件时发生异常");
                         return true;
                     });
                 }).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 触发批量操作事件(异步执行)
        /// </summary>
        /// <param name="senders">触发事件的对象集合</param>
        /// <param name="eventArgs">事件参数</param>
        public void Publish(IEnumerable<S> senders, T eventArgs)
        {
            foreach (Func<IEnumerable<S>, T, Task> handler in batchHandlers)
            {
                Task.Run(() => handler(senders, eventArgs)).ContinueWith(task =>
                {
                    task.Exception?.Handle(exception =>
                    {
                        LoggerLocator.GetLogger<EventBus<S, T>>().LogError(exception, "执行触发批量操作事件时发生异常");
                        return true;
                    });
                }).ConfigureAwait(false);
            }
        }


        #endregion

    }
}
