using System.Runtime.Serialization;

namespace MixItUp.Base.Model.User.Twitch
{
    [DataContract]
    public class TwitchWebhookFollowModel
    {
        [DataMember]
        public string StreamerID { get; set; }

        [DataMember]
        public string UserID { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string UserDisplayName { get; set; }
    }
}
