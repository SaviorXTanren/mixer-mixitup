using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayVisualTextItemV3AlignmentTypeEnum
    {
        Left,
        Center,
        Right,
        Justify,
    }

    [DataContract]
    public abstract class OverlayVisualTextV3ModelBase : OverlayItemV3ModelBase
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
        public OverlayVisualTextItemV3AlignmentTypeEnum TextAlignment { get; set; }
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

        public OverlayVisualTextV3ModelBase(OverlayItemV3Type type) : base(type) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[nameof(this.Text)] = this.Text;
            properties[nameof(this.FontSize)] = this.FontSize;
            properties[nameof(this.FontFamily)] = this.FontFamily;
            properties[nameof(this.FontColor)] = this.FontColor;
            properties[nameof(this.FontWeight)] = this.FontWeight;
            properties[nameof(this.TextDecoration)] = this.TextDecoration;
            properties[nameof(this.FontStyle)] = this.FontStyle;
            properties[nameof(this.TextAlignment)] = this.TextAlignment.ToString().ToLower();
            properties[nameof(this.ShadowColor)] = (!string.IsNullOrEmpty(this.ShadowColor)) ? $"1px 1px {this.ShadowColor}" : "none";
            return properties;
        }
    }
}