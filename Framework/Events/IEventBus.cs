using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="A"></typeparam>
    public interface IEventBus<S, A>
    {
        /// <summary>
        /// 操作订阅 
        /// </summary>
        void Subscribe(Func<S, A, Task> handler);

        /// <summary>
        /// 批量操作订阅
        /// </summary>
        void Subscribe(Func<IEnumerable<S>, A, Task> handler);

        /// <summary>
        /// 触发操作事件 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        void Publish(S sender, A eventArgs);

        /// <summary>
        /// 触发批量操作事件 
        /// </summary>
        /// <param name="senders"></param>
        /// <param name="eventArgs"></param>
        void Publish(IEnumerable<S> senders, A eventArgs);

    }
}
