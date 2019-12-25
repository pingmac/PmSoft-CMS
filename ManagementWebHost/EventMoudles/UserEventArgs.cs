using System;
using PmSoft.Events;

namespace ManagementWebHost.EventMoudles
{
    [Serializable]
    public class UserEventArgs : CommonEventArgs
    {
        public UserEventArgs() : base(string.Empty) { }
    }
}
