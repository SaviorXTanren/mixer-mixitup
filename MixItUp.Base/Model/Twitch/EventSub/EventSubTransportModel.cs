namespace MixItUp.Base.Model.Twitch.EventSub
{
    /// <summary>
    /// Information about the transport for a subscription
    /// </summary>
    public class EventSubTransportModel
    {
        /// <summary>
        /// The transport method. Supported values: webhook.
        /// </summary>
        public string method { get; set; }

        /// <summary>
        /// The callback URL where the notification should be sent.
        /// </summary>
        public string callback { get; set; }

        /// <summary>
        /// The secret used for verifying a signature.
        /// </summary>
        public string secret { get; set; }
    }
}
