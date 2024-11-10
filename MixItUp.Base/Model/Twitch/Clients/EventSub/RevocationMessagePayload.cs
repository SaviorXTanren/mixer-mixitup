namespace MixItUp.Base.Model.Twitch.Clients.EventSub
{
    /// <summary>
    /// The payload containing the revoked subscription.
    /// </summary>
    public class RevocationMessagePayload
    {
        /// <summary>
        /// The subscription being revoked.
        /// </summary>
        public Subscription Subscription { get; set; }
    }
}
