using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Desktop;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private ObservableCollection<MainMenuItem> menuItems = new ObservableCollection<MainMenuItem>();

        public MainMenuControl()
        {
            InitializeComponent();
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => await this.CheckMixerStatus());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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
                    Process.Start(menuItem.HelpLink);
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
                Process.Start(item.Link);
            }
            this.MenuToggleButton.IsChecked = false;
        }

        private async void BackupSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(ChannelSession.Settings.Channel.user.username + ".mixitup");
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ChannelSession.Services.Settings.Save(ChannelSession.Settings);

                    DesktopChannelSettings desktopSettings = (DesktopChannelSettings)ChannelSession.Settings;
                    string settingsFilePath = ChannelSession.Services.Settings.GetFilePath(desktopSettings);

                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    using (ZipArchive zipFile = ZipFile.Open(filePath, ZipArchiveMode.Create))
                    {
                        zipFile.CreateEntryFromFile(settingsFilePath, Path.GetFileName(settingsFilePath));
                        zipFile.CreateEntryFromFile(desktopSettings.DatabasePath, Path.GetFileName(desktopSettings.DatabasePath));
                    }
                }
            });
        }

        private async void SwitchThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will switch themes and restart Mix It Up. Are you sure you wish to do this?"))
            {
                App.AppSettings.DarkTheme = !App.AppSettings.DarkTheme;
                App.AppSettings.Save();
                ((MainWindow)this.Window).Restart();
            }
        }

        private async void RestoreSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will overwrite your current settings and close Mix It Up. Are you sure you wish to do this?"))
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Mix It Up Settings (*.mixitup)|*.mixitup|All files (*.*)|*.*");
                if (!string.IsNullOrEmpty(filePath))
                {
                    ((MainWindow)this.Window).RestoredSettingsFilePath = filePath;
                    ((MainWindow)this.Window).Restart();
                }
            }
        }

        private async void ReRunWizardSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("Mix It Up will restart and the New User Wizard will be re-run when you log in. This will allow you to re-import your data, which could duplicate and overwrite your Commands & User data. Are you sure you wish to do this?"))
            {
                MainWindow mainWindow = (MainWindow)this.Window;
                mainWindow.ReRunWizard();
            }
        }

        private void InstallationDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        private void MixerStatusAlertButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://status.mixer.com/"); }

        private void DocumentationButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki"); }

        private async void EnableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will enable diagnostic logging and restart Mix It Up. This should only be done with advised by a Mix It Up developer. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = true;
                ((MainWindow)this.Window).Restart();
            }
        }

        private async void DisableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await MessageBoxHelper.ShowConfirmationDialog("This will disable diagnostic logging and restart Mix It Up. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = false;
                ((MainWindow)this.Window).Restart();
            }
        }

        private async Task CheckMixerStatus()
        {
            while (true)
            {
                try
                {
                    IEnumerable<MixerIncident> incidents = await ChannelSession.Services.MixerStatus.GetCurrentIncidents();

                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        if (incidents != null && incidents.Count() > 0)
                        {
                            this.MixerStatusAlertButton.Visibility = Visibility.Visible;
                            StringBuilder tooltip = new StringBuilder();
                            tooltip.AppendLine("Mixer Status Alert:");
                            foreach (MixerIncident incident in incidents)
                            {
                                tooltip.AppendLine();
                                tooltip.AppendLine(string.Format("{0} - Last Updated: {1}", incident.Title, incident.LastUpdate.ToString("G")));
                            }
                            this.MixerStatusAlertButton.ToolTip = tooltip.ToString();
                        }
                        else
                        {
                            this.MixerStatusAlertButton.Visibility = Visibility.Collapsed;
                        }
                    });

                    await Task.Delay(60000);
                }
                catch (Exception ex) { MixItUp.Base.Util.Logger.Log(ex); }
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.FlyoutMenuDialog.Visibility = Visibility.Collapsed;
            this.SettingsGrid.Visibility = Visibility.Visible;
        }

        private void SettingsItemsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CloseSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            this.FlyoutMenuDialog.Visibility = Visibility.Visible;
            this.SettingsGrid.Visibility = Visibility.Collapsed;
        }
    }
}
