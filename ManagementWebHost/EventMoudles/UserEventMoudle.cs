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
            eventBus.Subscribe<CommonEventArgs, TestUserEventHandler>();
        }
    }

    public class TestUserEventHandler : IEventHandler<CommonEventArgs>
    {
        public async Task HandleAsync(CommonEventArgs eventArgs)
        {
            await Task.Delay(10);
        }
    }
}
