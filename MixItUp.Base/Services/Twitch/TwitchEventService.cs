using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Clients.Chat;
using MixItUp.Base.Model.Twitch.Clients.EventSub;
using MixItUp.Base.Model.Twitch.Clients.PubSub;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.Twitch.API;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch
{
    [Obsolete]
    public class TwitchSubEventModel
    {
        public static int GetSubPoints(int tier)
        {
            if (tier == 3)
            {
                return 6;
            }
            return tier;
        }

        public UserV2ViewModel User { get; set; }

        public string PlanTier { get; set; }

        public int PlanTierNumber { get; set; } = 1;

        public string PlanName { get; set; }

        public string Message { get; set; } = string.Empty;

        public bool IsPrimeUpgrade { get; set; }
        public bool IsGiftedUpgrade { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchSubEventModel(UserV2ViewModel user, PubSubSubscriptionsEventModel packet)
        {
            this.User = user;
            this.PlanTier = TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
            this.PlanTierNumber = TwitchPubSubService.GetSubTierNumberFromText(packet.sub_plan);
            this.PlanName = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
            if (packet.sub_message.ContainsKey("message"))
            {
                this.Message = packet.sub_message["message"].ToString();
            }
        }

        public TwitchSubEventModel(UserV2ViewModel user, ChatUserNoticePacketModel userNotice)
        {
            this.User = user;
            if (this.User.IsPlatformSubscriber)
            {
                this.PlanTier = this.PlanName = this.User.SubscriberTierString;
            }
            else
            {
                this.PlanTier = this.PlanName = MixItUp.Base.Resources.Tier1;
            }
            this.PlanTierNumber = 1;
        }
    }

    [Obsolete]
    public class TwitchGiftedSubEventModel
    {
        public UserV2ViewModel Gifter { get; set; }

        public UserV2ViewModel Receiver { get; set; }

        public bool IsAnonymous { get; set; }

        public int MonthsGifted { get; set; }

        public int PlanTierNumber { get; set; }

        public string PlanTier { get; set; }

        public string PlanName { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchGiftedSubEventModel(UserV2ViewModel gifter, UserV2ViewModel receiver, PubSubSubscriptionsGiftEventModel packet)
        {
            this.Gifter = gifter;
            this.Receiver = receiver;
            this.IsAnonymous = packet.IsAnonymousGiftedSubscription;
            this.MonthsGifted = packet.IsMultiMonth ? packet.multi_month_duration : 1;
            this.PlanTierNumber = TwitchPubSubService.GetSubTierNumberFromText(packet.sub_plan);
            this.PlanTier = TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
            this.PlanName = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
        }
    }

    [Obsolete]
    public class TwitchMassGiftedSubEventModel
    {
        public const string AnonymousGiftedUserNoticeLogin = "ananonymousgifter";

        public static bool IsAnonymousGifter(ChatUserNoticePacketModel userNotice) { return string.Equals(userNotice.Login, TwitchMassGiftedSubEventModel.AnonymousGiftedUserNoticeLogin, StringComparison.InvariantCultureIgnoreCase); }

        public UserV2ViewModel Gifter { get; set; }

        public int TotalGifted { get; set; }

        public int LifetimeGifted { get; set; }

        public string PlanTier { get; set; }

        public int PlanTierNumber { get; set; }

        public bool IsAnonymous { get; set; }

        public List<TwitchGiftedSubEventModel> Subs { get; set; } = new List<TwitchGiftedSubEventModel>();

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchMassGiftedSubEventModel(ChatUserNoticePacketModel userNotice, UserV2ViewModel gifter)
        {
            this.IsAnonymous = TwitchMassGiftedSubEventModel.IsAnonymousGifter(userNotice);
            this.Gifter = gifter;
            this.TotalGifted = userNotice.SubTotalGifted;
            this.LifetimeGifted = userNotice.SubTotalGiftedLifetime;
            this.PlanTier = TwitchPubSubService.GetSubTierNameFromText(userNotice.SubPlan);
            this.PlanTierNumber = 1;
        }
    }

    [Obsolete]
    public class TwitchUserBitsCheeredModel
    {
        public UserV2ViewModel User { get; set; }

        public int Amount { get; set; }

        public TwitchChatMessageViewModel Message { get; set; }

        public bool IsAnonymous { get { return this.User.Platform == StreamingPlatformTypeEnum.None; } }

        public TwitchUserBitsCheeredModel(UserV2ViewModel user, PubSubBitsEventV2Model bitsEvent)
        {
            this.User = user;
            this.Amount = bitsEvent.bits_used;
            this.Message = new TwitchChatMessageViewModel(bitsEvent, user);
        }
    }

    [Obsolete]
    public class TwitchWebhookUserFollowModel
    {
        public string ID { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
    }

    [Obsolete]
    public class TwitchEventSubService : StreamingPlatformServiceBase
    {
        private const string ChannelFollowEventSubSubscription = "channel.follow";
        private const string ChannelRaidEventSubSubscription = "channel.raid";

        public HashSet<string> FollowCache { get; private set; } = new HashSet<string>();

        private EventSubClient eventSub;
        private bool eventSubSubscriptionsConnected = false;
        private int lastHypeTrainLevel = 1;
        private DateTimeOffset streamStartCheckTime = DateTimeOffset.Now;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        private List<TwitchGiftedSubEventModel> pendingGiftedSubs = new List<TwitchGiftedSubEventModel>();
        private List<TwitchMassGiftedSubEventModel> pendingMassGiftedSubs = new List<TwitchMassGiftedSubEventModel>();
        private HashSet<string> channelPointRewardRedeems = new HashSet<string>();

        public override string Name { get { return MixItUp.Base.Resources.TwitchEventSub; } }

        public bool IsConnected { get; private set; }

        public bool StreamLiveStatus { get; private set; }

        public TwitchEventSubService() { }

        private async Task<Result> Connect()
        {
            this.IsConnected = false;
            if (ServiceManager.Get<TwitchSessionService>().UserConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.eventSub = new EventSubClient();

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.eventSub.OnTextReceivedOccurred += EventSub_OnTextReceivedOccurred;
                        }

                        this.eventSub.OnWelcomeMessageReceived += EventSub_OnWelcomeMessageReceived;
                        this.eventSub.OnReconnectMessageReceived += EventSub_OnReconnectMessageReceived;
                        this.eventSub.OnKeepAliveMessageReceived += EventSub_OnKeepAliveMessageReceived;
                        this.eventSub.OnNotificationMessageReceived += EventSub_OnNotificationMessageReceived;
                        this.eventSub.OnRevocationMessageReceived += EventSub_OnRevocationMessageReceived;

                        this.eventSubSubscriptionsConnected = false;

                        await this.eventSub.Connect();

                        await Task.Delay(2500);

                        for (int i = 0; !this.eventSubSubscriptionsConnected && i < 15; i++)
                        {
                            await Task.Delay(1000);
                        }

                        if (!this.eventSubSubscriptionsConnected)
                        {
                            return new Result(Resources.TwitchEventServiceFailedToConnectEventSub);
                        }

                        this.IsConnected = true;

                        this.eventSub.OnDisconnectOccurred += EventSub_OnDisconnectOccurred;

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                }));
            }
            return new Result(Resources.TwitchConnectionFailed);
        }

        public async Task TryConnect()
        {
            if (ServiceManager.Get<TwitchSessionService>().UserConnection != null)
            {
                // Load the follower cache
                IEnumerable<ChannelFollowerModel> followers = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIFollowers(ServiceManager.Get<TwitchSessionService>().User, maxResult: 100);
                foreach (ChannelFollowerModel follow in followers)
                {
                    this.FollowCache.Add(follow.user_id);
                }

                _ = Task.Run(async () =>
                {
                    // Wait 30 seconds before trying the first time
                    await Task.Delay(30000, this.cancellationTokenSource.Token);
                    _ = AsyncRunner.RunAsyncBackground(this.BackgroundEventChecks, this.cancellationTokenSource.Token, 60000);
                });

                // Start the reconnection in the background instead of blocking
                _ = Task.Run(() => EventSub_OnDisconnectOccurred(null, System.Net.WebSockets.WebSocketCloseStatus.Empty));
            }
        }

        private readonly IReadOnlyDictionary<string, string> DesiredSubscriptionsAndVersions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "stream.online", null },
            { "stream.offline", null },

            { "channel.update", "2" },

            { ChannelFollowEventSubSubscription, "2" },
            { ChannelRaidEventSubSubscription, null },

            { "channel.poll.begin", null },
            { "channel.poll.progress", null },
            { "channel.poll.end", null },

            { "channel.prediction.begin", null },
            { "channel.prediction.progress", null },
            { "channel.prediction.lock", null },
            { "channel.prediction.end", null },

            { "channel.ad_break.begin", null },

            { "channel.hype_train.begin", null },
            { "channel.hype_train.progress", null },
            { "channel.hype_train.end", null },

            { "channel.charity_campaign.donate", null },
        };

        private async void EventSub_OnWelcomeMessageReceived(object sender, WelcomeMessage message)
        {
            try
            {
                TwitchSessionService twitchSession = ServiceManager.Get<TwitchSessionService>();
                EventSubService eventSub = twitchSession.UserConnection.Connection.NewAPI.EventSub;

                IEnumerable<EventSubSubscriptionModel> allSubs = await eventSub.GetSubscriptions();
                HashSet<string> missingSubs = new HashSet<string>(DesiredSubscriptionsAndVersions.Keys, StringComparer.OrdinalIgnoreCase);
                foreach (EventSubSubscriptionModel sub in allSubs)
                {
                    if (DesiredSubscriptionsAndVersions.ContainsKey(sub.type) && string.Equals(sub.status, "connected", StringComparison.OrdinalIgnoreCase))
                    {
                        // Sub exists and is connected, remove from missing
                        missingSubs.Remove(sub.type);
                    }
                    else
                    {
                        // Got a sub we don't want, delete
                        await eventSub.DeleteSubscription(sub.id);
                    }
                }

                foreach (string missingSub in missingSubs)
                {
                    if (string.Equals(missingSub, ChannelFollowEventSubSubscription, StringComparison.OrdinalIgnoreCase))
                    {
                        Dictionary<string, string> conditions = new Dictionary<string, string>
                        {
                            { "broadcaster_user_id", ServiceManager.Get<TwitchSessionService>().UserID },
                            { "moderator_user_id", ServiceManager.Get<TwitchSessionService>().UserID }
                        };

                        await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub], conditions);
                    }
                    else if (missingSub.Equals(ChannelRaidEventSubSubscription, StringComparison.OrdinalIgnoreCase))
                    {
                        await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub],
                            new Dictionary<string, string> { { "from_broadcaster_user_id", ServiceManager.Get<TwitchSessionService>().UserID } });
                        await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub],
                            new Dictionary<string, string> { { "to_broadcaster_user_id", ServiceManager.Get<TwitchSessionService>().UserID } });
                    }
                    else
                    {
                        await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub]);
                    }
                }

                this.eventSubSubscriptionsConnected = true;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async Task RegisterEventSubSubscription(string type, WelcomeMessage message, string version = null, Dictionary<string, string> conditions = null)
        {
            try
            {
                if (conditions == null)
                {
                    conditions = new Dictionary<string, string> { { "broadcaster_user_id", ServiceManager.Get<TwitchSessionService>().UserID } };
                }

                await ServiceManager.Get<TwitchSessionService>().UserConnection.Connection.NewAPI.EventSub.CreateSubscription(
                    type,
                    "websocket",
                    conditions,
                    message.Payload.Session.Id,
                    version: version);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log(LogLevel.Error, $"Failed to connect EventSub for {type}");

                // Note: Do not re-throw, but log and move on, better to miss some events than to cause a retry loop
            }
        }

        private async void EventSub_OnRevocationMessageReceived(object sender, RevocationMessage e)
        {
            // TODO: Disconnect and reconnect
            await this.Disconnect(false);
        }

        private async void EventSub_OnNotificationMessageReceived(object sender, NotificationMessage message)
        {
            try
            {
                switch (message.Metadata.SubscriptionType)
                {
                    case "stream.online":
                        await HandleOnline(message.Payload.Event);
                        break;
                    case "stream.offline":
                        await HandleOffline(message.Payload.Event);
                        break;

                    case "channel.update":
                        await HandleChannelUpdate(message.Payload.Event);
                        break;

                    case ChannelFollowEventSubSubscription:
                        await HandleFollow(message.Payload.Event);
                        break;
                    case ChannelRaidEventSubSubscription:
                        await HandleRaid(message.Payload.Event);
                        break;

                    case "channel.poll.begin":
                        await HandlePollStart(message.Payload.Event);
                        break;
                    case "channel.poll.progress":
                        await HandlePollProgress(message.Payload.Event);
                        break;
                    case "channel.poll.end":
                        await HandlePollEnd(message.Payload.Event);
                        break;

                    case "channel.prediction.begin":
                        await HandlePredictionStart(message.Payload.Event);
                        break;
                    case "channel.prediction.progress":
                        await HandlePredictionProgress(message.Payload.Event);
                        break;
                    case "channel.prediction.lock":
                        await HandlePredictionLock(message.Payload.Event);
                        break;
                    case "channel.prediction.end":
                        await HandlePredictionEnd(message.Payload.Event);
                        break;

                    case "channel.ad_break.begin":
                        await HandleChannelAdBreakBegin(message.Payload.Event);
                        break;

                    case "channel.hype_train.begin":
                        await HandleHypeTrainBegin(message.Payload.Event);
                        break;
                    case "channel.hype_train.progress":
                        await HandleHypeTrainProgress(message.Payload.Event);
                        break;
                    case "channel.hype_train.end":
                        await HandleHypeTrainEnd(message.Payload.Event);
                        break;

                    case "channel.charity_campaign.donate":
                        await HandleCharityCampaignDonation(message.Payload.Event);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.ForceLog(LogLevel.Error, JSONSerializerHelper.SerializeToString(message.Payload));
            }
        }

        private async Task HandleFollow(JObject payload)
        {
            string followerId = payload["user_id"].Value<string>();
            string followerUsername = payload["user_login"].Value<string>();
            string followerDisplayName = payload["user_name"].Value<string>();

            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: followerId, platformUsername: followerUsername);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(followerId, followerUsername, followerDisplayName));
            }

            await ServiceManager.Get<TwitchEventSubService>().AddFollow(user);
        }

        private async Task HandleRaid(JObject payload)
        {
            try
            {
                string fromId = payload.GetValueOrDefault<string>("from_broadcaster_user_id", string.Empty);
                string fromUsername = payload.GetValueOrDefault<string>("from_broadcaster_user_login", string.Empty);
                string fromDisplayName = payload.GetValueOrDefault<string>("from_broadcaster_user_name", string.Empty);

                string toId = payload.GetValueOrDefault<string>("to_broadcaster_user_id", string.Empty);
                string toUsername = payload.GetValueOrDefault<string>("to_broadcaster_user_login", string.Empty);

                int viewers = payload.GetValueOrDefault<int>("viewers", 0);

                if (string.IsNullOrEmpty(fromId) || string.IsNullOrEmpty(toId))
                {
                    // Invalid raid event, ignore
                    return;
                }

                // The streamer was raided by a channel
                if (string.Equals(toId, ServiceManager.Get<TwitchSessionService>().Channel.broadcaster_id, StringComparison.OrdinalIgnoreCase))
                {
                    UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: fromId, platformUsername: fromUsername);
                    if (user == null)
                    {
                        user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(fromId, fromUsername, fromDisplayName));
                    }

                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch);
                    parameters.SpecialIdentifiers["hostviewercount"] = viewers.ToString();
                    parameters.SpecialIdentifiers["raidviewercount"] = viewers.ToString();

                    if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelRaided, parameters))
                    {
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidUserData] = user.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidViewerCountData] = viewers;

                        foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.ToList())
                        {
                            currency.AddAmount(user, currency.OnHostBonus);
                        }

                        foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                        {
                            if (user.MeetsRole(streamPass.UserPermission))
                            {
                                streamPass.AddAmount(user, streamPass.HostBonus);
                            }
                        }

                    EventService.RaidOccurred(user, viewers);

                        await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertRaid, user.FullDisplayName, viewers), ChannelSession.Settings.AlertRaidColor));
                    }
                }
                // The streamer is raiding another channel
                else if (string.Equals(fromId, ServiceManager.Get<TwitchSessionService>().Channel.broadcaster_id, StringComparison.OrdinalIgnoreCase))
                {
                    CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch, new List<string>() { toUsername });
                    parameters.SpecialIdentifiers["hostviewercount"] = viewers.ToString();
                    parameters.SpecialIdentifiers["raidviewercount"] = viewers.ToString();

                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelOutgoingRaidCompleted, parameters);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.ForceLog(LogLevel.Error, "Bad raid data: " + payload);
            }
        }

        private async Task HandleOnline(JObject payload)
        {
            Logger.Log(LogLevel.Debug, "Twitch Stream Online");
            this.StreamLiveStatus = true;
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        private async Task HandleOffline(JObject payload)
        {
            Logger.Log(LogLevel.Debug, "Twitch Stream Offline");
            this.StreamLiveStatus = false;
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        private async Task HandleChannelUpdate(JObject payload)
        {
            CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch);

            string gameID = payload["category_id"].ToString();
            GameModel game = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIGameByID(gameID);

            parameters.SpecialIdentifiers["streamtitle"] = payload["title"].ToString();
            parameters.SpecialIdentifiers["streamgameid"] = gameID;
            parameters.SpecialIdentifiers["streamgameimage"] = game?.box_art_url ?? string.Empty;
            parameters.SpecialIdentifiers["streamgame"] = parameters.SpecialIdentifiers["streamgamename"] = payload["category_name"].ToString();

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelUpdated, parameters);
        }

        private async Task HandlePollStart(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPolls: true);
            if (widgets.Count() > 0)
            {
                PollNotification poll = new PollNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.NewTwitchPoll(poll);
                }
            }
        }

        private async Task HandlePollProgress(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPolls: true);
            if (widgets.Count() > 0)
            {
                PollNotification poll = new PollNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.UpdateTwitchPoll(poll);
                }
            }
        }

        private async Task HandlePollEnd(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPolls: true);
            if (widgets.Count() > 0)
            {
                PollNotification poll = new PollNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.End(poll.Choices.OrderByDescending(c => c.Votes).First().ID);
                }
            }
        }

        private async Task HandlePredictionStart(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPredictions: true);
            if (widgets.Count() > 0)
            {
                PredictionNotification prediction = new PredictionNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.NewTwitchPrediction(prediction);
                }
            }
        }

        private async Task HandlePredictionProgress(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPredictions: true);
            if (widgets.Count() > 0)
            {
                PredictionNotification prediction = new PredictionNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.UpdateTwitchPrediction(prediction);
                }
            }
        }

        private Task HandlePredictionLock(JObject payload)
        {
            return Task.CompletedTask;
        }

        private async Task HandlePredictionEnd(JObject payload)
        {
            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forPredictions: true);
            if (widgets.Count() > 0)
            {
                PredictionNotification prediction = new PredictionNotification(payload);
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.End(prediction.WinningOutcomeID);
                }
            }
        }

        private async Task HandleChannelAdBreakBegin(JObject payload)
        {
            int.TryParse(payload["duration_seconds"].Value<string>(), out int duration);
            bool.TryParse(payload["is_automatic"].Value<string>(), out bool isAutomatic);

            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["adduration"] = duration.ToString();
            eventCommandSpecialIdentifiers["adisautomatic"] = isAutomatic.ToString();
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelAdStarted, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, string.Format(MixItUp.Base.Resources.AlertTwitchAdStarted, duration), ChannelSession.Settings.AlertTwitchAdsColor));

            if (duration > 0)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (token) =>
                {
                    await Task.Delay(duration * 1000);

                    eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                    eventCommandSpecialIdentifiers["adduration"] = duration.ToString();
                    eventCommandSpecialIdentifiers["adisautomatic"] = isAutomatic.ToString();
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelAdEnded, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));

                }, CancellationToken.None);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private async Task HandleHypeTrainBegin(JObject payload)
        {
            int totalPoints = payload["total"].Value<int>();
            int levelPoints = payload["progress"].Value<int>();
            int levelGoal = payload["goal"].Value<int>();
            int level = payload["level"].Value<int>();

            this.lastHypeTrainLevel = level;

            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelpoints"] = levelPoints.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevelgoal"] = levelGoal.ToString();
            eventCommandSpecialIdentifiers["hypetrainlevel"] = level.ToString();
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelHypeTrainBegin, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, MixItUp.Base.Resources.HypeTrainStarted, ChannelSession.Settings.AlertTwitchHypeTrainColor));
        }

        private async Task HandleHypeTrainProgress(JObject payload)
        {
            int level = payload["level"].Value<int>();
            if (level > this.lastHypeTrainLevel)
            {
                this.lastHypeTrainLevel = level;
                int totalPoints = payload["total"].Value<int>();
                int levelPoints = payload["progress"].Value<int>();
                int levelGoal = payload["goal"].Value<int>();

                Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
                eventCommandSpecialIdentifiers["hypetrainlevelpoints"] = levelPoints.ToString();
                eventCommandSpecialIdentifiers["hypetrainlevelgoal"] = levelGoal.ToString();
                eventCommandSpecialIdentifiers["hypetrainlevel"] = level.ToString();
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelHypeTrainLevelUp, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, string.Format(MixItUp.Base.Resources.HypeTrainLevelUp, level.ToString()), ChannelSession.Settings.AlertTwitchHypeTrainColor));
            }
        }

        private async Task HandleHypeTrainEnd(JObject payload)
        {
            int level = payload["level"].Value<int>();
            int totalPoints = payload["total"].Value<int>();

            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["hypetraintotallevel"] = level.ToString();
            eventCommandSpecialIdentifiers["hypetraintotalpoints"] = totalPoints.ToString();
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelHypeTrainEnd, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, string.Format(MixItUp.Base.Resources.HypeTrainEndedReachedLevel, level.ToString()), ChannelSession.Settings.AlertTwitchHypeTrainColor));
        }

        private async Task HandleCharityCampaignDonation(JObject payload)
        {
            CharityDonationNotification donation = new CharityDonationNotification(payload);

            Dictionary<string, string> additionalSpecialIdentifiers = new Dictionary<string, string>();
            additionalSpecialIdentifiers["charityname"] = donation.CharityName;
            additionalSpecialIdentifiers["charityimage"] = donation.CharityImage;

            await EventService.ProcessDonationEvent(EventTypeEnum.TwitchChannelCharityDonation, donation.ToGenericDonation(), additionalSpecialIdentifiers: additionalSpecialIdentifiers);
        }

        private void EventSub_OnKeepAliveMessageReceived(object sender, KeepAliveMessage e)
        {
            // TODO: If not received in 10 seconds, reconnect
        }

        private void EventSub_OnReconnectMessageReceived(object sender, ReconnectMessage e)
        {
            // NOTE: This SHOULD auto-disconnect

            // The URL of the reconnection message bight have encoded characters in it (EX: "cell-c.eventsub.wss.twitch.tv/ws?challenge=XXX\u0026id=XXX" where "\u0026" is "&").
            // There have also been issues noted with being able to re-connect properly to the URL specified.
        }

        private void EventSub_OnTextReceivedOccurred(object sender, string text)
        {
            Logger.Log(LogLevel.Debug, "EVENT SUB TEXT: " + text);
        }

        private async void EventSub_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus status)
        {
            int delayInMS = 2500;

            Result result;
            await this.Disconnect(false);
            do
            {
                ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TwitchEventSub);

                await Task.Delay(delayInMS);

                result = await this.Connect();

                // Double on every retry
                delayInMS *= 2;
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TwitchEventSub);
        }

        public async Task Disconnect(bool stopBackgroundWorker)
        {
            try
            {
                if (this.eventSub != null)
                {
                    this.eventSub.OnDisconnectOccurred -= EventSub_OnDisconnectOccurred;

                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        this.eventSub.OnTextReceivedOccurred -= EventSub_OnTextReceivedOccurred;
                    }

                    this.eventSub.OnWelcomeMessageReceived -= EventSub_OnWelcomeMessageReceived;
                    this.eventSub.OnReconnectMessageReceived -= EventSub_OnReconnectMessageReceived;
                    this.eventSub.OnKeepAliveMessageReceived -= EventSub_OnKeepAliveMessageReceived;
                    this.eventSub.OnNotificationMessageReceived -= EventSub_OnNotificationMessageReceived;
                    this.eventSub.OnRevocationMessageReceived -= EventSub_OnRevocationMessageReceived;

                    await this.eventSub.Disconnect();
                }

                if (stopBackgroundWorker && this.cancellationTokenSource != null)
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
            this.eventSub = null;
        }

        public async Task AddFollow(UserV2ViewModel user)
        {
            if (!this.FollowCache.Contains(user.PlatformID))
            {
                this.FollowCache.Add(user.PlatformID);

#pragma warning disable CS0612 // Type or member is obsolete
                if (user.HasRole(UserRoleEnum.Banned))
#pragma warning restore CS0612 // Type or member is obsolete
                {
                    return;
                }

                user.Roles.Add(UserRoleEnum.Follower);
                user.FollowDate = DateTimeOffset.Now;

                CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch);
                if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelFollowed, parameters))
                {
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestFollowerUserData] = user.ID;

                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user, currency.OnFollowBonus);
                    }

                    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                    {
                        if (user.MeetsRole(streamPass.UserPermission))
                        {
                            streamPass.AddAmount(user, streamPass.FollowBonus);
                        }
                    }

                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelFollowed, parameters);

                    EventService.FollowOccurred(user);

                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertFollow, user.FullDisplayName), ChannelSession.Settings.AlertFollowColor));
                }
            }
        }

        private async Task BackgroundEventChecks(CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                if (this.IsConnected)
                {
                    // We are connected and should get events via the event sub
                    return;
                }

                if (streamStartCheckTime != DateTimeOffset.MaxValue)
                {
                    DateTimeOffset startTime = await UptimePreMadeChatCommandModel.GetStartTime(StreamingPlatformTypeEnum.Twitch);
                    Logger.Log(LogLevel.Debug, "Check for stream start: " + startTime + " - " + streamStartCheckTime);
                    if (startTime > streamStartCheckTime)
                    {
                        Logger.Log(LogLevel.Debug, "Stream start detected");

                        streamStartCheckTime = DateTimeOffset.MaxValue;
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
                    }
                }

                IEnumerable<ChannelFollowerModel> followers = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIFollowers(ServiceManager.Get<TwitchSessionService>().User, maxResult: 100);
                foreach (ChannelFollowerModel follow in followers)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: follow.user_id, platformUsername: follow.user_name);
                    if (user == null)
                    {
                        user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(follow));
                    }
                    await this.AddFollow(user);
                }
            }
        }
    }

    [Obsolete]
    public class TwitchPubSubService : StreamingPlatformServiceBase
    {
        public const string PrimeSubPlan = "Prime";

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
            if (string.Equals(subPlan, PrimeSubPlan, StringComparison.OrdinalIgnoreCase))
            {
                return PrimeSubPlan;
            }

            int subTier = TwitchPubSubService.GetSubTierNumberFromText(subPlan);
            if (subTier > 0)
            {
                return $"{MixItUp.Base.Resources.Tier} {subTier}";
            }

            return subPlan;
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

        private List<TwitchGiftedSubEventModel> pendingGiftedSubs = new List<TwitchGiftedSubEventModel>();
        private List<TwitchMassGiftedSubEventModel> pendingMassGiftedSubs = new List<TwitchMassGiftedSubEventModel>();
        private HashSet<string> channelPointRewardRedeems = new HashSet<string>();

        public override string Name { get { return MixItUp.Base.Resources.TwitchPubSub; } }

        public bool IsConnected { get; private set; }

        public TwitchPubSubService() { }

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
                        foreach (PubSubTopicsEnum topic in TwitchPubSubService.topicTypes)
                        {
                            topics.Add(new PubSubListenTopicModel(topic, (string)ServiceManager.Get<TwitchSessionService>().UserID));
                        }

                        await this.pubSub.Listen(topics);

                        await Task.Delay(1000);

                        await this.pubSub.Ping();

                        this.cancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(this.BackgroundGiftedSubProcessor, this.cancellationTokenSource.Token, 3000);
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
            return new Result(Resources.TwitchConnectionFailed);
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
            CommandParametersModel parameters = new CommandParametersModel(subEvent.User, StreamingPlatformTypeEnum.Twitch);

            if (subEvent.IsPrimeUpgrade || subEvent.IsGiftedUpgrade)
            {
                var subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBroadcasterSubscription(ServiceManager.Get<TwitchSessionService>().User, ((TwitchUserPlatformV2Model)subEvent.User.PlatformModel).GetTwitchNewAPIUserModel());
                if (subscription != null)
                {
                    subEvent.PlanTier = TwitchPubSubService.GetSubTierNameFromText(subscription.tier);
                    subEvent.PlanName = subscription.tier;
                }
            }

            subEvent.User.Roles.Add(UserRoleEnum.Subscriber);
            subEvent.User.SubscribeDate = DateTimeOffset.Now;
            subEvent.User.SubscriberTier = subEvent.PlanTierNumber;

            parameters.SpecialIdentifiers["message"] = subEvent.Message;
            parameters.SpecialIdentifiers["usersubplanname"] = subEvent.PlanName;
            parameters.SpecialIdentifiers["usersubplan"] = subEvent.PlanTier;
            parameters.SpecialIdentifiers["usersubpoints"] = TwitchSubEventModel.GetSubPoints(subEvent.PlanTierNumber).ToString();
            parameters.SpecialIdentifiers["isprimeupgrade"] = subEvent.IsPrimeUpgrade.ToString();
            parameters.SpecialIdentifiers["isgiftupgrade"] = subEvent.IsGiftedUpgrade.ToString();

            if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters))
            {
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = subEvent.User.ID;
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                subEvent.User.TotalMonthsSubbed++;

                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                {
                    currency.AddAmount(subEvent.User, currency.OnSubscribeBonus);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    if (parameters.User.MeetsRole(streamPass.UserPermission))
                    {
                        streamPass.AddAmount(subEvent.User, streamPass.SubscribeBonus);
                    }
                }

                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters);
            }

            EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, subEvent.User, tier: subEvent.PlanTierNumber));

            if (subEvent.IsPrimeUpgrade)
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedPrimeSubscriptionTier, subEvent.User.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
            else if (subEvent.IsGiftedUpgrade)
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedGiftedSubscriptionTier, subEvent.User.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
            else
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertSubscribedTier, subEvent.User.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
        }

        public async Task AddMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent)
        {
            massGiftedSubEvent.Gifter.TotalSubsGifted = (uint)massGiftedSubEvent.LifetimeGifted;

            if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0)
            {
                if (massGiftedSubEvent.TotalGifted > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                {
                    lock (this.pendingMassGiftedSubs)
                    {
                        this.pendingMassGiftedSubs.Add(massGiftedSubEvent);
                    }
                }
            }
            else
            {
                await ProcessMassGiftedSub(massGiftedSubEvent);
            }
        }

        private async void PubSub_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TwitchPubSub);

            Result result;
            await this.Disconnect();
            do
            {
                await Task.Delay(2500);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TwitchPubSub);
        }

        private void PubSub_OnReconnectReceived(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TwitchPubSub);
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

        public async void PubSub_OnBitsV2Received(object sender, PubSubBitsEventV2Model packet)
        {
            UserV2ViewModel user;
            if (packet.is_anonymous)
            {
                user = UserV2ViewModel.CreateUnassociated();
            }
            else
            {
                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.user_id, platformUsername: packet.user_name);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet));
                }
            }

            TwitchUserBitsCheeredModel bitsCheered = new TwitchUserBitsCheeredModel(user, packet);

            if (!bitsCheered.IsAnonymous)
            {
                foreach (CurrencyModel bitsCurrency in ChannelSession.Settings.Currency.Values.Where(c => c.SpecialTracking == CurrencySpecialTrackingEnum.Bits))
                {
                    bitsCurrency.AddAmount(user, bitsCheered.Amount);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    if (user.MeetsRole(streamPass.UserPermission))
                    {
                        streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.BitsBonus * bitsCheered.Amount));
                    }
                }

                if (user.HasPlatformData(StreamingPlatformTypeEnum.Twitch))
                {
                    ((TwitchUserPlatformV2Model)user.PlatformModel).TotalBitsCheered = (uint)packet.total_bits_used;
                }

                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredUserData] = user.ID;
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredAmountData] = bitsCheered.Amount;
            }

            if (string.IsNullOrEmpty(await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, bitsCheered.Message.PlainTextMessage)))
            {
                CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch, bitsCheered.Message.ToArguments());
                parameters.SpecialIdentifiers["bitsamount"] = bitsCheered.Amount.ToString();
                parameters.SpecialIdentifiers["bitslifetimeamount"] = packet.total_bits_used.ToString();
                parameters.SpecialIdentifiers["messagenocheermotes"] = bitsCheered.Message.PlainTextMessageNoCheermotes;
                parameters.SpecialIdentifiers["message"] = bitsCheered.Message.PlainTextMessage;
                parameters.SpecialIdentifiers["isanonymous"] = bitsCheered.IsAnonymous.ToString();
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelBitsCheered, parameters);

                TwitchBitsCommandModel command = ServiceManager.Get<CommandService>().TwitchBitsCommands.FirstOrDefault(c => c.IsEnabled && c.IsSingle && c.StartingAmount == bitsCheered.Amount);
                if (command == null)
                {
                    command = ServiceManager.Get<CommandService>().TwitchBitsCommands.Where(c => c.IsEnabled && c.IsRange).OrderBy(c => c.Range).FirstOrDefault(c => c.IsInRange(bitsCheered.Amount));
                }

                if (command != null)
                {
                    await ServiceManager.Get<CommandService>().Queue(command, parameters);
                }
            }
            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertTwitchBitsCheered, user.FullDisplayName, bitsCheered.Amount), ChannelSession.Settings.AlertTwitchBitsCheeredColor));
            //EventService.TwitchBitsCheeredOccurred(bitsCheered);
        }

        public async void PubSub_OnSubscribedReceived(object sender, PubSubSubscriptionsEventModel packet)
        {
            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.user_id, platformUsername: packet.user_name);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet));
            }

            if (packet.IsSubscription || packet.cumulative_months == 1)
            {
                await this.AddSub(new TwitchSubEventModel(user, packet));
            }
            else
            {
                int months = Math.Max(packet.streak_months, packet.cumulative_months);
                string planTier = TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
                string message = (packet.sub_message.ContainsKey("message") && packet.sub_message["message"] != null) ? packet.sub_message["message"].ToString() : string.Empty;

                user.Roles.Add(UserRoleEnum.Subscriber);
                user.SubscribeDate = DateTimeOffset.Now.SubtractMonths(months - 1);
                user.SubscriberTier = TwitchPubSubService.GetSubTierNumberFromText(packet.sub_plan);

                CommandParametersModel parameters = new CommandParametersModel(user, new List<string>(message.Split(new char[] { ' ' })));
                parameters.SpecialIdentifiers["message"] = message;
                parameters.SpecialIdentifiers["usersubmonths"] = months.ToString();
                parameters.SpecialIdentifiers["usersubplanname"] = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
                parameters.SpecialIdentifiers["usersubplan"] = planTier;
                parameters.SpecialIdentifiers["usersubpoints"] = TwitchSubEventModel.GetSubPoints(user.SubscriberTier).ToString();
                parameters.SpecialIdentifiers["usersubstreak"] = packet.streak_months.ToString();

                string moderation = await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, message);
                if (!string.IsNullOrEmpty(moderation))
                {
                    parameters.SpecialIdentifiers["message"] = moderation;
                }

                if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelResubscribed, parameters))
                {
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                    if (months >= user.TotalMonthsSubbed)
                    {
                        user.TotalMonthsSubbed = months;
                    }
                    else
                    {
                        user.TotalMonthsSubbed++;
                    }

                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                    {
                        currency.AddAmount(user, currency.OnSubscribeBonus);
                    }

                    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                    {
                        if (parameters.User.MeetsRole(streamPass.UserPermission))
                        {
                            streamPass.AddAmount(user, streamPass.SubscribeBonus);
                        }
                    }
                }

                EventService.ResubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, user, months: months, tier: user.SubscriberTier));
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertResubscribedTier, user.FullDisplayName, months, planTier), ChannelSession.Settings.AlertSubColor));
            }
        }

        public async void PubSub_OnSubscriptionsGiftedReceived(object sender, PubSubSubscriptionsGiftEventModel packet)
        {
            UserV2ViewModel gifter = packet.IsAnonymousGiftedSubscription ? UserV2ViewModel.CreateUnassociated() : await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.user_id, platformUsername: packet.user_name);
            if (gifter == null)
            {
                gifter = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet));
            }

            UserV2ViewModel receiver = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.recipient_id, platformUsername: packet.recipient_user_name);
            if (receiver == null)
            {
                receiver = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet.recipient_id, packet.recipient_user_name, packet.recipient_display_name));
            }

            TwitchGiftedSubEventModel giftedSubEvent = new TwitchGiftedSubEventModel(gifter, receiver, packet);
            if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0)
            {
                lock (this.pendingGiftedSubs)
                {
                    this.pendingGiftedSubs.Add(giftedSubEvent);
                }
            }
            else
            {
                await ProcessGiftedSub(giftedSubEvent);
            }
        }

        private async Task BackgroundGiftedSubProcessor(CancellationToken cancellationToken)
        {
            if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0 && this.pendingGiftedSubs.Count > 0)
            {
                List<TwitchGiftedSubEventModel> tempGiftedSubs = new List<TwitchGiftedSubEventModel>();
                lock (this.pendingGiftedSubs)
                {
                    tempGiftedSubs.AddRange(this.pendingGiftedSubs.ToList().OrderBy(s => s.Processed));
                    this.pendingGiftedSubs.Clear();
                }

                List<TwitchMassGiftedSubEventModel> tempMassGiftedSubs = new List<TwitchMassGiftedSubEventModel>();
                lock (this.pendingMassGiftedSubs)
                {
                    tempMassGiftedSubs.AddRange(this.pendingMassGiftedSubs.ToList().OrderBy(s => s.Processed));
                }

                foreach (var giftedSub in tempGiftedSubs)
                {
                    TwitchMassGiftedSubEventModel massGiftedSub = null;
                    if (giftedSub.IsAnonymous || giftedSub.Gifter == null)
                    {
                        massGiftedSub = tempMassGiftedSubs.FirstOrDefault(ms => ms.IsAnonymous);
                    }
                    else
                    {
                        massGiftedSub = tempMassGiftedSubs.FirstOrDefault(ms => ms.Gifter.ID == giftedSub.Gifter.ID);
                    }

                    if (massGiftedSub != null)
                    {
                        await ProcessGiftedSub(giftedSub, fireEventCommand: false);

                        massGiftedSub.Subs.Add(giftedSub);
                        if (massGiftedSub.Subs.Count >= massGiftedSub.TotalGifted)
                        {
                            tempMassGiftedSubs.Remove(massGiftedSub);
                            lock (this.pendingMassGiftedSubs)
                            {
                                this.pendingMassGiftedSubs.Remove(massGiftedSub);
                            }

                            await ProcessMassGiftedSub(massGiftedSub);
                        }
                    }
                    else
                    {
                        await ProcessGiftedSub(giftedSub);
                    }
                }
            }
        }

        private async Task ProcessGiftedSub(TwitchGiftedSubEventModel giftedSubEvent, bool fireEventCommand = true)
        {
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftedSubEvent.Receiver.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = giftedSubEvent.MonthsGifted;

            giftedSubEvent.Receiver.Roles.Add(UserRoleEnum.Subscriber);
            giftedSubEvent.Receiver.SubscribeDate = DateTimeOffset.Now;
            giftedSubEvent.Receiver.SubscriberTier = giftedSubEvent.PlanTierNumber;
            giftedSubEvent.Receiver.TotalSubsReceived += (uint)giftedSubEvent.MonthsGifted;
            giftedSubEvent.Receiver.TotalMonthsSubbed += (uint)giftedSubEvent.MonthsGifted;

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                for (int i = 0; i < giftedSubEvent.MonthsGifted; i++)
                {
                    currency.AddAmount(giftedSubEvent.Gifter, currency.OnSubscribeBonus);
                }
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                if (giftedSubEvent.Gifter.MeetsRole(streamPass.UserPermission))
                {
                    streamPass.AddAmount(giftedSubEvent.Gifter, streamPass.SubscribeBonus);
                }
            }

            if (fireEventCommand)
            {
                CommandParametersModel parameters = new CommandParametersModel(giftedSubEvent.Gifter, StreamingPlatformTypeEnum.Twitch);
                parameters.SpecialIdentifiers["usersubplanname"] = giftedSubEvent.PlanName;
                parameters.SpecialIdentifiers["usersubplan"] = giftedSubEvent.PlanTier;
                parameters.SpecialIdentifiers["usersubpoints"] = TwitchSubEventModel.GetSubPoints(giftedSubEvent.PlanTierNumber).ToString();
                parameters.SpecialIdentifiers["usersubmonthsgifted"] = giftedSubEvent.MonthsGifted.ToString();
                parameters.SpecialIdentifiers["isanonymous"] = giftedSubEvent.IsAnonymous.ToString();
                parameters.Arguments.Add(giftedSubEvent.Receiver.Username);
                parameters.TargetUser = giftedSubEvent.Receiver;
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscriptionGifted, parameters);

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(giftedSubEvent.Gifter, string.Format(MixItUp.Base.Resources.AlertSubscriptionGiftedTier, giftedSubEvent.Gifter.FullDisplayName, giftedSubEvent.PlanTier, giftedSubEvent.Receiver.FullDisplayName), ChannelSession.Settings.AlertGiftedSubColor));

                EventService.SubscriptionGiftedOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, giftedSubEvent.Receiver, giftedSubEvent.Gifter, tier: giftedSubEvent.PlanTierNumber));
            }
        }

        private async Task ProcessMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent)
        {
            CommandParametersModel parameters = new CommandParametersModel(massGiftedSubEvent.Gifter, StreamingPlatformTypeEnum.Twitch);
            parameters.SpecialIdentifiers["subsgiftedamount"] = massGiftedSubEvent.TotalGifted.ToString();
            parameters.SpecialIdentifiers["substotalpoints"] = (massGiftedSubEvent.TotalGifted * TwitchSubEventModel.GetSubPoints(massGiftedSubEvent.PlanTierNumber)).ToString();
            parameters.SpecialIdentifiers["subsgiftedlifetimeamount"] = massGiftedSubEvent.LifetimeGifted.ToString();
            parameters.SpecialIdentifiers["usersubplan"] = massGiftedSubEvent.PlanTier;
            parameters.SpecialIdentifiers["isanonymous"] = massGiftedSubEvent.IsAnonymous.ToString();

            if (massGiftedSubEvent.Subs.Count > 0)
            {
                parameters.TargetUser = massGiftedSubEvent.Subs.First().Receiver;
            }

            foreach (TwitchGiftedSubEventModel sub in massGiftedSubEvent.Subs)
            {
                parameters.Arguments.Add(sub.Receiver.Username);
            }

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelMassSubscriptionsGifted, parameters);

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(massGiftedSubEvent.Gifter, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGiftedTier, massGiftedSubEvent.Gifter.FullDisplayName, massGiftedSubEvent.TotalGifted, massGiftedSubEvent.PlanTier), ChannelSession.Settings.AlertMassGiftedSubColor));

            List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
            for (int i = 0; i < massGiftedSubEvent.Subs.Count; i++)
            {
                subscriptions.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, massGiftedSubEvent.Subs[i].Receiver, massGiftedSubEvent.Gifter, tier: massGiftedSubEvent.PlanTierNumber));
            }
            EventService.MassSubscriptionsGiftedOccurred(subscriptions);
        }

        private async void PubSub_OnChannelPointsRedeemed(object sender, PubSubChannelPointsRedemptionEventModel packet)
        {
            PubSubChannelPointsRedeemedEventModel redemption = packet.redemption;

            if (channelPointRewardRedeems.Contains(redemption.id))
            {
                return;
            }
            channelPointRewardRedeems.Add(redemption.id);

            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: redemption.user.id, platformUsername: redemption.user.login);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(redemption.user));
            }

            List<string> arguments = null;
            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
            eventCommandSpecialIdentifiers["rewardname"] = redemption.reward.title;
            eventCommandSpecialIdentifiers["rewardcost"] = redemption.reward.cost.ToString();
            if (!string.IsNullOrEmpty(redemption.user_input))
            {
                eventCommandSpecialIdentifiers["message"] = redemption.user_input;
                arguments = new List<string>(redemption.user_input.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            if (string.IsNullOrEmpty(await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, redemption.user_input)))
            {
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelPointsRedeemed, new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch, arguments, eventCommandSpecialIdentifiers));

                TwitchChannelPointsCommandModel command = ServiceManager.Get<CommandService>().TwitchChannelPointsCommands.FirstOrDefault(c => string.Equals(c.ChannelPointRewardID.ToString(), redemption.reward.id, StringComparison.CurrentCultureIgnoreCase));
                if (command == null)
                {
                    command = ServiceManager.Get<CommandService>().TwitchChannelPointsCommands.FirstOrDefault(c => string.Equals(c.Name, redemption.reward.title, StringComparison.CurrentCultureIgnoreCase));
                }

                if (command != null)
                {
                    Dictionary<string, string> channelPointSpecialIdentifiers = new Dictionary<string, string>(eventCommandSpecialIdentifiers);
                    await ServiceManager.Get<CommandService>().Queue(command, new CommandParametersModel(user, platform: StreamingPlatformTypeEnum.Twitch, arguments: arguments, specialIdentifiers: channelPointSpecialIdentifiers));
                }
            }
            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertTwitchChannelPointRedeemed, user.FullDisplayName, redemption.reward.title), ChannelSession.Settings.AlertTwitchChannelPointsColor));
        }

        private async void PubSub_OnWhisperReceived(object sender, PubSubWhisperEventModel packet)
        {
            if (!string.IsNullOrEmpty(packet.body))
            {
                UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.from_id.ToString());
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet));
                }

                UserV2ViewModel recipient = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: packet.recipient.id.ToString());
                if (recipient == null)
                {
                    recipient = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(packet.recipient));
                }

                await ServiceManager.Get<ChatService>().AddMessage(new TwitchChatMessageViewModel(packet, user, recipient));
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
