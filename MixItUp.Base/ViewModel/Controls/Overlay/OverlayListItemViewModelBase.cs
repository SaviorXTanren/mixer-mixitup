using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
    public abstract class OverlayListItemViewModelBase : OverlayHTMLTemplateItemViewModelBase
    {
        public string TotalToShowString
        {
            get { return this.totalToShow.ToString(); }
            set
            {
                this.totalToShow = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int totalToShow;

        public string FadeOutString
        {
            get { return this.fadeOut.ToString(); }
            set
            {
                this.fadeOut = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int fadeOut;

        public string WidthString
        {
            get { return this.width.ToString(); }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int width;

        public string HeightString
        {
            get { return this.height.ToString(); }
            set
            {
                this.height = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        protected int height;

        public string Font
        {
            get { return this.font; }
            set
            {
                this.font = value;
                this.NotifyPropertyChanged();
            }
        }
        private string font;

        public string TextColor
        {
            get { return this.textColor; }
            set
            {
                this.textColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string textColor;

        public string BorderColor
        {
            get { return this.borderColor; }
            set
            {
                this.borderColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string borderColor;

        public string BackgroundColor
        {
            get { return this.backgroundColor; }
            set
            {
                this.backgroundColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string backgroundColor;

        public string EntranceAnimationString
        {
            get { return EnumHelper.GetEnumName(this.entranceAnimation); }
            set
            {
                this.entranceAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectEntranceAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        protected OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation;

        public string ExitAnimationString
        {
            get { return EnumHelper.GetEnumName(this.exitAnimation); }
            set
            {
                this.exitAnimation = EnumHelper.GetEnumValueFromString<OverlayItemEffectExitAnimationTypeEnum>(value);
                this.NotifyPropertyChanged();
            }
        }
        protected OverlayItemEffectExitAnimationTypeEnum exitAnimation;

        public OverlayListItemViewModelBase()
        {
            this.totalToShow = 5;
            this.fadeOut = 0;
            this.width = 400;
            this.height = 100;
            this.Font = "Arial";
        }

        public OverlayListItemViewModelBase(int totalToShow, int fadeOut, int width, int height, string textFont, string textColor, string borderColor, string backgroundColor,
            OverlayItemEffectEntranceAnimationTypeEnum entranceAnimation, OverlayItemEffectExitAnimationTypeEnum exitAnimation, string htmlText)
            : this()
        {
            this.totalToShow = totalToShow;
            this.fadeOut = fadeOut;
            this.width = width;
            this.height = height;
            this.Font = textFont;

            this.TextColor = ColorSchemes.GetColorName(textColor);
            this.BorderColor = ColorSchemes.GetColorName(borderColor);
            this.BackgroundColor = ColorSchemes.GetColorName(backgroundColor);

            this.entranceAnimation = entranceAnimation;
            this.exitAnimation = exitAnimation;

            this.HTML = htmlText;
        }

        protected bool Validate()
        {
            return this.totalToShow > 0 && this.fadeOut >= 0 && !string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.TextColor) && !string.IsNullOrEmpty(this.BorderColor)
                && !string.IsNullOrEmpty(this.BackgroundColor) && this.width > 0 && this.height > 0 && !string.IsNullOrEmpty(this.HTML);
        }
    }
}
