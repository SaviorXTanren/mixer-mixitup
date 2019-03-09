using System;
using System.Collections.Generic;
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

        [DataMember]
        public bool IsStreamer { get; set; }

        public RemoteProfileModel() { }

        public RemoteProfileModel(string name)
            : base()
        {
            this.ID = Guid.NewGuid();
            this.Name = name;
        }
    }

    [DataContract]
    public class RemoteProfileBoardsModel
    {
        [DataMember]
        public Guid ProfileID { get; set; }

        [DataMember]
        public Dictionary<Guid, RemoteBoardModel> Boards { get; set; } = new Dictionary<Guid, RemoteBoardModel>();

        public RemoteProfileBoardsModel() { }

        public RemoteProfileBoardsModel(Guid profileID)
            : base()
        {
            this.ProfileID = profileID;
            this.Boards[Guid.Empty] = new RemoteBoardModel();
        }
    }
}
