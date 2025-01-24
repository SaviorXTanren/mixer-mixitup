namespace MixItUp.Base.Model.Twitch.Chat
{
    /// <summary>
    /// Information about creating an announcement.
    /// </summary>
    public class AnnouncementModel
    {
        /// <summary>
        /// The announcement to make in the broadcaster’s chat room. Announcements are limited to a maximum of 500 characters; announcements longer than 500 characters are truncated.
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// The color used to highlight the announcement. Possible case-sensitive values are:
        ///     blue
        ///     green
        ///     orange
        ///     purple
        ///     primary (default)
        /// If color is set to primary or is not set, the channel’s accent color is used to highlight the announcement (see Profile Accent Color under profile settings, Channel and Videos, and Brand).
        /// </summary>
        public string color { get; set; }
    }
}
