using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
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
    public abstract class OverlayVisualTextItemV3ModelBase : OverlayItemV3ModelBase
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

        public OverlayVisualTextItemV3ModelBase(OverlayItemV3Type type) : base(type) { }

        protected override async Task<OverlayOutputV3Model> GetProcessedItem(OverlayOutputV3Model item, OverlayEndpointService overlayEndpointService, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, overlayEndpointService, parameters);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.Text), this.Text);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.Text), this.Text);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.Text), this.Text);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.FontSize), this.FontSize.ToString());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.FontSize), this.FontSize.ToString());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.FontSize), this.FontSize.ToString());

            item.HTML = ReplaceProperty(item.HTML, "FontFamily", this.FontName);
            item.CSS = ReplaceProperty(item.CSS, "FontFamily", this.FontName);
            item.Javascript = ReplaceProperty(item.Javascript, "FontFamily", this.FontName);

            item.HTML = ReplaceProperty(item.HTML, nameof(this.FontColor), this.FontColor);
            item.CSS = ReplaceProperty(item.CSS, nameof(this.FontColor), this.FontColor);
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.FontColor), this.FontColor);

            item.HTML = ReplaceProperty(item.HTML, "FontWeight", this.Bold ? "bold" : "normal");
            item.CSS = ReplaceProperty(item.CSS, "FontWeight", this.Bold ? "bold" : "normal");
            item.Javascript = ReplaceProperty(item.Javascript, "FontWeight", this.Bold ? "bold" : "normal");

            item.HTML = ReplaceProperty(item.HTML, "TextDecoration", this.Underline ? "underline" : "none");
            item.CSS = ReplaceProperty(item.CSS, "TextDecoration", this.Underline ? "underline" : "none");
            item.Javascript = ReplaceProperty(item.Javascript, "TextDecoration", this.Underline ? "underline" : "none");

            item.HTML = ReplaceProperty(item.HTML, "FontStyle", this.Italics ? "italic" : "normal");
            item.CSS = ReplaceProperty(item.CSS, "FontStyle", this.Italics ? "italic" : "normal");
            item.Javascript = ReplaceProperty(item.Javascript, "FontStyle", this.Italics ? "italic" : "normal");

            item.HTML = ReplaceProperty(item.HTML, nameof(this.TextAlignment), this.TextAlignment.ToString().ToLower());
            item.CSS = ReplaceProperty(item.CSS, nameof(this.TextAlignment), this.TextAlignment.ToString().ToLower());
            item.Javascript = ReplaceProperty(item.Javascript, nameof(this.TextAlignment), this.TextAlignment.ToString().ToLower());

            if (!string.IsNullOrEmpty(this.ShadowColor))
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", $"2px 2px {this.ShadowColor}");
                item.CSS = ReplaceProperty(item.CSS, "Shadow", $"2px 2px {this.ShadowColor}");
                item.Javascript = ReplaceProperty(item.Javascript, "Shadow", $"2px 2px {this.ShadowColor}");
            }
            else
            {
                item.HTML = ReplaceProperty(item.HTML, "Shadow", "none");
                item.CSS = ReplaceProperty(item.CSS, "Shadow", "none");
                item.Javascript = ReplaceProperty(item.Javascript, "Shadow", "none");
            }

            return item;
        }
    }
}