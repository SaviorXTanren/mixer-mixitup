using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Trovo.Chat
{
    /// <summary>
    /// The types of chat messages.
    /// </summary>
    public enum ChatMessageTypeEnum
    {
        /// <summary>
        /// Normal chat messages
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Spells, including: mana spells, elixir spells
        /// </summary>
        Spell = 5,
        /// <summary>
        /// Magic chat - super cap chat
        /// </summary>
        MagicChatSuperCapChat = 6,
        /// <summary>
        /// Magic chat - colorful chat
        /// </summary>
        MagicChatColorfulChat = 7,
        /// <summary>
        /// Magic chat - spell chat
        /// </summary>
        MagicChatSpellChat = 8,
        /// <summary>
        /// Magic chat - bullet screen chat
        /// </summary>
        MagicChatBulletScreenChat = 9,
        /// <summary>
        /// Subscription message. Shows when someone subscribes to the channel.
        /// </summary>
        SubscriptionAlert = 5001,
        /// <summary>
        /// System message.
        /// </summary>
        SystemMessage = 5002,
        /// <summary>
        /// Follow message. Shows when someone follows the channel.
        /// </summary>
        FollowAlert = 5003,
        /// <summary>
        /// Welcome message when viewer joins the channel.
        /// </summary>
        WelcomeMessage = 5004,
        /// <summary>
        /// Gift sub message. When a user randomly sends gift subscriptions to one or more users in the channel.
        /// </summary>
        GiftedSubscriptionSentMessage = 5005,
        /// <summary>
        /// Gift sub message. The detailed messages when a user sends a gift subscription to another user.
        /// </summary>
        GiftedSubscriptionMessage = 5006,
        /// <summary>
        /// Activity / events message. For platform level events.
        /// </summary>
        ActivityEventMessage = 5007,
        /// <summary>
        /// Welcome message when users join the channel from raid.
        /// </summary>
        WelcomeMessageFromRaid = 5008,
        /// <summary>
        /// Custom Spells
        /// </summary>
        CustomSpell = 5009,
        /// <summary>
        /// Stream on/off messages, invisible to the viewers
        /// </summary>
        StreamOnOff = 5012,
        /// <summary>
        /// Unfollow message. Shows when someone unfollows the channel.
        /// </summary>
        UnfollowMessage = 5013,
    }

    /// <summary>
    /// The types of roles for a user in chat.
    /// </summary>
    public enum ChatUserRolesTypeEnum
    {
        /// <summary>
        /// Streamer of the current channel
        /// </summary>
        Streamer = 100000,
        /// <summary>
        /// Moderator of the current channel
        /// </summary>
        Mod = 100001,
        /// <summary>
        /// Editor of the current channel
        /// </summary>
        Editor = 100002,
        /// <summary>
        /// User who subscribed the current channel
        /// </summary>
        Subscriber = 100004,
        /// <summary>
        /// Super moderator of the current channel
        /// </summary>
        Supermod = 100005,
        /// <summary>
        /// User who followed of the current channel
        /// </summary>
        Follower = 100006,
        /// <summary>
        /// User who have a role customized by the streamer of the current channel
        /// </summary>
        CustomRole = 200000,
        /// <summary>
        /// Primary tier of Trovo membership
        /// </summary>
        Ace = 300000,
        /// <summary>
        /// Premium tier of Trovo membership
        /// </summary>
        AcePlus = 300001,
        /// <summary>
        /// Admin of Trovo platform, across all channels.
        /// </summary>
        Admin = 500000,
        /// <summary>
        /// Warden of Trovo platform, across all channels, who helps to maintain the platform order.
        /// </summary>
        Warden = 500001,
    }

    /// <summary>
    /// Information about a chat message container.
    /// </summary>
    public class ChatMessageContainerModel
    {
        /// <summary>
        /// ID of the message
        /// </summary>
        public string eid { get; set; }

        /// <summary>
        /// A list of chats. One chat message may contain multiple chats.
        /// </summary>
        public List<ChatMessageModel> chats { get; set; } = new List<ChatMessageModel>();
    }

    /// <summary>
    /// Information about an individual chat message piece.
    /// </summary>
    public class ChatMessageModel
    {
        private const string FullAvatarURLFormat = "https://headicon.trovo.live/user/";
        private const string DefaultAvatarURLFormat = "https://headicon.trovo.live/default/";

        /// <summary>
        /// Streamer of the current channel
        /// </summary>
        public const string StreamerRole = "streamer";
        /// <summary>
        /// Super moderator of the current channel
        /// </summary>
        public const string SuperModRole = "supermod";
        /// <summary>
        /// Moderator of the current channel
        /// </summary>
        public const string ModeratorRole = "mod";
        /// <summary>
        /// Editor of the current channel
        /// </summary>
        public const string EditorRole = "editor";
        /// <summary>
        /// User who followed the current channel
        /// </summary>
        public const string FollowerRole = "follower";
        /// <summary>
        /// User who subscribed the current channel
        /// </summary>
        public const string SubscriberRole = "subscriber";
        /// <summary>
        /// Admin of Trovo platform, across all channels.
        /// </summary>
        public const string AdminRole = "admin";
        /// <summary>
        /// Warden of Trovo platform, across all channels, who helps to maintain the platform order.
        /// </summary>
        public const string WardenRole = "warden";
        /// <summary>
        /// Primary tier of Trovo membership
        /// </summary>
        public const string AceRole = "ace";
        /// <summary>
        /// Premium tier of Trovo membership
        /// </summary>
        public const string AcePlusRole = "ace+";
        /// <summary>
        /// User who have a role customized by the streamer of the current channel.
        /// </summary>
        public const string CustomRole = "custom role";

        /// <summary>
        /// The ID of the message.
        /// </summary>
        public string message_id { get; set; }

        /// <summary>
        /// The ID of the sender.
        /// </summary>
        public long sender_id { get; set; }

        /// <summary>
        /// Type of chat message.
        /// </summary>
        public ChatMessageTypeEnum type { get; set; }

        /// <summary>
        /// Content of the message
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// User ID of the sender.
        /// </summary>
        public long uid { get; set; }

        /// <summary>
        /// User name of the sender.
        /// </summary>
        public string user_name { get; set; }

        /// <summary>
        /// Display name of the sender
        /// </summary>
        public string nick_name { get; set; }

        /// <summary>
        /// URL of the sender’s profile picture
        /// </summary>
        public string avatar { get; set; }

        /// <summary>
        /// The subscription level of the user in the channel. “sub_L1” for tier 1 subscriber.
        /// </summary>
        public string sub_lv { get; set; }

        /// <summary>
        /// Subscription tier
        /// </summary>
        public string sub_tier { get; set; }

        /// <summary>
        /// The list of badge names of the sender.
        /// </summary>
        public List<string> medals { get; set; } = new List<string>();

        /// <summary>
        /// The list of decoration names of sender.
        /// </summary>
        public List<string> decos { get; set; } = new List<string>();

        /// <summary>
        /// The roles of the sender.
        /// </summary>
        public List<string> roles { get; set; } = new List<string>();

        /// <summary>
        /// The list of role of the message sender which is a json string. Different from "roles", "custom_role" contains more information. However, if you just need the role names, use "roles" instead.
        /// </summary>
        public string custom_role { get; set; }

        /// <summary>
        /// Extra info of chat, The content_data is different in different chat.
        /// </summary>
        public JObject content_data { get; set; }

        /// <summary>
        /// Name of the spell. Only for chat messages of spell (type = 5), in the content field.
        /// </summary>
        public string gift { get; set; }

        /// <summary>
        /// Number of spells. Only for chat messages of spell (type = 5), in the content field.
        /// </summary>
        public int num { get; set; }

        /// <summary>
        /// Returns the full avatar URL for the user.
        /// </summary>
        [JsonIgnore]
        public string FullAvatarURL
        {
            get
            {
                if (!string.IsNullOrEmpty(this.avatar))
                {
                    if (this.avatar.StartsWith(FullAvatarURLFormat) || this.avatar.StartsWith(DefaultAvatarURLFormat))
                    {
                        return this.avatar;
                    }
                    else
                    {
                        return FullAvatarURLFormat + this.avatar;
                    }
                }
                return null;
            }
        }
    }
}
