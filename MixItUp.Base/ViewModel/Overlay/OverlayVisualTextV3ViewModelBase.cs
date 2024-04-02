using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Overlay
{
    public abstract class OverlayVisualTextV3ViewModelBase : OverlayItemV3ViewModelBase
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
                if (value > 0)
                {
                    this.fontSize = value;
                }
                else
                {
                    this.fontSize = 0;
                }
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

        public IEnumerable<string> Fonts
        {
            get { return ServiceManager.Get<IFileService>().GetInstalledFonts(); }
        }

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
        protected int width;

        public OverlayVisualTextV3ViewModelBase(OverlayItemV3Type type)
            : base(type)
        {
            this.FontSize = 24;
            this.FontName = "Arial";
            this.FontColor = "Black";
            this.LeftAlignment = true;
        }

        public OverlayVisualTextV3ViewModelBase(OverlayVisualTextV3ModelBase item)
            : base(item)
        {
            this.Text = item.Text;
            this.fontSize = item.FontSize;
            this.FontName = item.FontName;
            this.FontColor = item.FontColor;
            this.Bold = item.Bold;
            this.Italics = item.Italics;
            this.Underline = item.Underline;
            this.LeftAlignment = item.TextAlignment == OverlayVisualTextItemV3AlignmentTypeEnum.Left;
            this.CenterAlignment = item.TextAlignment == OverlayVisualTextItemV3AlignmentTypeEnum.Center;
            this.RightAlignment = item.TextAlignment == OverlayVisualTextItemV3AlignmentTypeEnum.Right;
            this.JustifyAlignment = item.TextAlignment == OverlayVisualTextItemV3AlignmentTypeEnum.Justify;
            this.ShadowColor = item.ShadowColor;
            this.width = item.Width;
        }

        public void AssignProperties(OverlayVisualTextV3ModelBase item)
        {
            item.Text = this.Text;
            item.FontSize = this.FontSize;
            item.FontName = this.FontName;
            item.FontColor = this.FontColor;
            item.Bold = this.Bold;
            item.Italics = this.Italics;
            item.Underline = this.Underline;
            item.ShadowColor = this.ShadowColor;
            item.Width = this.width;

            if (this.LeftAlignment)
            {
                item.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Left;
            }
            else if (this.CenterAlignment)
            {
                item.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Center;
            }
            else if (this.RightAlignment)
            {
                item.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Right;
            }
            else if (this.JustifyAlignment)
            {
                item.TextAlignment = OverlayVisualTextItemV3AlignmentTypeEnum.Justify;
            }
        }
    }
}