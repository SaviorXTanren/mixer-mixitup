using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayYouTubeV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayYouTubeDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayYouTubeDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayYouTubeDefaultJavascript;

        [DataMember]
        public string VideoID { get; set; }

        [DataMember]
        public int StartTime { get; set; }

        [DataMember]
        public int Volume { get; set; }

        [JsonIgnore]
        public string HeightNumber { get { return this.Height.ToString(); } }
        [JsonIgnore]
        public string WidthNumber { get { return this.Width.ToString(); } }

        public OverlayYouTubeV3Model() : base(OverlayItemV3Type.YouTube) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();
            properties[nameof(this.VideoID)] = this.VideoID;
            properties[nameof(this.StartTime)] = this.StartTime.ToString();
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.HeightNumber)] = this.HeightNumber;
            properties[nameof(this.WidthNumber)] = this.WidthNumber;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, string> properties, CommandParametersModel parameters)
        {
            properties[nameof(this.VideoID)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.VideoID)], parameters);
            properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].Replace("https://www.youtube.com/watch?v=", "");
            properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].Replace("www.youtube.com/watch?v=", "");
            properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].Replace("youtube.com/watch?v=", "");
            properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].Replace("https://youtu.be/", "");
            if (properties[nameof(this.VideoID)].Contains("&"))
            {
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].Substring(0, properties[nameof(this.VideoID)].IndexOf("&"));
            }
        }
    }
}