using System;

namespace MixItUp.Base.Model.Remote
{
    public class RemoteDevice
    {
        public Guid ID { get; set; }
        public Guid DeviceID { get; set; }
        public string RemoteAddress { get; set; }
        public DateTimeOffset LastSeen { get; set; }
    }
}
