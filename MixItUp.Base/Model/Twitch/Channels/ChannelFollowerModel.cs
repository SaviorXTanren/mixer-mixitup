namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel follow
    /// </summary>
    public class ChannelFollowerModel
    {
        /// <summary>
        /// An ID that uniquely identifies the user that’s following the broadcaster.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The user’s login name.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// 	The user’s display name.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// The UTC timestamp when the user started following the broadcaster.
        /// </summary>
        public string followed_at { get; set; }
    }
}
