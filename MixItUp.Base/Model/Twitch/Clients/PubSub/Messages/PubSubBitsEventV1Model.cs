using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a Bits Event V2.
    /// </summary>
    public class PubSubBitsEventV1Model
    {
        /// <summary>
        /// Information about the user’s new badge level, if the user reached a new badge level with this cheer; otherwise. null.
        /// </summary>
        public JObject badge_entitlement { get; set; }
        /// <summary>
        /// Number of Bits used.
        /// </summary>
        public int bits_used { get; set; }
        /// <summary>
        /// User ID of the channel on which Bits were used.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// Name of the channel on which Bits were used.
        /// </summary>
        public string channel_name { get; set; }
        /// <summary>
        /// Chat message sent with the cheer.
        /// </summary>
        public string chat_message { get; set; }
        /// <summary>
        /// Event type associated with this use of Bits (for example, cheer).
        /// </summary>
        public string context { get; set; }
        /// <summary>
        /// Message ID
        /// </summary>
        public string message_id { get; set; }
        /// <summary>
        /// essage type (that is, the type of object contained in the data field)
        /// </summary>
        public string message_type { get; set; }
        /// <summary>
        /// Time when the Bits were used. RFC 3339 format.
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// All-time total number of Bits used on this channel by the specified user.
        /// </summary>
        public long total_bits_used { get; set; }
        /// <summary>
        /// User ID of the person who used the Bits.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// Login name of the person who used the Bits.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// Message version.
        /// </summary>
        public string version { get; set; }
    }
}
