using MixItUp.Base.Model.Actions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreCommandModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public HashSet<string> Tags { get; set; } = new HashSet<string>();

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserAvatarURL { get; set; }

        [DataMember]
        public double AverageRating { get; set; }

        [DataMember]
        public int Downloads { get; set; }

        [DataMember]
        public DateTimeOffset LastUpdated { get; set; }
    }

    [DataContract]
    public class StoreCommandDetailsModel : StoreCommandModel
    {
        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();
    }

    [DataContract]
    public class StoreCommandUploadModel : StoreCommandDetailsModel
    {
        [DataMember]
        public string MixItUpUserID { get; set; }
    }
}
