using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayVideoItemModel : OverlayFileItemModelBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItemModel() : base() { }

        public OverlayVideoItemModel(string filepath, int width, int height, int volume)
            : base(OverlayItemModelTypeEnum.Video, filepath, width, height)
        {
            this.Volume = volume;
        }
    }
}
