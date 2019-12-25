using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    public interface IEventHandler<TSender, TEventArgs> where TEventArgs : CommonEventArgs
    {
        Task HandleAsync(TEventArgs eventArgs);
    }
}
