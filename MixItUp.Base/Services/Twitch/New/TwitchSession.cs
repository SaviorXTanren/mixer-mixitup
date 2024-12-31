using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Bits;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Chat;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch.New
{
    public class TwitchSession : StreamingPlatformSessionBase
    {
        public override IEnumerable<string> StreamerScopes { get; protected set; } = new List<string>()
        {
            "bits:read",

            "channel:edit:commercial",

            "channel:manage:ads",
            "channel:manage:broadcast",
            "channel:manage:moderators",
            "channel:manage:polls",
            "channel:manage:predictions",
            "channel:manage:raids",
            "channel:manage:redemptions",
            "channel:manage:vips",

            "channel:moderate",

            "channel:read:ads",
            "channel:read:charity",
            "channel:read:editors",
            "channel:read:goals",
            "channel:read:hype_train",
            "channel:read:polls",
            "channel:read:predictions",
            "channel:read:redemptions",
            "channel:read:subscriptions",

            "clips:edit",

            "chat:edit",
            "chat:read",

            "moderation:read",

            "moderator:read:chatters",
            "moderator:read:chat_settings",
            "moderator:read:followers",
            "moderator:read:vips",
            "moderator:read:moderators",

            "moderator:manage:announcements",
            "moderator:manage:banned_users",
            "moderator:manage:blocked_terms",
            "moderator:manage:chat_messages",
            "moderator:manage:chat_settings",
            "moderator:manage:shoutouts",
            "moderator:manage:unban_requests",
            "moderator:manage:warnings",

            "user:edit",

            "user:manage:blocked_users",
            "user:manage:whispers",

            "user:read:blocked_users",
            "user:read:broadcast",
            "user:read:chat",
            "user:read:follows",
            "user:read:subscriptions",

            "user:write:chat",

            "whispers:read",
            "whispers:edit",
        };

        public override IEnumerable<string> BotScopes { get; protected set; } = new List<string>()
        {
            "bits:read",

            "channel:moderate",

            "chat:edit",
            "chat:read",

            "moderator:manage:announcements",

            "user:edit",

            "whispers:read",
            "whispers:edit",
        };

        public override int MaxMessageLength { get { return 500; } }

        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel Streamer { get; set; }
        public UserModel Bot { get; set; }
        public ChannelInformationModel Channel { get; set; }
        public StreamModel Stream { get; set; }
        public List<ChannelContentClassificationLabelModel> ContentClassificationLabels { get; private set; } = new List<ChannelContentClassificationLabelModel>();
        public AdScheduleModel AdSchedule { get; set; }
        public DateTimeOffset NextAdTimestamp { get; set; } = DateTimeOffset.MinValue;

        public override string StreamerID { get { return this.Streamer?.id; } }
        public override string StreamerUsername { get { return this.Streamer?.login; } }
        public override string BotID { get { return this.Bot?.id; } }
        public override string BotUsername { get { return this.Bot?.login; } }
        public override string ChannelID { get { return this.Streamer?.id; } }
        public override string ChannelLink { get { return string.Format("twitch.tv/{0}", this.StreamerUsername?.ToLower()); } }

        public TwitchService StreamerService { get; private set; }
        public TwitchService BotService { get; private set; }
        private TwitchClient Client;

        private List<string> emoteSetIDs = new List<string>();

        public IDictionary<string, TwitchChatEmoteViewModel> Emotes { get { return this.emotes; } }
        private Dictionary<string, TwitchChatEmoteViewModel> emotes = new Dictionary<string, TwitchChatEmoteViewModel>();

        public IDictionary<string, Dictionary<string, ChatBadgeModel>> ChatBadges { get { return this.chatBadges; } }
        private Dictionary<string, Dictionary<string, ChatBadgeModel>> chatBadges = new Dictionary<string, Dictionary<string, ChatBadgeModel>>();

        public IDictionary<string, TwitchBitsCheermoteViewModel> BitsCheermotes { get { return this.bitsCheermotes; } }
        private Dictionary<string, TwitchBitsCheermoteViewModel> bitsCheermotes = new Dictionary<string, TwitchBitsCheermoteViewModel>();

        private List<TwitchSubEventModel> pendingGiftedSubs = new List<TwitchSubEventModel>();
        private List<TwitchMassGiftedSubEventModel> pendingMassGiftedSubs = new List<TwitchMassGiftedSubEventModel>();

        private CancellationTokenSource cancellationTokenSource;

        public override async Task RefreshDetails()
        {
            StreamModel stream = await StreamerService.GetStream(Streamer);
            if (stream != null)
            {
                if (!string.Equals(this.StreamCategoryID, stream.game_id, StringComparison.OrdinalIgnoreCase))
                {
                    GameModel game = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIGameByID(this.StreamCategoryID);
                    if (game != null)
                    {
                        this.StreamCategoryImageURL = game.box_art_url;
                    }
                }

                this.StreamTitle = stream.title;
                this.StreamCategoryID = stream.game_id;
                this.StreamCategoryName = stream.game_name;

                this.Stream = stream;

                this.StreamViewerCount = (int)this.Stream.viewer_count;
            }
        }

        protected override async Task<Result> ConnectStreamer()
        {
            Result result = await StreamerService.Connect();
            if (!result.Success)
            {
                return result;
            }

            Streamer = await StreamerService.GetNewAPICurrentUser();
            if (Streamer == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            Channel = await StreamerService.GetChannelInformation(Streamer);
            if (Channel == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            result = await this.Client.Connect();
            if (!result.Success)
            {
                await Client.Disconnect();
                return result;
            }

            List<Task> initializationTasks = new List<Task>();

            List<Task<IEnumerable<ChatEmoteModel>>> twitchEmoteTasks = new List<Task<IEnumerable<ChatEmoteModel>>>();
            twitchEmoteTasks.Add(this.StreamerService.GetGlobalEmotes());
            twitchEmoteTasks.Add(this.StreamerService.GetChannelEmotes(ServiceManager.Get<TwitchSessionService>().User));
            if (this.emoteSetIDs != null)
            {
                twitchEmoteTasks.Add(this.StreamerService.GetEmoteSets(this.emoteSetIDs));
            }

            initializationTasks.AddRange(twitchEmoteTasks);

            Task<IEnumerable<ChatBadgeSetModel>> globalChatBadgesTask = this.StreamerService.GetGlobalChatBadges();
            initializationTasks.Add(globalChatBadgesTask);
            Task<IEnumerable<ChatBadgeSetModel>> channelChatBadgesTask = this.StreamerService.GetChannelChatBadges(ServiceManager.Get<TwitchSessionService>().User);
            initializationTasks.Add(channelChatBadgesTask);

            if (ChannelSession.Settings.ShowAlejoPronouns)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("AlejoPronouns");
                initializationTasks.Add(ServiceManager.Get<AlejoPronounsService>().Initialize());
            }

            if (ChannelSession.Settings.ShowBetterTTVEmotes)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("BetterTTV");
                initializationTasks.Add(ServiceManager.Get<BetterTTVService>().DownloadGlobalBetterTTVEmotes());
                initializationTasks.Add(ServiceManager.Get<BetterTTVService>().DownloadTwitchBetterTTVEmotes(ServiceManager.Get<TwitchSessionService>().User.id));
            }

            if (ChannelSession.Settings.ShowFrankerFaceZEmotes)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("FrankerFaceZ");
                initializationTasks.Add(ServiceManager.Get<FrankerFaceZService>().DownloadGlobalFrankerFaceZEmotes());
                initializationTasks.Add(ServiceManager.Get<FrankerFaceZService>().DownloadTwitchFrankerFaceZEmotes(ServiceManager.Get<TwitchSessionService>().Username));
            }

            Task<IEnumerable<BitsCheermoteModel>> cheermotesTask = this.StreamerService.GetBitsCheermotes(ServiceManager.Get<TwitchSessionService>().User);
            initializationTasks.Add(cheermotesTask);

            await Task.WhenAll(initializationTasks);

            foreach (Task<IEnumerable<ChatEmoteModel>> emoteTask in twitchEmoteTasks)
            {
                if (emoteTask.IsCompleted && emoteTask.Result != null)
                {
                    foreach (ChatEmoteModel emote in emoteTask.Result)
                    {
                        this.emotes[emote.name] = new TwitchChatEmoteViewModel(emote);
                    }
                }
            }

            foreach (ChatBadgeSetModel badgeSet in globalChatBadgesTask.Result)
            {
                this.chatBadges[badgeSet.set_id] = new Dictionary<string, ChatBadgeModel>();
                foreach (ChatBadgeModel badge in badgeSet.versions)
                {
                    this.chatBadges[badgeSet.set_id][badge.id] = badge;
                }
            }

            foreach (ChatBadgeSetModel badgeSet in channelChatBadgesTask.Result)
            {
                this.chatBadges[badgeSet.set_id] = new Dictionary<string, ChatBadgeModel>();
                foreach (ChatBadgeModel badge in badgeSet.versions)
                {
                    this.chatBadges[badgeSet.set_id][badge.id] = badge;
                }
            }

            List<TwitchBitsCheermoteViewModel> cheermotes = new List<TwitchBitsCheermoteViewModel>();
            foreach (BitsCheermoteModel bitsCheermote in cheermotesTask.Result)
            {
                this.bitsCheermotes[bitsCheermote.prefix] = new TwitchBitsCheermoteViewModel(bitsCheermote);
            }

            this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.ChatterUpdateBackground, this.cancellationTokenSource.Token, 60000);
            AsyncRunner.RunAsyncBackground(this.BackgroundGiftedSubProcessor, this.cancellationTokenSource.Token, 3000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return new Result();
        }

        protected override async Task DisconnectStreamer()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            await this.Client.Disconnect();
        }

        protected override async Task<Result> ConnectBot()
        {
            Result result = await BotService.Connect();
            if (!result.Success)
            {
                return result;
            }

            Bot = await BotService.GetNewAPICurrentUser();
            if (Bot == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            return new Result();
        }

        protected override Task DisconnectBot()
        {
            return Task.CompletedTask;
        }

        public async Task StreamOnline()
        {
            this.IsLive = true;
            this.StreamStart = DateTimeOffset.Now;

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        public async Task StreamOffline()
        {
            this.IsLive = true;
            this.StreamStart = DateTimeOffset.MinValue;

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        public async Task ChannelUpdated(ChannelUpdateNotification update)
        {
            if (!string.Equals(this.StreamCategoryID, update.category_id, StringComparison.OrdinalIgnoreCase))
            {
                GameModel game = await ServiceManager.Get<TwitchSession>().StreamerService.GetNewAPIGameByID(this.StreamCategoryID);
                if (game != null)
                {
                    this.StreamCategoryImageURL = game.box_art_url;
                }
            }

            this.StreamTitle = update.title;
            this.StreamCategoryID = update.category_id;
            this.StreamCategoryName = update.category_name;

            CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Twitch);
            parameters.SpecialIdentifiers["streamtitle"] = this.StreamTitle;
            parameters.SpecialIdentifiers["streamgameid"] = this.StreamCategoryID;
            parameters.SpecialIdentifiers["streamgameimage"] = this.StreamCategoryImageURL;
            parameters.SpecialIdentifiers["streamgame"] = this.StreamCategoryName;
            parameters.SpecialIdentifiers["streamgamename"] = this.StreamCategoryName;

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelUpdated, parameters);
        }

        public void AddGiftedSub(TwitchSubEventModel sub)
        {
            lock (this.pendingGiftedSubs)
            {
                this.pendingGiftedSubs.Add(sub);
            }
        }

        public async Task AddMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSub)
        {
            if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0)
            {
                if (massGiftedSub.TotalGifted > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                {
                    lock (this.pendingMassGiftedSubs)
                    {
                        this.pendingMassGiftedSubs.Add(massGiftedSub);
                    }
                }
            }
            else
            {
                await ProcessMassGiftedSub(massGiftedSub);
            }
        }

        private async Task ChatterUpdateBackground(CancellationToken cancellationToken)
        {
            IEnumerable<ChatterModel> chatterModels = await this.StreamerService.GetChatters(ServiceManager.Get<TwitchSessionService>().User);

            HashSet<string> chatters = (chatterModels != null) ? new HashSet<string>(chatterModels.Select(c => c.user_login)) : new HashSet<string>();

            HashSet<string> joinsToProcess = new HashSet<string>();
            List<UserV2ViewModel> leavesToProcess = new List<UserV2ViewModel>();

            IEnumerable<UserV2ViewModel> activeUsers = ServiceManager.Get<UserService>().GetActiveUsers(StreamingPlatformTypeEnum.Twitch);
            HashSet<string> activeUsernames = new HashSet<string>();
            activeUsernames.AddRange(activeUsers.Select(u => u.Username));

            foreach (string chatter in chatters)
            {
                if (!activeUsernames.Contains(chatter))
                {
                    joinsToProcess.Add(chatter);
                }
            }

            foreach (UserV2ViewModel activeUser in activeUsers)
            {
                if (!chatters.Contains(activeUser.Username))
                {
                    leavesToProcess.Add(activeUser);
                }
            }

            if (joinsToProcess.Count > 0)
            {
                List<UserV2ViewModel> processedUsers = new List<UserV2ViewModel>();
                foreach (string username in joinsToProcess)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformUsername: username, performPlatformSearch: true);
                    if (user != null)
                    {
                        processedUsers.Add(user);
                    }
                }

                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(processedUsers);
            }

            if (leavesToProcess.Count > 0)
            {
                await ServiceManager.Get<UserService>().RemoveActiveUsers(leavesToProcess);
            }
        }

        private async Task BackgroundGiftedSubProcessor(CancellationToken cancellationToken)
        {
            if (ChannelSession.Settings.MassGiftedSubsFilterAmount > 0 && this.pendingGiftedSubs.Count > 0)
            {
                List<TwitchSubEventModel> tempGiftedSubs = new List<TwitchSubEventModel>();
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

        private async Task ProcessGiftedSub(TwitchSubEventModel giftedSubEvent, bool fireEventCommand = true)
        {
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftedSubEvent.User.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = giftedSubEvent.Duration;

            giftedSubEvent.User.Roles.Add(UserRoleEnum.Subscriber);
            giftedSubEvent.User.SubscribeDate = DateTimeOffset.Now;
            giftedSubEvent.User.SubscriberTier = giftedSubEvent.Tier;
            giftedSubEvent.User.TotalSubsReceived += (uint)giftedSubEvent.Duration;
            giftedSubEvent.User.TotalMonthsSubbed += (uint)giftedSubEvent.Duration;

            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
            {
                currency.AddAmount(giftedSubEvent.Gifter, currency.OnSubscribeBonus * giftedSubEvent.Duration);
            }

            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
            {
                if (giftedSubEvent.Gifter.MeetsRole(streamPass.UserPermission))
                {
                    streamPass.AddAmount(giftedSubEvent.Gifter, streamPass.SubscribeBonus * giftedSubEvent.Duration);
                }
            }

            if (fireEventCommand)
            {
                CommandParametersModel parameters = new CommandParametersModel(giftedSubEvent.Gifter, StreamingPlatformTypeEnum.Twitch);
                parameters.SpecialIdentifiers["usersubplanname"] = giftedSubEvent.PlanName;
                parameters.SpecialIdentifiers["usersubplan"] = giftedSubEvent.PlanName;
                parameters.SpecialIdentifiers["usersubpoints"] = giftedSubEvent.SubPoints.ToString();
                parameters.SpecialIdentifiers["usersubmonthsgifted"] = giftedSubEvent.Duration.ToString();
                parameters.SpecialIdentifiers["isanonymous"] = giftedSubEvent.IsAnonymous.ToString();
                parameters.Arguments.Add(giftedSubEvent.User.Username);
                parameters.TargetUser = giftedSubEvent.User;
                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelSubscriptionGifted, parameters);

                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(giftedSubEvent.Gifter, string.Format(MixItUp.Base.Resources.AlertSubscriptionGiftedTier, giftedSubEvent.Gifter.FullDisplayName, giftedSubEvent.PlanName, giftedSubEvent.User.FullDisplayName), ChannelSession.Settings.AlertGiftedSubColor));

                EventService.SubscriptionGiftedOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, giftedSubEvent.User, giftedSubEvent.Gifter, tier: giftedSubEvent.Tier));
            }
        }

        private async Task ProcessMassGiftedSub(TwitchMassGiftedSubEventModel massGiftedSubEvent)
        {
            CommandParametersModel parameters = new CommandParametersModel(massGiftedSubEvent.Gifter, StreamingPlatformTypeEnum.Twitch);
            parameters.SpecialIdentifiers["subsgiftedamount"] = massGiftedSubEvent.TotalGifted.ToString();
            parameters.SpecialIdentifiers["substotalpoints"] = massGiftedSubEvent.TotalSubPoints.ToString();
            parameters.SpecialIdentifiers["subsgiftedlifetimeamount"] = massGiftedSubEvent.LifetimeGifted.ToString();
            parameters.SpecialIdentifiers["usersubplan"] = massGiftedSubEvent.TierName;
            parameters.SpecialIdentifiers["isanonymous"] = massGiftedSubEvent.IsAnonymous.ToString();

            if (massGiftedSubEvent.Subs.Count > 0)
            {
                parameters.TargetUser = massGiftedSubEvent.Subs.First().User;
            }

            foreach (TwitchSubEventModel sub in massGiftedSubEvent.Subs)
            {
                parameters.Arguments.Add(sub.User.Username);
            }

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelMassSubscriptionsGifted, parameters);

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(massGiftedSubEvent.Gifter, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGiftedTier, massGiftedSubEvent.Gifter.FullDisplayName, massGiftedSubEvent.TotalGifted, massGiftedSubEvent.TierName), ChannelSession.Settings.AlertMassGiftedSubColor));

            List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
            foreach (TwitchSubEventModel sub in massGiftedSubEvent.Subs)
            {
                subscriptions.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, sub.User, massGiftedSubEvent.Gifter, tier: massGiftedSubEvent.Tier));
            }
            EventService.MassSubscriptionsGiftedOccurred(subscriptions);
        }
    }
}
