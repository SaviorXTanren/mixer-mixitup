using MixItUp.Base.Model.Web;
using Newtonsoft.Json;
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
        [DataMember]
        public StreamingClient.Base.Model.OAuth.OAuthTokenModel YouTubeOAuthToken { get; set; }
        [DataMember]
        public string TrovoAccessToken { get; set; }
        [DataMember]
        public bool BypassTwitchWebhooks { get; set; }
    }

    [DataContract]
    public class CommunityCommandLoginResponseModel
    {
        [DataMember]
        public string AccessToken { get; set; }
    }
}

namespace StreamingClient.Base.Model.OAuth
{
    [Obsolete]
    [DataContract]
    public class OAuthTokenModel
    {
        [DataMember]
        public string clientID { get; set; }

        [DataMember]
        public string clientSecret { get; set; }

        [DataMember]
        public string authorizationCode { get; set; }

        [JsonProperty("refresh_token")]
        public string refreshToken { get; set; }

        [JsonProperty("access_token")]
        public string accessToken { get; set; }

        [JsonProperty("expires_in")]
        public long expiresIn { get; set; }

        public long expiresTimeStamp { get; set; }

        [DataMember]
        public string redirectUrl { get; set; }

        [DataMember]
        public DateTimeOffset AcquiredDateTime { get; set; }

        [DataMember]
        public string ScopeList { get; set; }

        [JsonIgnore]
        public DateTimeOffset ExpirationDateTime
        {
            get
            {
                if (expiresTimeStamp <= 0)
                {
                    return AcquiredDateTime.AddSeconds(expiresIn);
                }

                return DateTimeOffset.FromUnixTimeSeconds(expiresTimeStamp);
            }
        }

        public OAuthTokenModel()
        {
            AcquiredDateTime = DateTimeOffset.Now;
        }
    }
}