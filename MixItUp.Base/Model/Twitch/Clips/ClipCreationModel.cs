namespace MixItUp.Base.Model.Twitch.Clips
{
    /// <summary>
    /// Information about a created clip.
    /// </summary>
    public class ClipCreationModel
    {
        /// <summary>
        /// The ID of the clip.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The url to the clip.
        /// </summary>
        public string edit_url { get; set; }
    }
}
