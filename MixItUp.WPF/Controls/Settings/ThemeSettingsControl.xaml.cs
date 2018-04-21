using MixItUp.WPF.Controls.MainControls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Settings
{
    public class ColorSchemeOption
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }

        public SolidColorBrush ColorBrush { get; set; }

        public ColorSchemeOption() { }

        public ColorSchemeOption(string name, string colorCode)
        {
            this.Name = name;
            this.ColorCode = colorCode;
            this.ColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.ColorCode));
        }
    }

    /// <summary>
    /// Interaction logic for ThemeSettingsControl.xaml
    /// </summary>
    public partial class ThemeSettingsControl : MainControlBase
    {
        private Dictionary<string, string> colorSchemeDictionary = new Dictionary<string, string>()
        {
            { "Amber", "#ffb300" },
            { "Blue", "#2196f3" },
            { "Blue Grey", "#607d8b" },
            { "Brown", "#795548" },
            { "Cyan", "#00bcd4" },
            { "Deep Orange", "#ff5722" },
            { "Deep Purple", "#673ab7" },
            { "Green", "#4caf50" },
            { "Grey", "#9e9e9e" },
            { "Indigo", "#3f51b5" },
            { "Light Blue", "#03a9f4" },
            { "Light Green", "#8bc34a" },
            { "Lime", "#cddc39" },
            { "Orange", "#ff9800" },
            { "Pink", "#e91e63" },
            { "Purple", "#9c27b0" },
            { "Red", "#f44336" },
            { "Teal", "#009688" },
            { "Yellow", "#ffeb3b" },
        };

        private List<string> availableThemes = new List<string>() { "Light", "Dark" };

        private ObservableCollection<ColorSchemeOption> availableColorSchemes = new ObservableCollection<ColorSchemeOption>();

        public ThemeSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.ThemeNameComboBox.ItemsSource = this.availableThemes;
            this.ColorSchemeComboBox.ItemsSource = this.availableColorSchemes;

            this.availableColorSchemes.Clear();
            foreach (var kvp in this.colorSchemeDictionary)
            {
                this.availableColorSchemes.Add(new ColorSchemeOption(kvp.Key, kvp.Value));
            }

            this.ThemeNameComboBox.SelectedItem = App.AppSettings.ThemeName;
            this.ColorSchemeComboBox.SelectedItem = this.availableColorSchemes.FirstOrDefault(c => c.Name.Equals(App.AppSettings.ColorScheme));

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
                App.AppSettings.SettingsChangeRestartRequired = (!this.ThemeNameComboBox.SelectedItem.Equals(App.AppSettings.ThemeName));
                App.AppSettings.ThemeName = (string)this.ThemeNameComboBox.SelectedItem;
            }
        }

        private void ColorSchemeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.ColorSchemeComboBox.SelectedIndex >= 0)
            {
                ColorSchemeOption colorScheme = (ColorSchemeOption)this.ColorSchemeComboBox.SelectedItem;

                App.AppSettings.SettingsChangeRestartRequired = (!colorScheme.Name.Equals(App.AppSettings.ColorScheme));
                App.AppSettings.ColorScheme = colorScheme.Name;
            }
        }
    }
}
