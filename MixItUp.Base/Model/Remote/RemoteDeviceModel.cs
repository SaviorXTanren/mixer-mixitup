using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote
{
    [DataContract]
    public class RemoteDeviceModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsHost { get; set; }

        public RemoteDeviceModel() { }

        public RemoteDeviceModel(string name, bool isHost = false)
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
            this.IsHost = isHost;
        }
    }
}
