using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User.Twitch;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twitch.Base.Clients;
using Twitch.Base.Models.Clients.PubSub;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public interface ITwitchEventService
    {
        bool IsConnected { get; }

        Task<Result> Connect();
        Task Disconnect();
    }

    public class TwitchEventService : StreamingPlatformServiceBase, ITwitchEventService
    {
        public static string GetSubTierFromText(string subPlan)
        {
            if (!string.IsNullOrEmpty(subPlan) && int.TryParse(subPlan, out int subPlanNumber) && subPlanNumber >= 1000)
            {
                subPlanNumber = subPlanNumber / 1000;
                return $"{MixItUp.Base.Resources.Tier} {subPlanNumber}";
            }
            else
            {
                return subPlan;
            }
        }

        private static readonly List<PubSubTopicsEnum> topicTypes = new List<PubSubTopicsEnum>()
        {
            PubSubTopicsEnum.ChannelBitsEventsV2,
            PubSubTopicsEnum.ChannelBitsBadgeUnlocks,
            PubSubTopicsEnum.ChannelSubscriptionsV1,
            PubSubTopicsEnum.UserWhispers,
            PubSubTopicsEnum.ChannelPointsRedeemed
        };

        private PubSubClient pubSub;

        private CancellationTokenSource cancellationTokenSource;

        private HashSet<string> follows = new HashSet<string>();

        private DateTimeOffset streamStartCheckTime = DateTimeOffset.Now;

        public override string Name { get { return "Twitch Events"; } }

        public bool IsConnected { get; private set; }

        public TwitchEventService() { }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;
            if (ChannelSession.TwitchUserConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.pubSub = new PubSubClient(ChannelSession.TwitchUserConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.pubSub.OnSentOccurred += PubSub_OnSentOccurred;
                            this.pubSub.OnTextReceivedOccurred += PubSub_OnTextReceivedOccurred;
                            this.pubSub.OnMessageReceived += PubSub_OnMessageReceived;
                        }
                        this.pubSub.OnReconnectReceived += PubSub_OnReconnectReceived;
                        this.pubSub.OnDisconnectOccurred += PubSub_OnDisconnectOccurred;
                        this.pubSub.OnPongReceived += PubSub_OnPongReceived;
                        this.pubSub.OnResponseReceived += PubSub_OnResponseReceived;

                        this.pubSub.OnWhisperReceived += PubSub_OnWhisperReceived;
                        this.pubSub.OnBitsV2Received += PubSub_OnBitsV2Received;
                        this.pubSub.OnBitsBadgeReceived += PubSub_OnBitsBadgeReceived;
                        this.pubSub.OnSubscribedReceived += PubSub_OnSubscribedReceived;
                        this.pubSub.OnSubscriptionsGiftedReceived += PubSub_OnSubscriptionsGiftedReceived;
                        this.pubSub.OnChannelPointsRedeemed += PubSub_OnChannelPointsRedeemed;

                        await this.pubSub.Connect();

                        await Task.Delay(1000);

                        List<PubSubListenTopicModel> topics = new List<PubSubListenTopicModel>();
                        foreach (PubSubTopicsEnum topic in TwitchEventService.topicTypes)
                        {
                            topics.Add(new PubSubListenTopicModel(topic, (string)ChannelSession.TwitchUserNewAPI.id));
                        }

                        await this.pubSub.Listen(topics);

                        await Task.Delay(1000);

                        await this.pubSub.Ping();

                        this.cancellationTokenSource = new CancellationTokenSource();

                        follows.Clear();
                        IEnumerable<UserFollowModel> followers = await ChannelSession.TwitchUserConnection.GetNewAPIFollowers((UserModel)ChannelSession.TwitchUserNewAPI, maxResult: 100);
                        foreach (UserFollowModel follow in followers)
                        {
                            follows.Add(follow.from_id);
                        }

                        AsyncRunner.RunBackgroundTask(this.cancellationTokenSource.Token, 60000, this.BackgroundEventChecks);

                        this.IsConnected = true;

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                }));
            }
            return new Result("Twitch connection has not been established");
        }

        public async Task Disconnect()
        {
            try
            {
                if (this.pubSub != null)
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        this.pubSub.OnSentOccurred -= PubSub_OnSentOccurred;
                        this.pubSub.OnTextReceivedOccurred -= PubSub_OnTextReceivedOccurred;
                        this.pubSub.OnMessageReceived -= PubSub_OnMessageReceived;
                    }
                    this.pubSub.OnReconnectReceived -= PubSub_OnReconnectReceived;
                    this.pubSub.OnDisconnectOccurred -= PubSub_OnDisconnectOccurred;
                    this.pubSub.OnPongReceived -= PubSub_OnPongReceived;
                    this.pubSub.OnResponseReceived -= PubSub_OnResponseReceived;

                    this.pubSub.OnWhisperReceived -= PubSub_OnWhisperReceived;
                    this.pubSub.OnBitsV2Received -= PubSub_OnBitsV2Received;
                    this.pubSub.OnBitsBadgeReceived -= PubSub_OnBitsBadgeReceived;
                    this.pubSub.OnSubscribedReceived -= PubSub_OnSubscribedReceived;
                    this.pubSub.OnSubscriptionsGiftedReceived -= PubSub_OnSubscriptionsGiftedReceived;
                    this.pubSub.OnChannelPointsRedeemed -= PubSub_OnChannelPointsRedeemed;

                    await this.pubSub.Disconnect();
                }

                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.IsConnected = false;
            this.pubSub = null;
        }

        private async Task BackgroundEventChecks(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (streamStartCheckTime != DateTimeOffset.MaxValue)
                {
                    DateTimeOffset startTime = await UptimeChatCommand.GetStartTime();
                    if (startTime != DateTimeOffset.MinValue && startTime > streamStartCheckTime)
                    {
                        Logger.Log(LogLevel.Debug, "Stream start detected");

                        streamStartCheckTime = DateTimeOffset.MaxValue;
                        EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelStreamStart, ChannelSession.GetCurrentUser());
                        if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                        {
                            await ChannelSession.Services.Events.PerformEvent(trigger);
                        }
                    }
                }

                foreach (UserFollowModel follow in await ChannelSession.TwitchUserConnection.GetNewAPIFollowers(ChannelSession.TwitchUserNewAPI, maxResult: 100))
                {
                    if (!follows.Contains(follow.from_id))
                    {
                        follows.Add(follow.from_id);

                        UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(follow.from_id);
                        if (user == null)
                        {
                            user = new UserViewModel(follow);
                        }

                        EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelFollowed, user);
                        if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                        {
                            user.FollowDate = DateTimeOffset.Now;

                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestFollowerUserData] = user.ID;

                            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                            {
                                currency.AddAmount(user.Data, currency.OnFollowBonus);
                            }

                            await ChannelSession.Services.Events.PerformEvent(trigger);

                            GlobalEvents.FollowOccurred(user);
                            await this.AddAlertChatMessage(user, string.Format("{0} Followed", user.Username));
                        }
                    }
                }
            }
        }

        private async void PubSub_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Twitch PubSub");

            Result result;
            await this.Disconnect();
            do
            {
                await Task.Delay(2500);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Twitch PubSub");
        }

        private void PubSub_OnReconnectReceived(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Twitch PubSub");
        }

        private void PubSub_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, "PUB SUB SEND: " + packet);
        }

        private void PubSub_OnTextReceivedOccurred(object sender, string text)
        {
            Logger.Log(LogLevel.Debug, "PUB SUB TEXT: " + text);
        }

        private void PubSub_OnMessageReceived(object sender, PubSubMessagePacketModel packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("PUB SUB MESSAGE: {0} {1} ", packet.type, packet.message));

            Logger.Log(LogLevel.Debug, JSONSerializerHelper.SerializeToString(packet));
        }

        private void PubSub_OnResponseReceived(object sender, PubSubResponsePacketModel packet)
        {
            Logger.Log("PUB SUB RESPONSE: " + packet.error);
        }

        private async void PubSub_OnBitsV2Received(object sender, PubSubBitsEventV2Model packet)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (user == null)
            {
                user = new UserViewModel(packet);
            }

            TwitchUserBitsCheeredModel bitsCheered = new TwitchUserBitsCheeredModel(user, packet);

            foreach (CurrencyModel bitsCurrency in ChannelSession.Settings.Currency.Values.Where(c => c.SpecialTracking == CurrencySpecialTrackingEnum.Bits))
            {
                bitsCurrency.AddAmount(user.Data, bitsCheered.Amount);
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                streamPass.AddAmount(user.Data, (int)Math.Ceiling(streamPass.BitsBonus * bitsCheered.Amount));
            }

            user.Data.TotalBitsCheered += (uint)bitsCheered.Amount;

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredUserData] = user.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredAmountData] = bitsCheered.Amount;

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelBitsCheered, user);
            trigger.SpecialIdentifiers["bitsamount"] = bitsCheered.Amount.ToString();
            trigger.SpecialIdentifiers["message"] = bitsCheered.Message;
            await ChannelSession.Services.Events.PerformEvent(trigger);

            await this.AddAlertChatMessage(user, string.Format("{0} Cheered {1} Bits", user.Username, bitsCheered.Amount));

            GlobalEvents.BitsOccurred(bitsCheered);
        }

        private void PubSub_OnBitsBadgeReceived(object sender, PubSubBitBadgeEventModel packet)
        {

        }

        private async void PubSub_OnSubscribedReceived(object sender, PubSubSubscriptionsEventModel packet)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (user == null)
            {
                user = new UserViewModel(packet);
            }

            if (packet.IsSubscription || packet.cumulative_months == 1)
            {
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscribed, user);
                if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                {
                    trigger.SpecialIdentifiers["message"] = (packet.sub_message.ContainsKey("message")) ? packet.sub_message["message"].ToString() : string.Empty;
                    trigger.SpecialIdentifiers["usersubplanname"] = packet.sub_plan_name;
                    trigger.SpecialIdentifiers["usersubplan"] = TwitchEventService.GetSubTierFromText(packet.sub_plan);

                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                    user.Data.TwitchSubscribeDate = DateTimeOffset.Now;
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                    }
                    user.Data.TotalMonthsSubbed++;

                    await ChannelSession.Services.Events.PerformEvent(trigger);
                }

                GlobalEvents.SubscribeOccurred(user);

                await this.AddAlertChatMessage(user, string.Format("{0} Subscribed", user.Username));
            }
            else
            {
                int months = Math.Max(packet.streak_months, packet.cumulative_months);
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelResubscribed, user);
                if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                {
                    trigger.SpecialIdentifiers["message"] = (packet.sub_message.ContainsKey("message")) ? packet.sub_message["message"].ToString() : string.Empty;
                    trigger.SpecialIdentifiers["usersubmonths"] = months.ToString();
                    trigger.SpecialIdentifiers["usersubplanname"] = packet.sub_plan_name;
                    trigger.SpecialIdentifiers["usersubplan"] = TwitchEventService.GetSubTierFromText(packet.sub_plan);

                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                    user.Data.TwitchSubscribeDate = DateTimeOffset.Now.SubtractMonths(months - 1);
                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                    }
                    user.Data.TotalMonthsSubbed++;

                    await ChannelSession.Services.Events.PerformEvent(trigger);
                }

                GlobalEvents.ResubscribeOccurred(new Tuple<UserViewModel, int>(user, months));

                await this.AddAlertChatMessage(user, string.Format("{0} Re-Subscribed For {1} Months", user.Username, months));
            }
        }

        private async void PubSub_OnSubscriptionsGiftedReceived(object sender, PubSubSubscriptionsGiftEventModel packet)
        {
            UserViewModel gifter = packet.IsAnonymousGiftedSubscription ? new UserViewModel("An Anonymous Gifter") : ChannelSession.Services.User.GetUserByTwitchID(packet.user_id);
            if (gifter == null)
            {
                gifter = new UserViewModel(packet);
            }

            UserViewModel receiver = ChannelSession.Services.User.GetUserByTwitchID(packet.recipient_id);
            if (receiver == null)
            {
                receiver = new UserViewModel(new UserModel()
                {
                    id = packet.recipient_id,
                    login = packet.recipient_user_name,
                    display_name = packet.recipient_display_name
                });
            }

            uint monthsGifted = packet.IsMultiMonth ? (uint)packet.multi_month_duration : 1;

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = receiver.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = monthsGifted;

            receiver.Data.TwitchSubscribeDate = DateTimeOffset.Now;
            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                for (int i = 0; i < monthsGifted; i++)
                {
                    currency.AddAmount(gifter.Data, currency.OnSubscribeBonus);
                }
            }
            gifter.Data.TotalSubsGifted += monthsGifted;
            receiver.Data.TotalSubsReceived += monthsGifted;
            receiver.Data.TotalMonthsSubbed += monthsGifted;

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscriptionGifted, gifter);
            trigger.SpecialIdentifiers["usersubplanname"] = packet.sub_plan_name;
            trigger.SpecialIdentifiers["usersubplan"] = TwitchEventService.GetSubTierFromText(packet.sub_plan);
            trigger.SpecialIdentifiers["usersubmonthsgifted"] = monthsGifted.ToString();
            trigger.SpecialIdentifiers["isanonymous"] = packet.IsAnonymousGiftedSubscription.ToString();
            trigger.Arguments.Add(receiver.Username);
            await ChannelSession.Services.Events.PerformEvent(trigger);

            await this.AddAlertChatMessage(gifter, string.Format("{0} Gifted A Subscription To {1}", gifter.Username, receiver.Username));

            GlobalEvents.SubscriptionGiftedOccurred(gifter, receiver);
        }

        private async void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointsRedemptionEventModel packet)
        {
            PubSubChannelPointsRedeemedEventModel redemption = packet.redemption;

            UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(redemption.user.id);
            if (user == null)
            {
                user = new UserViewModel(redemption.user);
            }

            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers["rewardname"] = redemption.reward.title;
            specialIdentifiers["rewardcost"] = redemption.reward.cost.ToString();
            specialIdentifiers["message"] = redemption.user_input;

            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelPointsRedeemed, user);
            foreach (var kvp in specialIdentifiers)
            {
                trigger.SpecialIdentifiers[kvp.Key] = kvp.Value;
            }

            await ChannelSession.Services.Events.PerformEvent(trigger);

            TwitchChannelPointsCommand command = ChannelSession.Settings.TwitchChannelPointsCommands.FirstOrDefault(c => string.Equals(c.Name, redemption.reward.title, StringComparison.CurrentCultureIgnoreCase));
            if (command != null)
            {
                await command.Perform(user, extraSpecialIdentifiers: specialIdentifiers);
            }

            await this.AddAlertChatMessage(user, string.Format("{0} Redeemed {1}", user.Username, redemption.reward.title));
        }

        private async void PubSub_OnWhisperReceived(object sender, PubSubWhisperEventModel packet)
        {
            if (!string.IsNullOrEmpty(packet.body))
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByTwitchID(packet.from_id.ToString());
                await ChannelSession.Services.Chat.AddMessage(new TwitchChatMessageViewModel(packet, user));
            }
        }

        private async Task AddAlertChatMessage(UserViewModel user, string message)
        {
            if (ChannelSession.Settings.ChatShowEventAlerts)
            {
                await ChannelSession.Services.Chat.AddMessage(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, message, ChannelSession.Settings.ChatEventAlertsColorScheme));
            }
        }

        private void PubSub_OnPongReceived(object sender, EventArgs e)
        {
            Logger.Log(LogLevel.Debug, "Twitch Pong Received");
            Task.Run(async () =>
            {
                await Task.Delay(1000 * 60 * 3);
                await this.pubSub.Ping();
            });
        }
    }
}
