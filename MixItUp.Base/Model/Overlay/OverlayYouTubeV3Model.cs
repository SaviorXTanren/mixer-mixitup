using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
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
        public static readonly string DefaultJavascript = OverlayResources.OverlayActionDefaultJavascript + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayYouTubeDefaultJavascript;

        [DataMember]
        public string VideoID { get; set; }

        [DataMember]
        public string StartTime { get; set; }

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
            properties[nameof(this.StartTime)] = this.StartTime;
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.HeightNumber)] = this.HeightNumber;
            properties[nameof(this.WidthNumber)] = this.WidthNumber;
            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            if (!string.IsNullOrEmpty(this.VideoID))
            {
                string videoID = properties[nameof(this.VideoID)].ToString();

                videoID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(videoID, parameters);
                videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
                videoID = videoID.Replace("https://youtube.com/watch?v=", "");
                videoID = videoID.Replace("www.youtube.com/watch?v=", "");
                videoID = videoID.Replace("youtube.com/watch?v=", "");
                videoID = videoID.Replace("https://www.youtube.com/shorts/", "");
                videoID = videoID.Replace("https://youtube.com/shorts/", "");
                videoID = videoID.Replace("www.youtube.com/shorts/", "");
                videoID = videoID.Replace("youtube.com/shorts/", "");
                videoID = videoID.Replace("https://youtu.be/", "");
                if (videoID.ToString().Contains("?"))
                {
                    videoID = videoID.ToString().Substring(0, videoID.ToString().IndexOf("?"));
                }
                if (videoID.Contains("&"))
                {
                    videoID = videoID.Substring(0, videoID.IndexOf("&"));
                }

                properties[nameof(this.VideoID)] = videoID;
            }

            properties[nameof(this.StartTime)] = "0";
            if (!string.IsNullOrEmpty(this.StartTime))
            {
                string startTime = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.StartTime, parameters);
                if (int.TryParse(startTime, out int time) && time >= 0)
                {
                    properties[nameof(this.StartTime)] = time.ToString();
                }
            }
        }
    }
}