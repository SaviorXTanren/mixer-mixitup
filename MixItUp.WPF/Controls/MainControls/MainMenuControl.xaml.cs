using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        public string Link { get; private set; }

        public Visibility LinkIconVisibility { get { return (!string.IsNullOrEmpty(this.Link)) ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility HelpLinkVisibility { get { return (!string.IsNullOrEmpty(this.HelpLink)) ? Visibility.Visible : Visibility.Collapsed; } }

        public MainMenuItem(string name, MainControlBase control, string helpLink = null)
        {
            this.Name = name;
            this.Control = control;
            this.HelpLink = helpLink;
        }

        public MainMenuItem(string name, string link)
        {
            this.Name = name;
            this.Link = link;
        }
    }

    /// <summary>
    /// Interaction logic for MainMenuControl.xaml
    /// </summary>
    public partial class MainMenuControl : MainControlBase
    {
        private const string SwitchToLightThemeText = "Switch to Light Theme";
        private const string SwitchToDarkThemeText = "Switch to Dark Theme";

        private static readonly string DisconnectedServicesHeader = "These services have disconnected" + Environment.NewLine + "and are attempting to reconnect:";

        private HashSet<string> serviceDisconnections = new HashSet<string>();

        private ObservableCollection<MainMenuItem> menuItems = new ObservableCollection<MainMenuItem>();

        public MainMenuControl()
        {
            InitializeComponent();

            GlobalEvents.OnServiceDisconnect += GlobalEvents_OnServiceDisconnect;
            GlobalEvents.OnServiceReconnect += GlobalEvents_OnServiceReconnect;
        }

        public async Task AddMenuItem(string name, MainControlBase control, string helpLink = null)
        {
            await control.Initialize(this.Window);
            this.menuItems.Add(new MainMenuItem(name, control, helpLink));
            if (this.menuItems.Count == 1)
            {
                this.MenuItemSelected(this.menuItems.First());
            }
        }

        public void AddMenuItem(string name, string link)
        {
            this.menuItems.Add(new MainMenuItem(name, link));
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
                    ProcessHelper.LaunchLink(menuItem.HelpLink);
                }
            }
        }

        private void MenuItemSelected(MainMenuItem item)
        {
            if (item.Control != null)
            {
                this.DataContext = item;
                this.ActiveControlContentControl.Content = item.Control;
            }
            else if (!string.IsNullOrEmpty(item.Link))
            {
                ProcessHelper.LaunchLink(item.Link);
            }
            this.MenuToggleButton.IsChecked = false;
        }

        private async void GlobalEvents_OnServiceDisconnect(object sender, string serviceName)
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath))
            {
                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationServiceDisconnectSoundFilePath, ChannelSession.Settings.NotificationServiceDisconnectSoundVolume);
            }

            lock (this.serviceDisconnections)
            {
                this.serviceDisconnections.Add(serviceName);
            }
            this.RefreshServiceDisconnectionsAlertTooltip();
        }

        private async void GlobalEvents_OnServiceReconnect(object sender, string serviceName)
        {
            if (!string.IsNullOrEmpty(ChannelSession.Settings.NotificationServiceConnectSoundFilePath))
            {
                await ChannelSession.Services.AudioService.Play(ChannelSession.Settings.NotificationServiceConnectSoundFilePath, ChannelSession.Settings.NotificationServiceConnectSoundVolume);
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
                tooltip.AppendLine();
                lock (this.serviceDisconnections)
                {
                    foreach (string serviceName in this.serviceDisconnections.OrderBy(s => s))
                    {
                        tooltip.AppendLine("- " + serviceName);
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
                App.AppSettings.Save();
                if (App.AppSettings.SettingsChangeRestartRequired && await DialogHelper.ShowConfirmation("Some of the settings you have changed require a restart to take effect. Would you like to restart Mix It Up now?"))
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
    }
}
