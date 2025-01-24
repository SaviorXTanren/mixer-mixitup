using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public enum ChatNotificationType
    {
        sub,
        resub,
        sub_gift,
        community_sub_gift,
        gift_paid_upgrade,
        prime_paid_upgrade,
        raid,
        unraid,
        pay_it_forward,
        announcement,
        bits_badge_tier,
        charity_donation,
        shared_chat_sub,
        shared_chat_resub,
        shared_chat_sub_gift,
        shared_chat_community_sub_gift,
        shared_chat_gift_paid_upgrade,
        shared_chat_prime_paid_upgrade,
        shared_chat_raid,
        shared_chat_pay_it_forward,
        shared_chat_announcement,
    }

    public class ChatNotification
    {
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string chatter_user_id { get; set; }
        public string chatter_user_login { get; set; }
        public string chatter_user_name { get; set; }
        public bool chatter_is_anonymous { get; set; }
        public string color { get; set; }
        public List<ChatMessageNotificationBadge> badges { get; set; } = new List<ChatMessageNotificationBadge>();
        public string system_message { get; set; }
        public string message_id { get; set; }
        public ChatMessageNotificationMessage message { get; set; }
        public string notice_type { get; set; }
        public ChatNotificationSub sub { get; set; }
        public ChatNotificationResub resub { get; set; }
        public ChatNotificationSubGift sub_gift { get; set; }
        public ChatNotificationCommunitySubGift community_sub_gift { get; set; }
        public ChatNotificationGiftPaidUpgrade gift_paid_upgrade { get; set; }
        public ChatNotificationPrimePaidUpgrade prime_paid_upgrade { get; set; }
        public ChatNotificationPayItForward pay_it_forward { get; set; }
        public ChatNotificationRaid raid { get; set; }
        public ChatNotificationAnnouncement announcement { get; set; }
        public ChatNotificationBitsBadgeTier bits_badge_tier { get; set; }
        public ChatNotificationCharityDonation charity_donation { get; set; }
        public ChatNotificationSub shared_chat_sub { get; set; }
        public ChatNotificationResub shared_chat_resub { get; set; }
        public ChatNotificationSubGift shared_chat_sub_gift { get; set; }
        public ChatNotificationCommunitySubGift shared_chat_community_sub_gift { get; set; }
        public ChatNotificationGiftPaidUpgrade shared_chat_gift_paid_upgrade { get; set; }
        public ChatNotificationPrimePaidUpgrade shared_chat_prime_paid_upgrade { get; set; }
        public ChatNotificationPayItForward shared_chat_pay_it_forward { get; set; }
        public ChatNotificationRaid shared_chat_raid { get; set; }
        public ChatNotificationAnnouncement shared_chat_announcement { get; set; }
        public string source_broadcaster_user_id { get; set; }
        public string source_broadcaster_user_login { get; set; }
        public string source_broadcaster_user_name { get; set; }
        public string source_message_id { get; set; }
        public List<ChatMessageNotificationBadge> source_badges { get; set; } = new List<ChatMessageNotificationBadge>();

        [JsonProperty("noticetypeenum")]
        public ChatNotificationType NoticeType { get { return EnumHelper.GetEnumValueFromString<ChatNotificationType>(this.notice_type); } }
    }

    public class ChatNotificationSub
    {
        public string sub_tier { get; set; }
        public bool is_prime { get; set; }
        public int duration_months { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(sub_tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }

    public class ChatNotificationResub
    {
        public int? cumulative_months { get; set; }
        public int? duration_months { get; set; }
        public int? streak_months { get; set; }
        public string sub_tier { get; set; }
        public bool? is_prime { get; set; }
        public bool? is_gift { get; set; }
        public bool? gifter_is_anonymous { get; set; }
        public string gifter_user_id { get; set; }
        public string gifter_user_name { get; set; }
        public string gifter_user_login { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(sub_tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }

    public class ChatNotificationSubGift
    {
        public int? duration_months { get; set; }
        public int? cumulative_total { get; set; }
        public string recipient_user_id { get; set; }
        public string recipient_user_name { get; set; }
        public string recipient_user_login { get; set; }
        public string sub_tier { get; set; }
        public string community_gift_id { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(sub_tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }

    public class ChatNotificationCommunitySubGift
    {
        public string id { get; set; }
        public int? total { get; set; }
        public string sub_tier { get; set; }
        public int? cumulative_total { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(sub_tier); } }

        public int TotalSubPoints { get { return TwitchClient.GetSubPoints(TierNumber) * this.total.GetValueOrDefault(); } }
    }

    public class ChatNotificationGiftPaidUpgrade
    {
        public bool? gifter_is_anonymous { get; set; }
        public string gifter_user_id { get; set; }
        public string gifter_user_name { get; set; }
        public string gifter_user_login { get; set; }
    }

    public class ChatNotificationPrimePaidUpgrade
    {
        public string sub_tier { get; set; }

        public int TierNumber { get { return TwitchClient.GetSubTierNumberFromText(sub_tier); } }

        public int SubPoints { get { return TwitchClient.GetSubPoints(TierNumber); } }
    }

    public class ChatNotificationPayItForward
    {
        public bool? gifter_is_anonymous { get; set; }
        public string gifter_user_id { get; set; }
        public string gifter_user_name { get; set; }
        public string gifter_user_login { get; set; }
    }

    public class ChatNotificationRaid
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_login { get; set; }
        public string profile_image_url { get; set; }
        public int? viewer_count { get; set; }
    }

    public class ChatNotificationAnnouncement
    {
        public string color { get; set; }
    }

    public class ChatNotificationBitsBadgeTier
    {
        public int tier { get; set; }
    }

    public class ChatNotificationCharityDonation
    {
        public string charity_name { get; set; }
        public ChatNotificationCharityDonationAmount amount { get; set; }
    }

    public class ChatNotificationCharityDonationAmount
    {
        public int? value { get; set; }
        public int? decimal_place { get; set; }
        public string currency { get; set; }
    }
}
