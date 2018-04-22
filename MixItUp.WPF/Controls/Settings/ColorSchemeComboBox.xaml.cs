using MixItUp.Base.Themes;
using System.Collections.ObjectModel;
using System.Windows.Controls;
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
            if (!string.IsNullOrEmpty(this.ColorCode))
            {
                this.ColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.ColorCode));
            }
        }
    }

    /// <summary>
    /// Interaction logic for ColorSchemeComboBox.xaml
    /// </summary>
    public partial class ColorSchemeComboBox : UserControl
    {
        public event SelectionChangedEventHandler SelectionChanged;

        public int SelectedIndex { get { return this.ColorComboBox.SelectedIndex; } set { this.ColorComboBox.SelectedIndex = value; } }
        public object SelectedItem { get { return this.ColorComboBox.SelectedItem; } set { this.ColorComboBox.SelectedItem = value; } }

        public ObservableCollection<ColorSchemeOption> AvailableColorSchemes = new ObservableCollection<ColorSchemeOption>();

        public ColorSchemeComboBox()
        {
            InitializeComponent();

            this.ColorComboBox.SelectionChanged += ColorComboBox_SelectionChanged;
            this.ColorComboBox.ItemsSource = this.AvailableColorSchemes;

            this.AvailableColorSchemes.Clear();
            foreach (var kvp in ColorSchemes.ColorSchemeDictionary)
            {
                this.AvailableColorSchemes.Insert(0, new ColorSchemeOption(kvp.Key, kvp.Value));
            }
        }

        public void AddDefaultOption() { this.AvailableColorSchemes.Add(new ColorSchemeOption(ColorSchemes.DefaultColorScheme, "")); }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(sender, e);
            }
        }
    }
}
