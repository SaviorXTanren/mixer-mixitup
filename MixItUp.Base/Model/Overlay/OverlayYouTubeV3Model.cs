using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
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

        public OverlayYouTubeV3Model() : base(OverlayItemV3Type.YouTube) { }

        protected override async Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            Dictionary<string, string> properties = await base.GetCustomProperties(parameters);

            string videoID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.VideoID, parameters);
            videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
            videoID = videoID.Replace("www.youtube.com/watch?v=", "");
            videoID = videoID.Replace("youtube.com/watch?v=", "");
            videoID = videoID.Replace("https://youtu.be/", "");
            if (videoID.Contains("&"))
            {
                videoID = videoID.Substring(0, videoID.IndexOf("&"));
            }

            properties[nameof(this.VideoID)] = videoID;
            properties[nameof(this.StartTime)] = this.StartTime.ToString();
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties["HeightNumber"] = this.Height.ToString();
            properties["WidthNumber"] = this.Width.ToString();

            return properties;
        }
    }
}