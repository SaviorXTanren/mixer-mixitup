using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    public enum ChatNotificationMessageType
    {
        text,
        channel_points_highlighted,
        channel_points_sub_only,
        user_intro,
        power_ups_message_effect,
        power_ups_gigantified_emote,
    }

    public enum ChatNotificationMessageFragmentType
    {
        text,
        cheermote,
        emote,
        mention,
    }

    public class ChatMessageNotification
    {
        public string broadcaster_user_id { get; set; }
        public string broadcaster_user_login { get; set; }
        public string broadcaster_user_name { get; set; }
        public string chatter_user_id { get; set; }
        public string chatter_user_login { get; set; }
        public string chatter_user_name { get; set; }
        public string message_id { get; set; }
        public ChatMessageNotificationMessage message { get; set; }
        public string color { get; set; }
        public List<ChatMessageNotificationBadge> badges { get; set; } = new List<ChatMessageNotificationBadge>();
        public string message_type { get; set; }
        public ChatMessageNotificationCheer cheer { get; set; }
        public ChatMessageNotificationFragmentReply reply { get; set; }
        public string channel_points_custom_reward_id { get; set; }
        public string source_broadcaster_user_id { get; set; }
        public string source_broadcaster_user_login { get; set; }
        public string source_broadcaster_user_name { get; set; }
        public string source_message_id { get; set; }
        public List<ChatMessageNotificationBadge> source_badges { get; set; } = new List<ChatMessageNotificationBadge>();

        [JsonProperty("messagetypeenum")]
        public ChatNotificationMessageType MessageType { get { return EnumHelper.GetEnumValueFromString<ChatNotificationMessageType>(this.message_type); } }
    }

    public class ChatMessageNotificationMessage
    {
        public string text { get; set; }
        public List<ChatMessageNotificationFragment> fragments { get; set; } = new List<ChatMessageNotificationFragment>();
    }

    public class ChatMessageNotificationFragment
    {
        public string type { get; set; }
        public string text { get; set; }
        public ChatMessageNotificationFragmentCheermote cheermote { get; set; }
        public ChatMessageNotificationFragmentEmote emote { get; set; }
        public ChatMessageNotificationFragmentMention mention { get; set; }

        [JsonProperty("typeenum")]
        public ChatNotificationMessageFragmentType Type { get { return EnumHelper.GetEnumValueFromString<ChatNotificationMessageFragmentType>(this.type); } }
    }

    public class ChatMessageNotificationFragmentCheermote
    {
        public string prefix { get; set; }
        public int bits { get; set; }
        public int tier { get; set; }
    }

    public class ChatMessageNotificationFragmentEmote
    {
        public string id { get; set; }
        public string emote_set_id { get; set; }
        public string owner_id { get; set; }
        public HashSet<string> format { get; set; } = new HashSet<string>();

        public bool HasStatic { get { return this.format.Contains("static"); } }
        public bool HasAnimated { get { return this.format.Contains("animated"); } }
    }

    public class ChatMessageNotificationFragmentMention
    {
        public string user_id { get; set; }
        public string user_name { get; set; }
        public string user_login { get; set; }
    }

    public class ChatMessageNotificationBadge
    {
        public string set_id { get; set; }
        [JsonProperty("id")]
        public string ID { get; set; }
        public string info { get; set; }
    }

    public class ChatMessageNotificationCheer
    {
        public int bits { get; set; }
    }

    public class ChatMessageNotificationFragmentReply
    {
        public string parent_message_id { get; set; }
        public string parent_message_body { get; set; }
        public string parent_user_id { get; set; }
        public string parent_user_name { get; set; }
        public string parent_user_login { get; set; }
        public string thread_message_id { get; set; }
        public string thread_user_id { get; set; }
        public string thread_user_name { get; set; }
        public string thread_user_login { get; set; }
    }
}
