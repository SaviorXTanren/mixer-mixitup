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

        public int FontSize
        {
            get { return this.fontSize; }
            set
            {
                this.fontSize = value;
                this.NotifyPropertyChanged();
            }
        }
        private int fontSize;

        public string SelectedFontName
        {
            get { return this.selectedFontName; }
            set
            {
                this.selectedFontName = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedFontName;

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

        public string ShadowSize
        {
            get { return this.shadowSize; }
            set
            {
                this.shadowSize = value;
                this.NotifyPropertyChanged();
            }
        }
        private string shadowSize;

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

        public int Width
        {
            get { return this.width; }
            set
            {
                this.width = value;
                this.NotifyPropertyChanged();
            }
        }
        private int width;

        public int Height
        {
            get { return this.height; }
            set
            {
                this.height = value;
                this.NotifyPropertyChanged();
            }
        }
        private int height;

        public OverlayTextItemV3ViewModel(OverlayTextItemV3Model item)
        {
            this.Text = item.Text;
            this.FontSize = item.FontSize;
            this.SelectedFontName = item.FontName;
            this.FontColor = item.FontColor;
            this.Bold = item.Bold;
            this.Italics = item.Italics;
            this.Underline = item.Underline;
            this.LeftAlignment = item.Alignment == OverlayTextAlignmentEnum.Left;
            this.CenterAlignment = item.Alignment == OverlayTextAlignmentEnum.Center;
            this.RightAlignment = item.Alignment == OverlayTextAlignmentEnum.Right;
            this.JustifyAlignment = item.Alignment == OverlayTextAlignmentEnum.Justify;
            this.ShadowSize = item.ShadowSize;
            this.ShadowColor = item.ShadowColor;
            this.Width = item.Width;
            this.Height = item.Height;
        }

        public OverlayTextItemV3ViewModel()
        {

        }

        public OverlayTextItemV3Model GetItem()
        {
            OverlayTextItemV3Model result = new OverlayTextItemV3Model()
            {
                Text = this.Text,
                FontSize = this.FontSize,
                FontName = this.SelectedFontName,
                FontColor = this.FontColor,
                Bold = this.Bold,
                Italics = this.Italics,
                Underline = this.underline,
                ShadowSize = this.ShadowSize,
                ShadowColor = this.ShadowColor,
                Width = this.width,
                Height = this.height
            };

            if (this.LeftAlignment)
            {
                result.Alignment = OverlayTextAlignmentEnum.Left;
            }
            else if (this.CenterAlignment)
            {
                result.Alignment = OverlayTextAlignmentEnum.Center;
            }
            else if (this.RightAlignment)
            {
                result.Alignment = OverlayTextAlignmentEnum.Right;
            }
            else if (this.JustifyAlignment)
            {
                result.Alignment = OverlayTextAlignmentEnum.Justify;
            }

            return result;
        }
    }
}
