namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a Bits Badge.
    /// </summary>
    public class PubSubBitBadgeEventModel
    {
        /// <summary>
        /// ID of user who earned the new Bits badge
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Login of user who earned the new Bits badge
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// ID of channel where user earned the new Bits badge
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// Login of channel where user earned the new Bits badge
        /// </summary>
        public string channel_name { get; set; }
        /// <summary>
        /// alue of Bits badge tier that was earned (1000, 10000, etc.)
        /// </summary>
        public long badge_tier { get; set; }
        /// <summary>
        /// [Optional] Custom message included with share
        /// </summary>
        public string chat_message { get; set; }
        /// <summary>
        /// Time when the new Bits badge was earned. RFC 3339 format.
        /// </summary>
        public string time { get; set; }
    }
}
