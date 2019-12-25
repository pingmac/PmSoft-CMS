using PmSoft.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementWebHost.EventMoudles
{
    public class UserEventMoudle : IEventMoudle
    {
        private readonly IEventBus eventBus;

        public UserEventMoudle(IEventBus eventBus)
        {
            this.eventBus = eventBus;
        }

        public void RegisterEventHandler()
        {
            eventBus.Subscribe<object, CommonEventArgs, CommonEventHandler>();
            eventBus.Subscribe<int, CommonEventArgs, Common2EventHandler>();
            eventBus.Subscribe<object, UserEventArgs, UserEventHandler>();
        }
    }

    public class CommonEventHandler : IEventHandler<object, CommonEventArgs>
    {
        public async Task HandleAsync(object sender, CommonEventArgs eventArgs)
        {
            await Task.Delay(10);
        }
    }

    public class Common2EventHandler : IEventHandler<int, CommonEventArgs>
    {
        public async Task HandleAsync(int sender, CommonEventArgs eventArgs)
        {
            await Task.Delay(10);
        }
    }

    public class UserEventHandler : IEventHandler<object, UserEventArgs>
    {
        public async Task HandleAsync(object sender, UserEventArgs eventArgs)
        {
            await Task.Delay(10);
        }
    }
}
