using AutoUpdaterDotNET;
using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop;
using MixItUp.Installer;
using MixItUp.WPF.Properties;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF
{
    public class StreamerLoginItem
    {
        public IChannelSettings Setting;

        public StreamerLoginItem(IChannelSettings setting)
        {
            this.Setting = setting;
        }

        public string Name { get { return this.Setting.Channel.user.username; } }
    }

    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : LoadingWindowBase
    {
        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            GlobalEvents.OnShowMessageBox += GlobalEvents_OnShowMessageBox;

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            if (Environment.OSVersion.Version < minimumOSVersion)
            {
                await MessageBoxHelper.ShowMessageDialog("Thank you for using Mix It Up, but unfortunately we only support Windows 8 & higher. If you are running Windows 8 or higher and see this message, please contact Mix It Up support for assistance.");
                this.Close();
                return;
            }

            if (!App.AppSettings.InstallerFolderUpgradeAsked)
            {
                App.AppSettings.InstallerFolderUpgradeAsked = true;
                App.AppSettings.Save();

                string currentInstallDirectory = ChannelSession.Services.FileService.GetApplicationDirectory();
                if (!Logger.IsDebug && !currentInstallDirectory.Equals(InstallerHelpers.InstallDirectory))
                {
                    if (await MessageBoxHelper.ShowConfirmationDialog("We noticed you are not running Mix It Up from the default installation folder." +
                        " We now have a full installer that puts Mix It Up into a safe location and creates a Start Menu shortcut for you to easily launch it." +
                        Environment.NewLine + Environment.NewLine + 
                        " We can install the latest version of Mix It Up with the Start Menu shortcut, copy over your current settings over there for you, and keep this version of Mix It Up as is for backup purposes." +
                        Environment.NewLine + Environment.NewLine +
                        "The process should take no more than a miunute; would you like us to do this for you?"))
                    {
                        bool installationSuccessful = await Task.Run(() =>
                        {
                            try
                            {
                                return InstallerHelpers.DownloadMixItUp() && InstallerHelpers.CreateMixItUpShortcut();
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                            return false;
                        });

                        if (installationSuccessful)
                        {
                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, "Settings"),
                                Path.Combine(InstallerHelpers.InstallDirectory, "Settings"));

                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, OBSStudioAction.OBSStudioReferenceTextFilesDirectory),
                                Path.Combine(InstallerHelpers.InstallDirectory, OBSStudioAction.OBSStudioReferenceTextFilesDirectory));

                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, XSplitAction.XSplitReferenceTextFilesDirectory),
                                Path.Combine(InstallerHelpers.InstallDirectory, XSplitAction.XSplitReferenceTextFilesDirectory));

                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, "Counters"),
                                Path.Combine(InstallerHelpers.InstallDirectory, "Counters"));

                            Process.Start(Path.Combine(InstallerHelpers.StartMenuDirectory, InstallerHelpers.ShortcutFileName));
                            this.Close();
                            return;
                        }
                        else
                        {
                            await MessageBoxHelper.ShowMessageDialog("The installation failed for an unknown reason");
                        }
                    }
                }
            }

            List<IChannelSettings> settings = new List<IChannelSettings>(await ChannelSession.Services.Settings.GetAllSettings());
            settings = settings.Where(s => s.IsStreamer).ToList();
            if (settings.Count() > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                settings.Add(new DesktopChannelSettings() { Channel = new ExpandedChannelModel() { id = 0, user = new UserModel() { username = "NEW STREAMER" } } });
                this.ExistingStreamerComboBox.ItemsSource = settings.Select(cs => new StreamerLoginItem(cs));
                if (settings.Count() == 2)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            await base.OnLoaded();

            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
            AutoUpdater.Start("https://updates.mixitupapp.com/AutoUpdater.xml");
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = false;

            await this.RunAsyncOperation(async () =>
            {
                if (this.ExistingStreamerComboBox.Visibility == Visibility.Visible)
                {
                    if (this.ExistingStreamerComboBox.SelectedIndex >= 0)
                    {
                        StreamerLoginItem loginItem = (StreamerLoginItem)this.ExistingStreamerComboBox.SelectedItem;
                        IChannelSettings setting = loginItem.Setting;
                        if (setting.Channel.id == 0)
                        {
                            result = await this.NewStreamerLogin();
                        }
                        else
                        {
                            result = await ChannelSession.ConnectUser(setting);
                            if (result)
                            {
                                if (!await ChannelSession.ConnectBot(setting))
                                {
                                    await MessageBoxHelper.ShowMessageDialog("Bot Account failed to authenticate, please re-connect it from the Services section.");
                                }

                                LoadingWindowBase newWindow = null;
                                if (ChannelSession.Settings.ReRunWizard)
                                {
                                    newWindow = new NewUserWizardWindow();
                                }
                                else
                                {
                                    newWindow = new MainWindow();
                                }
                                this.Hide();
                                newWindow.Show();
                                this.Close();
                            }
                        }
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("You must select a Streamer account to log in to");
                    }
                }
                else
                {
                    result = await this.NewStreamerLogin();
                }
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
        }

        private void ModeratorChannelTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ModeratorLoginButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void ModeratorLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.ModeratorChannelTextBox.Text))
            {
                await MessageBoxHelper.ShowMessageDialog("A channel name must be entered");
                return;
            }

            if (await this.EstablishConnection(ChannelSession.ModeratorScopes, this.ModeratorChannelTextBox.Text))
            {
                IEnumerable<UserWithGroupsModel> users = await ChannelSession.Connection.GetUsersWithRoles(ChannelSession.Channel, UserRole.Mod);
                if (users.Any(uwg => uwg.id.Equals(ChannelSession.User.id)))
                {
                    MainWindow window = new MainWindow();
                    this.Hide();
                    window.Show();
                    this.Close();
                }
                else
                {
                    await MessageBoxHelper.ShowMessageDialog("You are not a moderator for this channel.");
                }
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
        }

        private async Task<bool> NewStreamerLogin()
        {
            if (await this.EstablishConnection(ChannelSession.StreamerScopes, channelName: null))
            {
                NewUserWizardWindow window = new NewUserWizardWindow();
                window.Show();
                this.Close();
                return true;
            }
            return false;
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            return await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectUser(scopes, channelName);
            });
        }

        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null && args.IsUpdateAvailable)
            {
                UpdateWindow window = new UpdateWindow(args);
                window.Show();
            }
        }

        private async void GlobalEvents_OnShowMessageBox(object sender, string message)
        {
            await this.RunAsyncOperation(async () =>
            {
                await MessageBoxHelper.ShowMessageDialog(message);
            });
        }
    }
}
