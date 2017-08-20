using Mixer.Base;
using Mixer.Base.Model.Channel;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.WPF.Util;
using MixItUp.WPF.Windows;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            bool result = await this.Login(new List<ClientScopeEnum>()
            {
                ClientScopeEnum.chat__bypass_links,
                ClientScopeEnum.chat__bypass_slowchat,
                ClientScopeEnum.chat__change_ban,
                ClientScopeEnum.chat__change_role,
                ClientScopeEnum.chat__chat,
                ClientScopeEnum.chat__connect,
                ClientScopeEnum.chat__clear_messages,
                ClientScopeEnum.chat__edit_options,
                ClientScopeEnum.chat__giveaway_start,
                ClientScopeEnum.chat__poll_start,
                ClientScopeEnum.chat__poll_vote,
                ClientScopeEnum.chat__purge,
                ClientScopeEnum.chat__remove_message,
                ClientScopeEnum.chat__timeout,
                ClientScopeEnum.chat__view_deleted,
                ClientScopeEnum.chat__whisper,

                ClientScopeEnum.channel__details__self,
                ClientScopeEnum.channel__update__self,

                ClientScopeEnum.interactive__manage__self,
                ClientScopeEnum.interactive__robot__self,

                ClientScopeEnum.user__details__self,
            }, channelName: null);

            if (result)
            {
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

            bool result = await this.Login(new List<ClientScopeEnum>()
            {
                ClientScopeEnum.chat__bypass_links,
                ClientScopeEnum.chat__bypass_slowchat,
                ClientScopeEnum.chat__change_ban,
                ClientScopeEnum.chat__change_role,
                ClientScopeEnum.chat__chat,
                ClientScopeEnum.chat__connect,
                ClientScopeEnum.chat__clear_messages,
                ClientScopeEnum.chat__edit_options,
                ClientScopeEnum.chat__giveaway_start,
                ClientScopeEnum.chat__poll_start,
                ClientScopeEnum.chat__poll_vote,
                ClientScopeEnum.chat__purge,
                ClientScopeEnum.chat__remove_message,
                ClientScopeEnum.chat__timeout,
                ClientScopeEnum.chat__view_deleted,
                ClientScopeEnum.chat__whisper,

                ClientScopeEnum.user__details__self,
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

        private async Task<bool> Login(IEnumerable<ClientScopeEnum> scopes, string channelName)
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
