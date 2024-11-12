using System.Collections.Generic;

namespace MixItUp.Base.Model.Trovo.Chat
{
    /// <summary>
    /// Information about the set of available emotes.
    /// </summary>
    public class ChatEmotePackageModel
    {
        /// <summary>
        /// The customized emotes available.
        /// </summary>
        public CustomizedChatEmotesModel customizedEmotes { get; set; }

        /// <summary>
        /// The event emotes available.
        /// </summary>
        public List<EventChatEmoteModel> eventEmotes { get; set; }

        /// <summary>
        /// The global emotes available.
        /// </summary>
        public List<GlobalChatEmoteModel> globalEmotes { get; set; }
    }

    /// <summary>
    /// Information about customized emotes.
    /// </summary>
    public class CustomizedChatEmotesModel
    {
        /// <summary>
        /// The emotes available from channels.
        /// </summary>
        public List<ChannelChatEmotesModel> channel { get; set; }
    }

    /// <summary>
    /// Information about a channel chat emote.
    /// </summary>
    public class ChannelChatEmotesModel
    {
        /// <summary>
        /// The ID of the channel that the emote belongs to.
        /// </summary>
        public string channel_id { get; set; }

        /// <summary>
        /// The set of emotes for the channel.
        /// </summary>
        public List<ChatEmoteModel> emotes { get; set; }
    }

    /// <summary>
    /// Information about a chat emote.
    /// </summary>
    public class ChatEmoteModel
    {
        /// <summary>
        /// The name of the emote.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The description of the emote.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The URL of the emote image.
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// The available status of the emote.
        /// </summary>
        public string status { get; set; }
    }

    /// <summary>
    /// Information about a global chat emote.
    /// </summary>
    public class GlobalChatEmoteModel : ChatEmoteModel
    {
        /// <summary>
        /// The Gifp URL of the emote.
        /// </summary>
        public string gifp { get; set; }

        /// <summary>
        /// The Webp url of the emote.
        /// </summary>
        public string webp { get; set; }
    }

    /// <summary>
    /// Information about an event chat emote.
    /// </summary>
    public class EventChatEmoteModel : GlobalChatEmoteModel
    {
        /// <summary>
        /// The name of platform-level limited emoticons activity.
        /// </summary>
        public string activity_name { get; set; }

        /// <summary>
        /// The update time of the emote.
        /// </summary>
        public string update_time { get; set; }
    }

    public class ChatEmotesModel
    {
        public ChatEmotePackageModel channels { get; set; }
    }
}