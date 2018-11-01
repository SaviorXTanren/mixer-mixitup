using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayHTMLItem : OverlayItemBase
    {
        [DataMember]
        public string HTMLText { get; set; }

        public OverlayHTMLItem() { }

        public OverlayHTMLItem(string htmlText)
        {
            this.HTMLText = htmlText;
        }
    }
}
