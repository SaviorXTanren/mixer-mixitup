namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel user moderator.
    /// </summary>
    public class ChannelModeratorUserModel
    {
        /// <summary>
        /// User ID of a moderator in the channel.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Login of a moderator in the channel.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// Display name of a moderator in the channel.
        /// </summary>
        public string user_name { get; set; }
    }
}
