using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class CommunityCommandReviewModel
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public Guid CommandID { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public string UserAvatarURL { get; set; }

        [DataMember]
        public int Rating { get; set; }

        [DataMember]
        public string Review { get; set; }

        [DataMember]
        public DateTimeOffset DateTime { get; set; }
    }
}
