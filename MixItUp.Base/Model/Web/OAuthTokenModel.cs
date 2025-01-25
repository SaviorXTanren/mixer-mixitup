using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Web
{
    /// <summary>
    /// A token received from an OAuth authentication service.
    /// </summary>
    [DataContract]
    public class OAuthTokenModel
    {
        public static string GenerateScopeList(IEnumerable<string> scopes) { return string.Join(",", scopes ?? new List<string>()); }

        /// <summary>
        /// The ID of the client service.
        /// </summary>
        [DataMember]
        public string clientID { get; set; }

        [DataMember]
        [Obsolete]
        public string clientSecret { get; set; }

        /// <summary>
        /// The token used for refreshing the authentication.
        /// </summary>
        [JsonProperty("refresh_token")]
        public string refreshToken { get; set; }

        /// <summary>
        /// The token used for accessing the OAuth service.
        /// </summary>
        [JsonProperty("access_token")]
        public string accessToken { get; set; }

        /// <summary>
        /// The expiration time of the token in seconds from when it was obtained.
        /// </summary>
        [JsonProperty("expires_in")]
        public long expiresIn { get; set; }
        /// <summary>
        /// The timestamp of the expiration, if supported by the service, in seconds from Unix Epoch
        /// </summary>
        public long expiresTimeStamp { get; set; }

        /// <summary>
        /// The redirect URL used as part of the token.
        /// </summary>
        [DataMember]
        public string redirectUrl { get; set; }

        /// <summary>
        /// The time when the token was obtained.
        /// </summary>
        [DataMember]
        public DateTimeOffset AcquiredDateTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Comma-delimited list of all scopes requested as part of the authorization token.
        /// </summary>
        [DataMember]
        public string ScopeList { get; set; }

        /// <summary>
        /// The expiration time of the token.
        /// </summary>
        [JsonIgnore]
        public DateTimeOffset ExpirationDateTime { get { return (this.expiresTimeStamp > 0) ? DateTimeOffset.FromUnixTimeSeconds(this.expiresTimeStamp) : this.AcquiredDateTime.AddSeconds(this.expiresIn); } }
        [JsonIgnore]
        public TimeSpan TimeUntilExpiration { get { return this.ExpirationDateTime - DateTimeOffset.Now; } }
        [JsonIgnore]
        public bool IsExpired { get { return this.ExpirationDateTime < DateTimeOffset.Now; } }

        public void ResetToken()
        {
            this.accessToken = string.Empty;
            this.refreshToken = string.Empty;
            this.expiresIn = 0;
            this.AcquiredDateTime = DateTimeOffset.MinValue;
        }
    }
}

