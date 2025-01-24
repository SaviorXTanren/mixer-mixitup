using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayFontV3AlignmentTypeEnum
    {
        Left,
        Center,
        Right,
        Justify,
    }

    [DataContract]
    public class OverlayFontV3Model
    {
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public int FontSize { get; set; }
        [DataMember]
        public string FontName { get; set; }
        [DataMember]
        public string FontColor { get; set; }
        [DataMember]
        public bool Bold { get; set; }
        [DataMember]
        public bool Italics { get; set; }
        [DataMember]
        public bool Underline { get; set; }
        [DataMember]
        public OverlayFontV3AlignmentTypeEnum TextAlignment { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }

        [JsonIgnore]
        public string FontFamily { get { return this.FontName; } }
        [JsonIgnore]
        public string FontWeight { get { return this.Bold ? "bold" : "normal"; } }
        [JsonIgnore]
        public string TextDecoration { get { return this.Underline ? "underline" : "none"; } }
        [JsonIgnore]
        public string FontStyle { get { return this.Italics ? "italic" : "normal"; } }
    }
}
