using MixItUp.Base;
using MixItUp.Desktop;
using MixItUp.WPF.Controls.Command;
using MixItUp.WPF.Controls.MainControls;
using MixItUp.WPF.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Settings
{
    /// <summary>
    /// Interaction logic for AdvancedSettingsControl.xaml
    /// </summary>
    public partial class AdvancedSettingsControl : SettingsControlBase
    {
        public static readonly string UnlockedAllTooltip =
            "Mix It Up has build in Command locking functionality which ensures only" + Environment.NewLine +
            "1 command type (Chat, Interactive, etc) can run at the same time and" + Environment.NewLine +
            "ensures that each command finishes in the order it was run in." + Environment.NewLine + Environment.NewLine +
            "This option will allow you to disable locking on ALL commands. Be aware" + Environment.NewLine +
            "that this could cause some unforeseen issues, so please use with caution.";

        public AdvancedSettingsControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {
            this.UnlockAllCommandsTextBlock.ToolTip = UnlockedAllTooltip;
            this.UnlockAllCommandsToggleButton.ToolTip = UnlockedAllTooltip;

            this.UnlockAllCommandsToggleButton.IsChecked = ChannelSession.Settings.UnlockAllCommands;
            this.DisableDiagnosticLogsButton.Visibility = (ChannelSession.Settings.DiagnosticLogging) ? Visibility.Visible : Visibility.Collapsed;
            this.EnableDiagnosticLogsButton.Visibility = (ChannelSession.Settings.DiagnosticLogging) ? Visibility.Collapsed : Visibility.Visible;

            await base.InitializeInternal();
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.InitializeInternal();
        }

        private void InstallationDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
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

        private void UnlockAllCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.UnlockAllCommands = this.UnlockAllCommandsToggleButton.IsChecked.GetValueOrDefault();
        }
    }
}
