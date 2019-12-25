using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public interface IEventBus<TEventArgs>
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TEventHandler">事件处理类</typeparam>
        void Subscribe<TEventHandler>() where TEventHandler : IEventHandler<CommonEventArgs>;
 
        /// <summary>
        /// 发布事件
        /// </summary>
        /// <param name="eventArgs">事件参数</param>
        /// <returns></returns>
        Task<bool> PublishAsync(TEventArgs eventArgs);
    }
}
