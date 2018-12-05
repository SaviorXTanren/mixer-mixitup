using Mixer.Base.Util;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Overlay
{
    /// <summary>
    /// Interaction logic for OverlayGameStatsControl.xaml
    /// </summary>
    public partial class OverlayGameStatsControl : OverlayItemControl
    {
        private OverlayGameStats item;

        public OverlayGameStatsControl()
        {
            InitializeComponent();
        }

        public OverlayGameStatsControl(OverlayGameStats item)
            : this()
        {
            this.item = item;
        }

        public override void SetItem(OverlayItemBase item)
        {
            this.item = (OverlayGameStats)item;

            this.GameComboBox.SelectedItem = this.item.Setup.Name;

            this.UsernameTextBox.Text = this.item.Setup.Username;
            this.PlatformComboBox.SelectedItem = EnumHelper.GetEnumName(this.item.Setup.Platform);

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

            this.TextFontComboBox.Text = this.item.TextFont;
            this.TextSizeComboBox.Text = this.item.TextSize.ToString();

            this.HTMLText.Text = this.item.HTMLText;
        }

        public override OverlayItemBase GetItem()
        {
            if (this.GameComboBox.SelectedIndex < 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.UsernameTextBox.Text))
            {
                return null;
            }

            if (this.PlatformComboBox.SelectedIndex < 0)
            {
                return null;
            }
            GameStatsPlatformTypeEnum platform = EnumHelper.GetEnumValueFromString<GameStatsPlatformTypeEnum>((string)this.PlatformComboBox.SelectedItem);
            
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

            if (string.IsNullOrEmpty(this.TextSizeComboBox.Text) || !int.TryParse(this.TextSizeComboBox.Text, out int textSize))
            {
                return null;
            }

            if (string.IsNullOrEmpty(this.HTMLText.Text))
            {
                return null;
            }

            GameStatsSetupBase setup = null;
            if (this.GameComboBox.SelectedIndex >= 0)
            {
                string gameName = (string)this.GameComboBox.SelectedItem;
                switch (gameName)
                {
                    case RainboxSixSiegeGameStatsSetup.GameName:
                        setup = new RainboxSixSiegeGameStatsSetup(this.UsernameTextBox.Text, platform);
                        break;
                }
            }

            if (setup == null)
            {
                return null;
            }

            return new OverlayGameStats(this.HTMLText.Text, setup, borderColor, backgroundColor, textColor, this.TextFontComboBox.Text, textSize);
        }

        protected override Task OnLoaded()
        {
            this.GameComboBox.ItemsSource = new List<string>() { "Rainbox Six Siege" };
            this.PlatformComboBox.ItemsSource = EnumHelper.GetEnumNames<GameStatsPlatformTypeEnum>();

            this.TextFontComboBox.ItemsSource = InstalledFonts.GetInstalledFonts();

            this.BorderColorComboBox.ItemsSource = this.BackgroundColorComboBox.ItemsSource = this.TextColorComboBox.ItemsSource = ColorSchemes.ColorSchemeDictionary.Keys;

            this.TextFontComboBox.Text = "Arial";

            if (this.item != null)
            {
                this.SetItem(this.item);
            }

            return Task.FromResult(0);
        }

        private void GameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.GameComboBox.SelectedIndex >= 0)
            {
                string gameName = (string)this.GameComboBox.SelectedItem;
                switch (gameName)
                {
                    case RainboxSixSiegeGameStatsSetup.GameName:
                        this.HTMLText.Text = RainboxSixSiegeGameStatsSetup.DefaultHTMLTemplate;
                        break;
                }
            }
        }
    }
}
