using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.Base.ViewModel;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MixItUp.WPF
{
    public class StreamerLoginItem
    {
        public ChannelSettings Setting;

        public StreamerLoginItem(ChannelSettings setting)
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
        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            this.Title += " - v" + Assembly.GetEntryAssembly().GetName().Version.ToString();

            List<ChannelSettings> settings = new List<ChannelSettings>(await ChannelSettings.GetAllAvailableSettings());
            settings = settings.Where(s => s.IsStreamer).ToList();
            if (settings.Count() > 0)
            {
                this.ExistingStreamerComboBox.Visibility = Visibility.Visible;
                settings.Add(new ChannelSettings() { Channel = new ExpandedChannelModel() { id = 0, user = new UserModel() { username = "NEW STREAMER" } } });
                this.ExistingStreamerComboBox.ItemsSource = settings.Select(cs => new StreamerLoginItem(cs));
                if (settings.Count() == 2)
                {
                    this.ExistingStreamerComboBox.SelectedIndex = 0;
                }
            }

            await base.OnLoaded();

            await this.CheckIfNewerVersionExists();
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
                        ChannelSettings setting = loginItem.Setting;
                        if (setting.Channel.id == 0)
                        {
                            result = await this.NewStreamerLogin();
                        }
                        else
                        {
                            result = await ChannelSession.Initialize(setting);
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

            if (result)
            {
                MainWindow window = new MainWindow();
                this.Hide();
                window.Show();
                this.Close();
            }
            else
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to initialize session, please try again");
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

            bool result = await this.RunAsyncOperation(async () =>
            {
                return await this.EstablishConnection(ChannelSession.ModeratorScopes, this.ModeratorChannelTextBox.Text);
            });

            if (result)
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
                await MessageBoxHelper.ShowMessageDialog("Unable to initialize session, please try again");
            }
        }

        private async Task<bool> NewStreamerLogin()
        {
            return await this.EstablishConnection(ChannelSession.StreamerScopes, channelName: null);
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Initialize(scopes, channelName);
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            return result;
        }

        private async Task CheckIfNewerVersionExists()
        {
            using (HttpClient httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri("https://api.github.com/");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));

                HttpResponseMessage response = await httpClient.GetAsync("repos/SaviorXTanren/mixer-mixitup/releases");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string resultString = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(resultString))
                    {
                        JArray result = JArray.Parse(resultString);
                        for (int i = 0; i < result.Count; i++)
                        {
                            if (!((bool)result[i]["prerelease"]))
                            {
                                Version latestVersion = new Version(result[0]["tag_name"].ToString());
                                if (Assembly.GetEntryAssembly().GetName().Version.CompareTo(latestVersion) < 0)
                                {
                                    if (MessageBox.Show("There is a newer version of Mix It Up, would you like to download it?", "Mix It Up - Newer Version",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                    {
                                        Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/releases");
                                        this.Close();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
