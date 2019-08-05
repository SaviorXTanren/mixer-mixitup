using MixItUp.Base.Util;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.Settings
{
    public class ColorSchemeOption : IEquatable<ColorSchemeOption>
    {
        public string Name { get; set; }
        public string ColorCode { get; set; }

        public SolidColorBrush ColorBrush { get; set; }

        public ColorSchemeOption() { }

        public ColorSchemeOption(string name)
        {
            this.Name = name;
        }

        public ColorSchemeOption(string name, string colorCode)
            : this(name)
        {
            this.ColorCode = colorCode;
            if (!string.IsNullOrEmpty(this.ColorCode))
            {
                this.ColorBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom(this.ColorCode));
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ColorSchemeOption)
            {
                return this.Equals((ColorSchemeOption)obj);
            }
            return false;
        }

        public bool Equals(ColorSchemeOption other) { return this.Name.Equals(other.Name); }

        public override int GetHashCode() { return this.Name.GetHashCode(); }
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
            foreach (var kvp in ColorSchemes.HTMLColorSchemeDictionary)
            {
                this.AvailableColorSchemes.Add(new ColorSchemeOption(kvp.Key, kvp.Value));
            }
        }

        public void RemoveNonThemes()
        {
            this.AvailableColorSchemes.Remove(new ColorSchemeOption("Black"));
            this.AvailableColorSchemes.Remove(new ColorSchemeOption("White"));
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SelectionChanged != null)
            {
                this.SelectionChanged(sender, e);
            }
        }
    }
}
