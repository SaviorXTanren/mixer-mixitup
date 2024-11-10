namespace MixItUp.Base.Model.Twitch.EventSub
{
    /// <summary>
    /// The challenge sent by Twitch to the registered callback
    /// </summary>
    public class EventSubSubscriptionChallengeModel
    {
        /// <summary>
        /// Your response must be a raw string. If your server is using a web framework, 
        /// be careful that your web framework isn’t modifying the response in an incompatible 
        /// way. For example, some web frameworks default to converting responses into JSON objects.
        /// </summary>
        public string challenge { get; set; }

        /// <summary>
        /// The subscription that must be approved.
        /// </summary>
        public EventSubSubscriptionModel subscription { get; set; }
    }
}
