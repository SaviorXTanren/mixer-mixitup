using MixItUp.Base.Model.Overlay;
using StreamingClient.Base.Util;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayClipPlaybackItemViewModel : OverlayItemViewModelBase
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

        public bool Muted
        {
            get { return this.muted; }
            set
            {
                this.muted = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool muted;

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

        public OverlayClipPlaybackItemViewModel()
        {
            this.width = OverlayVideoItemModel.DefaultWidth;
            this.height = OverlayVideoItemModel.DefaultHeight;
            this.Muted = false;
        }

        public OverlayClipPlaybackItemViewModel(OverlayClipPlaybackItemModel item)
            : this()
        {
            this.width = item.Width;
            this.height = item.Height;
            this.Muted = item.Muted;
            this.entranceAnimation = item.Effects.EntranceAnimation;
            this.exitAnimation = item.Effects.ExitAnimation;
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (this.width > 0 && this.height > 0)
            {
                return new OverlayClipPlaybackItemModel(width, height, this.muted, this.entranceAnimation, this.exitAnimation);
            }
            return null;
        }
    }
}