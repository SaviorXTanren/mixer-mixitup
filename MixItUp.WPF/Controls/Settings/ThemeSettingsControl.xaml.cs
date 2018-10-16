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

        private List<string> availableFullThemes = new List<string>() { "None" };

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
            this.FullThemeComboBox.SelectedItem = App.AppSettings.FullThemeName;

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
                if (!this.FullThemeComboBox.SelectedItem.Equals(App.AppSettings.FullThemeName))
                {
                    App.AppSettings.SettingsChangeRestartRequired = true;
                }
                App.AppSettings.FullThemeName = (string)this.FullThemeComboBox.SelectedItem;
            }
        }
    }
}
