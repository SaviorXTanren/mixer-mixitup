using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows.Forms;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for DebugControl.xaml
    /// </summary>
    public partial class DebugControl : MainControlBase
    {
        public DebugControl()
        {
            InitializeComponent();
        }

        private async void TriggerGenericDonation_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID);
            if (user == null)
            {
                user = ChannelSession.User;
            }

            UserDonationModel donation = new UserDonationModel()
            {
                Source = UserDonationSourceEnum.Streamlabs,

                ID = Guid.NewGuid().ToString(),
                Username = user.Username,
                Message = "This is a donation message!",

                Amount = 12.34,

                DateTime = DateTimeOffset.Now,

                User = user
            };

            await EventService.ProcessDonationEvent(EventTypeEnum.StreamlabsDonation, donation);
        }

        private void TriggerTwitchTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            ServiceManager.Get<TwitchPubSubService>().PubSub_OnSubscribedReceived(this, new PubSubSubscriptionsEventModel()
            {
                user_name = twitchUser.Username,
                user_id = twitchUser.ID,
                display_name = twitchUser.DisplayName,

                channel_name = ServiceManager.Get<TwitchSessionService>().User.login,
                channel_id = ServiceManager.Get<TwitchSessionService>().User.id,

                time = DateTimeOffset.Now.ToString(),

                sub_plan = "1000",

                cumulative_months = 1,
                streak_months = 1,

                context = "sub",

                sub_message = new JObject()
            });
        }

        private void Twitch100BitsCheer_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            ServiceManager.Get<TwitchPubSubService>().PubSub_OnBitsV2Received(this, new PubSubBitsEventV2Model()
            {
                user_name = twitchUser.Username,
                user_id = twitchUser.ID,

                bits_used = 100,
                total_bits_used = 1234,

                channel_id = ServiceManager.Get<TwitchSessionService>().User.id,

                message_id = Guid.NewGuid().ToString(),
                chat_message = "This is a message",

                time = DateTimeOffset.Now.ToString(),
            });
        }
    }
}
