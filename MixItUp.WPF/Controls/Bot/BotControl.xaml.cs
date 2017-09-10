using Mixer.Base;
using Mixer.Base.Model.OAuth;
using Mixer.Base.Model.User;
using MixItUp.Base;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Bot
{
    /// <summary>
    /// Interaction logic for BotControl.xaml
    /// </summary>
    public partial class BotControl : MainControlBase
    {
        private List<OAuthClientScopeEnum> BotScopes = new List<OAuthClientScopeEnum>()
        {
            OAuthClientScopeEnum.chat__bypass_links,
            OAuthClientScopeEnum.chat__bypass_slowchat,
            OAuthClientScopeEnum.chat__chat,
            OAuthClientScopeEnum.chat__connect,
            OAuthClientScopeEnum.chat__edit_options,
            OAuthClientScopeEnum.chat__giveaway_start,
            OAuthClientScopeEnum.chat__poll_start,
            OAuthClientScopeEnum.chat__poll_vote,
            OAuthClientScopeEnum.chat__whisper,

            OAuthClientScopeEnum.user__details__self,
        };

        public BotControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {            
            if (ChannelSession.Settings.BotOAuthToken != null)
            {
                bool result = await MixerAPIHandler.InitializeBotConnection(ChannelSession.Settings.BotOAuthToken);
                if (result)
                {
                    await this.InitializeBotSession();
                    this.ExistingBotGrid.Visibility = Visibility.Visible;
                }
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Visible;
            }

            await base.InitializeInternal();
        }

        private async void LogInBotButton_Click(object sender, RoutedEventArgs e)
        {
            string clientID = ConfigurationManager.AppSettings["ClientID"];
            if (string.IsNullOrEmpty(clientID))
            {
                throw new ArgumentException("ClientID value isn't set in application configuration");
            }

            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await MixerAPIHandler.InitializeBotConnection(clientID, this.BotScopes, (OAuthShortCodeModel shortCode) =>
                {
                    this.BotShortCodeTextBox.IsEnabled = true;
                    this.BotShortCodeTextBox.Text = shortCode.code;

                    Process.Start("https://mixer.com/oauth/shortcode?code=" + shortCode.code);
                });
            });

            if (!result)
            {
                MessageBoxHelper.ShowError("Unable to authenticate Bot with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                await this.InitializeBotSession();
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingBotGrid.Visibility = Visibility.Visible;
            }
        }

        private void LogOutBotButton_Click(object sender, RoutedEventArgs e)
        {
            MixerAPIHandler.CloseBotConnection();
            ChannelSession.Settings.BotOAuthToken = null;

            this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            this.NewBotLoginGrid.Visibility = Visibility.Visible;
        }

        private async Task<bool> InitializeBotSession()
        {
            return await this.Window.RunAsyncOperation(async () =>
            {
                PrivatePopulatedUserModel user = await MixerAPIHandler.BotConnection.Users.GetCurrentUser();
                if (user != null)
                {
                    ChannelSession.InitializeBot(user);
                    return true;
                }
                return false;
            });
        }
    }
}
