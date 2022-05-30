using MixItUp.Base.Model.Commands;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayPositionedItemV3ModelBase : OverlayItemV3ModelBase
    {
        public const string PositionedHTML = "<div style=\"position: absolute; margin: 0px; left: {XPosition}{PositionType}; top: {YPosition}{PositionType}; transform: translate(-{XTranslation}%, -{YTranslation}%); {Width} {Height}\">{InnerHTML}</div>";

        [DataMember]
        public int XPosition { get; set; }
        [DataMember]
        public int YPosition { get; set; }
        [DataMember]
        public bool IsPercentagePosition { get; set; }

        [DataMember]
        public int XTranslation { get; set; }
        [DataMember]
        public int YTranslation { get; set; }

        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayPositionedItemV3ModelBase() { }

        protected override async Task<OverlayItemV3ModelBase> GetProcessedItem(OverlayItemV3ModelBase item, CommandParametersModel parameters)
        {
            item = await base.GetProcessedItem(item, parameters);

            item.HTML = this.ReplaceProperty(PositionedHTML, "InnerHTML", item.HTML);

            item.HTML = this.ReplaceProperty(item.HTML, "XPosition", this.XPosition.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "YPosition", this.YPosition.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "PositionType", this.IsPercentagePosition ? "%" : "px");
            item.HTML = this.ReplaceProperty(item.HTML, "XTranslation", this.XTranslation.ToString());
            item.HTML = this.ReplaceProperty(item.HTML, "YTranslation", this.YTranslation.ToString());

            if (this.Width > 0)
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Width", $"width: {this.Width}px;");
            }
            else
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Width", $"width: auto;");
            }

            if (this.Height > 0)
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Height", $"height: {this.Height}px;");
            }
            else
            {
                item.HTML = this.ReplaceProperty(item.HTML, "Height", $"height: auto;");
            }

            return item;
        }
    }
}
