using MixItUp.Base.Services.Twitch.New;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public class UserSubscriptionMessageNotification
    {
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string tier { get; set; }

        public UserSubscriptionMessageNotificationMessage message { get; set; }

        public int cumulative_months { get; set; }
        public int? streak_months { get; set; }
        public int duration_months { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }

    public class UserSubscriptionMessageNotificationMessage
    {
        public string text { get; set; }
        public List<UserSubscriptionMessageNotificationEmote> emotes { get; set; } = new List<UserSubscriptionMessageNotificationEmote>();
    }

    public class UserSubscriptionMessageNotificationEmote
    {
        public string id { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
    }
}
