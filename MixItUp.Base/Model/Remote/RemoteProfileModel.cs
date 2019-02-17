using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Remote.Models
{
    [DataContract]
    public class RemoteProfileModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        public RemoteProfileModel() { }

        public RemoteProfileModel(string name)
            : base()
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }
    }

    [DataContract]
    public class RemoteProfileBoardModel
    {
        [DataMember]
        public RemoteProfileModel Profile { get; set; }

        [DataMember]
        public RemoteBoardModel Board { get; set; }

        public RemoteProfileBoardModel() { }

        public RemoteProfileBoardModel(RemoteProfileModel profile)
        {
            this.Profile = profile;
            this.Board = new RemoteBoardModel();
        }
    }
}
