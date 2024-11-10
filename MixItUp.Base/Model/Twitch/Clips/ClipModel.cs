namespace MixItUp.Base.Model.Twitch.Clips
{
    /// <summary>
    /// Information about a clip.
    /// </summary>
    public class ClipModel
    {
        /// <summary>
        /// The ID of the clip.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The url of the clip.
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// The url of the embeddable clip.
        /// </summary>
        public string embed_url { get; set; }
        /// <summary>
        /// The ID of the broadcaster of the clip.
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// The name of the broadcaster of the clip.
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// The ID of the creator of the clip.
        /// </summary>
        public string creator_id { get; set; }
        /// <summary>
        /// The name of the creator of the clip.
        /// </summary>
        public string creator_name { get; set; }
        /// <summary>
        /// The ID of the video.
        /// </summary>
        public string video_id { get; set; }
        /// <summary>
        /// The ID of the game.
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// The language identifier.
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// The title of the clip.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The view count of the clip.
        /// </summary>
        public long view_count { get; set; }
        /// <summary>
        /// The date the clip was created at.
        /// </summary>
        public string created_at { get; set; }
        /// <summary>
        /// The url of the clip thumbnail.
        /// </summary>
        public string thumbnail_url { get; set; }
        /// <summary>
        /// The length of the clip, in seconds. Precision is 0.1.
        /// </summary>
        public float duration { get; set; }
        /// <summary>
        /// The zero-based offset, in seconds, to where the clip starts in the video (VOD). Is null if the video is not available or hasn’t been created yet from the live stream (see video_id).
        /// 
        /// Note that there’s a delay between when a clip is created during a broadcast and when the offset is set.During the delay period, vod_offset is null. The delay is indeterminant but is typically minutes long.
        /// </summary>
        public int? vod_offset { get; set; }
    }
}
