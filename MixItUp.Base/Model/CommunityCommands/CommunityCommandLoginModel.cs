using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class CommunityCommandLoginModel
    {
        [DataMember]
        public Guid UserID { get; set; }

        [DataMember]
        public string TwitchAccessToken { get; set; }
    }

    [DataContract]
    public class CommunityCommandLoginResponseModel
    {
        [DataMember]
        public string AccessToken { get; set; }
    }
}
