using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

        public OverlayVisualTextV3ModelBase(OverlayItemV3Type type) : base(type) { }

        protected override async Task<Dictionary<string, string>> GetCustomProperties(CommandParametersModel parameters)
        {
            Dictionary<string, string> properties = await base.GetCustomProperties(parameters);

            properties[nameof(this.Text)] = this.Text;
            properties[nameof(this.FontSize)] = this.FontSize.ToString();
            properties["FontFamily"] = this.FontName;
            properties[nameof(this.FontColor)] = this.FontColor;
            properties["FontWeight"] = this.Bold ? "bold" : "normal";
            properties["TextDecoration"] = this.Underline ? "underline" : "none";
            properties["FontStyle"] = this.Italics ? "italic" : "normal";
            properties[nameof(this.TextAlignment)] = this.TextAlignment.ToString().ToLower();
            properties[nameof(this.ShadowColor)] = (!string.IsNullOrEmpty(this.ShadowColor)) ? $"2px 2px {this.ShadowColor}" : "none";

            return properties;
        }
    }
}