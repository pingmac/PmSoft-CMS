using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PmSoft.Events
{
    public interface IEventHandler<TEventArgs> where TEventArgs : CommonEventArgs
    {
        Task HandleAsync(TEventArgs eventArgs);
    }
}
