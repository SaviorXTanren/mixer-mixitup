using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Store
{
    [DataContract]
    public class StoreLoginModel
    {
        [DataMember]
        public Guid UserID { get; set; }

        [DataMember]
        public string TwitchAccessToken { get; set; }

        // TODO: Add other access token types as needed
    }

    [DataContract]
    public class StoreLoginResponseModel
    {
        [DataMember]
        public string AccessToken { get; set; }
    }
}
