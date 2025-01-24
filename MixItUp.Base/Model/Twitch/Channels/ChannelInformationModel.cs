using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Channels
{
    /// <summary>
    /// Information about a channel.
    /// </summary>
    public class ChannelInformationModel
    {
        /// <summary>
        /// Twitch User ID of this channel owner
        /// </summary>
        public string broadcaster_id { get; set; }
        /// <summary>
        /// Twitch User name of this channel owner
        /// </summary>
        public string broadcaster_name { get; set; }
        /// <summary>
        /// Language of the channel
        /// </summary>
        public string broadcaster_language { get; set; }
        /// <summary>
        /// Current game ID being played on the channel
        /// </summary>
        public string game_id { get; set; }
        /// <summary>
        /// Current game name being played on the channel
        /// </summary>
        public string game_name { get; set; }
        /// <summary>
        /// Title of the stream
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// The value of the broadcaster’s stream delay setting, in seconds. This field’s value defaults to zero unless
        /// 
        /// 1) the request specifies a user access token,
        /// 2) the ID in the broadcaster_id query parameter matches the user ID in the access token, and
        /// 3) the broadcaster has partner status and they set a non-zero stream delay value.
        /// </summary>
        public uint delay { get; set; }
        /// <summary>
        /// The tags applied to the channel.
        /// </summary>
        public List<string> tags { get; set; }
        /// <summary>
        /// The CCLs applied to the channel.
        /// </summary>
        public List<string> content_classification_labels { get; set; }
        /// <summary>
        /// Boolean flag indicating if the channel has branded content.
        /// </summary>
        public bool is_branded_content { get; set; }
    }
}
