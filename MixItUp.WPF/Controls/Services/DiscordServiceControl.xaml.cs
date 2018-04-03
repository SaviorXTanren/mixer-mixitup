using MixItUp.Base;
using MixItUp.Desktop.Services;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for DiscordServiceControl.xaml
    /// </summary>
    public partial class DiscordServiceControl : ServicesControlBase
    {
        public DiscordServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Discord");

            if (ChannelSession.Settings.DiscordOAuthToken != null)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
            }

            return base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeDiscord();
            });

            if (!result)
            {
                await MessageBoxHelper.ShowMessageDialog("Unable to authenticate with Discord. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (ChannelSession.Services.Discord.User != null && ChannelSession.Services.Discord.Server != null &&
                        DiscordService.ClientBotPermissions.Equals(ChannelSession.Services.Discord.BotPermissions))
                    {
                        this.NewLoginGrid.Visibility = Visibility.Collapsed;
                        this.ExistingAccountGrid.Visibility = Visibility.Visible;

                        this.SetCompletedIcon(visible: true);
                    }
                    else
                    {
                        await MessageBoxHelper.ShowMessageDialog("We were unable to complete the authentication with Discord. Please ensure you do not change any options on the approval webpage and correctly select the Discord server you would like us to connect to.");
                        await ChannelSession.Services.DisconnectDiscord();
                    }
                });
            }
        }

        private async void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.Services.DisconnectDiscord();
            });
            ChannelSession.Settings.DiscordOAuthToken = null;

            this.ExistingAccountGrid.Visibility = Visibility.Collapsed;
            this.NewLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
