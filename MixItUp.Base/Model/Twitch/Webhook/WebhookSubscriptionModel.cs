namespace MixItUp.Base.Model.Twitch.Webhook
{
    /// <summary>
    /// Information about Webhook Subscription
    /// </summary>
    public class WebhookSubscriptionModel
    {
        /// <summary>
        /// The topic used in the initial subscription.
        /// </summary>
        public string topic { get; set; }

        /// <summary>
        /// The callback provided for this subscription.
        /// </summary>
        public string callback { get; set; }

        /// <summary>
        /// Date and time when this subscription expires. Encoded as RFC3339. The timezone is always UTC (“Z”).
        /// </summary>
        public string expires_at { get; set; }
    }
}
