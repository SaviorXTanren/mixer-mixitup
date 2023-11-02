using MixItUp.Base.Model.Commands;
using System.Collections.Generic;
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

        protected override async Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            Dictionary<string, string> properties = await base.GetCustomProperties(parameters);

            string clipHeight = "100%";
            if (this.Width > 0)
            {
                clipHeight = this.Height.ToString();
            }

            string clipWidth = "100%";
            if (this.Width > 0)
            {
                clipWidth = this.Width.ToString();
            }

            properties["ClipHeight"] = clipHeight;
            properties["ClipWidth"] = clipWidth;
            properties["ClipID"] = this.TempClipID;

            return properties;
        }
    }
}