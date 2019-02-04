using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote
{
    [DataContract]
    public class RemoteDeviceGroupModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTimeOffset LastUse { get; set; }

        [DataMember]
        public List<RemoteDeviceModel> Devices { get; set; }

        public RemoteDeviceGroupModel()
        {
            this.Devices = new List<RemoteDeviceModel>();
        }

        public RemoteDeviceGroupModel(string name)
            : this()
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
            this.LastUse = DateTimeOffset.Now;
        }
    }
}
