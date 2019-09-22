using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using StreamingClient.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayStreamClipItemViewModel : OverlayItemViewModelBase
    {
        public string WidthString
        {
            get { return this.width.ToString(); }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public string HeightString
        {
            get { return this.height.ToString(); }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int height;

        public int Volume
        {
            get { return this.volume; }
            set
            {
                this.volume = value;
                this.NotifyPropertyChanged();
            }
        }
        private int volume;

        public string EntranceAnimationString
        {
            get { return EnumHelper.GetEnumName(this.entranceAnimation); }
            set
            {
                this.entranceAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectEntranceAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation;

        public string ExitAnimationString
        {
            get { return EnumHelper.GetEnumName(this.exitAnimation); }
            set
            {
                this.exitAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectExitAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayItemEffectExitAnimationTypeEnum exitAnimation;

        public OverlayStreamClipItemViewModel()
        {
            this.width = OverlayVideoItemModel.DefaultWidth;
            this.height = OverlayVideoItemModel.DefaultHeight;
            this.Volume = 100;
        }

        public OverlayStreamClipItemViewModel(OverlayStreamClipItemModel item)
            : this()
        {
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
            this.entranceAnimation = item.Effects.EntranceAnimation;
            this.exitAnimation = item.Effects.ExitAnimation;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.width > 0 && this.height > 0)
            {
                return new OverlayStreamClipItemModel(width, height, this.volume, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}
