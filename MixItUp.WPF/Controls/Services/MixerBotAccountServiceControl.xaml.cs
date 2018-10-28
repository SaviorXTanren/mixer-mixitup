using Mixer.Base.Model.TestStreams;
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.BotProfileAvatar.SetUserAvatarUrl(ChannelSession.BotUser.id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
                TestStreamSettingsModel testStreamSettings = await ChannelSession.Connection.GetTestStreamSettings(ChannelSession.Channel);
                if (testStreamSettings != null && testStreamSettings.isActive.GetValueOrDefault())
                {
                    if (!await MessageBoxHelper.ShowConfirmationDialog("Because test stream settings are enabled, your bot account will not be able to connect correctly. You will need to disable Test Streams in order to use your bot account for things such a chat messages & whispers. Are you sure you still wish to connect it?"))
                    {
                        return false;
                    }
                }
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.BotProfileAvatar.SetUserAvatarUrl(ChannelSession.BotUser.id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
