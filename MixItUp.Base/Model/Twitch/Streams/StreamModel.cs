using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Streams
{
    /// <summary>
    /// Information about a stream.
    /// </summary>
    public class StreamModel
    {
        /// <summary>
        /// The ID of the stream.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// The ID of the channel.
        /// </summary>
        public string user_id { get; set; }
        /// <summary>
        /// The name of the channel.
        /// </summary>
        public string user_name { get; set; }
        /// <summary>
        /// Login of the user who is streaming.
        /// </summary>
        public string user_login { get; set; }
        /// <summary>
        /// The ID of the game.
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// Name of the game being played.
        /// </summary>
        public string game_name { get; set; }
        /// <summary>
        /// The list of community IDs.
        /// </summary>
        public List<string> community_ids { get; set; }
        /// <summary>
        /// The type of stream.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// The title of the stream.
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The viewer count of the stream.
        /// </summary>
        public long viewer_count { get; set; }
        /// <summary>
        /// The date the stream started at.
        /// </summary>
        public string started_at { get; set; }
        /// <summary>
        /// The language of the stream.
        /// </summary>
        public string language { get; set; }
        /// <summary>
        /// The url for the thumbnail image of the stream.
        /// </summary>
        public string thumbnail_url { get; set; }
        /// <summary>
        /// The list of tag IDs.
        /// </summary>
        public List<string> tag_ids { get; set; }
        /// <summary>
        /// Indicates if the broadcaster has specified their channel contains mature content that may be inappropriate for younger audiences.
        /// </summary>
        public bool is_mature { get; set; }
    }
}
