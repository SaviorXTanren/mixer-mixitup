using MixItUp.Base;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Twitch.Clients.Chat;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private void Twitch5GiftedTier1Sub_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUsers().FirstOrDefault(u => u.ID != ChannelSession.User.ID && u.HasPlatformData(Base.Model.StreamingPlatformTypeEnum.Twitch));
            if (user == null)
            {
                user = ChannelSession.User;
            }

            TwitchUserPlatformV2Model twitchUser = user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch);

            for (int i = 0; i < 5; i++)
            {
                ServiceManager.Get<TwitchPubSubService>().PubSub_OnSubscriptionsGiftedReceived(this, new PubSubSubscriptionsGiftEventModel()
                {
                    user_name = twitchUser.Username,
                    user_id = twitchUser.ID,
                    display_name = twitchUser.DisplayName,

                    recipient_user_name = twitchUser.Username,
                    recipient_id = twitchUser.ID,
                    recipient_display_name = twitchUser.DisplayName,

                    channel_name = ServiceManager.Get<TwitchSessionService>().User.login,
                    channel_id = ServiceManager.Get<TwitchSessionService>().User.id,

                    time = DateTimeOffset.Now.ToString(),

                    sub_plan = "1000",

                    cumulative_months = 1,
                    streak_months = 1,

                    context = "subgift",

                    sub_message = new JObject()
                });
            }

            ChatRawPacketModel chatRawPacket = new ChatRawPacketModel()
            {
                RawText = "@badge-info=subscriber/1;badges=subscriber/0;color=#00FF7F;display-name=" + twitchUser.DisplayName + ";emotes=;flags=;id=44cb7c41-ecf4-435c-98bd-1e2a891df7af;login=" + twitchUser.Username + ";mod=0;msg-id=submysterygift;msg-param-mass-gift-count=5;msg-param-origin-id=89\\\\s82\\\\s5f\\\\s60\\\\s08\\\\s2e\\\\s6d\\\\sc2\\\\s76\\\\s90\\\\s45\\\\sce\\\\s97\\\\s71\\\\s8f\\\\s25\\\\s52\\\\sac\\\\sd2\\\\sf8;msg-param-sender-count=5;msg-param-sub-plan=1000;room-id=" + ServiceManager.Get<TwitchSessionService>().User.id + ";subscriber=1;system-msg=" + twitchUser.DisplayName + "\\\\sis\\\\sgifting\\\\s5\\\\sTier\\\\s1\\\\sSubs\\\\sto\\\\s" + ServiceManager.Get<TwitchSessionService>().User.display_name + "'s\\\\scommunity!\\\\sThey've\\\\sgifted\\\\sa\\\\stotal\\\\sof\\\\s5\\\\sin\\\\sthe\\\\schannel!;tmi-sent-ts=1594506772399;user-id=" + twitchUser.ID + ";user-type= :tmi.twitch.tv USERNOTICE #" + ServiceManager.Get<TwitchSessionService>().User.login,
                Prefix = "tmi.twitch.tv",
                Command = "USERNOTICE",
                Parameters = new List<string>() { "#mixitupapp" },
                Tags = new Dictionary<string, string>()
                {
                    { "badge-info", "subscriber/1" },
                    { "badges", "subscriber/0" },
                    { "color", "#00FF7F" },
                    { "display-name", twitchUser.DisplayName },
                    { "emotes", "" },
                    { "flags", "" },
                    { "id", "44cb7c41-ecf4-435c-98bd-1e2a891df7af" },
                    { "login", twitchUser.Username },
                    { "mod", "0" },
                    { "msg-id", "submysterygift" },
                    { "msg-param-mass-gift-count", "5" },
                    { "msg-param-origin-id", "89 s82 s5f s60 s08 s2e s6d sc2 s76 s90 s45 sce s97 s71 s8f s25 s52 sac sd2 sf8" },
                    { "msg-param-sender-count", "5" },
                    { "msg-param-sub-plan", "1000" },
                    { "room-id", ServiceManager.Get<TwitchSessionService>().User.id },
                    { "subscriber", "1" },
                    { "system-msg", twitchUser.DisplayName + " sis sgifting s5 sTier s1 sSubs sto s" + ServiceManager.Get<TwitchSessionService>().User.display_name + "'s scommunity! sThey've sgifted sa stotal sof s5 sin sthe schannel!" },
                    { "tmi-sent-ts", "1594506772399" },
                    { "user-id", twitchUser.ID },
                    { "user-type", "" }
                }
            };
            ServiceManager.Get<TwitchChatService>().UserClient_OnUserNoticeReceived(this, new ChatUserNoticePacketModel(chatRawPacket));
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
