using MixItUp.Base;
using MixItUp.Base.Util;
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

            if (ChannelSession.Services.Discord != null && ChannelSession.Services.Discord.IsConnected)
            {
                this.ExistingAccountGrid.Visibility = Visibility.Visible;
                this.CustomApplicationToggleButtonGrid.Visibility = Visibility.Collapsed;
                this.CustomApplicationGrid.Visibility = Visibility.Collapsed;

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewLoginGrid.Visibility = Visibility.Visible;
                this.CustomApplicationToggleButtonGrid.Visibility = Visibility.Visible;
            }

            this.UseCustomApplicationToggleButton.IsChecked = !string.IsNullOrEmpty(ChannelSession.Settings.DiscordCustomClientID);
            if (this.UseCustomApplicationToggleButton.IsChecked.GetValueOrDefault() && this.CustomApplicationToggleButtonGrid.Visibility == Visibility.Visible)
            {
                this.CustomClientIDTextBox.Text = ChannelSession.Settings.DiscordCustomClientID;
                this.CustomClientSecretTextBox.Text = ChannelSession.Settings.DiscordCustomClientSecret;
                this.CustomBotTokenTextBox.Text = ChannelSession.Settings.DiscordCustomBotToken;
            }

            return base.OnLoaded();
        }

        private async void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            ChannelSession.Settings.DiscordCustomClientID = null;
            ChannelSession.Settings.DiscordCustomClientSecret = null;
            ChannelSession.Settings.DiscordCustomBotToken = null;

            if (this.UseCustomApplicationToggleButton.IsChecked.GetValueOrDefault())
            {
                if (string.IsNullOrEmpty(this.CustomClientIDTextBox.Text) || string.IsNullOrEmpty(this.CustomClientSecretTextBox.Text) || string.IsNullOrEmpty(this.CustomBotTokenTextBox.Text))
                {
                    await DialogHelper.ShowMessage("A Client ID, Client Secret, and Bot Token must be specified if using a custom Discord application.");
                    return;
                }

                ChannelSession.Settings.DiscordCustomClientID = this.CustomClientIDTextBox.Text;
                ChannelSession.Settings.DiscordCustomClientSecret = this.CustomClientSecretTextBox.Text;
                ChannelSession.Settings.DiscordCustomBotToken = this.CustomBotTokenTextBox.Text;
            }

            bool result = await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.Services.InitializeDiscord();
            });

            if (!result)
            {
                await DialogHelper.ShowMessage("Unable to authenticate with Discord. Please ensure you approved access for the application in a timely manner.");
            }
            else
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    if (ChannelSession.Services.Discord.User != null && ChannelSession.Services.Discord.Server != null &&
                        DiscordService.ClientBotPermissions.Equals(ChannelSession.Services.Discord.BotPermissions))
                    {
                        this.NewLoginGrid.Visibility = Visibility.Collapsed;
                        this.CustomApplicationToggleButtonGrid.Visibility = Visibility.Collapsed;
                        this.CustomApplicationGrid.Visibility = Visibility.Collapsed;
                        this.ExistingAccountGrid.Visibility = Visibility.Visible;

                        this.SetCompletedIcon(visible: true);
                    }
                    else
                    {
                        await DialogHelper.ShowMessage("We were unable to complete the authentication with Discord. Please ensure you do not change any options on the approval webpage and correctly select the Discord server you would like us to connect to.");
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

            this.UseCustomApplicationToggleButton.IsChecked = false;
            this.CustomApplicationToggleButtonGrid.Visibility = Visibility.Collapsed;

            this.SetCompletedIcon(visible: false);
        }

        private void UseCustomApplicationToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            this.CustomApplicationGrid.Visibility = (this.UseCustomApplicationToggleButton.IsChecked.GetValueOrDefault()) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
