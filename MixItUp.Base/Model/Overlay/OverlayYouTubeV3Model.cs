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
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript + "\n\n" + OverlayResources.OverlayYouTubeDefaultJavascript;

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

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.VideoID)] = this.VideoID;
            properties[nameof(this.StartTime)] = this.StartTime.ToString();
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.HeightNumber)] = this.HeightNumber;
            properties[nameof(this.WidthNumber)] = this.WidthNumber;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            if (!string.IsNullOrEmpty(this.VideoID))
            {
                properties[nameof(this.VideoID)] = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(properties[nameof(this.VideoID)].ToString(), parameters);
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("https://www.youtube.com/watch?v=", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("https://youtube.com/watch?v=", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("www.youtube.com/watch?v=", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("youtube.com/watch?v=", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("https://www.youtube.com/shorts/", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("https://youtube.com/shorts/", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("www.youtube.com/shorts/", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("youtube.com/shorts/", "");
                properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Replace("https://youtu.be/", "");
                if (properties[nameof(this.VideoID)].ToString().Contains("?"))
                {
                    properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Substring(0, properties[nameof(this.VideoID)].ToString().IndexOf("?"));
                }
                if (properties[nameof(this.VideoID)].ToString().Contains("&"))
                {
                    properties[nameof(this.VideoID)] = properties[nameof(this.VideoID)].ToString().Substring(0, properties[nameof(this.VideoID)].ToString().IndexOf("&"));
                }
            }
        }
    }
}