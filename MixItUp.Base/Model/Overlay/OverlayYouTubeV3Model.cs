using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayYouTubeV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = Resources.OverlayYouTubeDefaultHTML;
        public static readonly string DefaultCSS = Resources.OverlayYouTubeDefaultCSS;
        public static readonly string DefaultJavascript = Resources.OverlayYouTubeDefaultJavascript;

        [DataMember]
        public string VideoID { get; set; }

        [DataMember]
        public int StartTime { get; set; }

        [DataMember]
        public int Volume { get; set; }

        public OverlayYouTubeV3Model() : base(OverlayItemV3Type.YouTube) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            string videoID = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.VideoID, parameters);
            videoID = videoID.Replace("https://www.youtube.com/watch?v=", "");
            videoID = videoID.Replace("https://youtu.be/", "");
            if (videoID.Contains("&"))
            {
                videoID = videoID.Substring(0, videoID.IndexOf("&"));
            }

            item.HTML = ReplaceProperty(item.HTML, nameof(this.VideoID), videoID);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.VideoID), videoID);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.VideoID), videoID);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.StartTime), this.StartTime.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.StartTime), this.StartTime.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.StartTime), this.StartTime.ToString());

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Volume), this.Volume.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Volume), this.Volume.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Volume), this.Volume.ToString());

            item.HTML = ReplaceProperty(item.HTML, "HeightNumber", this.Height.ToString());
            item.CSS = ReplaceProperty(item.CSS, "HeightNumber", this.Height.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, "HeightNumber", this.Height.ToString());

            item.HTML = ReplaceProperty(item.HTML, "WidthNumber", this.Width.ToString());
            item.CSS = ReplaceProperty(item.CSS, "WidthNumber", this.Width.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, "WidthNumber", this.Width.ToString());

            return item;
        }
    }
}