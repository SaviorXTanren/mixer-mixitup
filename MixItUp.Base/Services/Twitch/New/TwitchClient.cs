using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Clients.EventSub;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.New
{
    public class TwitchClient
    {
        public const string TwitchEventSubConnectionURL = "wss://eventsub.wss.twitch.tv/ws";

        public const string PrimeSubPlan = "Prime";

        public static int GetSubPoints(int tier)
        {
            if (tier == 3)
            {
                return 6;
            }
            return tier;
        }

        public static int GetSubTierNumberFromText(string subPlan)
        {
            if (int.TryParse(subPlan, out int subPlanNumber) && subPlanNumber >= 1000)
            {
                return subPlanNumber / 1000;
            }
            return 1;
        }

        private readonly IReadOnlyDictionary<string, string> DesiredSubscriptionsAndVersions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "stream.online", null },
            { "stream.offline", null },

            { "channel.update", "2" },

            { "channel.follow", "2" },
            { "channel.raid", null },

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

            { "user.whisper.message", null },

            { "channel.subscribe", null },
            { "channel.subscription.message", null },
            { "channel.subscription.gift", null },

            { "channel.channel_points_custom_reward_redemption.add", null },
        };

        public bool IsConnected { get { return webSocket != null && webSocket.IsOpen() && this.eventSubSubscriptionsConnected; } }

        public bool StreamLiveStatus { get; private set; }

        private AdvancedClientWebSocket webSocket;

        private bool eventSubSubscriptionsConnected = false;

        private HashSet<string> followCache = new HashSet<string>();
        private HashSet<string> channelPointRewardRedeemsCache = new HashSet<string>();

        private int lastHypeTrainLevel = 1;

        public TwitchClient()
        {
            webSocket = new AdvancedClientWebSocket();

            if (ChannelSession.AppSettings.DiagnosticLogging)
            {
                webSocket.PacketSent += WebSocket_PacketSent;
            }
            webSocket.PacketReceived += UserWebSocket_PacketReceived;
            webSocket.Disconnected += WebSocket_Disconnected;
        }

        public async Task<Result> Connect()
        {
            if (await webSocket.Connect(TwitchEventSubConnectionURL))
            {
                await Task.Delay(2500);

                for (int i = 0; i < 15; i++)
                {
                    if (this.eventSubSubscriptionsConnected)
                    {
                        return new Result();
                    }
                    await Task.Delay(1000);
                }
            }

            return new Result(Resources.TwitchEventServiceFailedToConnectEventSub);
        }

        public async Task Disconnect()
        {
            await webSocket.Disconnect();
        }

        private async Task ProcessSessionWelcome(WelcomeMessage message)
        {
            IEnumerable<EventSubSubscriptionModel> allSubs = await ServiceManager.Get<TwitchSession>().StreamerService.GetEventSubSubscriptions();

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
                    await ServiceManager.Get<TwitchSession>().StreamerService.DeleteEventSubSubscription(sub.id);
                }
            }

            foreach (string missingSub in missingSubs)
            {
                if (string.Equals(missingSub, "channel.follow", StringComparison.OrdinalIgnoreCase))
                {
                    Dictionary<string, string> conditions = new Dictionary<string, string>
                        {
                            { "broadcaster_user_id", ServiceManager.Get<TwitchSession>().StreamerID },
                            { "moderator_user_id", ServiceManager.Get<TwitchSession>().StreamerID }
                        };

                    await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub], conditions);
                }
                else if (missingSub.Equals("channel.raid", StringComparison.OrdinalIgnoreCase))
                {
                    await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub],
                        new Dictionary<string, string> { { "from_broadcaster_user_id", ServiceManager.Get<TwitchSession>().StreamerID } });
                    await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub],
                        new Dictionary<string, string> { { "to_broadcaster_user_id", ServiceManager.Get<TwitchSession>().StreamerID } });
                }
                else
                {
                    await this.RegisterEventSubSubscription(missingSub, message, DesiredSubscriptionsAndVersions[missingSub]);
                }
            }

            this.eventSubSubscriptionsConnected = true;

            IEnumerable<ChannelFollowerModel> followers = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIFollowers(ServiceManager.Get<TwitchSession>().Streamer, maxResults: 100);
            if (followers != null)
            {
                this.followCache.Clear();
                foreach (ChannelFollowerModel follow in followers)
                {
                    this.followCache.Add(follow.user_id);
                }
            }
        }

        private async Task RegisterEventSubSubscription(string type, WelcomeMessage message, string version = null, Dictionary<string, string> conditions = null)
        {
            try
            {
                if (conditions == null)
                {
                    conditions = new Dictionary<string, string> { { "broadcaster_user_id", ServiceManager.Get<TwitchSession>().StreamerID } };
                }
                await ServiceManager.Get<TwitchSession>().StreamerService.CreateEventSubSubscription(type, "websocket", conditions, message.Payload.Session.Id, version: version);
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log(LogLevel.Error, $"Failed to connect EventSub for {type}");

                // Note: Do not re-throw, but log and move on, better to miss some events than to cause a retry loop
            }
        }

        private async Task ProcessNotification(NotificationMessage message)
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

                    case "channel.follow":
                        await HandleFollow(message.Payload.Event);
                        break;
                    case "channel.raid":
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

                    case "user.whisper.message":
                        await HandleWhisper(message.Payload.Event);
                        break;

                    case "channel.subscribe":
                        await HandleSubscription(message.Payload.Event);
                        break;
                    case "channel.subscription.message":
                        await HandleSubscriptionMessage(message.Payload.Event);
                        break;
                    case "channel.subscription.gift":
                        await HandleSubscriptionGift(message.Payload.Event);
                        break;

                    case "channel.channel_points_custom_reward_redemption.add":
                        await HandleChannelPointRewardAddCustomRedemption(message.Payload.Event);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.ForceLog(LogLevel.Error, JSONSerializerHelper.SerializeToString(message.Payload));
            }
        }

        private async Task HandleOnline(JObject payload)
        {
            this.StreamLiveStatus = true;
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        private async Task HandleOffline(JObject payload)
        {
            this.StreamLiveStatus = false;
            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        private async Task HandleChannelUpdate(JObject payload)
        {
            CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch);

            string gameID = payload["category_id"].ToString();
            GameModel game = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIGameByID(gameID);

            parameters.SpecialIdentifiers["streamtitle"] = payload["title"].ToString();
            parameters.SpecialIdentifiers["streamgameid"] = gameID;
            parameters.SpecialIdentifiers["streamgameimage"] = game?.box_art_url ?? string.Empty;
            parameters.SpecialIdentifiers["streamgame"] = parameters.SpecialIdentifiers["streamgamename"] = payload["category_name"].ToString();

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelUpdated, parameters);
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

            await this.AddFollow(user);
        }

        private async Task AddFollow(UserV2ViewModel user)
        {
            if (!this.followCache.Contains(user.PlatformID))
            {
                this.followCache.Add(user.PlatformID);

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

        private async Task HandlePredictionLock(JObject payload)
        {

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

        private async Task HandleWhisper(JObject payload)
        {
            string from_id = payload["from_user_id"].Value<string>();

            UserV2ViewModel from = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: from_id);
            if (from == null)
            {
                string from_username = payload["from_user_login"].Value<string>();
                string from_displayname = payload["from_user_name"].Value<string>();

                from = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(from_id, from_username, from_displayname));
            }

            string to_id = payload["to_user_id"].Value<string>();
            UserV2ViewModel to = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: to_id);

            string message = payload["whisper"]["text"].Value<string>();

            await ServiceManager.Get<ChatService>().AddMessage(new TwitchChatMessageViewModel(from, message, recipient: to));
        }

        private async Task HandleSubscription(JObject payload)
        {
            UserSubscriptionNotification subscription = payload.ToObject<UserSubscriptionNotification>();

            UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscription.user_id, platformUsername: subscription.user_name);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(subscription));
            }

            CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch);

            if (subEvent.IsPrimeUpgrade || subEvent.IsGiftedUpgrade)
            {
                var subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBroadcasterSubscription(ServiceManager.Get<TwitchSessionService>().User, ((TwitchUserPlatformV2Model)subEvent.User.PlatformModel).GetTwitchNewAPIUserModel());
                if (subscription != null)
                {
                    subEvent.PlanTier = TwitchPubSubService.GetSubTierNameFromText(subscription.tier);
                    subEvent.PlanName = subscription.tier;
                }
            }

            user.Roles.Add(UserRoleEnum.Subscriber);
            user.SubscribeDate = DateTimeOffset.Now;
            user.SubscriberTier = subscription.TierNumber;

            parameters.SpecialIdentifiers["message"] = subEvent.Message;
            parameters.SpecialIdentifiers["usersubplanname"] = subEvent.PlanName;
            parameters.SpecialIdentifiers["usersubplan"] = subEvent.PlanTier;
            parameters.SpecialIdentifiers["usersubpoints"] = subscription.SubPoints.ToString();
            parameters.SpecialIdentifiers["isprimeupgrade"] = subEvent.IsPrimeUpgrade.ToString();
            parameters.SpecialIdentifiers["isgiftupgrade"] = subEvent.IsGiftedUpgrade.ToString();

            if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters))
            {
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                user.TotalMonthsSubbed++;

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

                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters);
            }

            EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, user, tier: subscription.TierNumber));

            if (subEvent.IsPrimeUpgrade)
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedPrimeSubscriptionTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
            else if (subEvent.IsGiftedUpgrade)
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedGiftedSubscriptionTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
            else
            {
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertSubscribedTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            }
        }

        private async Task HandleSubscriptionMessage(JObject payload)
        {
            UserSubscriptionMessageNotification subscriptionMessage = payload.ToObject<UserSubscriptionMessageNotification>();


        }

        private async Task HandleSubscriptionGift(JObject payload)
        {
            UserSubscriptionGiftNotification subscriptionGift = payload.ToObject<UserSubscriptionGiftNotification>();

            UserV2ViewModel gifter = subscriptionGift.is_anonymous ? UserV2ViewModel.CreateUnassociated() : await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscriptionGift.user_id);
            if (gifter == null)
            {
                gifter = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(subscriptionGift));
            }




        }

        private async Task HandleChannelPointRewardAddCustomRedemption(JObject payload)
        {
            ChannelPointRewardCustomRedemptionNotification redemption = payload.ToObject<ChannelPointRewardCustomRedemptionNotification>();

            if (channelPointRewardRedeemsCache.Contains(redemption.id))
            {
                return;
            }
            channelPointRewardRedeemsCache.Add(redemption.id);

            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: redemption.user_id);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(redemption));
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

        private async void UserWebSocket_PacketReceived(object sender, string packet)
        {
            try
            {
                if (ChannelSession.IsDebug())
                {
                    Logger.Log(LogLevel.Debug, "Twitch EventSub Packet Received: " + packet);
                }

                if (!string.IsNullOrEmpty(packet))
                {
                    JObject jsonData = JObject.Parse(packet);
                    string messageTypeString = jsonData["metadata"]?["message_type"]?.Value<string>();
                    if (string.Equals(messageTypeString, "session_welcome", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessSessionWelcome(jsonData.ToObject<WelcomeMessage>());
                    }
                    else if (string.Equals(messageTypeString, "session_keepalive", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: If not received in 10 seconds, reconnect
                    }
                    else if (string.Equals(messageTypeString, "session_reconnect", StringComparison.OrdinalIgnoreCase))
                    {
                        // NOTE: This SHOULD auto-disconnect

                        // The URL of the reconnection message bight have encoded characters in it (EX: "cell-c.eventsub.wss.twitch.tv/ws?challenge=XXX\u0026id=XXX" where "\u0026" is "&").
                        // There have also been issues noted with being able to re-connect properly to the URL specified.
                    }
                    else if (string.Equals(messageTypeString, "revocation", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: Disconnect and reconnect
                        await this.Disconnect();
                    }
                    else if (string.Equals(messageTypeString, "notification", StringComparison.OrdinalIgnoreCase))
                    {
                        await ProcessNotification(jsonData.ToObject<NotificationMessage>());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void WebSocket_PacketSent(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Twitch EventSub Packet Sent: {0}", packet));
        }

        private void WebSocket_Disconnected(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred(Resources.TrovoUserChat);
        }
    }
}
