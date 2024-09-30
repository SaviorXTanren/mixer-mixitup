using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace MixItUp.WPF.Controls.MainControls
{
    public class MainMenuItem : NotifyPropertyChangedBase
    {
        public string Name { get; private set; }
        public MainControlBase Control { get; private set; }
        public string HelpLink { get; private set; }

        public bool Visible
        {
            get { return this.visible; }
            set
            {
                this.visible = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool visible;

        public Visibility HelpLinkVisibility { get { return (!string.IsNullOrEmpty(this.HelpLink)) ? Visibility.Visible : Visibility.Collapsed; } }

        public MainMenuItem(string name, MainControlBase control, string helpLink = null)
        {
            this.Name = name;
            this.Control = control;
            this.HelpLink = helpLink;
        }
    }

    /// <summary>
    /// Interaction logic for MainMenuControl.xaml
    /// </summary>
    public partial class MainMenuControl : MainControlBase
    {
        public static event EventHandler<bool> OnMainMenuStateChanged = delegate { };

        private readonly string SwitchToLightThemeText = MixItUp.Base.Resources.SwitchToLightTheme;
        private readonly string SwitchToDarkThemeText = MixItUp.Base.Resources.SwitchToDarkTheme;

        private static readonly string DisconnectedServicesHeader = MixItUp.Base.Resources.ServiceDisconnectedLine1 + Environment.NewLine + MixItUp.Base.Resources.ServiceDisconnectedLine2;

        private HashSet<string> serviceDisconnections = new HashSet<string>();

        private ObservableCollection<MainMenuItem> menuItems = new ObservableCollection<MainMenuItem>();
        private List<MainMenuItem> allMenuItems = new List<MainMenuItem>();

        public MainMenuControl()
        {
            InitializeComponent();

            ServiceManager.OnServiceDisconnect += ServiceManager_OnServiceDisconnect;
            ServiceManager.OnServiceReconnect += ServiceManager_OnServiceReconnect;
        }

        public async Task<MainMenuItem> AddMenuItem(string name, MainControlBase control, string helpLink = null)
        {
            await control.Initialize(this.Window);
            MainMenuItem item = new MainMenuItem(name, control, helpLink);
            this.menuItems.Add(item);
            this.allMenuItems.Add(item);
            return item;
        }

        public void ShowMenuItem(MainMenuItem item)
        {
            if (!this.menuItems.Contains(item))
            {
                for (int i = this.allMenuItems.IndexOf(item) - 1; i >= 0; i--)
                {
                    if (this.menuItems.Contains(this.allMenuItems[i]))
                    {
                        this.menuItems.Insert(this.menuItems.IndexOf(this.allMenuItems[i]) + 1, item);
                        return;
                    }
                }
            }
        }

        public void HideMenuItem(MainMenuItem item)
        {
            this.menuItems.Remove(item);
        }

        public void MenuItemSelected(string name)
        {
            MainMenuItem item = this.menuItems.FirstOrDefault(i => i.Name.Equals(name));
            if (item != null)
            {
                this.MenuItemSelected(item);
            }
        }

        public void MenuItemSelected(MainMenuItem item)
        {
            if (item.Control != null)
            {
                this.DataContext = item;
                this.ActiveControlContentControl.Content = item.Control;
            }
            this.MenuToggleButton.IsChecked = false;
        }

        protected override async Task InitializeInternal()
        {
            this.MenuItemsListBox.ItemsSource = this.menuItems;

            await this.MainSettings.Initialize(this.Window);

            await base.InitializeInternal();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //until we had a StaysOpen glag to Drawer, this will help with scroll bars
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar)
                {
                    return;
                }
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            this.MenuToggleButton.IsChecked = false;
        }

        private void MenuItemsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.MenuItemsListBox.SelectedIndex >= 0)
            {
                MainMenuItem item = (MainMenuItem)this.MenuItemsListBox.SelectedItem;
                this.MenuItemSelected(item);
            }
        }

        private void SectionHelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null && this.DataContext is MainMenuItem)
            {
                MainMenuItem menuItem = (MainMenuItem)this.DataContext;
                if (!string.IsNullOrEmpty(menuItem.HelpLink))
                {
                    ServiceManager.Get<IProcessService>().LaunchLink(menuItem.HelpLink);
                }
            }
        }

        private async void ServiceManager_OnServiceDisconnect(object sender, string serviceName)
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath))
            {
                await ServiceManager.Get<IAudioService>().PlayNotification(ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath, ChannelSession.Settings.NotificationServiceDisconnectSoundVolume);
            }

            lock (this.serviceDisconnections)
            {
                this.serviceDisconnections.Add(serviceName);
            }
            this.RefreshServiceDisconnectionsAlertTooltip();
        }

        private async void ServiceManager_OnServiceReconnect(object sender, string serviceName)
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationServiceConnectSoundFilePath))
            {
                await ServiceManager.Get<IAudioService>().PlayNotification(ChannelSession.Settings.NotificationServiceConnectSoundFilePath, ChannelSession.Settings.NotificationServiceConnectSoundVolume);
            }

            lock (this.serviceDisconnections)
            {
                this.serviceDisconnections.Remove(serviceName);
            }
            this.RefreshServiceDisconnectionsAlertTooltip();
        }

        private void RefreshServiceDisconnectionsAlertTooltip()
        {
            this.Dispatcher.Invoke(() =>
            {
                StringBuilder tooltip = new StringBuilder();
                tooltip.AppendLine(DisconnectedServicesHeader);
                lock (this.serviceDisconnections)
                {
                    foreach (string serviceName in this.serviceDisconnections.OrderBy(s => s))
                    {
                        tooltip.AppendLine();
                        tooltip.Append("- " + serviceName);
                    }
                    this.DisconnectionAlertButton.Visibility = (serviceDisconnections.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
                }
                this.DisconnectionAlertButton.ToolTip = tooltip.ToString();
            });
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.FlyoutMenuDialog.Visibility = Visibility.Collapsed;
            this.SettingsGrid.Visibility = Visibility.Visible;
        }

        private async void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.AppSettings.Save();
                if (ChannelSession.AppSettings.SettingsChangeRestartRequired && await DialogHelper.ShowConfirmation(MixItUp.Base.Resources.SettingsChangedRestartPrompt))
                {
                    ((MainWindow)this.Window).Restart();
                }
                else
                {
                    this.FlyoutMenuDialog.Visibility = Visibility.Visible;
                    this.SettingsGrid.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void MenuToggleButton_Checked(object sender, RoutedEventArgs e) { OnMainMenuStateChanged(null, true); }

        private void MenuToggleButton_Unchecked(object sender, RoutedEventArgs e) { OnMainMenuStateChanged(null, false); }
    }
}
