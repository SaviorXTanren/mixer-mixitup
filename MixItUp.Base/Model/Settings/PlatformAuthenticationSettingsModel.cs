using StreamingClient.Base.Model.OAuth;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Settings
{
    [DataContract]
    public class PlatformAuthenticationSettingsModel : IEquatable<PlatformAuthenticationSettingsModel>
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

        [DataMember]
        public bool IsEnabled { get; set; }

        public PlatformAuthenticationSettingsModel() { }

        public PlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum type) { this.Type = type; }

        public override bool Equals(object obj)
        {
            if (obj is PlatformAuthenticationSettingsModel)
            {
                return this.Equals((PlatformAuthenticationSettingsModel)obj);
            }
            return false;
        }

        public bool Equals(PlatformAuthenticationSettingsModel other) { return this.Type == other.Type; }

        public override int GetHashCode() { return this.Type.GetHashCode(); }
    }
}
