using MixItUp.Base.Model.Overlay;
using MixItUp.Base.ViewModels;

namespace MixItUp.Base.ViewModel.Overlay
{
    public class OverlayTextItemV3ViewModel : UIViewModelBase
    {
        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                this.NotifyPropertyChanged();
            }
        }
        private string text;

        public string FontSize
        {
            get { return this.fontSize.ToString(); }
            set
            {
                this.fontSize = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int fontSize;

        public string FontName
        {
            get { return this.fontName; }
            set
            {
                this.fontName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string fontName;

        public string FontColor
        {
            get { return this.fontColor; }
            set
            {
                this.fontColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string fontColor;

        public bool Bold
        {
            get { return this.bold; }
            set
            {
                this.bold = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool bold;

        public bool Italics
        {
            get { return this.italics; }
            set
            {
                this.italics = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool italics;

        public bool Underline
        {
            get { return this.underline; }
            set
            {
                this.underline = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool underline;

        public bool LeftAlignment
        {
            get { return this.leftAlignment; }
            set
            {
                this.leftAlignment = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool leftAlignment;

        public bool CenterAlignment
        {
            get { return this.centerAlignment; }
            set
            {
                this.centerAlignment = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool centerAlignment;

        public bool RightAlignment
        {
            get { return this.rightAlignment; }
            set
            {
                this.rightAlignment = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool rightAlignment;

        public bool JustifyAlignment
        {
            get { return this.justifyAlignment; }
            set
            {
                this.justifyAlignment = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool justifyAlignment;

        public string ShadowColor
        {
            get { return this.shadowColor; }
            set
            {
                this.shadowColor = value;
                this.NotifyPropertyChanged();
            }
        }
        private string shadowColor;

        public string Width
        {
            get { return this.width > 0 ? this.width.ToString() : string.Empty; }
            set
            {
                this.width = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public OverlayTextItemV3ViewModel(OverlayTextItemV3Model item)
        {
            this.Text = item.Text;
            this.fontSize = item.FontSize;
            this.FontName = item.FontName;
            this.FontColor = item.FontColor;
            this.Bold = item.Bold;
            this.Italics = item.Italics;
            this.Underline = item.Underline;
            this.LeftAlignment = item.TextAlignment == OverlayTextItemV3AlignmentTypeEnum.Left;
            this.CenterAlignment = item.TextAlignment == OverlayTextItemV3AlignmentTypeEnum.Center;
            this.RightAlignment = item.TextAlignment == OverlayTextItemV3AlignmentTypeEnum.Right;
            this.JustifyAlignment = item.TextAlignment == OverlayTextItemV3AlignmentTypeEnum.Justify;
            this.ShadowColor = item.ShadowColor;
            this.width = item.Width;
        }

        public OverlayTextItemV3ViewModel()
        {
            this.FontSize = "24";
            this.FontName = "Arial";
            this.FontColor = "Black";
        }

        public OverlayTextItemV3Model GetItem()
        {
            OverlayTextItemV3Model result = new OverlayTextItemV3Model()
            {
                Text = this.Text,
                FontSize = this.fontSize,
                FontName = this.FontName,
                FontColor = this.FontColor,
                Bold = this.Bold,
                Italics = this.Italics,
                Underline = this.Underline,
                ShadowColor = this.ShadowColor,
                Width = this.width,
            };

            if (this.LeftAlignment)
            {
                result.TextAlignment = OverlayTextItemV3AlignmentTypeEnum.Left;
            }
            else if (this.CenterAlignment)
            {
                result.TextAlignment = OverlayTextItemV3AlignmentTypeEnum.Center;
            }
            else if (this.RightAlignment)
            {
                result.TextAlignment = OverlayTextItemV3AlignmentTypeEnum.Right;
            }
            else if (this.JustifyAlignment)
            {
                result.TextAlignment = OverlayTextItemV3AlignmentTypeEnum.Justify;
            }

            return result;
        }
    }
}
