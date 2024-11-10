namespace MixItUp.Base.Model.Twitch.Subscriptions
{
    /// <summary>
    /// Information about a subscription.
    /// </summary>
    public class SubscriptionModel
    {
        /// <summary>
        /// The ID of the broadcaster.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The name of the broadcaster.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// Whether the subscription was a gift.
        /// </summary>
        public bool is_gift { get; set; }
        /// <summary>
        /// The login of the gifter if it was a gifted subscription
        /// </summary>
        public string gifter_login { get; set; }
        /// <summary>
        /// The display name of the gifter if it was a gifted subscription
        /// </summary>
        public string gifter_name { get; set; }
        /// <summary>
        /// The tier of the subscription.
        /// </summary>
        public string tier { get; set; }
        /// <summary>
        /// The plan name of the subscription.
        /// </summary>
        public string plan_name { get; set; }
        /// <summary>
        /// The subscriber user ID.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The subscriber user name.
        /// </summary>
        public string user_name { get; set; }
    }
}
