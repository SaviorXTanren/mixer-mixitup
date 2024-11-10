using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Chat
{
    /// <summary>
    /// Information about a chat emote.
    /// </summary>
    public class ChatEmoteModel
    {
        private const string imageUrlSize1 = "url_1x";
        private const string imageUrlSize2 = "url_2x";
        private const string imageUrlSize4 = "url_4x";

        /// <summary>
        /// The format name for a static image.
        /// </summary>
        public const string StaticFormatName = "static";
        /// <summary>
        /// The format name for an animated image.
        /// </summary>
        public const string AnimatedFormatName = "animated";

        /// <summary>
        /// The 1 scale for an image.
        /// </summary>
        public const string Scale1Name = "1.0";
        /// <summary>
        /// The 2 scale for an image.
        /// </summary>
        public const string Scale2Name = "2.0";
        /// <summary>
        /// The 3 scale for an image.
        /// </summary>
        public const string Scale3Name = "3.0";

        /// <summary>
        /// The light theme for an image.
        /// </summary>
        public const string LightThemeName = "light";
        /// <summary>
        /// The dark theme for an image.
        /// </summary>
        public const string DarkThemeName = "dark";

        /// <summary>
        /// The ID of the emote.
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The name of the emote.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The images for the emote.
        /// </summary>
        public JObject images { get; set; } = new JObject();

        /// <summary>
        /// The subscription tier required for this emote if it's a subscription emote.
        /// </summary>
        public string tier { get; set; }

        /// <summary>
        /// The type of emote. The most common values for custom channel emotes are:
        /// 
        /// subscriptions: Indicates a custom subscriber emote.
        /// 
        /// bitstier: Indicates a custom Bits tier emote.
        /// 
        /// follower: Indicates a custom follower emote.
        /// </summary>
        public string emote_type { get; set; }

        /// <summary>
        /// The ID of the set that this emote belongs to.
        /// </summary>
        public string emote_set_id { get; set; }

        /// <summary>
        /// The format of the image to get. For example, a static PNG or animated GIF.
        /// 
        /// Use default if you want the server to return an animated GIF if it exists, otherwise, a static PNG.
        /// </summary>
        public HashSet<string> format { get; set; } = new HashSet<string>();

        /// <summary>
        /// The list of supported scale sizes for the emote.
        /// </summary>
        public HashSet<string> scale { get; set; } = new HashSet<string>();

        /// <summary>
        /// The list of supported theme modes for the emote.
        /// </summary>
        public HashSet<string> theme_mode { get; set; } = new HashSet<string>();

        /// <summary>
        /// The ID of the broadcaster that this emote belongs to.
        /// </summary>
        public string owner_id { get; set; }

        /// <summary>
        /// The size 1 image url of the emote.
        /// </summary>
        public string Size1URL { get { return this.images.ContainsKey(imageUrlSize1) ? this.images[imageUrlSize1].ToString() : null; } }
        /// <summary>
        /// The size 2 image url of the emote.
        /// </summary>
        public string Size2URL { get { return this.images.ContainsKey(imageUrlSize2) ? this.images[imageUrlSize2].ToString() : null; } }
        /// <summary>
        /// The size 4 image url of the emote.
        /// </summary>
        public string Size4URL { get { return this.images.ContainsKey(imageUrlSize4) ? this.images[imageUrlSize4].ToString() : null; } }

        /// <summary>
        /// Whether the emote has a static format image.
        /// </summary>
        public bool HasStatic { get { return this.format.Contains(StaticFormatName); } }
        /// <summary>
        /// Whether the emote has an animated format image.
        /// </summary>
        public bool HasAnimated { get { return this.format.Contains(AnimatedFormatName); } }

        /// <summary>
        /// Builds a URL to the image with the following parameters.
        /// </summary>
        /// <param name="format">The format type of the image</param>
        /// <param name="theme">The theme type of the image</param>
        /// <param name="scale">The scale type of the image</param>
        /// <returns>The URL of the image</returns>
        public string BuildImageURL(string format, string theme, string scale)
        {
            return $"https://static-cdn.jtvnw.net/emoticons/v2/{this.id}/{format}/{theme}/{scale}";
        }
    }
}
