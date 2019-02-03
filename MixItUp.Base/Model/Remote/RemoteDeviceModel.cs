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
        public string GroupID { get; set; }

        public RemoteDeviceModel() { }

        public RemoteDeviceModel(string name)
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }
    }
}
