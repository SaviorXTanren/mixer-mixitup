using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayTimerControl.xaml
    /// </summary>
    public partial class OverlayTimerControl : OverlayItemControl
    {
        private OverlayTimer item;

        public OverlayTimerControl()
        {
            InitializeComponent();
        }

        public OverlayTimerControl(OverlayTimer item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayTimer)item;

            this.TotalLengthTextBox.Text = this.item.TotalLength.ToString();

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.TextFontComboBox.Text = this.item.TextFont;

            this.TextSizeComboBox.Text = this.item.TextSize.ToString();

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (!int.TryParse(this.TotalLengthTextBox.Text, out int length) || length <= 0)
            {
                return null;
            }

            string textColor = this.TextColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(textColor))
            {
                textColor = ColorSchemes.ColorSchemeDictionary[textColor];
            }

            if (string.IsNullOrEmpty(this.TextFontComboBox.Text))
            {
                return null;
            }

            if (!int.TryParse(this.TextSizeComboBox.Text, out int size) || size <= 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            return new OverlayTimer(this.HTMLText.Text, length, textColor, this.TextFontComboBox.Text, size);
        }

        protected override Task OnLoaded()
        {
            this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;
            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();
            this.TextSizeComboBox.ItemsSource = OverlayTextItemControl.sampleFontSize.Select(f => f.ToString());

            this.TextFontComboBox.Text = "Arial";
            this.HTMLText.Text = OverlayTimer.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
