using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace MixItUp.Base.Model.Twitch.Bits
{
    /// <summary>
    /// Information about a bits cheermote.
    /// </summary>
    public class BitsCheermoteModel
    {
        /// <summary>
        /// The prefix name of the cheermotes.
        /// </summary>
        public string prefix { get; set; }
        /// <summary>
        /// Shows whether the emote is global_first_party, global_third_party, channel_custom, display_only, or sponsored.
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Order of the emotes as shown in the bits card, in ascending order.
        /// </summary>
        public int order { get; set; }
        /// <summary>
        /// The data when this Cheermote was last updated.
        /// </summary>
        public string last_updated { get; set; }
        /// <summary>
        /// Indicates whether or not this emote provides a charity contribution match during charity campaigns.
        /// </summary>
        public bool is_charitable { get; set; }
        /// <summary>
        /// An array of Cheermotes with their metadata.
        /// </summary>
        public List<BitsCheermoteTierModel> tiers { get; set; } = new List<BitsCheermoteTierModel>();
    }

    /// <summary>
    /// Information about a bits cheermote tier.
    /// </summary>
    public class BitsCheermoteTierModel
    {
        /// <summary>
        /// ID of the emote tier. Possible tiers are: 1,100,500,1000,5000, 10k, or 100k.
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// Minimum number of bits needed to be used to hit the given tier of emote.  
        /// </summary>
        public int min_bits { get; set; }
        /// <summary>
        /// Hex code for the color associated with the bits of that tier. Grey, Purple, Teal, Blue, or Red color to match the base bit type.
        /// </summary>
        public string color { get; set; }
        /// <summary>
        /// Indicates whether or not emote information is accessible to users.
        /// </summary>
        public bool can_cheer { get; set; }
        /// <summary>
        /// Indicates whether or not we hide the emote from the bits card.
        /// </summary>
        public bool show_in_bits_card { get; set; }
        /// <summary>
        /// Structure containing both animated and static image sets, sorted by light and dark.
        /// </summary>
        public JObject images { get; set; }

        /// <summary>
        /// Gets the dark, animated images.
        /// </summary>
        public Dictionary<string, string> DarkAnimatedImages { get { return this.GetCheermoteImages("dark", "animated"); } }
        /// <summary>
        /// Gets the dark, static images.
        /// </summary>
        public Dictionary<string, string> DarkStaticImages { get { return this.GetCheermoteImages("dark", "static"); } }
        /// <summary>
        /// Gets the light, animated images.
        /// </summary>
        public Dictionary<string, string> LightAnimatedImages { get { return this.GetCheermoteImages("light", "animated"); } }
        /// <summary>
        /// Gets the light, static images.
        /// </summary>
        public Dictionary<string, string> LightStaticImages { get { return this.GetCheermoteImages("light", "static"); } }

        private Dictionary<string, string> GetCheermoteImages(string color, string imageType)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();
            if (this.images != null && this.images.ContainsKey(color))
            {
                JObject colorJObj = (JObject)this.images[color];
                if (colorJObj != null && colorJObj.ContainsKey(imageType))
                {
                    JObject imageTypeJObj = (JObject)colorJObj[imageType];
                    foreach (var kvp in imageTypeJObj)
                    {
                        results[kvp.Key] = kvp.Value.ToString();
                    }
                }
            }
            return results;
        }
    }
}
