using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.Model.API;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Desktop;
using MixItUp.WPF.Windows;
using MixItUp.WPF.Windows.Wizard;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            await this.CheckForUpdates();

            foreach (IChannelSettings setting in (await ChannelSession.Services.Settings.GetAllSettings()).OrderBy(s => s.Channel.token))
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
                this.streamerSettings.Add(new DesktopChannelSettings() { Channel = new ExpandedChannelModel() { id = 0, user = new UserWithGroupsModel() { username = "NEW STREAMER" }, token = "NEW STREAMER" } });
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
                if (autoLogInSettings != null && autoLogInSettings.LicenseAccepted)
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
            await this.RunAsyncOperation(async () =>
            {
                if (this.ExistingStreamerComboBox.Visibility == Visibility.Visible)
                {
                    if (this.ExistingStreamerComboBox.SelectedIndex >= 0)
                    {
                        IChannelSettings setting = (IChannelSettings)this.ExistingStreamerComboBox.SelectedItem;
                        if (setting.Channel.id == 0)
                        {
                            await this.NewStreamerLogin();
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
                        await DialogHelper.ShowMessage("You must select a Streamer account to log in to");
                    }
                }
                else
                {
                    await this.NewStreamerLogin();
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
                    await DialogHelper.ShowMessage("A channel name must be entered");
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
                    if (await this.ShowLicenseAgreement())
                    {
                        authenticationSuccessful = await this.EstablishConnection(ChannelSession.ModeratorScopes, this.ModeratorChannelComboBox.Text);
                    }
                }

                if (authenticationSuccessful)
                {
                    IEnumerable<UserWithGroupsModel> users = await ChannelSession.MixerStreamerConnection.GetUsersWithRoles(ChannelSession.MixerChannel, MixerRoleEnum.Mod);
                    if (users.Any(uwg => uwg.id.Equals(ChannelSession.MixerStreamerUser.id)) || ChannelSession.IsDebug())
                    {
                        ShowMainWindow(new MainWindow());
                        this.Hide();
                        this.Close();
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("You are not a moderator for this channel.");
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage("Unable to authenticate with Mixer, please try again");
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

                if (this.currentUpdate.SystemVersion > Assembly.GetEntryAssembly().GetName().Version)
                {
                    updateFound = true;
                    UpdateWindow window = new UpdateWindow(this.currentUpdate);
                    window.Show();
                }
            }
        }

        private async Task<bool> ExistingSettingLogin(IChannelSettings setting)
        {
            if (setting.LicenseAccepted || await this.ShowLicenseAgreement())
            {
                bool result = await ChannelSession.ConnectUser(setting);
                if (result)
                {
                    if (!await ChannelSession.ConnectBot(setting))
                    {
                        await DialogHelper.ShowMessage("Bot Account failed to authenticate, please re-connect it from the Services section.");
                    }
                }
                else
                {
                    await DialogHelper.ShowMessage("Unable to authenticate with Mixer, please try again");
                }
                return result;
            }
            return false;
        }

        private async Task NewStreamerLogin()
        {
            if (await this.ShowLicenseAgreement())
            {
                ShowMainWindow(new NewUserWizardWindow());
                this.Hide();
                this.Close();
            }
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            return await ChannelSession.ConnectUser(scopes, channelName);
        }

        private async void GlobalEvents_OnShowMessageBox(object sender, string message)
        {
            await this.RunAsyncOperation(async () =>
            {
                await DialogHelper.ShowMessage(message);
            });
        }

        private Task<bool> ShowLicenseAgreement()
        {
            LicenseAgreementWindow window = new LicenseAgreementWindow();
            TaskCompletionSource<bool> task = new TaskCompletionSource<bool>();
            window.Owner = Application.Current.MainWindow;
            window.Closed += (s, a) => task.SetResult(window.Accepted);
            window.Show();
            window.Focus();
            return task.Task;
        }
    }
}