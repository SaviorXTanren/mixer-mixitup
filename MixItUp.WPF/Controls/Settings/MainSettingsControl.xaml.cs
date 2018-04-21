using MixItUp.WPF.Controls.MainControls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Settings
{
    public class SettingsOption
    {
        public string Name { get; set; }
        public MainControlBase Control { get; set; }

        public SettingsOption(string name, MainControlBase control)
        {
            this.Name = name;
            this.Control = control;
        }
    }

    /// <summary>
    /// Interaction logic for MainSettingsControl.xaml
    /// </summary>
    public partial class MainSettingsControl : MainControlBase
    {
        private ObservableCollection<SettingsOption> settingsGroups = new ObservableCollection<SettingsOption>();

        public MainSettingsControl()
        {
            InitializeComponent();

            this.SettingsItemsListBox.ItemsSource = this.settingsGroups;
        }

        protected override async Task InitializeInternal()
        {
            this.settingsGroups.Clear();

            this.settingsGroups.Add(new SettingsOption("Themes & Colors", new ThemeSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Advanced", new AdvancedSettingsControl()));

            foreach (SettingsOption settings in this.settingsGroups)
            {
                await settings.Control.Initialize(this.Window);
            }

            if (this.SettingsItemsListBox.SelectedIndex < 0)
            {
                this.SettingsItemsListBox.SelectedIndex = 0;
            }
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void SettingsItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.SettingsItemsListBox.SelectedIndex >= 0)
            {
                SettingsOption settings = (SettingsOption)this.SettingsItemsListBox.SelectedItem;
                this.SettingsContent.Content = settings.Control;
            }
        }
    }
}
