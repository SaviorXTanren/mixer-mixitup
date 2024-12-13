using MixItUp.Base.Services.Twitch.New;
using System;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public enum ChannelPointRewardCustomRedemptionNotificationStatus
    {
        Unknown,
        Unfulfilled,
        Fulfilled,
        Canceled,
    }

    public class ChannelPointRewardCustomRedemptionNotification
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string user_input { get; set; }
        public string status { get; set; }
        public string redeemed_at { get; set; }

        public ChannelPointRewardCustomReward reward { get; set; }

        public ChannelPointRewardCustomRedemptionNotificationStatus StatusType
        {
            get
            {
                switch (status)
                {
                    case "unfulfilled": return ChannelPointRewardCustomRedemptionNotificationStatus.Unfulfilled;
                    case "fulfilled": return ChannelPointRewardCustomRedemptionNotificationStatus.Fulfilled;
                    case "canceled": return ChannelPointRewardCustomRedemptionNotificationStatus.Canceled;
                    default: return ChannelPointRewardCustomRedemptionNotificationStatus.Unknown;
                }
            }
        }

        public DateTimeOffset RedeemedAt { get { return TwitchService.GetTwitchDateTime(redeemed_at); } }
    }

    public class ChannelPointRewardCustomReward
    {
        public string id { get; set; }
        public string title { get; set; }
        public int cost { get; set; }
        public string prompt { get; set; }
    }
}
