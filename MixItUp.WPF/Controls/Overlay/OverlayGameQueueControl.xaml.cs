using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameQueueControl.xaml
    /// </summary>
    public partial class OverlayGameQueueControl : OverlayItemControl
    {
        private OverlayGameQueue item;

        public OverlayGameQueueControl()
        {
            InitializeComponent();
        }

        public OverlayGameQueueControl(OverlayGameQueue item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayGameQueue)item;

            this.TotalToShowTextBox.Text = this.item.TotalToShow.ToString();

            this.WidthTextBox.Text = this.item.Width.ToString();
            this.HeightTextBox.Text = this.item.Height.ToString();

            this.TextFontComboBox.Text = this.item.TextFont;

            this.BorderColorComboBox.Text = this.item.BorderColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.BorderColor))
            {
                this.BorderColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BorderColor)).Key;
            }

            this.BackgroundColorComboBox.Text = this.item.BackgroundColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.BackgroundColor))
            {
                this.BackgroundColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.BackgroundColor)).Key;
            }

            this.TextColorComboBox.Text = this.item.TextColor;
            if (ColorSchemes.ColorSchemeDictionary.ContainsValue(this.item.TextColor))
            {
                this.TextColorComboBox.Text = ColorSchemes.ColorSchemeDictionary.FirstOrDefault(c => c.Value.Equals(this.item.TextColor)).Key;
            }

            this.AddEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.AddEventAnimation);
            this.RemoveEventAnimationComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.RemoveEventAnimation);

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {

            if (string.IsNullOrEmpty(this.TotalToShowTextBox.Text) || !int.TryParse(this.TotalToShowTextBox.Text, out int totalToShow) || totalToShow <= 0)
            {
                return null;
            }

            string borderColor = this.BorderColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(borderColor))
            {
                borderColor = ColorSchemes.ColorSchemeDictionary[borderColor];
            }

            string backgroundColor = this.BackgroundColorComboBox.Text;
            if (ColorSchemes.ColorSchemeDictionary.ContainsKey(backgroundColor))
            {
                backgroundColor = ColorSchemes.ColorSchemeDictionary[backgroundColor];
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

            if (string.IsNullOrEmpty(this.WidthTextBox.Text) || !int.TryParse(this.WidthTextBox.Text, out int width) ||
                string.IsNullOrEmpty(this.HeightTextBox.Text) || !int.TryParse(this.HeightTextBox.Text, out int height))
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            OverlayEffectEntranceAnimationTypeEnum addEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectEntranceAnimationTypeEnum>((string)this.AddEventAnimationComboBox.SelectedItem);
            OverlayEffectExitAnimationTypeEnum removeEventAnimation = EnumHelper.GetEnumValueFromString<OverlayEffectExitAnimationTypeEnum>((string)this.RemoveEventAnimationComboBox.SelectedItem);

            return new OverlayGameQueue(this.HTMLText.Text,totalToShow, this.TextFontComboBox.Text, width, height, borderColor, backgroundColor, textColor, addEventAnimation, removeEventAnimation);
        }

        protected override Task OnLoaded()
        {
            this.TotalToShowTextBox.Text = "5";

            this.WidthTextBox.Text = "0";
            this.HeightTextBox.Text = "0";

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.BorderColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.AddEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectEntranceAnimationTypeEnum>();
            this.AddEventAnimationComboBox.SelectedIndex = 0;
            this.RemoveEventAnimationComboBox.ItemsSource = EnumHelper.GetEnumNames<OverlayEffectExitAnimationTypeEnum>();
            this.RemoveEventAnimationComboBox.SelectedIndex = 0;

            this.WidthTextBox.Text = "400";
            this.HeightTextBox.Text = "100";
            this.TextFontComboBox.Text = "Arial";
            this.HTMLText.Text = OverlayGameQueue.HTMLTemplate;

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }
    }
}
