using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
        public LoginWindow()
        {
            InitializeComponent();

            this.Initialize(this.StatusBar);
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.Login(new List<OAuthClientScopeEnum>()
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
            }, channelName: null);

            if (result)
            {
                if (this.BotLoginCheckBox.IsChecked.GetValueOrDefault())
                {
                    string clientID = ConfigurationManager.AppSettings["ClientID"];
                    if (string.IsNullOrEmpty(clientID))
                    {
                        throw new ArgumentException("ClientID value isn't set in application configuration");
                    }

                    result = await this.RunAsyncOperation(async () =>
                    {
                        if (await MixerAPIHandler.InitializeBotConnection(clientID, (OAuthShortCodeModel shortCode) =>
                        {
                            this.BotShortCodeGrid.Visibility = Visibility.Visible;
                            this.BotShortCodeTextBox.Text = shortCode.code;

                            Process.Start("https://mixer.com/oauth/shortcode?code=" + shortCode.code);
                        }))
                        {
                            PrivatePopulatedUserModel user = await MixerAPIHandler.BotConnection.Users.GetCurrentUser();
                            if (user != null)
                            {
                                ChannelSession.InitializeBot(user);
                                return true;
                            }
                        }
                        return false;
                    });

                    if (!result)
                    {
                        MessageBoxHelper.ShowError("Unable to authenticate Bot with Mixer. Please ensure you approved access for the application in a timely manner.");
                        return;
                    }
                }

                await ChannelSession.LoadSettings();

                StreamerWindow window = new StreamerWindow();
                this.Hide();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
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

            bool result = await this.Login(new List<OAuthClientScopeEnum>()
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
            }, this.ModeratorChannelTextBox.Text);

            if (result)
            {
                ModeratorWindow window = new ModeratorWindow();
                this.Hide();
                window.Show();
                this.Close();
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
        }

        private async Task<bool> Login(IEnumerable<OAuthClientScopeEnum> scopes, string channelName)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            return await this.RunAsyncOperation(async () =>
            {
                if (await MixerAPIHandler.InitializeMixerClient(clientID, scopes))
                {
                    PrivatePopulatedUserModel user = await MixerAPIHandler.MixerConnection.Users.GetCurrentUser();
                    if (user != null)
                    {
                        ExpandedChannelModel channel = await MixerAPIHandler.MixerConnection.Channels.GetChannel((channelName == null) ? user.username : channelName);
                        if (channel != null)
                        {
                            ChannelSession.Initialize(user, channel);
                            await ChannelSession.LoadSettings();
                            return true;
                        }
                    }
                }
                return false;
            });
        }
    }
}
