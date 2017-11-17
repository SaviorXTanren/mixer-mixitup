using Mixer.Base.Model.OAuth;
using MixItUp.Base;
using MixItUp.WPF.Util;
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
                if (!string.IsNullOrEmpty(ChannelSession.BotUser.avatarUrl))
                {
                    this.BotProfileAvatar.SetImageUrl(ChannelSession.BotUser.avatarUrl);
                }
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Visible;
            }

            if (ChannelSession.Settings.EnableOverlay)
            {
                await ChannelSession.Services.InitializeOverlayServer();

                this.EnableOverlayButton.Visibility = Visibility.Collapsed;
                this.DisableOverlayButton.Visibility = Visibility.Visible;
                this.TestOverlayButton.IsEnabled = true;
            }

            if (!string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP))
            {
                this.OBSStudioIPAddressTextBox.Text = ChannelSession.Settings.OBSStudioServerIP;
                this.OBSStudioPasswordTextBox.Password = ChannelSession.Settings.OBSStudioServerPassword;

                this.OBSStudioEnableConnectionButton.Visibility = Visibility.Collapsed;
                this.OBSStudioDisableConnectionButton.Visibility = Visibility.Visible;

                await ChannelSession.Services.InitializeOBSWebsocket();
            }
            else
            {
                this.OBSStudioIPAddressTextBox.Text = ChannelSession.DefaultOBSStudioConnection;
            }

            if (ChannelSession.Settings.EnableXSplitConnection)
            {
                this.EnableXSplitConnectionButton.Visibility = Visibility.Collapsed;
                this.DisableXSplitConnectionButton.Visibility = Visibility.Visible;
                this.TestXSplitConnectionButton.IsEnabled = true;

                await ChannelSession.Services.InitializeXSplitServer();
            }

            await base.InitializeInternal();
        }

        private async void LogInBotButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.Window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectBot((OAuthShortCodeModel shortCode) =>
                {
                    this.BotShortCodeTextBox.IsEnabled = true;
                    this.BotShortCodeTextBox.Text = shortCode.code;

                    Process.Start("https://mixer.com/oauth/shortcode?approval_prompt=force&code=" + shortCode.code);
                });
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate Bot with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingBotGrid.Visibility = Visibility.Visible;

                this.BotLoggedInNameTextBlock.Text = ChannelSession.BotUser.username;
                if (!string.IsNullOrEmpty(ChannelSession.BotUser.avatarUrl))
                {
                    this.BotProfileAvatar.SetImageUrl(ChannelSession.BotUser.avatarUrl);
                }
            }
        }

        private async void LogOutBotButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.DisconnectBot();
            });
            ChannelSession.Settings.BotOAuthToken = null;

            this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            this.NewBotLoginGrid.Visibility = Visibility.Visible;
        }

        private async void EnableOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.InitializeOverlayServer();

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
                await ChannelSession.Services.DisconnectOverlayServer();

                ChannelSession.Settings.EnableOverlay = false;
                await ChannelSession.Settings.Save();

                this.EnableOverlayButton.Visibility = Visibility.Visible;
                this.DisableOverlayButton.Visibility = Visibility.Collapsed;
                this.TestOverlayButton.IsEnabled = false;
            });
        }

        private void TestOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Services.OverlayServer.TestConnection();
        }

        private async void OBSStudioEnableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.OBSStudioServerIP = this.OBSStudioIPAddressTextBox.Text;
            ChannelSession.Settings.OBSStudioServerPassword = this.OBSStudioPasswordTextBox.Password;

            await this.Window.RunAsyncOperation(async () =>
            {
                if (await ChannelSession.Services.InitializeOBSWebsocket())
                {
                    this.OBSStudioEnableConnectionButton.Visibility = Visibility.Collapsed;
                    this.OBSStudioDisableConnectionButton.Visibility = Visibility.Visible;
                }
                else
                {
                    ChannelSession.Settings.OBSStudioServerIP = null;
                    ChannelSession.Settings.OBSStudioServerPassword = null;

                    await MessageBoxHelper.ShowMessageDialog("Could not connect to OBS Studio. Please make sure OBS Studio is running, the obs-websocket plugin is installed, and the connection and password match your settings in OBS Studio");
                }
            });
        }

        private async void OBSStudioDisableConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await ChannelSession.Services.DisconnectOBSStudio();

            ChannelSession.Settings.OBSStudioServerIP = null;
            ChannelSession.Settings.OBSStudioServerPassword = null;

            this.OBSStudioEnableConnectionButton.Visibility = Visibility.Visible;
            this.OBSStudioDisableConnectionButton.Visibility = Visibility.Collapsed;
        }

        private async void EnableXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                ChannelSession.Settings.EnableXSplitConnection = true;
                this.EnableXSplitConnectionButton.Visibility = Visibility.Collapsed;
                this.DisableXSplitConnectionButton.Visibility = Visibility.Visible;
                this.TestXSplitConnectionButton.IsEnabled = true;

                await ChannelSession.Services.InitializeXSplitServer();

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

                await ChannelSession.Services.DisconnectXSplitServer();

                await ChannelSession.Settings.Save();
            });
        }

        private async void TestXSplitConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChannelSession.Services.XSplitServer != null)
            {
                await this.Window.RunAsyncOperation(async () =>
                {
                    if (await ChannelSession.Services.XSplitServer.TestConnection())
                    {
                        await MessageBoxHelper.ShowMessageDialog("Connection successful!");
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("Could not connect to XSplit. Please make sure XSplit is running, the Mix It Up plugin is installed, and is running");
                    }
                });
            }
        }
    }
}
