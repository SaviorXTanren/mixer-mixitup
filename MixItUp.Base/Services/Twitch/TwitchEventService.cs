using MixItUp.Base.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
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
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public class TwitchSubEventModel
    {
        public UserViewModel User { get; set; }

        public string PlanTier { get; set; }

        public int PlanTierNumber { get; set; }

        public string PlanName { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsGiftedUpgrade { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchSubEventModel(UserViewModel user, PubSubSubscriptionsEventModel packet)
        {
            this.User = user;
            this.PlanTier = TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
            this.PlanTierNumber = TwitchEventService.GetSubTierNumberFromText(packet.sub_plan);
            this.PlanName = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
            if (packet.sub_message.ContainsKey("message"))
            {
                this.Message = packet.sub_message["message"].ToString();
            }
        }

        public TwitchSubEventModel(ChatUserNoticePacketModel userNotice)
        {
            this.User = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, userNotice.UserID.ToString());
            if (this.User == null)
            {
                this.User = new UserViewModel(userNotice);
            }
            this.User.SetTwitchChatDetails(userNotice);

            if (this.User.IsPlatformSubscriber)
            {
                this.PlanTier = this.PlanName = this.User.SubscribeTierString;
            }
            else
            {
                this.PlanTier = this.PlanName = MixItUp.Base.Resources.Tier1;
            }

            this.IsGiftedUpgrade = true;
        }
    }

    public class TwitchMassGiftedSubEventModel
    {
        private const string AnonymousGiftedUserNoticeLogin = "ananonymousgifter";

        public UserViewModel Gifter { get; set; }

        public int TotalGifted { get; set; }

        public int LifetimeGifted { get; set; }

        public string PlanTier { get; set; }

        public int PlanTierNumber { get; set; }

        public bool IsAnonymous { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchMassGiftedSubEventModel(ChatUserNoticePacketModel userNotice)
        {
            this.IsAnonymous = string.Equals(userNotice.Login, AnonymousGiftedUserNoticeLogin, StringComparison.InvariantCultureIgnoreCase);

            this.Gifter = new UserViewModel("An Anonymous Gifter");
            if (!this.IsAnonymous)
            {
                this.Gifter = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, userNotice.UserID.ToString());
                if (this.Gifter == null)
                {
                    this.Gifter = new UserViewModel(userNotice);
                }
                this.Gifter.SetTwitchChatDetails(userNotice);
            }

            this.TotalGifted = userNotice.SubTotalGifted;
            this.LifetimeGifted = userNotice.SubTotalGiftedLifetime;
            this.PlanTier = TwitchEventService.GetSubTierNameFromText(userNotice.SubPlan);
            this.PlanTierNumber = 1;
        }
    }

    public interface ITwitchEventService
    {
        bool IsConnected { get; }

        Task<Result> Connect();
        Task Disconnect();

        Task AddSub(TwitchSubEventModel subEvent);

        Task AddMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent);
    }

    public class TwitchEventService : StreamingPlatformServiceBase, ITwitchEventService
    {
        private class TwitchGiftedSubEventModel
        {
            public UserViewModel Gifter { get; set; }

            public UserViewModel Receiver { get; set; }
            
            public bool IsAnonymous { get; set; }

            public int MonthsGifted { get; set; }

            public int PlanTierNumber { get; set; }

            public string PlanTier { get; set; }

            public string PlanName { get; set; }

            public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

            public TwitchGiftedSubEventModel(UserViewModel gifter, UserViewModel receiver, PubSubSubscriptionsGiftEventModel packet)
            {
                this.Gifter = gifter;
                this.Receiver = receiver;
                this.IsAnonymous = packet.IsAnonymousGiftedSubscription;
                this.MonthsGifted = packet.IsMultiMonth ? packet.multi_month_duration : 1;
                this.PlanTierNumber = TwitchEventService.GetSubTierNumberFromText(packet.sub_plan);
                this.PlanTier = TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
                this.PlanName = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
            }
        }

        public const int BackgroundGiftedSubProcessorTime = 3000;

        public static int GetSubTierNumberFromText(string subPlan)
        {
            if (!string.IsNullOrEmpty(subPlan) && int.TryParse(subPlan, out int subPlanNumber) && subPlanNumber >= 1000)
            {
                return subPlanNumber / 1000;
            }
            return 1;
        }

        public static string GetSubTierNameFromText(string subPlan)
        {
            int subTier = TwitchEventService.GetSubTierNumberFromText(subPlan);
            if (subTier > 0)
            {
                return $"{MixItUp.Base.Resources.Tier} {subTier}";
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

        private List<TwitchGiftedSubEventModel> newGiftedSubTracker = new List<TwitchGiftedSubEventModel>();
        private List<TwitchMassGiftedSubEventModel> newMassGiftedSubTracker = new List<TwitchMassGiftedSubEventModel>();
        private Task giftedSubProcessorTask = null;

        public override string Name { get { return "Twitch Events"; } }

        public bool IsConnected { get; private set; }

        public TwitchEventService() { }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;
            if (ServiceManager.Get<TwitchSessionService>().UserConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.pubSub = new PubSubClient(ServiceManager.Get<TwitchSessionService>().UserConnection.Connection);

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
                        this.pubSub.OnSubscribedReceived += PubSub_OnSubscribedReceived;
                        this.pubSub.OnSubscriptionsGiftedReceived += PubSub_OnSubscriptionsGiftedReceived;
                        this.pubSub.OnChannelPointsRedeemed += PubSub_OnChannelPointsRedeemed;

                        await this.pubSub.Connect();

                        await Task.Delay(1000);

                        List<PubSubListenTopicModel> topics = new List<PubSubListenTopicModel>();
                        foreach (PubSubTopicsEnum topic in TwitchEventService.topicTypes)
                        {
                            topics.Add(new PubSubListenTopicModel(topic, (string)ServiceManager.Get<TwitchSessionService>().UserNewAPI.id));
                        }

                        await this.pubSub.Listen(topics);

                        await Task.Delay(1000);

                        await this.pubSub.Ping();

                        this.cancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(this.BackgroundEventChecks, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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

        public async Task AddSub(TwitchSubEventModel subEvent)
        {
            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscribed, subEvent.User);

            if (subEvent.IsGiftedUpgrade)
            {
                var subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.CheckIfSubscribedV5(ServiceManager.Get<TwitchSessionService>().ChannelV5, subEvent.User.GetTwitchV5APIUserModel());
                if (subscription != null && !string.IsNullOrEmpty(subscription.created_at))
                {
                    subEvent.PlanTier = TwitchEventService.GetSubTierNameFromText(subscription.sub_plan);
                    subEvent.PlanName = subscription.sub_plan_name;
                }
            }

            if (ChannelSession.Services.Events.CanPerformEvent(trigger))
            {
                trigger.SpecialIdentifiers["message"] = subEvent.Message;
                trigger.SpecialIdentifiers["usersubplanname"] = subEvent.PlanName;
                trigger.SpecialIdentifiers["usersubplan"] = subEvent.PlanTier;

                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = subEvent.User.ID;
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                subEvent.User.SubscribeDate = DateTimeOffset.Now;
                subEvent.User.Data.TwitchSubscriberTier = subEvent.PlanTierNumber;
                subEvent.User.Data.TotalMonthsSubbed++;

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                {
                    currency.AddAmount(subEvent.User.Data, currency.OnSubscribeBonus);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    if (trigger.User.HasPermissionsTo(streamPass.Permission))
                    {
                        streamPass.AddAmount(subEvent.User.Data, streamPass.SubscribeBonus);
                    }
                }

                await ChannelSession.Services.Events.PerformEvent(trigger);
            }

            GlobalEvents.SubscribeOccurred(subEvent.User);

            if (subEvent.IsGiftedUpgrade)
            {
                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, subEvent.User, string.Format("{0} Continued Their Gifted Sub at {1}", subEvent.User.DisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
            else
            {
                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, subEvent.User, string.Format("{0} Subscribed at {1}", subEvent.User.DisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));

            }
        }

        public async Task AddMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent)
        {
            if (ChannelSession.Settings.TwitchMassGiftedSubsFilterAmount > 0)
            {
                if (massGiftedSubEvent.TotalGifted > ChannelSession.Settings.TwitchMassGiftedSubsFilterAmount)
                {
                    lock (this.newMassGiftedSubTracker)
                    {
                        this.newMassGiftedSubTracker.Add(massGiftedSubEvent);
                    }
                }
            }
            else
            {
                await ProcessMassGiftedSub(massGiftedSubEvent);
            }
        }

        private async Task BackgroundEventChecks(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (streamStartCheckTime != DateTimeOffset.MaxValue)
                {
                    DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime();
                    Logger.Log(LogLevel.Debug, "Check for stream start: " + startTime + " - " + streamStartCheckTime);
                    if (startTime > streamStartCheckTime)
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

                IEnumerable<UserFollowModel> followers = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIFollowers(ServiceManager.Get<TwitchSessionService>().UserNewAPI, maxResult: 100);
                if (this.follows.Count() > 0)
                {
                    foreach (UserFollowModel follow in followers)
                    {
                        if (!this.follows.Contains(follow.from_id))
                        {
                            this.follows.Add(follow.from_id);

                            UserViewModel user = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, follow.from_id);
                            if (user == null)
                            {
                                user = new UserViewModel(follow);
                            }

                            if (user.UserRoles.Contains(UserRoleEnum.Banned))
                            {
                                return;
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

                                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                                {
                                    if (user.HasPermissionsTo(streamPass.Permission))
                                    {
                                        streamPass.AddAmount(user.Data, streamPass.FollowBonus);
                                    }
                                }

                                await ChannelSession.Services.Events.PerformEvent(trigger);

                                GlobalEvents.FollowOccurred(user);

                                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Followed", user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                            }
                        }
                    }
                }
                else
                {
                    foreach (UserFollowModel follow in followers)
                    {
                        this.follows.Add(follow.from_id);
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
            UserViewModel user = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, packet.user_id);
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
                if (user.HasPermissionsTo(streamPass.Permission))
                {
                    streamPass.AddAmount(user.Data, (int)Math.Ceiling(streamPass.BitsBonus * bitsCheered.Amount));
                }
            }

            user.Data.TotalBitsCheered += (uint)bitsCheered.Amount;

            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredUserData] = user.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredAmountData] = bitsCheered.Amount;

            if (string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(user, bitsCheered.Message)))
            {
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelBitsCheered, user);
                trigger.SpecialIdentifiers["bitsamount"] = bitsCheered.Amount.ToString();
                trigger.SpecialIdentifiers["message"] = bitsCheered.Message;
                await ChannelSession.Services.Events.PerformEvent(trigger);
            }
            await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Cheered {1} Bits", user.DisplayName, bitsCheered.Amount), ChannelSession.Settings.AlertBitsCheeredColor));
            GlobalEvents.BitsOccurred(bitsCheered);
        }

        private async void PubSub_OnSubscribedReceived(object sender, PubSubSubscriptionsEventModel packet)
        {
            UserViewModel user = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, packet.user_id);
            if (user == null)
            {
                user = new UserViewModel(packet);
            }

            if (packet.IsSubscription || packet.cumulative_months == 1)
            {
                await this.AddSub(new TwitchSubEventModel(user, packet));
            }
            else
            {
                int months = Math.Max(packet.streak_months, packet.cumulative_months);
                string planTier = TwitchEventService.GetSubTierNameFromText(packet.sub_plan);

                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelResubscribed, user);
                if (ChannelSession.Services.Events.CanPerformEvent(trigger))
                {
                    trigger.SpecialIdentifiers["message"] = (packet.sub_message.ContainsKey("message")) ? packet.sub_message["message"].ToString() : string.Empty;
                    trigger.SpecialIdentifiers["usersubmonths"] = months.ToString();
                    trigger.SpecialIdentifiers["usersubplanname"] = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
                    trigger.SpecialIdentifiers["usersubplan"] = planTier;
                    trigger.SpecialIdentifiers["usersubstreak"] = packet.streak_months.ToString();

                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                    user.SubscribeDate = DateTimeOffset.Now.SubtractMonths(months - 1);
                    user.Data.TwitchSubscriberTier = TwitchEventService.GetSubTierNumberFromText(packet.sub_plan);
                    user.Data.TotalMonthsSubbed++;

                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                    }

                    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                    {
                        if (trigger.User.HasPermissionsTo(streamPass.Permission))
                        {
                            streamPass.AddAmount(user.Data, streamPass.SubscribeBonus);
                        }
                    }

                    if (string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(user, trigger.SpecialIdentifiers["message"])))
                    {
                        await ChannelSession.Services.Events.PerformEvent(trigger);
                    }
                }

                GlobalEvents.ResubscribeOccurred(new Tuple<UserViewModel, int>(user, months));
                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Re-Subscribed For {1} Months at {2}", user.DisplayName, months, planTier), ChannelSession.Settings.AlertSubColor));
            }
        }

        private async void PubSub_OnSubscriptionsGiftedReceived(object sender, PubSubSubscriptionsGiftEventModel packet)
        {
            UserViewModel gifter = packet.IsAnonymousGiftedSubscription ? new UserViewModel("An Anonymous Gifter") : ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, packet.user_id);
            if (gifter == null)
            {
                gifter = new UserViewModel(packet);
            }

            UserViewModel receiver = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, packet.recipient_id);
            if (receiver == null)
            {
                receiver = new UserViewModel(new UserModel()
                {
                    id = packet.recipient_id,
                    login = packet.recipient_user_name,
                    display_name = packet.recipient_display_name
                });
            }

            TwitchGiftedSubEventModel giftedSubEvent = new TwitchGiftedSubEventModel(gifter, receiver, packet);
            if (ChannelSession.Settings.TwitchMassGiftedSubsFilterAmount > 0)
            {
                lock (this.newGiftedSubTracker)
                {
                    this.newGiftedSubTracker.Add(giftedSubEvent);
                }

                if (giftedSubProcessorTask == null)
                {
                    giftedSubProcessorTask = Task.Run(BackgroundGiftedSubProcessor);
                }
            }
            else
            {
                await ProcessGiftedSub(giftedSubEvent);
            }
        }

        private async Task BackgroundGiftedSubProcessor()
        {
            Dictionary<Guid, List<TwitchGiftedSubEventModel>> giftedSubs = new Dictionary<Guid, List<TwitchGiftedSubEventModel>>();
            List<TwitchMassGiftedSubEventModel> massGiftedSubs = new List<TwitchMassGiftedSubEventModel>();

            List<TwitchGiftedSubEventModel> tempGiftedSubs = new List<TwitchGiftedSubEventModel>();
            List<TwitchMassGiftedSubEventModel> tempMassGiftedSubs = new List<TwitchMassGiftedSubEventModel>();

            do
            {
                await Task.Delay(BackgroundGiftedSubProcessorTime);

                lock (this.newGiftedSubTracker)
                {
                    tempGiftedSubs = this.newGiftedSubTracker.ToList();
                    this.newGiftedSubTracker.Clear();
                }

                lock (this.newMassGiftedSubTracker)
                {
                    tempMassGiftedSubs = this.newMassGiftedSubTracker.ToList();
                    this.newMassGiftedSubTracker.Clear();
                }

                foreach (var group in tempGiftedSubs.GroupBy(s => s.Gifter.ID, s => s))
                {
                    Guid id = group.First().IsAnonymous ? Guid.Empty : group.Key;
                    if (!giftedSubs.ContainsKey(id))
                    {
                        giftedSubs[id] = new List<TwitchGiftedSubEventModel>();
                    }
                    giftedSubs[id].AddRange(group.OrderBy(s => s.Processed));
                }

                massGiftedSubs.AddRange(tempMassGiftedSubs.OrderBy(s => s.Processed));

            } while (tempGiftedSubs.Count > 0 || tempMassGiftedSubs.Count > 0);

            giftedSubProcessorTask = null;

            foreach (TwitchMassGiftedSubEventModel massGiftedSub in massGiftedSubs)
            {
                Guid gifterID = (massGiftedSub.IsAnonymous) ? Guid.Empty : massGiftedSub.Gifter.ID;
                if (giftedSubs.ContainsKey(gifterID))
                {
                    for (int i = 0; i < massGiftedSub.TotalGifted && giftedSubs[gifterID].Count > 0; i++)
                    {
                        TwitchGiftedSubEventModel giftedSub = giftedSubs[gifterID][0];
                        giftedSubs[gifterID].Remove(giftedSub);
                        await ProcessGiftedSub(giftedSub, fireEventCommand: false);
                    }
                }
                await ProcessMassGiftedSub(massGiftedSub);
            }

            foreach (TwitchGiftedSubEventModel giftedSub in giftedSubs.SelectMany(kvp => kvp.Value))
            {
                await ProcessGiftedSub(giftedSub);
            }
        }

        private async Task ProcessGiftedSub(TwitchGiftedSubEventModel giftedSubEvent, bool fireEventCommand = true)
        {
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftedSubEvent.Receiver.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = giftedSubEvent.MonthsGifted;

            giftedSubEvent.Receiver.SubscribeDate = DateTimeOffset.Now;
            giftedSubEvent.Receiver.Data.TwitchSubscriberTier = giftedSubEvent.PlanTierNumber;
            giftedSubEvent.Gifter.Data.TotalSubsGifted += (uint)giftedSubEvent.MonthsGifted;
            giftedSubEvent.Receiver.Data.TotalSubsReceived += (uint)giftedSubEvent.MonthsGifted;
            giftedSubEvent.Receiver.Data.TotalMonthsSubbed += (uint)giftedSubEvent.MonthsGifted;

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                for (int i = 0; i < giftedSubEvent.MonthsGifted; i++)
                {
                    currency.AddAmount(giftedSubEvent.Gifter.Data, currency.OnSubscribeBonus);
                }
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                if (giftedSubEvent.Gifter.HasPermissionsTo(streamPass.Permission))
                {
                    streamPass.AddAmount(giftedSubEvent.Gifter.Data, streamPass.SubscribeBonus);
                }
            }

            if (fireEventCommand)
            {
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelSubscriptionGifted, giftedSubEvent.Gifter);
                trigger.SpecialIdentifiers["usersubplanname"] = giftedSubEvent.PlanName;
                trigger.SpecialIdentifiers["usersubplan"] = giftedSubEvent.PlanTier;
                trigger.SpecialIdentifiers["usersubmonthsgifted"] = giftedSubEvent.MonthsGifted.ToString();
                trigger.SpecialIdentifiers["isanonymous"] = giftedSubEvent.IsAnonymous.ToString();
                trigger.Arguments.Add(giftedSubEvent.Receiver.Username);
                trigger.TargetUser = giftedSubEvent.Receiver;
                await ChannelSession.Services.Events.PerformEvent(trigger);

                await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, giftedSubEvent.Gifter, string.Format("{0} Gifted A {1} Subscription To {2}", giftedSubEvent.Gifter.DisplayName, giftedSubEvent.PlanTier, giftedSubEvent.Receiver.DisplayName), ChannelSession.Settings.AlertGiftedSubColor));
            }

            GlobalEvents.SubscriptionGiftedOccurred(giftedSubEvent.Gifter, giftedSubEvent.Receiver);
        }

        private async Task ProcessMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent)
        {
            EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelMassSubscriptionsGifted, massGiftedSubEvent.Gifter);
            trigger.SpecialIdentifiers["subsgiftedamount"] = massGiftedSubEvent.TotalGifted.ToString();
            trigger.SpecialIdentifiers["subsgiftedlifetimeamount"] = massGiftedSubEvent.LifetimeGifted.ToString();
            trigger.SpecialIdentifiers["usersubplan"] = massGiftedSubEvent.PlanTier;
            trigger.SpecialIdentifiers["isanonymous"] = massGiftedSubEvent.IsAnonymous.ToString();
            await ChannelSession.Services.Events.PerformEvent(trigger);

            await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, massGiftedSubEvent.Gifter, string.Format("{0} Gifted {1} {2} Subs", massGiftedSubEvent.Gifter.DisplayName, massGiftedSubEvent.TotalGifted, massGiftedSubEvent.PlanTier), ChannelSession.Settings.AlertMassGiftedSubColor));
        }

        private async void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointsRedemptionEventModel packet)
        {
            PubSubChannelPointsRedeemedEventModel redemption = packet.redemption;

            UserViewModel user = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, redemption.user.id);
            if (user == null)
            {
                user = new UserViewModel(redemption.user);
            }

            List<string> arguments = null;
            Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
            specialIdentifiers["rewardname"] = redemption.reward.title;
            specialIdentifiers["rewardcost"] = redemption.reward.cost.ToString();
            if (!string.IsNullOrEmpty(redemption.user_input))
            {
                specialIdentifiers["message"] = redemption.user_input;
                arguments = new List<string>(redemption.user_input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (string.IsNullOrEmpty(await ChannelSession.Services.Moderation.ShouldTextBeModerated(user, redemption.user_input)))
            {
                EventTrigger trigger = new EventTrigger(EventTypeEnum.TwitchChannelPointsRedeemed, user, specialIdentifiers);
                trigger.Arguments = arguments;
                await ChannelSession.Services.Events.PerformEvent(trigger);

                TwitchChannelPointsCommandModel command = ChannelSession.TwitchChannelPointsCommands.FirstOrDefault(c => string.Equals(c.Name, redemption.reward.title, StringComparison.CurrentCultureIgnoreCase));
                if (command != null)
                {
                    await command.Perform(new CommandParametersModel(user, platform: StreamingPlatformTypeEnum.Twitch, arguments: arguments, specialIdentifiers: specialIdentifiers));
                }
            }
            await ChannelSession.Services.Alerts.AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, user, string.Format("{0} Redeemed {1}", user.DisplayName, redemption.reward.title), ChannelSession.Settings.AlertChannelPointsColor));
        }

        private async void PubSub_OnWhisperReceived(object sender, PubSubWhisperEventModel packet)
        {
            if (!string.IsNullOrEmpty(packet.body))
            {
                UserViewModel user = ChannelSession.Services.User.GetUserByPlatformID(StreamingPlatformTypeEnum.Twitch, packet.from_id.ToString());
                await ChannelSession.Services.Chat.AddMessage(new TwitchChatMessageViewModel(packet, user));
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
