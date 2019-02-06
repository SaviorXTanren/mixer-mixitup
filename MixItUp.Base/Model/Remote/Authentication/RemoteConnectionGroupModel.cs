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
        public List<RemoteConnectionModel> Clients { get; set; }

        public RemoteConnectionGroupModel()
        {
            this.Clients = new List<RemoteConnectionModel>();
        }

        public RemoteConnectionGroupModel(RemoteConnectionModel host)
            : this()
        {
            this.Host = host;

            this.ID = Guid.NewGuid();
            this.LastUse = DateTimeOffset.Now;
        }

        [JsonIgnore]
        public string Name { get { return this.Host.Name; } }
    }
}
