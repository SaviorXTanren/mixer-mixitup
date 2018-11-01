using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayWebPageItem : OverlayItemBase
    {
        [DataMember]
        public string URL { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        public OverlayWebPageItem() { }

        public OverlayWebPageItem(string url, int width, int height)
        {
            this.URL = url;
            this.Width = width;
            this.Height = height;
        }
    }
}
