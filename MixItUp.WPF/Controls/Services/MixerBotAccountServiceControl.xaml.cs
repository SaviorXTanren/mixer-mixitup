using Mixer.Base.Model.TestStreams;
using MixItUp.Base;
using MixItUp.Base.Util;
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

            if (ChannelSession.Settings.MixerBotOAuthToken != null && ChannelSession.MixerBot != null)
            {
                this.ExistingBotGrid.Visibility = Visibility.Visible;
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;

                this.BotLoggedInNameTextBlock.Text = ChannelSession.MixerBot.username;
                if (!string.IsNullOrEmpty(ChannelSession.MixerBot.avatarUrl))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.BotProfileAvatar.SetMixerUserAvatarUrl(ChannelSession.MixerBot.id);
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
                TestStreamSettingsModel testStreamSettings = await ChannelSession.MixerUserConnection.GetTestStreamSettings(ChannelSession.MixerChannel);
                if (testStreamSettings != null && testStreamSettings.isActive.GetValueOrDefault())
                {
                    if (!await DialogHelper.ShowConfirmation("Because test stream settings are enabled, your bot account will not be able to connect correctly. You will need to disable Test Streams in order to use your bot account for things such a chat messages & whispers. Are you sure you still wish to connect it?"))
                    {
                        return false;
                    }
                }
                return (await ChannelSession.ConnectMixerBot()).Success;
            });

            if (!result)
            {
                await DialogHelper.ShowMessage("Unable to authenticate Bot with Mixer. Please ensure you approved access for the application in a timely manner.");
            }
            else if (ChannelSession.MixerBot.id.Equals(ChannelSession.MixerUser.id))
            {
                await this.groupBoxControl.window.RunAsyncOperation(async () =>
                {
                    await ChannelSession.DisconnectMixerBot();
                });
                ChannelSession.Settings.MixerBotOAuthToken = null;
                await DialogHelper.ShowMessage("You must sign in to a different account than your Streamer account.");
            }
            else
            {
                this.NewBotLoginGrid.Visibility = Visibility.Collapsed;
                this.ExistingBotGrid.Visibility = Visibility.Visible;

                this.BotLoggedInNameTextBlock.Text = ChannelSession.MixerBot.username;
                if (!string.IsNullOrEmpty(ChannelSession.MixerBot.avatarUrl))
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.BotProfileAvatar.SetMixerUserAvatarUrl(ChannelSession.MixerBot.id);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }

                this.SetCompletedIcon(visible: true);
            }
        }

        private async void LogOutBotButton_Click(object sender, RoutedEventArgs e)
        {
            await this.groupBoxControl.window.RunAsyncOperation(async () =>
            {
                await ChannelSession.DisconnectMixerBot();
            });
            ChannelSession.Settings.MixerBotOAuthToken = null;

            this.ExistingBotGrid.Visibility = Visibility.Collapsed;
            this.NewBotLoginGrid.Visibility = Visibility.Visible;

            this.SetCompletedIcon(visible: false);
        }
    }
}
