using Newtonsoft.Json.Linq;

namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// The notification information such as the subscription and event details.
    /// <see cref="Subscription"/>
    /// </summary>
    public class NotificationMessagePayload
    {
        /// <summary>
        /// The subscription details.
        /// </summary>
        public Subscription Subscription { get; set; }

        /// <summary>
        /// The event details which can parsed using this documentation:
        /// https://dev.twitch.tv/docs/eventsub/eventsub-reference#events
        /// </summary>
        public JObject Event { get; set; }
    }
}
