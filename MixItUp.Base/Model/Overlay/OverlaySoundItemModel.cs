using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlaySoundItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public override string FileType { get { return "sound"; } set { } }

        [DataMember]
        public override string FullLink { get { return this.GetFileFullLink(this.FileID, this.FileType, this.FilePath); } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlaySoundItemModel() : base() { }

        public OverlaySoundItemModel(string filepath, int volume, double duration)
            : base(OverlayItemModelTypeEnum.Sound, filepath, 0, 0)
        {
            this.Volume = volume;

            this.Position = new OverlayItemPositionModel
            {
                Horizontal = -10000,
                Vertical = -10000,
                Layer = -10000,
                PositionType = OverlayItemPositionType.Pixel,
            };

            this.Effects = new OverlayItemEffectsModel
            {
                Duration = duration,
                EntranceAnimation = OverlayItemEffectEntranceAnimationTypeEnum.None,
                ExitAnimation = OverlayItemEffectExitAnimationTypeEnum.None,
                VisibleAnimation = OverlayItemEffectVisibleAnimationTypeEnum.None,
            };
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
