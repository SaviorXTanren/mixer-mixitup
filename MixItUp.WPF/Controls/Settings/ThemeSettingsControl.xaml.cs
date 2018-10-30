using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for ThemeSettingsControl.xaml
    /// </summary>
    public partial class ThemeSettingsControl : SettingsControlBase
    {
        private List<string> availableBackgroundColors = new List<string>() { "Light", "Dark" };

        private Dictionary<string, string> availableFullThemes = new Dictionary<string, string>() { { "None", string.Empty }, { "1 Year Anniversary", "1YearAnniversary" },
            { "AwkwardTyson - Americana", "AwkwardTysonAmericana" }, { "Azhtral's Cosmic Fire" , "AzhtralsCosmicFire" }, { "Dusty's Purple Potion", "DustysPurplePotion" },
            { "Insert Coin Theater", "InsertCoinTheater" }, { "Nibbles' Carrot Patch", "NibblesCarrotPatch" }, { "Stark Contrast", "StarkContrast" },
            { "Tacos After Dark", "TacosAfterDark" }, { "Team Boom", "TeamBoom" } };

        public ThemeSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.BackgroundColorComboBox.ItemsSource = this.availableBackgroundColors;
            this.FullThemeComboBox.ItemsSource = this.availableFullThemes;

            this.ColorSchemeComboBox.SelectedItem = this.ColorSchemeComboBox.AvailableColorSchemes.FirstOrDefault(c => c.Name.Equals(App.AppSettings.ColorScheme));
            this.BackgroundColorComboBox.SelectedItem = App.AppSettings.BackgroundColor;
            this.FullThemeComboBox.SelectedItem = this.availableFullThemes.FirstOrDefault(t => t.Value.Equals(App.AppSettings.FullThemeName));

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void ColorSchemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ColorSchemeComboBox.SelectedIndex >= 0)
            {
                ColorSchemeOption colorScheme = (ColorSchemeOption)this.ColorSchemeComboBox.SelectedItem;

                if (!colorScheme.Name.Equals(App.AppSettings.ColorScheme))
                {
                    App.AppSettings.SettingsChangeRestartRequired = true;
                }
                App.AppSettings.ColorScheme = colorScheme.Name;
            }
        }

        private void BackgroundColorComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.BackgroundColorComboBox.SelectedIndex >= 0)
            {
                if (!this.BackgroundColorComboBox.SelectedItem.Equals(App.AppSettings.BackgroundColor))
                {
                    App.AppSettings.SettingsChangeRestartRequired = true;
                }
                App.AppSettings.BackgroundColor = (string)this.BackgroundColorComboBox.SelectedItem;
            }
        }

        private void FullThemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.FullThemeComboBox.SelectedIndex >= 0)
            {
                KeyValuePair<string, string> selectedValue = (KeyValuePair<string, string>)this.FullThemeComboBox.SelectedItem;
                if (!selectedValue.Value.Equals(App.AppSettings.FullThemeName))
                {
                    App.AppSettings.SettingsChangeRestartRequired = true;
                }
                App.AppSettings.FullThemeName = selectedValue.Value;
            }
        }
    }
}
