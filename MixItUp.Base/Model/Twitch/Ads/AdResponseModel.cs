namespace MixItUp.Base.Model.Twitch.Ads
{
    /// <summary>
    /// Information about the response of running an ad.
    /// </summary>
    public class AdResponseModel
    {
        /// <summary>
        /// Length of the triggered commercial.
        /// </summary>
        public int length { get; set; }
        /// <summary>
        /// Provides contextual information on why the request failed
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// Seconds until the next commercial can be served on this channel
        /// </summary>
        public int retryAfter { get; set; }
    }
}
