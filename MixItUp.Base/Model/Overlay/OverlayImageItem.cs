using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayImageItem : OverlayItemBase
    {
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}?nonce={1}", this.ID, Guid.NewGuid());
                }
                return this.FilePath;
            }
            set { }
        }

        public OverlayImageItem() { }

        public OverlayImageItem(string filepath, int width, int height)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.ID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
