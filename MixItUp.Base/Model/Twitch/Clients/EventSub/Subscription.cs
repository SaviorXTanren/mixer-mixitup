using Newtonsoft.Json.Linq;
using System;

namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// An object that contains information about your subscription.
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// An ID that uniquely identifies this subscription.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The subscription’s status, which is set to enabled.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The type of event sent in the message.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The version number of the subscription type’s definition.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The event’s cost.
        /// </summary>
        public string Cost { get; set; }

        /// <summary>
        /// The conditions under which the event fires. For example, if you requested
        /// notifications when a broadcaster gets a new follower, this object contains the broadcaster’s ID.
        /// </summary>
        public JObject Condition { get; set; }

        /// <summary>
        /// An object that contains information about the transport used for notifications.
        /// </summary>
        public Transport Transport { get; set; }

        /// <summary>
        /// The UTC date and time that the subscription was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }
    }
}
