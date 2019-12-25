using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件总线接口
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <typeparam name="TEventHandler"></typeparam>
        void Subscribe<TSender, TEventArgs, TEventHandler>() where TEventArgs : CommonEventArgs where TEventHandler : IEventHandler<TSender, TEventArgs>;

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="TSender"></typeparam>
        /// <typeparam name="TEventArgs"></typeparam>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        Task<bool> PublishAsync<TSender, TEventArgs>(TSender sender, TEventArgs eventArgs) where TEventArgs : CommonEventArgs;
    }
}
