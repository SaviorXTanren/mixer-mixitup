namespace MixItUp.Base.Model.Twitch.Streams
{
    /// <summary>
    /// Information about a created stream marker.
    /// </summary>
    public class CreatedStreamMarkerModel
    {
        /// <summary>
        /// The ID of the stream marker.
        /// </summary>
        public long id { get; set; }
        /// <summary>
        /// The date the stream marker was created at.
        /// </summary>
        public string created_at { get; set; }
        /// <summary>
        /// The description of the stream marker.
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// The number of seconds into the stream.
        /// </summary>
        public long position_seconds { get; set; }
    }
}