using Mixer.Base;
using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.Base.Overlay;
using MixItUp.WPF.Util;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for ServicesControl.xaml
    /// </summary>
    public partial class ServicesControl : MainControlBase
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

        public ServicesControl()
        {
            InitializeComponent();
        }

        protected override async Task InitializeInternal()
        {            
            if (ChannelSession.Settings.BotOAuthToken != null)
            {
                this.ExistingBotGrid.Visibility = Visibility.Visible;
                this.BotLoggedInNameTextBlock.Text = ChannelSession.BotUser.username;
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Visible;
            }

            if (ChannelSession.Settings.EnableOverlay)
            {
                ChannelSession.InitializeOverlayServer();

                this.EnableOverlayButton.Visibility = Visibility.Collapsed;
                this.DisableOverlayButton.Visibility = Visibility.Visible;
                this.TestOverlayButton.IsEnabled = true;
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
            {
                this.OBSStudioIPAddressTextBox.Text = ChannelSession.Settings.OBSStudioServerIP;

                await ChannelSession.InitializeOBSWebsocket();
            }
            this.OBSStudioPasswordTextBox.Password = ChannelSession.Settings.OBSStudioServerPassword;

            if (ChannelSession.Settings.EnableXSplitConnection)
            {
                this.EnableXSplitConnectionButton.Visibility = Visibility.Collapsed;
                this.DisableXSplitConnectionButton.Visibility = Visibility.Visible;

                ChannelSession.InitializeXSplitServer();
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
                return await ChannelSession.InitializeBot(clientID, this.BotScopes, (OAuthShortCodeModel shortCode) =>
                {
                    this.BotShortCodeTextBox.IsEnabled = true;
                    this.BotShortCodeTextBox.Text = shortCode.code;

                    Process.Start("https://mixer.com/oauth/shortcode?code=" + shortCode.code);
                });
            });

            if (!result)
            {
                MessageBoxHelper.ShowDialog("Unable to authenticate Bot with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingBotGrid.Visibility = Visibility.Visible;
                this.BotLoggedInNameTextBlock.Text = ChannelSession.BotUser.username;
            }
        }

        private void LogOutBotButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.DisconnectBot();
            ChannelSession.Settings.BotOAuthToken = null;

            this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            this.NewBotLoginGrid.Visibility = Visibility.Visible;
        }

        private async void EnableOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.InitializeOverlayServer();

                ChannelSession.Settings.EnableOverlay = true;
                await ChannelSession.Settings.Save();

                this.EnableOverlayButton.Visibility = Visibility.Collapsed;
                this.DisableOverlayButton.Visibility = Visibility.Visible;
                this.TestOverlayButton.IsEnabled = true;
            });
        }

        private async void DisableOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.DisconnectOverlayServer();

                ChannelSession.Settings.EnableOverlay = false;
                await ChannelSession.Settings.Save();

                this.EnableOverlayButton.Visibility = Visibility.Visible;
                this.DisableOverlayButton.Visibility = Visibility.Collapsed;
                this.TestOverlayButton.IsEnabled = false;
            });
        }

        private void TestOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.OverlayServer.SetText(new OverlayText() { text = "Connection Test", duration = 5, horizontal = 50, vertical = 50 });
        }

        private void OBSStudioIPAddressTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.OBSStudioServerIP = this.OBSStudioIPAddressTextBox.Text;
            ChannelSession.Settings.OBSStudioServerPassword = this.OBSStudioPasswordTextBox.Password;
        }

        private async void OBSStudioTestConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.OBSStudioIPAddressTextBox.Text))
            {
                ChannelSession.DisconnectOBSStudio();

                ChannelSession.Settings.OBSStudioServerIP = this.OBSStudioIPAddressTextBox.Text;
                ChannelSession.Settings.OBSStudioServerPassword = this.OBSStudioPasswordTextBox.Password;

                await this.Window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.InitializeOBSWebsocket())
                    {
                        MessageBoxHelper.ShowDialog("Connection successful!");
                    }
                    else
                    {
                        MessageBoxHelper.ShowDialog("Could not connect to OBS Studio. Please make sure OBS Studio is running, the obs-websocket plugin is installed, and the connection and password match your settings in OBS Studio");
                    }
                });
            }
        }

        private async void EnableXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.EnableXSplitConnection = true;
                this.EnableXSplitConnectionButton.Visibility = Visibility.Collapsed;
                this.DisableXSplitConnectionButton.Visibility = Visibility.Visible;
                this.TestXSplitConnectionButton.IsEnabled = true;

                ChannelSession.InitializeXSplitServer();

                await ChannelSession.Settings.Save();
            });
        }

        private async void DisableXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.EnableXSplitConnection = false;
                this.EnableXSplitConnectionButton.Visibility = Visibility.Visible;
                this.DisableXSplitConnectionButton.Visibility = Visibility.Collapsed;
                this.TestXSplitConnectionButton.IsEnabled = false;

                ChannelSession.DisconnectXSplitServer();

                await ChannelSession.Settings.Save();
            });
        }

        private async void TestXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.XSplitServer != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.XSplitServer.TestConnection())
                    {
                        MessageBoxHelper.ShowDialog("Connection successful!");
                    }
                    else
                    {
                        MessageBoxHelper.ShowDialog("Could not connect to XSplit. Please make sure XSplit is running, the Mix It Up plugin is installed, and is running");
                    }
                });
            }
        }
    }
}
