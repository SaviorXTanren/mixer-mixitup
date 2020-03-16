using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
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
            this.SettingsBackupRateComboBox.ItemsSource = Enum.GetValues(typeof(SettingsBackupRateEnum));
            this.UnlockAllCommandsTextBlock.ToolTip = UnlockedAllTooltip;
            this.UnlockAllCommandsToggleButton.ToolTip = UnlockedAllTooltip;

            this.SettingsBackupRateComboBox.SelectedItem = ChannelSession.Settings.SettingsBackupRate;
            if (!string.IsNullOrEmpty(ChannelSession.Settings.SettingsBackupLocation))
            {
                this.SettingsBackupRateComboBox.IsEnabled = true;
            }

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
            ProcessHelper.LaunchFolder(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));
        }

        private async void BackupSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                string filePath = ChannelSession.Services.FileService.ShowSaveFileDialog(ChannelSession.Settings.Name + "." + SettingsV2Model.SettingsBackupFileExtension);
                if (!string.IsNullOrEmpty(filePath))
                {
                    await ChannelSession.Services.Settings.SavePackagedBackup(ChannelSession.Settings, filePath);
                }
            });
        }

        private async void RestoreSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("This will overwrite your current settings and close Mix It Up. Are you sure you wish to do this?"))
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog(string.Format("Mix It Up Settings V2 Backup (*.{0})|*.{0}|Mix It Up Settings V1 Backup (*.{1})|*.{1}|All files (*.*)|*.*",
                    SettingsV2Model.SettingsBackupFileExtension,
#pragma warning disable CS0612 // Type or member is obsolete
                    SettingsV1Model.SettingsBackupFileExtension));
#pragma warning restore CS0612 // Type or member is obsolete
                if (!string.IsNullOrEmpty(filePath))
                {
                    Result<SettingsV2Model> result = await ChannelSession.Services.Settings.RestorePackagedBackup(filePath);
                    if (result.Success)
                    {
                        ((MainWindow)this.Window).RestoredSettingsFilePath = filePath;
                        ((MainWindow)this.Window).Restart();
                    }
                    else
                    {
                        await DialogHelper.ShowMessage(result.Message);
                    }
                }
            }
        }

        private void SettingsBackupLocationButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = ChannelSession.Services.FileService.ShowOpenFolderDialog();
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                ChannelSession.Settings.SettingsBackupLocation = folderPath;
                this.SettingsBackupRateComboBox.IsEnabled = true;
            }
        }

        private void SettingsBackupRateComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (this.SettingsBackupRateComboBox.SelectedIndex >= 0)
            {
                ChannelSession.Settings.SettingsBackupRate = (SettingsBackupRateEnum)this.SettingsBackupRateComboBox.SelectedItem;
            }
        }

        private async void ReRunWizardSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("Mix It Up will restart and the New User Wizard will be re-run when you log in. This will allow you to re-import your data, which could duplicate and overwrite your Commands & User data. Are you sure you wish to do this?"))
            {
                MainWindow mainWindow = (MainWindow)this.Window;
                mainWindow.ReRunWizard();
            }
        }

        private async void EnableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("This will enable diagnostic logging and restart Mix It Up. This should only be done with advised by a Mix It Up developer. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = true;
                ((MainWindow)this.Window).Restart();
            }
        }

        private async void DisableDiagnosticLogsButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("This will disable diagnostic logging and restart Mix It Up. Are you sure you wish to do this?"))
            {
                ChannelSession.Settings.DiagnosticLogging = false;
                ((MainWindow)this.Window).Restart();
            }
        }

        private void UnlockAllCommandsToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.UnlockAllCommands = this.UnlockAllCommandsToggleButton.IsChecked.GetValueOrDefault();
        }

        private async void ClearAllUserDataButton_Click(object sender, RoutedEventArgs e)
        {
            if (await DialogHelper.ShowConfirmation("This will clear all data for your Users, which includes their Hours, Currency, Rank, & Custom User Commands, then restart Mix It Up." +
                Environment.NewLine + Environment.NewLine + "This CAN NOT be un-done! Are you sure you wish to do this?"))
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    await ChannelSession.Settings.ClearAllUserData();
                    await ChannelSession.SaveSettings();
                });
                ((MainWindow)this.Window).Restart();
            }
        }

        private async void UnbanAllUsersButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (ChannelSession.IsStreamer)
                {
                    if (await DialogHelper.ShowConfirmation("This will unban all currently banned users from your channel. This will take some time to complete, are you sure you wish to do this?"))
                    {
                        await ChannelSession.MixerUserConnection.GetUsersWithRoles(ChannelSession.MixerChannel, UserRoleEnum.Banned, async (collection) =>
                        {
                            foreach (UserWithGroupsModel user in collection)
                            {
                                await ChannelSession.MixerUserConnection.RemoveUserRoles(ChannelSession.MixerChannel, user, new List<UserRoleEnum>() { UserRoleEnum.Banned });
                            }
                        });
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage("This can only be run by the channel owner");
                }
            });
        }
    }
}
