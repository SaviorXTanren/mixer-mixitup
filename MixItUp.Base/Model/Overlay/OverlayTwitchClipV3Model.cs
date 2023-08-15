using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayTwitchClipV3Model : OverlayItemV3ModelBase
    {
        public static readonly string DefaultHTML = Resources.OverlayTwitchClipDefaultHTML;
        public static readonly string DefaultCSS = string.Empty;
        public static readonly string DefaultJavascript = Resources.OverlayTwitchClipDefaultJavascript;

        [DataMember]
        public string VideoID { get; set; }

        [DataMember]
        public int StartTime { get; set; }

        [DataMember]
        public int Volume { get; set; }

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

            item.HTML = ReplaceProperty(item.HTML, "ClipID", "EnchantingScaryCarabeefArsonNoSexy");
            item.CSS = ReplaceProperty(item.CSS, "ClipID", "EnchantingScaryCarabeefArsonNoSexy");
            item.Javascript = ReplaceProperty(item.Javascript, "ClipID", "EnchantingScaryCarabeefArsonNoSexy");

            return item;
        }
    }
}