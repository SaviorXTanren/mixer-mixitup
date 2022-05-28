using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTextAlignmentEnum
    {
        Default,
        Left,
        Center,
        Right,
        Justify,
    }

    [DataContract]
    public class OverlayTextItemV3Model : OverlayItemV3Model
    {
        public const string DefaultHTML = "<p style=\"font-size: {FontSize}px; color: {FontColor}; font-family: '{FontFamily}'; font-weight: {FontWeight}; text-decoration: {TextDecoration}; font-style: {FontStyle}; text-shadow: -{ShadowSize}px 0 {ShadowColor}, 0 {ShadowSize}px {ShadowColor}, {ShadowSize}px 0 {ShadowColor}, 0 -{ShadowSize}px {ShadowColor}; white-space: nowrap;\">{Text}</p>";
        public const string DefaultCSS = "";
        public const string DefaultJavascript = "";

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
        public OverlayTextAlignmentEnum Alignment { get; set; }
        [DataMember]
        public string ShadowSize { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayTextItemV3Model()
        {
            this.HTML = DefaultHTML;
            this.CSS = DefaultCSS;
            this.Javascript = DefaultJavascript;
        }

        protected override Task<OverlayItemV3Model> GetProcessedItem(OverlayItemV3Model item, CommandParametersModel parameters)
        {
            item.HTML = this.ReplaceProperty(item.HTML, "Text", this.Text);
            item.HTML = this.ReplaceProperty(item.HTML, "FontSize", this.FontSize.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "FontFamily", this.FontName);
            item.HTML = this.ReplaceProperty(item.HTML, "FontColor", this.FontColor);
            item.HTML = this.ReplaceProperty(item.HTML, "FontWeight", this.Bold ? "bold" : "normal");
            item.HTML = this.ReplaceProperty(item.HTML, "TextDecoration", this.Underline ? "underline" : "none");
            item.HTML = this.ReplaceProperty(item.HTML, "FontStyle", this.Italics ? "italic" : "normal");
            item.HTML = this.ReplaceProperty(item.HTML, "ShadowSize", this.ShadowSize.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "ShadowColor", this.ShadowColor);

            return Task.FromResult(item);
        }
    }
}
