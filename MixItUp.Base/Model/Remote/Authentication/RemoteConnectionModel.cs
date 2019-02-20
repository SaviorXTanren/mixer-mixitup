using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Remote.Authentication
{
    [DataContract]
    public class RemoteConnectionModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsTemporary { get; set; }

        public RemoteConnectionModel() { }

        public RemoteConnectionModel(string name)
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }
    }
}
