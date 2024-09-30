using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System;

namespace MixItUp.Base.ViewModel.Overlay
{
    [Obsolete]
    public class OverlayTextItemViewModel : OverlayItemViewModelBase
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

        public string SizeString
        {
            get { return this.size.ToString(); }
            set
            {
                this.size = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        private int size;

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

        public bool Italic
        {
            get { return this.italic; }
            set
            {
                this.italic = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool italic;

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

        public string Color
        {
            get { return this.color; }
            set
            {
                this.color = MixItUp.Base.Resources.ResourceManager.GetSafeString(value);
                this.NotifyPropertyChanged();
            }
        }
        private string color;

        public string ShadowColor
        {
            get { return this.shadowColor; }
            set
            {
                this.shadowColor = MixItUp.Base.Resources.ResourceManager.GetSafeString(value);
                this.NotifyPropertyChanged();
            }
        }
        private string shadowColor;

        public override bool SupportsRefreshUpdating { get { return true; } }

        public OverlayTextItemViewModel()
        {
            this.Font = "Arial";
            this.size = 24;
            //this.Color = ColorSchemes.GetColorName("Black");
        }

        public OverlayTextItemViewModel(OverlayTextItemModel item)
            : this()
        {
            this.Text = item.Text;
            this.size = item.Size;
            this.Font = item.Font;
            this.Bold = item.Bold;
            this.Italic = item.Italic;
            this.Underline = item.Underline;
            //this.Color = ColorSchemes.GetColorName(item.Color);
            //this.ShadowColor = ColorSchemes.GetColorName(item.ShadowColor);
        }

        public override OverlayItemModelBase GetOverlayItem()
        {
            if (!string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(this.Color) && this.size > 0)
            {
                //this.Color = ColorSchemes.GetColorCode(this.Color);
                //this.ShadowColor = ColorSchemes.GetColorCode(this.ShadowColor);

                return new OverlayTextItemModel(this.Text, this.Color, this.size, this.Font, this.Bold, this.Italic, this.Underline, this.ShadowColor);
            }
            return null;
        }
    }
}
