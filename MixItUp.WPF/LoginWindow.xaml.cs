using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        private List<OAuthClientScopeEnum> StreamerScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.channel__details__self,
            OAuthClientScopeEnum.channel__update__self,
            OAuthClientScopeEnum.channel__analytics__self,

            OAuthClientScopeEnum.interactive__manage__self,
            OAuthClientScopeEnum.interactive__robot__self,

            OAuthClientScopeEnum.user__details__self,
        };

        private List<OAuthClientScopeEnum> ModeratorScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__change_ban,
            OAuthClientScopeEnum.chat__change_role,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__clear_messages,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__purge,
            OAuthClientScopeEnum.chat__remove_message,
            OAuthClientScopeEnum.chat__timeout,
            OAuthClientScopeEnum.chat__view_deleted,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,
        };

        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        protected override async Task OnLoaded()
        {
            List<ChannelSettings> settings = new List<ChannelSettings>(await ChannelSettings.GetAllAvailableSettings());
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
                        MessageBoxHelper.ShowError("You must select a Streamer account to log in to");
                    }
                }
                else
                {
                    result = await this.NewStreamerLogin();
                }
            });

            if (result)
            {
                StreamerWindow window = new StreamerWindow();
                this.Hide();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to initialize session, please try again");
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
                MessageBoxHelper.ShowError("A channel name must be entered");
                return;
            }

            bool result = await this.RunAsyncOperation(async () =>
            {
                return await this.EstablishConnection(this.ModeratorScopes, this.ModeratorChannelTextBox.Text);
            });

            if (result)
            {
                ModeratorWindow window = new ModeratorWindow();
                this.Hide();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to initialize session, please try again");
            }
        }

        private async Task<bool> NewStreamerLogin()
        {
            return await this.EstablishConnection(this.StreamerScopes, channelName: null);
        }

        private async Task<bool> EstablishConnection(IEnumerable<OAuthClientScopeEnum> scopes, string channelName = null)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            bool result = await this.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Initialize(clientID, scopes, channelName);
            });

            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            return result;
        }
    }
}
