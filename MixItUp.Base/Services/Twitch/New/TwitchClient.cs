using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Clients.Chat;
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
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.New
{
    public class TwitchSubEventModel
    {
        public const string PrimeSubPlan = "Prime";

        public UserV2ViewModel User { get; set; }
        public UserV2ViewModel Gifter { get; set; }

        public int Tier { get; set; } = 1;
        public string TierName { get; set; }

        public string PlanName { get; set; }

        public string Message { get; set; } = string.Empty;

        public int Duration { get; set; }

        public int SubPoints { get; set; }

        public bool IsAnonymous { get; set; }
        public bool IsPrime { get; set; }
        public bool IsPrimeUpgrade { get; set; }
        public bool IsGiftedUpgrade { get; set; }

        public string CommunityGiftID { get; set; }

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchSubEventModel(UserV2ViewModel user, ChatNotification notification, UserV2ViewModel gifter = null)
        {
            this.User = user;
            this.Gifter = gifter;

            if (notification.NoticeType == ChatNotificationType.sub)
            {
                this.Tier = notification.sub.TierNumber;
                this.SubPoints = notification.sub.SubPoints;
                this.IsPrime = notification.sub.is_prime;
            }
            else if (notification.NoticeType == ChatNotificationType.resub)
            {
                this.Tier = notification.resub.TierNumber;
                this.Duration = notification.resub.duration_months;
                this.SubPoints = notification.resub.SubPoints;
                this.IsPrime = notification.resub.is_prime;
                this.IsAnonymous = notification.resub.gifter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.sub_gift)
            {
                this.Tier = notification.sub_gift.TierNumber;
                this.Duration = notification.sub_gift.duration_months;
                this.SubPoints = notification.sub_gift.SubPoints;
                this.CommunityGiftID = notification.sub_gift.community_gift_id;
                this.IsAnonymous = notification.chatter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.gift_paid_upgrade)
            {
                this.IsGiftedUpgrade = true;
            }
            else if (notification.NoticeType == ChatNotificationType.prime_paid_upgrade)
            {
                this.IsPrimeUpgrade = true;
            }

            // Shared
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub)
            {
                this.Tier = notification.shared_chat_sub.TierNumber;
                this.SubPoints = notification.shared_chat_sub.SubPoints;
                this.IsPrime = notification.shared_chat_sub.is_prime;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_resub)
            {
                this.Tier = notification.shared_chat_resub.TierNumber;
                this.Duration = notification.shared_chat_resub.duration_months;
                this.SubPoints = notification.shared_chat_resub.SubPoints;
                this.IsPrime = notification.shared_chat_resub.is_prime;
                this.IsAnonymous = notification.shared_chat_resub.gifter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub_gift)
            {
                this.Tier = notification.shared_chat_sub_gift.TierNumber;
                this.Duration = notification.shared_chat_sub_gift.duration_months;
                this.SubPoints = notification.shared_chat_sub_gift.SubPoints;
                this.CommunityGiftID = notification.shared_chat_sub_gift.community_gift_id;
                this.IsAnonymous = notification.chatter_is_anonymous;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_pay_it_forward)
            {
                this.IsGiftedUpgrade = true;
            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_prime_paid_upgrade)
            {
                this.IsPrimeUpgrade = true;
            }

            if (this.IsPrime)
            {
                this.TierName = PrimeSubPlan;
            }
            else
            {
                this.TierName = $"{MixItUp.Base.Resources.Tier} {this.Tier}";
            }
            this.PlanName = this.TierName;
        }
    }

    public class TwitchMassGiftedSubEventModel
    {
        public UserV2ViewModel Gifter { get; set; }

        public string CommunityGiftID { get; set; }

        public int TotalGifted { get; set; }

        public int LifetimeGifted { get; set; }

        public int Tier { get; set; }

        public string TierName { get; set; }

        public int TotalSubPoints { get; set; }

        public bool IsAnonymous { get; set; }

        public List<TwitchSubEventModel> Subs { get; set; } = new List<TwitchSubEventModel>();

        public DateTimeOffset Processed { get; set; } = DateTimeOffset.Now;

        public TwitchMassGiftedSubEventModel(ChatNotificationCommunitySubGift communitySubGift, UserV2ViewModel gifter, bool isAnonymous)
        {
            this.Gifter = gifter;
            this.CommunityGiftID = communitySubGift.id;

            this.TotalGifted = communitySubGift.total;
            this.LifetimeGifted = communitySubGift.cumulative_total.GetValueOrDefault();

            this.Tier = communitySubGift.TierNumber;
            this.TierName = this.TierName = $"{MixItUp.Base.Resources.Tier} {this.Tier}";

            this.TotalSubPoints = communitySubGift.TotalSubPoints;

            this.IsAnonymous = isAnonymous;
        }
    }

    public class TwitchBitsCheeredModel
    {
        public UserV2ViewModel User { get; set; }

        public int Amount { get; set; }

        public TwitchChatMessageViewModel Message { get; set; }

        public TwitchBitsCheeredModel(UserV2ViewModel user, int amount, TwitchChatMessageViewModel message)
        {
            this.User = user;
            this.Amount = amount;
            this.Message = message;
        }
    }

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

            //{ "channel.subscribe", null },
            //{ "channel.subscription.message", null },
            //{ "channel.subscription.gift", null },

            { "channel.channel_points_custom_reward_redemption.add", null },

            { "channel.chat.message", null },
            { "channel.chat.message_delete", null },
            { "channel.chat.notification", null },
            { "channel.chat.user_message_hold", null },
            { "channel.chat.user_message_update", null },
            { "channel.chat.clear", null },
            { "channel.chat.clear_user_messages", null },

            { "channel.shared_chat.begin", null },
            { "channel.shared_chat.update", null },
            { "channel.shared_chat.end", null },

            { "channel.cheer", null },

            { "channel.moderate", "2" },
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

                    //case "channel.subscribe":
                    //    await HandleSubscription(message.Payload.Event);
                    //    break;
                    //case "channel.subscription.message":
                    //    await HandleSubscriptionMessage(message.Payload.Event);
                    //    break;
                    //case "channel.subscription.gift":
                    //    await HandleSubscriptionGift(message.Payload.Event);
                    //    break;

                    case "channel.channel_points_custom_reward_redemption.add":
                        await HandleChannelPointRewardAddCustomRedemption(message.Payload.Event);
                        break;

                    case "channel.chat.message":
                        await HandleChatMessage(message.Payload.Event);
                        break;
                    case "channel.chat.message_delete":
                        await HandleChatMessageDeleted(message.Payload.Event);
                        break;
                    case "channel.chat.notification":
                        await HandleChatNotification(message.Payload.Event);
                        break;
                    case "channel.chat.user_message_hold":
                        await HandleChatMessageHeld(message.Payload.Event);
                        break;
                    case "channel.chat.user_message_update":
                        await HandleChatMessageHoldUpdate(message.Payload.Event);
                        break;
                    case "channel.chat.clear":
                        await HandleChatClear(message.Payload.Event);
                        break;
                    case "channel.chat.clear_user_messages":
                        await HandleChatUserClear(message.Payload.Event);
                        break;

                    case "channel.shared_chat.begin":
                        await HandleSharedChatBegin(message.Payload.Event);
                        break;
                    case "channel.shared_chat.update":
                        await HandleSharedChatUpdate(message.Payload.Event);
                        break;
                    case "channel.shared_chat.end":
                        await HandleSharedChatEnd(message.Payload.Event);
                        break;

                    case "channel.moderate":
                        await HandleModeration(message.Payload.Event);
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

        //private async Task HandleSubscription(JObject payload)
        //{
        //    UserSubscriptionNotification subscription = payload.ToObject<UserSubscriptionNotification>();
        //    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscription.user_id, platformUsername: subscription.user_name);
        //    if (user == null)
        //    {
        //        user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(subscription));
        //    }
        //    await this.ProcessSub(new TwitchSubEventModel(user, subscription));
        //}

        //private async Task HandleSubscriptionMessage(JObject payload)
        //{
        //    UserSubscriptionMessageNotification subscriptionMessage = payload.ToObject<UserSubscriptionMessageNotification>();
        //}

        //private async Task HandleSubscriptionGift(JObject payload)
        //{
        //    UserSubscriptionGiftNotification subscriptionGift = payload.ToObject<UserSubscriptionGiftNotification>();
        //    UserV2ViewModel gifter = subscriptionGift.is_anonymous ? UserV2ViewModel.CreateUnassociated() : await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: subscriptionGift.user_id);
        //    if (gifter == null)
        //    {
        //        gifter = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(subscriptionGift));
        //    }
        //}

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

        private async Task HandleChatMessage(JObject payload)
        {
            ChatMessageNotification messageNotification = payload.ToObject<ChatMessageNotification>();

            Logger.ForceLog(LogLevel.Debug, "Twitch Chat Message Received: " + JSONSerializerHelper.SerializeToString(messageNotification));

            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: messageNotification.chatter_user_id);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(messageNotification));
            }
            user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch).SetUserProperties(messageNotification);

            TwitchChatMessageViewModel message = new TwitchChatMessageViewModel(messageNotification, user);
            await ServiceManager.Get<ChatService>().AddMessage(message);

            if (message.HasBits)
            {
                int bits = messageNotification.cheer.bits;

                foreach (CurrencyModel bitsCurrency in ChannelSession.Settings.Currency.Values.Where(c => c.SpecialTracking == CurrencySpecialTrackingEnum.Bits))
                {
                    bitsCurrency.AddAmount(user, bits);
                }

                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                {
                    if (user.MeetsRole(streamPass.UserPermission))
                    {
                        streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.BitsBonus * bits));
                    }
                }

                if (user.HasPlatformData(StreamingPlatformTypeEnum.Twitch))
                {
                    ((TwitchUserPlatformV2Model)user.PlatformModel).TotalBitsCheered += bits;
                }

                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredUserData] = user.ID;
                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestBitsCheeredAmountData] = bits;

                if (string.IsNullOrEmpty(await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, message.PlainTextMessageNoCheermotes)))
                {
                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Twitch, message.ToArguments());
                    parameters.SpecialIdentifiers["bitsamount"] = bits.ToString();
                    parameters.SpecialIdentifiers["messagenocheermotes"] = message.PlainTextMessageNoCheermotes;
                    parameters.SpecialIdentifiers["message"] = message.PlainTextMessage;
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelBitsCheered, parameters);

                    TwitchBitsCommandModel command = ServiceManager.Get<CommandService>().TwitchBitsCommands.FirstOrDefault(c => c.IsEnabled && c.IsSingle && c.StartingAmount == bits);
                    if (command == null)
                    {
                        command = ServiceManager.Get<CommandService>().TwitchBitsCommands.Where(c => c.IsEnabled && c.IsRange).OrderBy(c => c.Range).FirstOrDefault(c => c.IsInRange(bits));
                    }

                    if (command != null)
                    {
                        await ServiceManager.Get<CommandService>().Queue(command, parameters);
                    }
                }
                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertTwitchBitsCheered, user.FullDisplayName, bits), ChannelSession.Settings.AlertTwitchBitsCheeredColor));
                EventService.TwitchBitsCheeredOccurred(new TwitchBitsCheeredModel(user, bits, message));
            }
        }

        private async Task HandleChatMessageDeleted(JObject payload)
        {
            ChatMessageDeletedNotification messageDeleted = payload.ToObject<ChatMessageDeletedNotification>();

            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: messageDeleted.target_user_id);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(messageDeleted));
            }

            await ServiceManager.Get<ChatService>().DeleteMessage(new TwitchChatMessageViewModel(messageDeleted, user), externalDeletion: true);
        }

        private async Task HandleChatNotification(JObject payload)
        {
            ChatNotification notification = payload.ToObject<ChatNotification>();

            Logger.ForceLog(LogLevel.Debug, "Twitch Chat Message Received: " + JSONSerializerHelper.SerializeToString(notification));

            UserV2ViewModel user;
            if (notification.chatter_is_anonymous)
            {
                user = UserV2ViewModel.CreateUnassociated(Resources.AnAnonymousGifter);
            }
            else
            {
                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: notification.chatter_user_id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(notification));
                }
                user.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch).SetUserProperties(notification);
            }

            if (notification.NoticeType == ChatNotificationType.announcement)
            {
                TwitchChatMessageViewModel message = new TwitchChatMessageViewModel(notification, user);
                await ServiceManager.Get<ChatService>().AddMessage(message);
            }

            // Subs
            else if (notification.NoticeType == ChatNotificationType.sub)
            {
                await this.ProcessSub(new TwitchSubEventModel(user, notification));
            }
            else if (notification.NoticeType == ChatNotificationType.resub)
            {
                if (notification.resub.is_gift)
                {
                    UserV2ViewModel gifter;
                    if (notification.resub.gifter_is_anonymous)
                    {
                        gifter = UserV2ViewModel.CreateUnassociated(Resources.AnAnonymousGifter);
                    }
                    else
                    {
                        gifter = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: notification.resub.gifter_user_id);
                        if (gifter == null)
                        {
                            gifter = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(notification.resub));
                        }
                    }

                    await this.ProcessSub(new TwitchSubEventModel(user, notification, gifter));
                }
                else
                {
                    await this.ProcessSub(new TwitchSubEventModel(user, notification));
                }
            }
            else if (notification.NoticeType == ChatNotificationType.sub_gift)
            {
                UserV2ViewModel gifter = user;

                user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: notification.sub_gift.recipient_user_id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(notification.sub_gift));
                }

                if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0)
                {
                    ServiceManager.Get<TwitchSession>().AddGiftedSub(new TwitchSubEventModel(user, notification, gifter));
                }
                else
                {
                    await this.ProcessSub(new TwitchSubEventModel(user, notification, gifter));
                }
            }
            else if (notification.NoticeType == ChatNotificationType.gift_paid_upgrade)
            {
                await this.ProcessSub(new TwitchSubEventModel(user, notification));
            }
            else if (notification.NoticeType == ChatNotificationType.prime_paid_upgrade)
            {
                await this.ProcessSub(new TwitchSubEventModel(user, notification));
            }
            else if (notification.NoticeType == ChatNotificationType.community_sub_gift)
            {
                await ServiceManager.Get<TwitchSession>().AddMassGiftedSub(new TwitchMassGiftedSubEventModel(notification.community_sub_gift, user, notification.chatter_is_anonymous));
            }

            // Shared
            else if (notification.NoticeType == ChatNotificationType.shared_chat_announcement)
            {

            }


            else if (notification.NoticeType == ChatNotificationType.shared_chat_raid)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_resub)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_sub_gift)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_gift_paid_upgrade)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_prime_paid_upgrade)
            {

            }
            else if (notification.NoticeType == ChatNotificationType.shared_chat_community_sub_gift)
            {

            }
        }

        private async Task HandleChatMessageHeld(JObject payload)
        {
            Logger.ForceLog(LogLevel.Debug, "Packet Received: " + JSONSerializerHelper.SerializeToString(payload));
        }

        private async Task HandleChatMessageHoldUpdate(JObject payload)
        {
            Logger.ForceLog(LogLevel.Debug, "Packet Received: " + JSONSerializerHelper.SerializeToString(payload));
        }

        private async Task HandleChatClear(JObject payload)
        {
            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Twitch, MixItUp.Base.Resources.ChatCleared, ChannelSession.Settings.AlertModerationColor));
            ChatService.ChatCleared();
        }

        private async Task HandleChatUserClear(JObject payload)
        {
            ChatUserClearNotification userClear = payload.ToObject<ChatUserClearNotification>();

            UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: userClear.target_user_id);
            if (user == null)
            {
                user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(userClear));
            }

            // TODO: Mark all message by user as deleted
        }

        private async Task HandleSharedChatBegin(JObject payload)
        {
            Logger.ForceLog(LogLevel.Debug, "Packet Received: " + JSONSerializerHelper.SerializeToString(payload));
        }

        private async Task HandleSharedChatUpdate(JObject payload)
        {
            Logger.ForceLog(LogLevel.Debug, "Packet Received: " + JSONSerializerHelper.SerializeToString(payload));
        }

        private async Task HandleSharedChatEnd(JObject payload)
        {
            Logger.ForceLog(LogLevel.Debug, "Packet Received: " + JSONSerializerHelper.SerializeToString(payload));
        }

        private async Task HandleModeration(JObject payload)
        {
            ModerationNotification moderation = payload.ToObject<ModerationNotification>();

            UserV2ViewModel moderator = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: moderation.moderator_user_id);
            if (moderator == null)
            {
                moderator = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(moderation));
            }

            if (moderation.ActionType == ModerationNotificationActionType.ban)
            {
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: moderation.ban.user_id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(moderation.ban));
                }

                CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch);
                parameters.Arguments.Add("@" + user.Username);
                parameters.TargetUser = user;
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserBan, parameters);

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertBanned, user.FullDisplayName), ChannelSession.Settings.AlertModerationColor));

                await ServiceManager.Get<UserService>().RemoveActiveUser(user);

                ChatService.ChatUserBanned(user);
            }
            else if (moderation.ActionType == ModerationNotificationActionType.delete)
            {
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: moderation.delete.user_id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(moderation.delete));
                }
                await ServiceManager.Get<ChatService>().DeleteMessage(new TwitchChatMessageViewModel(moderation.delete, user), externalDeletion: true);
            }
            else if (moderation.ActionType == ModerationNotificationActionType.timeout)
            {
                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: moderation.timeout.user_id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(moderation.timeout));
                }

                int timeoutLength = moderation.timeout.TotalSeconds;

                CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch);
                parameters.Arguments.Add("@" + user.Username);
                parameters.TargetUser = user;
                parameters.SpecialIdentifiers["timeoutlength"] = timeoutLength.ToString();
                parameters.SpecialIdentifiers["timeoutreason"] = moderation.timeout.reason;
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserTimeout, parameters);

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertTimedOut, user.FullDisplayName, timeoutLength), ChannelSession.Settings.AlertModerationColor));

                ChatService.ChatUserTimedOut(user);
            }
            else if (moderation.ActionType == ModerationNotificationActionType.mod)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.unmod)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.vip)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.unvip)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.shared_chat_ban)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.shared_chat_delete)
            {

            }
            else if (moderation.ActionType == ModerationNotificationActionType.shared_chat_timeout)
            {

            }
        }

        private async Task ProcessSub(TwitchSubEventModel subscription)
        {
            // NEW SUB

            //CommandParametersModel parameters = new CommandParametersModel(subscription.User, StreamingPlatformTypeEnum.Twitch);

            //if (subEvent.IsPrimeUpgrade || subEvent.IsGiftedUpgrade)
            //{
            //    var subscription = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetBroadcasterSubscription(ServiceManager.Get<TwitchSessionService>().User, ((TwitchUserPlatformV2Model)subEvent.User.PlatformModel).GetTwitchNewAPIUserModel());
            //    if (subscription != null)
            //    {
            //        subEvent.PlanTier = TwitchPubSubService.GetSubTierNameFromText(subscription.tier);
            //        subEvent.PlanName = subscription.tier;
            //    }
            //}

            //subscription.User.Roles.Add(UserRoleEnum.Subscriber);
            //subscription.User.SubscribeDate = DateTimeOffset.Now;
            //subscription.User.SubscriberTier = subscription.Tier;

            //parameters.SpecialIdentifiers["message"] = subEvent.Message;
            //parameters.SpecialIdentifiers["usersubplanname"] = subEvent.PlanName;
            //parameters.SpecialIdentifiers["usersubplan"] = subEvent.PlanTier;
            //parameters.SpecialIdentifiers["usersubpoints"] = subscription.SubPoints.ToString();
            //parameters.SpecialIdentifiers["isprimeupgrade"] = subEvent.IsPrimeUpgrade.ToString();
            //parameters.SpecialIdentifiers["isgiftupgrade"] = subEvent.IsGiftedUpgrade.ToString();

            //if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters))
            //{
            //    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = subscription.User.ID;
            //    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

            //    subscription.User.TotalMonthsSubbed++;

            //    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            //    {
            //        currency.AddAmount(subscription.User, currency.OnSubscribeBonus);
            //    }

            //    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            //    {
            //        if (parameters.User.MeetsRole(streamPass.UserPermission))
            //        {
            //            streamPass.AddAmount(subscription.User, streamPass.SubscribeBonus);
            //        }
            //    }

            //    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscribed, parameters);
            //}

            //EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, subscription.User, tier: subscription.Tier));

            //if (subEvent.IsPrimeUpgrade)
            //{
            //    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedPrimeSubscriptionTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            //}
            //else if (subEvent.IsGiftedUpgrade)
            //{
            //    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertContinuedGiftedSubscriptionTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            //}
            //else
            //{
            //    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(subEvent.User, string.Format(MixItUp.Base.Resources.AlertSubscribedTier, user.FullDisplayName, subEvent.PlanTier), ChannelSession.Settings.AlertSubColor));
            //}



            // RESUB

            //int months = Math.Max(packet.streak_months, packet.cumulative_months);
            //string planTier = TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
            //string message = (packet.sub_message.ContainsKey("message") && packet.sub_message["message"] != null) ? packet.sub_message["message"].ToString() : string.Empty;

            //user.Roles.Add(UserRoleEnum.Subscriber);
            //user.SubscribeDate = DateTimeOffset.Now.SubtractMonths(months - 1);
            //user.SubscriberTier = TwitchPubSubService.GetSubTierNumberFromText(packet.sub_plan);

            //CommandParametersModel parameters = new CommandParametersModel(user, new List<string>(message.Split(new char[] { ' ' })));
            //parameters.SpecialIdentifiers["message"] = message;
            //parameters.SpecialIdentifiers["usersubmonths"] = months.ToString();
            //parameters.SpecialIdentifiers["usersubplanname"] = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchPubSubService.GetSubTierNameFromText(packet.sub_plan);
            //parameters.SpecialIdentifiers["usersubplan"] = planTier;
            //parameters.SpecialIdentifiers["usersubpoints"] = TwitchSubEventModel.GetSubPoints(user.SubscriberTier).ToString();
            //parameters.SpecialIdentifiers["usersubstreak"] = packet.streak_months.ToString();

            //string moderation = await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, message);
            //if (!string.IsNullOrEmpty(moderation))
            //{
            //    parameters.SpecialIdentifiers["message"] = moderation;
            //}

            //if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelResubscribed, parameters))
            //{
            //    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
            //    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

            //    if (months >= user.TotalMonthsSubbed)
            //    {
            //        user.TotalMonthsSubbed = months;
            //    }
            //    else
            //    {
            //        user.TotalMonthsSubbed++;
            //    }

            //    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            //    {
            //        currency.AddAmount(user, currency.OnSubscribeBonus);
            //    }

            //    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            //    {
            //        if (parameters.User.MeetsRole(streamPass.UserPermission))
            //        {
            //            streamPass.AddAmount(user, streamPass.SubscribeBonus);
            //        }
            //    }
            //}

            //EventService.ResubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, user, months: months, tier: user.SubscriberTier));
            //await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertResubscribedTier, user.FullDisplayName, months, planTier), ChannelSession.Settings.AlertSubColor));
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
