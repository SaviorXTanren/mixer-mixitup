using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Overlay
{
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
                this.color = value;
                this.NotifyPropertyChanged();
            }
        }
        private string color;

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

        public OverlayTextItemViewModel()
        {
            this.Font = "Arial";
        }

        public OverlayTextItemViewModel(OverlayTextItem item)
            : this()
        {
            this.Text = item.Text;
            this.size = item.Size;
            this.Font = item.Font;
            this.Bold = item.Bold;
            this.Italic = item.Italic;
            this.Underline = item.Underline;

            this.Color = item.Color;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.Color))
            {
                this.Color = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.Color)).Key;
            }

            this.ShadowColor = item.ShadowColor;
            if (ColorSchemes.HTMLColorSchemeDictionary.ContainsValue(this.ShadowColor))
            {
                this.ShadowColor = ColorSchemes.HTMLColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.ShadowColor)).Key;
            }
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.Text) && !string.IsNullOrEmpty(this.Font) && !string.IsNullOrEmpty(this.Color) && this.size > 0)
            {
                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.Color))
                {
                    this.Color = ColorSchemes.HTMLColorSchemeDictionary[this.Color];
                }

                if (ColorSchemes.HTMLColorSchemeDictionary.ContainsKey(this.ShadowColor))
                {
                    this.ShadowColor = ColorSchemes.HTMLColorSchemeDictionary[this.ShadowColor];
                }

                return new OverlayTextItem(this.Text, this.Color, this.size, this.Font, this.Bold, this.Italic, this.Underline, this.ShadowColor);
            }
            return null;
        }
    }
}
