using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTwitchClipV3ClipType
    {
        LatestClip,
        RandomClip,
        SpecificClip,
    }

    [DataContract]
    public class OverlayTwitchClipV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = OverlayResources.OverlayTwitchClipDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTwitchClipDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayTwitchClipDefaultJavascript;

        [DataMember]
        public OverlayTwitchClipV3ClipType ClipType { get; set; }

        [DataMember]
        public string ClipReferenceID { get; set; }

        [JsonIgnore]
        public string TempClipID { get; set; }
        [JsonIgnore]
        public float TempClipDuration { get; set; }

        public OverlayTwitchClipV3Model() : base(OverlayItemV3Type.TwitchClip) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            string clipHeight = "100%";
            if (this.Width > 0)
            {
                clipHeight = this.Height.ToString();
            }

            item.HTML = ReplaceProperty(item.HTML, "ClipHeight", clipHeight);
            item.CSS = ReplaceProperty(item.CSS, "ClipHeight", clipHeight);
            item.Javascript = ReplaceProperty(item.Javascript, "ClipHeight", clipHeight);

            string clipWidth = "100%";
            if (this.Width > 0)
            {
                clipWidth = this.Width.ToString();
            }

            item.HTML = ReplaceProperty(item.HTML, "ClipWidth", clipWidth);
            item.CSS = ReplaceProperty(item.CSS, "ClipWidth", clipWidth);
            item.Javascript = ReplaceProperty(item.Javascript, "ClipWidth", clipWidth);

            item.HTML = ReplaceProperty(item.HTML, "ClipID", this.TempClipID);
            item.CSS = ReplaceProperty(item.CSS, "ClipID", this.TempClipID);
            item.Javascript = ReplaceProperty(item.Javascript, "ClipID", this.TempClipID);

            return item;
        }
    }
}