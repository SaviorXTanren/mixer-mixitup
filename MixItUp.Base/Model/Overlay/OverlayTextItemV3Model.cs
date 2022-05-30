using MixItUp.Base.Model.Commands;
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
    public class OverlayTextItemV3Model : OverlayPositionedItemV3ModelBase
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

        public OverlayTextItemV3Model()
        {
            this.HTML = DefaultHTML;
        }

        protected override async Task<OverlayItemV3ModelBase> GetProcessedItem(OverlayItemV3ModelBase item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            item.HTML = this.ReplaceProperty(item.HTML, "Text", this.Text);
            item.HTML = this.ReplaceProperty(item.HTML, "FontSize", this.FontSize.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "FontFamily", this.FontName);
            item.HTML = this.ReplaceProperty(item.HTML, "FontColor", this.FontColor);
            item.HTML = this.ReplaceProperty(item.HTML, "FontWeight", this.Bold ? "bold" : "normal");
            item.HTML = this.ReplaceProperty(item.HTML, "TextDecoration", this.Underline ? "underline" : "none");
            item.HTML = this.ReplaceProperty(item.HTML, "FontStyle", this.Italics ? "italic" : "normal");
            item.HTML = this.ReplaceProperty(item.HTML, "TextAlignment", this.TextAlignment.ToString().ToLower());

            if (!string.IsNullOrEmpty(this.ShadowColor))
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Shadow", $"text-shadow: 2px 2px {this.ShadowColor};");
            }
            else
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Shadow", string.Empty);
            }

            return item;
        }
    }
}
