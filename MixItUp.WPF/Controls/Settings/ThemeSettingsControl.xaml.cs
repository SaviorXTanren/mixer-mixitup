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
        private List<string> availableThemes = new List<string>() { "Light", "Dark" };

        public ThemeSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.ColorSchemeComboBox.SelectionChanged += ColorSchemeComboBox_SelectionChanged;

            this.ThemeNameComboBox.ItemsSource = this.availableThemes;

            this.ThemeNameComboBox.SelectedItem = App.AppSettings.ThemeName;
            this.ColorSchemeComboBox.SelectedItem = this.ColorSchemeComboBox.AvailableColorSchemes.FirstOrDefault(c => c.Name.Equals(App.AppSettings.ColorScheme));

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void ThemeNameComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ThemeNameComboBox.SelectedIndex >= 0)
            {
                if (!this.ThemeNameComboBox.SelectedItem.Equals(App.AppSettings.ThemeName))
                {
                    App.AppSettings.SettingsChangeRestartRequired = true;
                }
                App.AppSettings.ThemeName = (string)this.ThemeNameComboBox.SelectedItem;
            }
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
    }
}
