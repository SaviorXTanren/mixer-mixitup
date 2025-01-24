using Newtonsoft.Json;

namespace MixItUp.Base.Model.Twitch.Clients.PubSub.Messages
{
    /// <summary>
    /// Information about a Subscription Gift.
    /// </summary>
    public class PubSubSubscriptionsGiftEventModel : PubSubSubscriptionsEventModel
    {
        /// <summary>
        /// The recipient ID of the gift.
        /// </summary>
        public string recipient_id { get; set; }
        /// <summary>
        /// The recipient name of the gift.
        /// </summary>
        public string recipient_user_name { get; set; }
        /// <summary>
        /// The recipient display name of the gift.
        /// </summary>
        public string recipient_display_name { get; set; }
        /// <summary>
        /// The amount of months gifted to the recipient if it was more than 1 month.
        /// </summary>
        public int multi_month_duration { get; set; }

        /// <summary>
        /// Indicates whether the event is a multi-month subscription gift.
        /// </summary>
        [JsonIgnore]
        public bool IsMultiMonth { get { return this.multi_month_duration > 1; } }
    }
}
