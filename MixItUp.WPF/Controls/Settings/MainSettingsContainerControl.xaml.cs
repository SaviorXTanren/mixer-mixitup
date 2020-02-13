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

            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.General, new GeneralSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.ThemesAndColors, new ThemeSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Notifications, new NotificationsSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Chat, new ChatSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Users, new UsersSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.MixPlay, new InteractiveSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Overlays, new OverlaySettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.HotKeys, new HotKeysSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Remote, new RemoteSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.SerialDevices, new SerialDevicesSettingsControl()));
            this.settingsGroups.Add(new SettingsOption(MixItUp.Base.Resources.Advanced, new AdvancedSettingsControl()));
        }

        protected override async Task InitializeInternal()
        {
            if (this.Window != null)
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
