using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public class OverlayMixerClipItemViewModel : OverlayItemViewModelBase
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
                this.entranceAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEffectEntranceAnimationTypeEnum entranceAnimation;

        public string ExitAnimationString
        {
            get { return EnumHelper.GetEnumName(this.exitAnimation); }
            set
            {
                this.exitAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        private OverlayEffectExitAnimationTypeEnum exitAnimation;

        public OverlayMixerClipItemViewModel()
        {
            this.width = OverlayVideoItem.DefaultWidth;
            this.height = OverlayVideoItem.DefaultHeight;
            this.Volume = 100;
        }

        public OverlayMixerClipItemViewModel(OverlayMixerClip item)
            : this()
        {
            this.width = item.Width;
            this.height = item.Height;
            this.Volume = item.Volume;
            this.entranceAnimation = item.EntranceAnimation;
            this.exitAnimation = item.ExitAnimation;
        }

        public override OverlayItemBase GetItem()
        {
            if (this.width > 0 && this.height > 0)
            {
                return new OverlayMixerClip(width, height, this.volume, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}
