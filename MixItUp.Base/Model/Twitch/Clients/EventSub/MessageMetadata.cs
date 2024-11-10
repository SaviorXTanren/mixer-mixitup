using Newtonsoft.Json;
using System;

namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// An object that identifies the message.
    /// </summary>
    public class MessageMetadata
    {
        /// <summary>
        /// An ID that uniquely identifies the message. Twitch sends messages at least once,
        /// but if Twitch is unsure of whether you received a notification, it’ll resend the 
        /// message. This means you may receive a notification twice. If Twitch resends the message,
        /// the message ID will be the same.
        /// </summary>
        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        /// <summary>
        /// The type of message.
        /// </summary>
        [JsonProperty("message_type")]
        public MessageType MessageType { get; set; }

        /// <summary>
        /// The UTC date and time that the message was sent.
        /// </summary>
        [JsonProperty("message_timestamp")]
        public DateTimeOffset MessageTimeStamp { get; set; }

        /// <summary>
        /// The type of event sent in the message.
        /// </summary>
        [JsonProperty("subscription_type")]
        public string SubscriptionType { get; set; }

        /// <summary>
        /// The version number of the subscription type’s definition.
        /// This is the same value specified in the subscription request.
        /// </summary>
        [JsonProperty("subscription_version")]
        public string SubscriptionVersion { get; set; }
    }
}
