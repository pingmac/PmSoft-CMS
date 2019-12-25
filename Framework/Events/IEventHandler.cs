using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    /// <summary>
    /// 事件处理程序模块接口
    /// </summary>
    /// <typeparam name="TEventArgs"></typeparam>
    public interface IEventHandler<TEventArgs> where TEventArgs : CommonEventArgs
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        Task HandleAsync(TEventArgs eventArgs);
    }
}
