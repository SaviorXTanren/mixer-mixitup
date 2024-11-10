using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.EventSub
{
    /// <summary>
    /// Information about a subscription.
    /// </summary>
    public class EventSubSubscriptionModel
    {
        /// <summary>
        /// The ID of the subscription.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The notification’s subscription type.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The version of the subscription.
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// The status of the subscription.
        /// </summary>
        public string status { get; set; }

        /// <summary>
        /// How much the subscription counts against your limit. See Subscription Limits for more information.
        /// </summary>
        public int cost { get; set; }

        /// <summary>
        /// Subscription-specific parameters.
        /// </summary>
        public Dictionary<string, string> condition { get; set; }

        /// <summary>
        /// The subscription transport details.
        /// </summary>
        public EventSubTransportModel transport { get; set; }

        /// <summary>
        /// The time the subscription was created.
        /// </summary>
        public string created_at { get; set; }
    }
}
