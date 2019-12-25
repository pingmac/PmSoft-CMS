using PmSoft.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ManagementWebHost.EventMoudles
{
    public class UserEventMoudle : IEventMoudle
    {
        public void RegisterEventHandler()
        {
            RedisEventBus<object, CommonEventArgs>.Instance().Subscribe<TestUserEventHandler>();
        }
    }

    public class TestUserEventHandler : IEventHandler<object, CommonEventArgs>
    {
        public async Task HandleAsync(CommonEventArgs eventArgs)
        {
            await Task.Delay(10);
        }
    }
}
