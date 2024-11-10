using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Chat
{
    /// <summary>
    /// Information about a set of chat badges.
    /// </summary>
    public class ChatBadgeSetModel
    {
        /// <summary>
        /// The id of the set of chat badges.
        /// </summary>
        public string set_id { get; set; }

        /// <summary>
        /// The versions of the chat badges.
        /// </summary>
        public List<ChatBadgeModel> versions { get; set; } = new List<ChatBadgeModel>();
    }

    /// <summary>
    /// Information about a chat badge.
    /// </summary>
    public class ChatBadgeModel
    {
        /// <summary>
        /// The ID of the chat badge.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The title of the chat badge.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// The description of the chat badge.
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// The action to take when clicking on the badge. Set to null if no action is specified.
        /// </summary>
        public string click_action { get; set; }

        /// <summary>
        /// The URL to navigate to when clicking on the badge. Set to null if no URL is specified.
        /// </summary>
        public string click_url { get; set; }

        /// <summary>
        /// The 1x size image of the chat badge.
        /// </summary>
        public string image_url_1x { get; set; }

        /// <summary>
        /// The 2x size image of the chat badge.
        /// </summary>
        public string image_url_2x { get; set; }

        /// <summary>
        /// The 4x size image of the chat badge.
        /// </summary>
        public string image_url_4x { get; set; }
    }
}
