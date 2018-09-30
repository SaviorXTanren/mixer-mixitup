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
    /// Interaction logic for MainSettingsContainerControl.xaml
    /// </summary>
    public partial class MainSettingsContainerControl : MainControlBase
    {
        private ObservableCollection<SettingsOption> settingsGroups = new ObservableCollection<SettingsOption>();

        public MainSettingsContainerControl()
        {
            InitializeComponent();

            this.SettingsItemsListBox.ItemsSource = this.settingsGroups;

            this.settingsGroups.Add(new SettingsOption("General", new GeneralSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Themes & Colors", new ThemeSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Notifications", new NotificationsSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Chat", new ChatSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Interactive", new InteractiveSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Serial Devices", new SerialDevicesSettingsControl()));
            this.settingsGroups.Add(new SettingsOption("Advanced", new AdvancedSettingsControl()));
        }

        protected override async Task InitializeInternal()
        {
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
