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
        public static readonly string DefaultHTML = OverlayResources.OverlayTwitchClipVideoDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayTwitchClipVideoDefaultCSS;
        public static readonly string DefaultJavascript = string.Empty;

        [DataMember]
        public OverlayTwitchClipV3ClipType ClipType { get; set; }

        [DataMember]
        public string ClipReferenceID { get; set; }

        [DataMember]
        public double Volume { get; set; }

        [JsonIgnore]
        public string ClipID { get; set; }
        [JsonIgnore]
        public float ClipDuration { get; set; }
        [JsonIgnore]
        public string ClipDirectLink { get; set; }

        [JsonIgnore]
        public string ClipHeight { get { return (this.Height > 0) ? $"{this.Height}px" : "100%"; } }
        [JsonIgnore]
        public string ClipWidth { get { return (this.Width > 0) ? $"{this.Width}px" : "100%"; } }

        public OverlayTwitchClipV3Model() : base(OverlayItemV3Type.TwitchClip) { }

        public override Dictionary<string, string> GetGenerationProperties()
        {
            Dictionary<string, string> properties = base.GetGenerationProperties();
            properties[nameof(this.ClipID)] = this.ClipID;
            properties[nameof(this.ClipDirectLink)] = this.ClipDirectLink;
            properties[nameof(this.Volume)] = this.Volume.ToString();
            properties[nameof(this.ClipHeight)] = this.ClipHeight;
            properties[nameof(this.ClipWidth)] = this.ClipWidth;
            return properties;
        }

        public void SetDirectLinkFromThumbnailURL(string thumbnailURL)
        {
            int index = thumbnailURL.IndexOf("-preview-");
            if (index >= 0)
            {
                this.ClipDirectLink = thumbnailURL.Substring(0, index) + ".mp4";
            }
        }
    }
}