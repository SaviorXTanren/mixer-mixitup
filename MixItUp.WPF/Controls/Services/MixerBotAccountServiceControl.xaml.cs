using MixItUp.Base;
using MixItUp.WPF.Util;
using System.Threading.Tasks;
using System.Windows;

namespace MixItUp.WPF.Controls.Services
{
    /// <summary>
    /// Interaction logic for MixerBotAccountServiceControl.xaml
    /// </summary>
    public partial class MixerBotAccountServiceControl : ServicesControlBase
    {
        public MixerBotAccountServiceControl()
        {
            InitializeComponent();
        }

        protected override Task OnLoaded()
        {
            this.SetHeaderText("Mixer Bot Account");

            if (ChannelSession.Settings.BotOAuthToken != null)
            {
                this.ExistingBotGrid.Visibility = Visibility.Visible;
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;

                this.BotLoggedInNameTextBlock.Text = ChannelSession.BotUser.username;
                if (!string.IsNullOrEmpty(ChannelSession.BotUser.avatarUrl))
                {
                    this.BotProfileAvatar.SetImageUrl(ChannelSession.BotUser.avatarUrl);
                }

                this.SetCompletedIcon(visible: true);
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Visible;
                this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            }

            return base.OnLoaded();
        }

        private async void LogInBotButton_Click(object sender, RoutedEventArgs e)
        {
            bool result = await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                return await ChannelSession.ConnectBot();
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

                this.SetCompletedIcon(visible: true);
            }
        }

        private async void LogOutBotButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.DisconnectBot();
            });
            ChannelSession.Settings.BotOAuthToken = null;

            this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            this.NewBotLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
