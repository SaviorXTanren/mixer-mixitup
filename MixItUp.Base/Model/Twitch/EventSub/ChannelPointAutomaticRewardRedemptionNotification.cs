using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public enum ChannelPointAutomaticRewardType
    {
        Unknown,
        single_message_bypass_sub_mode,
        send_highlighted_message,
        random_sub_emote_unlock,
        chosen_sub_emote_unlock,
        chosen_modified_sub_emote_unlock,
        message_effect,
        gigantify_an_emote,
        celebration,
    }

    public class ChannelPointAutomaticRewardRedemptionNotification
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string user_login { get; set; }
        public string user_name { get; set; }
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public ChannelPointAutomaticReward reward { get; set; }
        public ChannelPointAutomaticRewardMessage message { get; set; }
        public string user_input { get; set; }
        public string redeemed_at { get; set; }

        public DateTimeOffset RedeemedAt { get { return TwitchService.GetTwitchDateTime(redeemed_at); } }
    }

    public class ChannelPointAutomaticReward
    {
        public string type { get; set; }
        public int cost { get; set; }
        public ChannelPointAutomaticRewardUnlockedEmote unlocked_emote { get; set; }

        [JsonProperty("typeenum")]
        public ChannelPointAutomaticRewardType Type { get { return EnumHelper.GetEnumValueFromString<ChannelPointAutomaticRewardType>(this.type); } }
    }

    public class ChannelPointAutomaticRewardUnlockedEmote
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class ChannelPointAutomaticRewardMessage
    {
        public string text { get; set; }
        public List<ChannelPointAutomaticRewardMessageEmote> emotes = new List<ChannelPointAutomaticRewardMessageEmote>();
    }

    public class ChannelPointAutomaticRewardMessageEmote
    {
        public string id { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
    }
}
