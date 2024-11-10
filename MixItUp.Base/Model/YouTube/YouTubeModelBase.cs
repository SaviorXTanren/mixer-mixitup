namespace MixItUp.Base.Model.YouTube
{
    /// <summary>
    /// Base information about a YouTube item.
    /// </summary>
    public class YouTubeModelBase
    {
        /// <summary>
        /// The ID of the item.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The kind of result.
        /// </summary>
        public string kind { get; set; }
        /// <summary>
        /// Identifier for the version of the entry.
        /// </summary>
        public string etag { get; set; }
    }
}
