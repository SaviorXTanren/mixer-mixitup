using System;

namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel moderator event.
    /// </summary>
    public class ChannelModeratorEventModel
    {
        private const string ModEventType = "moderation.moderator.add";
        private const string UnmodEventType = "moderation.moderator.remove";

        /// <summary>
        /// Event ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Displays moderation.moderator.add or moderation.moderator.remove
        /// </summary>
        public string event_type { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp for events.
        /// </summary>
        public string event_timestamp { get; set; }
        /// <summary>
        /// The version of the endpoint.
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// The data for the event.
        /// </summary>
        public ChannelModeratorEventDataModel event_data { get; set; }

        /// <summary>
        /// Indicates if the event is a ban.
        /// </summary>
        public bool IsMod { get { return string.Equals(this.event_type, ModEventType, StringComparison.InvariantCultureIgnoreCase); } }
        /// <summary>
        /// Indicates if the event is an unban.
        /// </summary>
        public bool IsUnmod { get { return string.Equals(this.event_type, UnmodEventType, StringComparison.InvariantCultureIgnoreCase); } }
    }

    /// <summary>
    /// Information about the data of a channel ban.
    /// </summary>
    public class ChannelModeratorEventDataModel
    {
        /// <summary>
        /// The ID of the channel.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// Login of the broadcaster.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The name of the user.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// When the event expires.
        /// </summary>
        public string expires_at { get; set; }
    }
}
