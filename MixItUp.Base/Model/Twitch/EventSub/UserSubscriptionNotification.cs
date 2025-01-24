using MixItUp.Base.Services.Twitch.New;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class UserSubscriptionNotification
    {
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string tier { get; set; }
        public bool is_gift { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }
}
