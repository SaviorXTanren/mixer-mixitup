using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlayVideoItemModel : OverlayFileItemModelBase
    {
        public const int DefaultHeight = 315;
        public const int DefaultWidth = 560;

        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public bool Loop { get; set; }

        [DataMember]
        public override string FileType { get { return "video"; } set { } }

        [DataMember]
        public override string FullLink { get { return OverlayItemModelBase.GetFileFullLink(this.FileID, this.FileType, this.FilePath); } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayVideoItemModel() : base() { }

        public OverlayVideoItemModel(string filepath, int width, int height, int volume, bool loop)
            : base(OverlayItemModelTypeEnum.Video, filepath, width, height)
        {
            this.Volume = volume;
            this.Loop = loop;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
