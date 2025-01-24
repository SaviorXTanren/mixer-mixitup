namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel content classification label.
    /// </summary>
    public class ChannelContentClassificationLabelModel
    {
        /// <summary>
        /// Unique identifier for the CCL.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Localized name of the CCL.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Localized description of the CCL.
        /// </summary>
        public string description { get; set; }
    }
}
