using MixItUp.Base.Util;
using MixItUp.WPF.Controls.MainControls;
using System.Collections.Generic;
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

            List<SettingsOption> settings = new List<SettingsOption>();

            settings.Add(new SettingsOption(MixItUp.Base.Resources.General, new GeneralSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.ThemesAndColors, new ThemeSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Chat, new ChatSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Commands, new CommandsSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Alerts, new AlertsSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Notifications, new NotificationsSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Users, new UsersSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Overlays, new OverlaySettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Counters, new CountersSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.HotKeys, new HotKeysSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.SerialDevices, new SerialDevicesSettingsControl()));
            settings.Add(new SettingsOption(MixItUp.Base.Resources.Advanced, new AdvancedSettingsControl()));

            this.settingsGroups.AddRange(settings);
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
