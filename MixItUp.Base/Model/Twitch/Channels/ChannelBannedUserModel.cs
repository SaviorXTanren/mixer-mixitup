namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel user ban.
    /// </summary>
    public class ChannelBannedUserModel
    {
        /// <summary>
        /// User ID of a user who has been banned.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Display name of a user who has been banned.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// RFC3339 formatted timestamp for timeouts; empty string for bans.
        /// </summary>
        public string expires_at { get; set; }
    }
}