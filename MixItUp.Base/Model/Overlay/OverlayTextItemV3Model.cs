using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayTextItemV3AlignmentTypeEnum
    {
        Left,
        Center,
        Right,
        Justify,
    }

    [DataContract]
    public class OverlayTextItemV3Model : OverlayItemV3ModelBase
    {
        public const string DefaultHTML = "<p style=\"font-size: {FontSize}px; color: {FontColor}; font-family: '{FontFamily}'; font-weight: {FontWeight}; text-decoration: {TextDecoration}; font-style: {FontStyle}; text-align: {TextAlignment}; {Shadow}\">{Text}</p>";

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
        public OverlayTextItemV3AlignmentTypeEnum TextAlignment { get; set; }
        [DataMember]
        public string ShadowColor { get; set; }

        public OverlayTextItemV3Model() : base(OverlayItemV3Type.Text) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, "Text", this.Text);
            item.HTML = ReplaceProperty(item.HTML, "FontSize", this.FontSize.ToString());
            item.HTML = ReplaceProperty(item.HTML, "FontFamily", this.FontName);
            item.HTML = ReplaceProperty(item.HTML, "FontColor", this.FontColor);
            item.HTML = ReplaceProperty(item.HTML, "FontWeight", this.Bold ? "bold" : "normal");
            item.HTML = ReplaceProperty(item.HTML, "TextDecoration", this.Underline ? "underline" : "none");
            item.HTML = ReplaceProperty(item.HTML, "FontStyle", this.Italics ? "italic" : "normal");
            item.HTML = ReplaceProperty(item.HTML, "TextAlignment", this.TextAlignment.ToString().ToLower());

            if (!string.IsNullOrEmpty(this.ShadowColor))
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", $"text-shadow: 2px 2px {this.ShadowColor};");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", string.Empty);
            }

            return item;
        }
    }
}
