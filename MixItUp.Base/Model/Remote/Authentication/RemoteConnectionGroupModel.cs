using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteConnectionGroupModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public DateTimeOffset LastUse { get; set; }

        [DataMember]
        public RemoteConnectionModel Host { get; set; }

        [DataMember]
        public List<RemoteConnectionModel> Devices { get; set; }

        public RemoteConnectionGroupModel()
        {
            this.Devices = new List<RemoteConnectionModel>();
        }

        public RemoteConnectionGroupModel(RemoteConnectionModel hostDevice)
            : this()
        {
            this.Host = hostDevice;

            this.ID = Guid.NewGuid();
            this.LastUse = DateTimeOffset.Now;
        }

        [JsonIgnore]
        public string Name { get { return this.Host.Name; } }
    }
}
