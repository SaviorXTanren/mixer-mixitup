using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
        public string ClipID { get; set; }
        [JsonIgnore]
        public float ClipDuration { get; set; }

        [JsonIgnore]
        public string ClipHeight { get { return (this.Height > 0) ? $"{this.Height}px" : "100%"; } }
        [JsonIgnore]
        public string ClipWidth { get { return (this.Width > 0) ? $"{this.Width}px" : "100%"; } }

        public OverlayTwitchClipV3Model() : base(OverlayItemV3Type.TwitchClip) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();
            properties[nameof(this.ClipID)] = this.ClipID;
            properties[nameof(this.ClipHeight)] = this.ClipHeight;
            properties[nameof(this.ClipWidth)] = this.ClipWidth;
            return properties;
        }
    }
}