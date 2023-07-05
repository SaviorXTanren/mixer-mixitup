using Newtonsoft.Json;
using StreamingClient.Base.Model.OAuth;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Settings
{
    public static class OAuthTokenModelExtensions
    {
        public static void ResetToken(this OAuthTokenModel token)
        {
            token.accessToken = string.Empty;
            token.refreshToken = string.Empty;
            token.expiresIn = 0;
        }
    }

    [DataContract]
    public class StreamingPlatformAuthenticationSettingsModel : IEquatable<StreamingPlatformAuthenticationSettingsModel>
    {
        [DataMember]
        public StreamingPlatformTypeEnum Type { get; set; }

        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public OAuthTokenModel UserOAuthToken { get; set; }

        [DataMember]
        public string BotID { get; set; }
        [DataMember]
        public OAuthTokenModel BotOAuthToken { get; set; }

        [DataMember]
        public string ChannelID { get; set; }

        public StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum type) { this.Type = type; }

        [Obsolete]
        public StreamingPlatformAuthenticationSettingsModel() { }

        [JsonIgnore]
        public bool IsEnabled { get { return this.UserOAuthToken != null; } }

        public void ClearUserData()
        {
            this.UserID = null;
            this.UserOAuthToken = null;
            this.ChannelID = null;
            this.ClearBotData();
        }

        public void ClearBotData()
        {
            this.BotID = null;
            this.BotOAuthToken = null;
        }

        public override bool Equals(object obj)
        {
            if (obj is StreamingPlatformAuthenticationSettingsModel)
            {
                return this.Equals((StreamingPlatformAuthenticationSettingsModel)obj);
            }
            return false;
        }

        public bool Equals(StreamingPlatformAuthenticationSettingsModel other) { return this.Type == other.Type && this.UserID == other.UserID; }

        public override int GetHashCode() { return this.Type.GetHashCode(); }
    }
}
