using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreCommandReviewModel
    {
        [DataMember]
        public Guid CommandID { get; set; }

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

    [DataContract]
    public class StoreCommandReviewUploadModel : StoreCommandReviewModel
    {
        [DataMember]
        public Guid MixItUpUserID { get; set; }
    }
}
