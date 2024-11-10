namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a followed channel.
    /// </summary>
    public class ChannelFollowedModel
    {
        /// <summary>
        /// An ID that uniquely identifies the broadcaster that this user is following.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The broadcaster’s login name.
        /// </summary>
        public string broadcaster_login { get; set; }
        /// <summary>
        /// The broadcaster’s display name.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The UTC timestamp when the user started following the broadcaster.
        /// </summary>
        public string followed_at { get; set; }
    }
}
