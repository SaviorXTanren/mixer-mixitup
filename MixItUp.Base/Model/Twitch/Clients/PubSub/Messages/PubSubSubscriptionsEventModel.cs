using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a Subscription.
    /// </summary>
    public class PubSubSubscriptionsEventModel
    {
        /// <summary>
        /// The name of the subscriber.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// The ID of the subscriber.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The display name of the subscriber.
        /// </summary>
        public string display_name { get; set; }
        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string channel_name { get; set; }
        /// <summary>
        /// The ID of the channel.
        /// </summary>
        public string channel_id { get; set; }
        /// <summary>
        /// The date of the subscription.
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// The subscription plan.
        /// </summary>
        public string sub_plan { get; set; }
        /// <summary>
        /// The name of the subscription plan.
        /// </summary>
        public string sub_plan_name { get; set; }
        /// <summary>
        /// The number of months subscribed (Deprecated).
        /// </summary>
        public int months { get; set; }
        /// <summary>
        /// The total months of the subscriptions.
        /// </summary>
        public int cumulative_months { get; set; }
        /// <summary>
        /// The streak months of the subscription.
        /// </summary>
        public int streak_months { get; set; }
        /// <summary>
        /// The type of subscription (sub/resub/gift).
        /// </summary>
        public string context { get; set; }
        /// <summary>
        /// The message for the subscription.
        /// </summary>
        public JObject sub_message { get; set; }

        /// <summary>
        /// Whether the event is a subscription.
        /// </summary>
        [JsonIgnore]
        public bool IsSubscription { get { return this.context.Equals("sub"); } }
        /// <summary>
        /// Whether the event is a resubscription.
        /// </summary>
        [JsonIgnore]
        public bool IsResubscription { get { return this.context.Equals("resub"); } }
        /// <summary>
        /// Whether the event is a gifted subscription.
        /// </summary>
        [JsonIgnore]
        public bool IsGiftedSubscription { get { return this.context.Equals("subgift"); } }
        /// <summary>
        /// Whether the event is a anonymous gifted subscription.
        /// </summary>
        [JsonIgnore]
        public bool IsAnonymousGiftedSubscription { get { return this.context.Equals("anonsubgift"); } }

        /// <summary>
        /// The message included with the subscription, if any.
        /// </summary>
        [JsonIgnore]
        public string SubMessageText
        {
            get
            {
                if (sub_message != null && sub_message.ContainsKey("message"))
                {
                    return sub_message["message"].ToString();
                }
                return string.Empty;
            }
        }
    }
}
