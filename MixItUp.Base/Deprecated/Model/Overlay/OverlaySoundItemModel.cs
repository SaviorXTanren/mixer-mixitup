using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlaySoundItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public override string FileType { get { return "sound"; } set { } }

        [DataMember]
        public override string FullLink { get { return OverlayItemModelBase.GetFileFullLink(this.FileID, this.FileType, this.FilePath); } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlaySoundItemModel() : base() { }

        public OverlaySoundItemModel(string filepath, int volume)
            : base(OverlayItemModelTypeEnum.Sound, filepath, 0, 0)
        {
            this.Volume = volume;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
