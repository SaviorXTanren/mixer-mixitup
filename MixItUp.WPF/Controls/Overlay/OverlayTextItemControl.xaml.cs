using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTextItemControl.xaml
    /// </summary>
    public partial class OverlayTextItemControl : OverlayItemControl
    {
        private static readonly List<int> sampleFontSize = new List<int>() { 12, 24, 36, 48, 60, 72, 84, 96, 108, 120 };

        private OverlayTextItem item;

        public OverlayTextItemControl()
        {
            InitializeComponent();
        }

        public OverlayTextItemControl(OverlayTextItem item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayTextItem)item;
            this.TextTextBox.Text = this.item.Text;
            this.TextSizeComboBox.Text = this.item.Size.ToString();
            string color = this.item.Color;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(color))
            {
                color = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(color)).Key;
            }
            this.TextFontComboBox.Text = this.item.Font;
            this.TextBoldCheckBox.IsSelected = this.item.Bold;
            this.TextItalicCheckBox.IsSelected = this.item.Italic;
            this.TextUnderlineCheckBox.IsSelected = this.item.Underline;
            this.TextColorComboBox.Text = color;
            string shadowColor = this.item.ShadowColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(shadowColor))
            {
                shadowColor = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(shadowColor)).Key;
            }
            this.TextShadowColorComboBox.Text = shadowColor;
        }

        public override OverlayItemBase GetItem()
        {
            if (!string.IsNullOrEmpty(this.TextTextBox.Text) && !string.IsNullOrEmpty(this.TextColorComboBox.Text))
            {
                string color = this.TextColorComboBox.Text;
                if (ColorSchemes.ColorSchemeDictionary.ContainsKey(color))
                {
                    color = ColorSchemes.ColorSchemeDictionary[color];
                }

                string font = this.TextFontComboBox.Text;
                if (string.IsNullOrEmpty(font))
                {
                    font = null;
                }

                string shadowColor = this.TextShadowColorComboBox.Text;
                if (ColorSchemes.ColorSchemeDictionary.ContainsKey(shadowColor))
                {
                    shadowColor = ColorSchemes.ColorSchemeDictionary[shadowColor];
                }

                if (int.TryParse(this.TextSizeComboBox.Text, out int size) && size > 0)
                {
                    return new OverlayTextItem(this.TextTextBox.Text, color, size, font, this.TextBoldCheckBox.IsSelected, this.TextItalicCheckBox.IsSelected,
                        this.TextUnderlineCheckBox.IsSelected, shadowColor);
                }
            }
            return null;
        }

        protected override Task OnLoaded()
        {
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.TextSizeComboBox.ItemsSource = OverlayTextItemControl.sampleFontSize.Select(f => f.ToString());
            this.TextShadowColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
