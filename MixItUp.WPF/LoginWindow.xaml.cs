using AutoUpdaterDotNET;
using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.API;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop;
using MixItUp.Installer;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : LoadingWindowBase
    {
        private static readonly Version minimumOSVersion = new Version(6, 2, 0, 0);

        private MixItUpUpdateModel currentUpdate;
        private bool updateFound = false;

        private ObservableCollection<IChannelSettings> streamerSettings = new ObservableCollection<IChannelSettings>();
        private ObservableCollection<IChannelSettings> moderatorSettings = new ObservableCollection<IChannelSettings>();

        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            GlobalEvents.OnShowMessageBox += GlobalEvents_OnShowMessageBox;

            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            this.ExistingStreamerComboBox.ItemsSource = streamerSettings;
            this.ModeratorChannelComboBox.ItemsSource = moderatorSettings;

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
                        " We now have a full installer that puts Mix It Up into your Local App Data folder and creates a Start Menu shortcut for you to easily launch it." +
                        Environment.NewLine + Environment.NewLine + 
                        "We can install the latest version of Mix It Up with the Start Menu shortcut, copy over your current settings over there for you, and keep this version of Mix It Up as is for backup purposes." +
                        Environment.NewLine + Environment.NewLine +
                        "The process should take no more than a miunute; would you like us to do this for you?"))
                    {
                        bool installationSuccessful = await Task.Run(() =>
                        {
                            try
                            {
                                return InstallerHelpers.DownloadMixItUp() && InstallerHelpers.InstallMixItUp() && InstallerHelpers.CreateMixItUpShortcut();
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                            return false;
                        });

                        if (installationSuccessful)
                        {
                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, "Settings"),
                                Path.Combine(InstallerHelpers.InstallDirectory, "Settings"));

                            await ChannelSession.Services.FileService.CopyDirectory(Path.Combine(currentInstallDirectory, "Counters"),
                                Path.Combine(InstallerHelpers.InstallDirectory, "Counters"));

                            await MessageBoxHelper.ShowMessageDialog("Mix It Up was successfully installed to your Local App Data folder and a shortcut was created on the Start Menu." + 
                                " We will now close this version of Mix It Up and launch your newly installed version.");

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

            await this.CheckForUpdates();

            foreach (IChannelSettings setting in (await ChannelSession.Services.Settings.GetAllSettings()).OrderBy(s => s.Channel.user.username))
            {
                if (setting.IsStreamer)
                {
                    this.streamerSettings.Add(setting);
                }
                else
                {
                    this.moderatorSettings.Add(setting);
                }
            }

            if (this.streamerSettings.Count > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                this.streamerSettings.Add(new DesktopChannelSettings() { Channel = new ExpandedChannelModel() { id = 0, user = new UserModel() { username = "NEW STREAMER" } } });
                if (this.streamerSettings.Count() == 2)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            if (this.moderatorSettings.Count == 1)
            {
                this.ModeratorChannelComboBox.SelectedIndex = 0;
            }

            if (App.AppSettings.AutoLogInAccount > 0)
            {
                var allSettings = this.streamerSettings.ToList();
                allSettings.AddRange(this.moderatorSettings);

                IChannelSettings autoLogInSettings = allSettings.FirstOrDefault(s => s.Channel.user.id == App.AppSettings.AutoLogInAccount);
                if (autoLogInSettings != null)
                {
                    await Task.Delay(5000);

                    if (!updateFound)
                    {
                        if (await this.ExistingSettingLogin(autoLogInSettings))
                        {
                            LoadingWindowBase newWindow = null;
                            if (ChannelSession.Settings.ReRunWizard)
                            {
                                newWindow = new NewUserWizardWindow();
                            }
                            else
                            {
                                newWindow = new MainWindow();
                            }
                            ShowMainWindow(newWindow);
                            this.Hide();
                            this.Close();
                            return;
                        }
                    }
                }
            }

            await base.OnLoaded();
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
                        IChannelSettings setting = (IChannelSettings)this.ExistingStreamerComboBox.SelectedItem;
                        if (setting.Channel.id == 0)
                        {
                            result = await this.NewStreamerLogin();
                        }
                        else
                        {
                            if (await this.ExistingSettingLogin(setting))
                            {
                                LoadingWindowBase newWindow = null;
                                if (ChannelSession.Settings.ReRunWizard)
                                {
                                    newWindow = new NewUserWizardWindow();
                                }
                                else
                                {
                                    newWindow = new MainWindow();
                                }
                                ShowMainWindow(newWindow);
                                this.Hide();
                                this.Close();
                                return;
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
        }

        private void ModeratorChannelComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.ModeratorLoginButton_Click(this, new RoutedEventArgs());
            }
        }

        private async void ModeratorLoginButton_Click(object sender, RoutedEventArgs e)
        {
            await this.RunAsyncOperation(async () =>
            {
                if (string.IsNullOrEmpty(this.ModeratorChannelComboBox.Text))
                {
                    await MessageBoxHelper.ShowMessageDialog("A channel name must be entered");
                    return;
                }

                bool authenticationSuccessful = false;
                if (this.ModeratorChannelComboBox.SelectedIndex >= 0)
                {
                    IChannelSettings setting = (IChannelSettings)this.ModeratorChannelComboBox.SelectedItem;
                    authenticationSuccessful = await this.ExistingSettingLogin(setting);
                }
                else
                {
                    authenticationSuccessful = await this.EstablishConnection(ChannelSession.ModeratorScopes, this.ModeratorChannelComboBox.Text);
                }

                if (authenticationSuccessful)
                {
                    IEnumerable<UserWithGroupsModel> users = await ChannelSession.Connection.GetUsersWithRoles(ChannelSession.Channel, MixerRoleEnum.Mod);
                    if (users.Any(uwg => uwg.id.Equals(ChannelSession.User.id)) || Logger.IsDebug)
                    {
                        ShowMainWindow(new MainWindow());
                        this.Hide();
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
            });
        }

        private async Task CheckForUpdates()
        {
            this.currentUpdate = await ChannelSession.Services.MixItUpService.GetLatestUpdate();
            if (this.currentUpdate != null)
            {
                if (App.AppSettings.PreviewProgram)
                {
                    MixItUpUpdateModel previewUpdate = await ChannelSession.Services.MixItUpService.GetLatestPreviewUpdate();
                    if (previewUpdate != null && previewUpdate.SystemVersion >= this.currentUpdate.SystemVersion)
                    {
                        this.currentUpdate = previewUpdate;
                    }
                }

                AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckForUpdateEvent;
                AutoUpdater.Start(this.currentUpdate.AutoUpdaterLink);
            }
        }

        private async Task<bool> ExistingSettingLogin(IChannelSettings setting)
        {
            bool result = await ChannelSession.ConnectUser(setting);
            if (result)
            {
                if (!await ChannelSession.ConnectBot(setting))
                {
                    await MessageBoxHelper.ShowMessageDialog("Bot Account failed to authenticate, please re-connect it from the Services section.");
                }
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
            return result;
        }

        private async Task<bool> NewStreamerLogin()
        {
            if (await this.EstablishConnection(ChannelSession.StreamerScopes, channelName: null))
            {
                ShowMainWindow(new NewUserWizardWindow());
                this.Hide();
                this.Close();
                return true;
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer, please try again");
            }
            return false;
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            return await ChannelSession.ConnectUser(scopes, channelName);
        }

        private void AutoUpdater_CheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args != null && args.IsUpdateAvailable)
            {
                updateFound = true;
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
