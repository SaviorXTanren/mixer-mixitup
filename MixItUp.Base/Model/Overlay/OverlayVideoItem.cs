using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayVideoItem : OverlayItemBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public string ID { get; set; }

        [DataMember]
        public string FullLink
        {
            get
            {
                if (!Uri.IsWellFormedUriString(this.FilePath, UriKind.RelativeOrAbsolute))
                {
                    return string.Format("/overlay/files/{0}", this.ID);
                }
                return this.FilePath;
            }
            set { }
        }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItem() { this.Volume = 100; }

        public OverlayVideoItem(string filepath, int width, int height, int volume)
        {
            this.FilePath = filepath;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
            this.ID = Guid.NewGuid().ToString().Replace("-", string.Empty);
        }
    }
}
