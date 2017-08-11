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

            this.SetStatusBar(this.StatusBar);
        }

        private async void StreamerLoginButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.RunAsyncOperation(async () =>
            {
                return await this.Login(new List<ClientScopeEnum>()
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

                    ClientScopeEnum.user__details__self,
                });
            });

            if (result)
            {
                ChannelModel channel = await this.RunAsyncOperation(async () =>
                {
                    UserModel user = await MixerAPIHandler.MixerConnection.Users.GetCurrentUser();
                    return await MixerAPIHandler.MixerConnection.Channels.GetChannel(user.username);
                });

                if (channel != null)
                {
                    StreamerWindow window = new StreamerWindow(channel);
                    this.Hide();
                    window.Show();
                    this.Close();
                }
                else
                {
                    MessageBoxHelper.ShowError("Unable to connect to channel.");
                }
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
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
                return await this.Login(new List<ClientScopeEnum>()
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
                });
            });

            if (result)
            {
                ChannelModel channel = await this.RunAsyncOperation(async () =>
                {
                    return await MixerAPIHandler.MixerConnection.Channels.GetChannel(this.ModeratorChannelTextBox.Text);
                });

                if (channel != null)
                {
                    ModeratorWindow window = new ModeratorWindow(channel);
                    this.Hide();
                    window.Show();
                    this.Close();
                }
                else
                {
                    MessageBoxHelper.ShowError("Unable to connect to channel. Please ensure the channel you entered is valid.");
                }
            }
            else
            {
                MessageBoxHelper.ShowError("Unable to authenticate with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
        }

        private async Task<bool> Login(IEnumerable<ClientScopeEnum> scopes)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            return await MixerAPIHandler.InitializeMixerClient(clientID, scopes);
        }
    }
}
