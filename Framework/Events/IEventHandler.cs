using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件处理程序模块接口
    /// </summary>
    /// <typeparam name="TSender"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    public interface IEventHandler<TSender, TEventArgs> where TEventArgs : CommonEventArgs
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        Task HandleAsync(TSender sender, TEventArgs eventArgs);
    }
}
