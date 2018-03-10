using System;

namespace MixItUp.Base.Model.Remote
{
    public class RemoteDeviceModel
    {
        public Guid ID { get; set; }
        public string RemoteAddress { get; set; }
        public DateTimeOffset LastSeen { get; set; }
    }
}
