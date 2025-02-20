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
using MixItUp.Base.Model.Twitch.Subscriptions;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
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
        private const string SlashMeMessagePrefix = "/me";

        public static readonly IEnumerable<string> StreamerScopes = new List<string>()
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

        public static readonly IEnumerable<string> BotScopes = new List<string>()
        {
            "bits:read",

            "channel:moderate",

            "chat:edit",
            "chat:read",

            "moderator:manage:announcements",

            "user:edit",

            "user:manage:whispers",

            "user:write:chat",

            "whispers:read",
            "whispers:edit",
        };

        public override int MaxMessageLength { get { return 500; } }
        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.Twitch; } }

        public override OAuthServiceBase StreamerOAuthService { get { return this.StreamerService; } }
        public override OAuthServiceBase BotOAuthService { get { return this.BotService; } }

        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel StreamerModel { get; set; }
        public UserModel BotModel { get; set; }
        public ChannelInformationModel Channel { get; set; }
        public StreamModel Stream { get; set; }
        public List<ChannelContentClassificationLabelModel> ContentClassificationLabels { get; private set; } = new List<ChannelContentClassificationLabelModel>();
        public AdScheduleModel AdSchedule { get; set; }
        public DateTimeOffset NextAdTimestamp { get; set; } = DateTimeOffset.MinValue;

        public TwitchService StreamerService { get; private set; } = new TwitchService(StreamerScopes);
        public TwitchService BotService { get; private set; } = new TwitchService(BotScopes, isBotService: true);

        public TwitchClient Client { get; private set; } = new TwitchClient();

        private List<string> emoteSetIDs = new List<string>();

        private int noValidStreamCount = 0;

        public IDictionary<string, TwitchChatEmoteViewModel> Emotes { get { return this.emotes; } }
        private Dictionary<string, TwitchChatEmoteViewModel> emotes = new Dictionary<string, TwitchChatEmoteViewModel>();

        public IDictionary<string, Dictionary<string, ChatBadgeModel>> ChatBadges { get { return this.chatBadges; } }
        private Dictionary<string, Dictionary<string, ChatBadgeModel>> chatBadges = new Dictionary<string, Dictionary<string, ChatBadgeModel>>();

        public IDictionary<string, TwitchBitsCheermoteViewModel> BitsCheermotes { get { return this.bitsCheermotes; } }
        private Dictionary<string, TwitchBitsCheermoteViewModel> bitsCheermotes = new Dictionary<string, TwitchBitsCheermoteViewModel>();

        private List<TwitchSubcriptionEventModel> pendingGiftedSubs = new List<TwitchSubcriptionEventModel>();
        private List<TwitchMassGiftedSubcriptionsEventModel> pendingMassGiftedSubs = new List<TwitchMassGiftedSubcriptionsEventModel>();

        private CancellationTokenSource cancellationTokenSource;

        protected override async Task<Result> InitializeStreamerInternal()
        {
            this.StreamerModel = await this.StreamerService.GetNewAPICurrentUser();
            if (this.StreamerModel == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            this.Channel = await this.StreamerService.GetChannelInformation(StreamerModel);
            if (this.Channel == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            this.StreamerID = this.StreamerModel?.id;
            this.StreamerUsername = this.StreamerModel?.login;
            this.StreamerAvatarURL = this.StreamerModel?.profile_image_url;

            this.ChannelID = this.StreamerModel?.id;
            this.ChannelLink = string.Format("twitch.tv/{0}", this.StreamerUsername?.ToLower());

            this.Streamer = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: this.StreamerID);
            if (this.Streamer == null)
            {
                this.Streamer = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(this.StreamerModel));
            }

            Result result = await this.Client.Connect();
            if (!result.Success)
            {
                await Client.Disconnect();
                return result;
            }

            List<Task> initializationTasks = new List<Task>();

            List<Task<IEnumerable<ChatEmoteModel>>> twitchEmoteTasks = new List<Task<IEnumerable<ChatEmoteModel>>>();
            twitchEmoteTasks.Add(this.StreamerService.GetGlobalEmotes());
            twitchEmoteTasks.Add(this.StreamerService.GetChannelEmotes(this.StreamerModel));
            if (this.emoteSetIDs != null)
            {
                twitchEmoteTasks.Add(this.StreamerService.GetEmoteSets(this.emoteSetIDs));
            }

            initializationTasks.AddRange(twitchEmoteTasks);

            Task<IEnumerable<ChatBadgeSetModel>> globalChatBadgesTask = this.StreamerService.GetGlobalChatBadges();
            initializationTasks.Add(globalChatBadgesTask);
            Task<IEnumerable<ChatBadgeSetModel>> channelChatBadgesTask = this.StreamerService.GetChannelChatBadges(this.StreamerModel);
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
                initializationTasks.Add(ServiceManager.Get<BetterTTVService>().DownloadTwitchBetterTTVEmotes(this.StreamerID));
            }

            if (ChannelSession.Settings.ShowFrankerFaceZEmotes)
            {
                ServiceManager.Get<ITelemetryService>().TrackService("FrankerFaceZ");
                initializationTasks.Add(ServiceManager.Get<FrankerFaceZService>().DownloadGlobalFrankerFaceZEmotes());
                initializationTasks.Add(ServiceManager.Get<FrankerFaceZService>().DownloadTwitchFrankerFaceZEmotes(this.StreamerUsername));
            }

            Task<IEnumerable<BitsCheermoteModel>> cheermotesTask = this.StreamerService.GetBitsCheermotes(this.StreamerModel);
            initializationTasks.Add(cheermotesTask);

            Task<IEnumerable<ChannelEditorUserModel>> channelEditorsTask = this.StreamerService.GetChannelEditors(this.StreamerModel);
            initializationTasks.Add(channelEditorsTask);

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

            if (globalChatBadgesTask.IsCompleted && globalChatBadgesTask.Result != null)
            {
                foreach (ChatBadgeSetModel badgeSet in globalChatBadgesTask.Result)
                {
                    this.chatBadges[badgeSet.set_id] = new Dictionary<string, ChatBadgeModel>();
                    foreach (ChatBadgeModel badge in badgeSet.versions)
                    {
                        this.chatBadges[badgeSet.set_id][badge.id] = badge;
                    }
                }
            }

            if (channelChatBadgesTask.IsCompleted && channelChatBadgesTask.Result != null)
            {
                foreach (ChatBadgeSetModel badgeSet in channelChatBadgesTask.Result)
                {
                    this.chatBadges[badgeSet.set_id] = new Dictionary<string, ChatBadgeModel>();
                    foreach (ChatBadgeModel badge in badgeSet.versions)
                    {
                        this.chatBadges[badgeSet.set_id][badge.id] = badge;
                    }
                }
            }

            if (cheermotesTask.IsCompleted && cheermotesTask.Result != null)
            {
                List<TwitchBitsCheermoteViewModel> cheermotes = new List<TwitchBitsCheermoteViewModel>();
                foreach (BitsCheermoteModel bitsCheermote in cheermotesTask.Result)
                {
                    this.bitsCheermotes[bitsCheermote.prefix] = new TwitchBitsCheermoteViewModel(bitsCheermote);
                }
            }

            if (channelEditorsTask.IsCompleted && channelEditorsTask.Result != null)
            {
                foreach (ChannelEditorUserModel channelEditor in channelEditorsTask.Result)
                {
                    this.ChannelEditors.Add(channelEditor.user_id);
                }
            }

            this.cancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(this.ChatterUpdateBackground, this.cancellationTokenSource.Token, 60000);
            AsyncRunner.RunAsyncBackground(this.BackgroundGiftedSubProcessor, this.cancellationTokenSource.Token, 3000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return new Result();
        }

        protected override async Task DisconnectStreamerInternal()
        {
            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
                this.cancellationTokenSource = null;
            }

            await this.Client.Disconnect();
        }

        protected override async Task<Result> InitializeBotInternal()
        {
            this.BotModel = await this.BotService.GetNewAPICurrentUser();
            if (this.BotModel == null)
            {
                return new Result(Resources.TwitchFailedToGetUserData);
            }

            this.BotID = this.BotModel?.id;
            this.BotUsername = this.BotModel?.login;
            this.BotAvatarURL = this.BotModel?.profile_image_url;

            this.Bot = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: this.BotID);
            if (this.Bot == null)
            {
                this.Bot = await ServiceManager.Get<UserService>().CreateUser(new TwitchUserPlatformV2Model(this.BotModel));
            }

            return new Result();
        }

        protected override Task DisconnectBotInternal()
        {
            return Task.CompletedTask;
        }

        public override async Task RefreshOAuthTokenIfCloseToExpiring()
        {
            await this.StreamerService.RefreshOAuthTokenIfCloseToExpiring();
            await this.BotService.RefreshOAuthTokenIfCloseToExpiring();
        }

        public override async Task<Result> RefreshDetails()
        {
            ChannelInformationModel channel = await StreamerService.GetChannelInformation(this.StreamerModel);
            if (channel == null)
            {
                return new Result(Resources.TwitchFailedToGetChannelData);
            }

            this.Channel = channel;

            if (!string.Equals(this.StreamCategoryID, channel.game_id, StringComparison.OrdinalIgnoreCase))
            {
                GameModel game = await this.StreamerService.GetNewAPIGameByID(channel.game_id);
                if (game != null)
                {
                    this.SetCategoryImageURL(game);
                }
            }

            this.StreamTitle = this.Channel.title;
            this.StreamCategoryID = this.Channel.game_id;
            this.StreamCategoryName = this.Channel.game_name;

            StreamModel stream = await StreamerService.GetActiveStream(this.StreamerModel);
            if (stream != null)
            {
                this.noValidStreamCount = 0;

                this.IsLive = true;
                this.Stream = stream;
                this.StreamStart = TwitchService.GetTwitchDateTime(this.Stream.started_at);
                this.StreamViewerCount = (int)this.Stream.viewer_count;
            }
            else
            {
                this.noValidStreamCount++;
                if (this.noValidStreamCount >= 3)
                {
                    this.IsLive = false;
                    this.Stream = null;
                    this.StreamStart = DateTimeOffset.MinValue;
                    this.StreamViewerCount = 0;
                }
            }

            AdScheduleModel adSchedule = await this.StreamerService.GetAdSchedule(this.StreamerModel);
            if (adSchedule != null)
            {
                this.AdSchedule = adSchedule;
            }

            if (this.AdSchedule != null)
            {
                DateTimeOffset nextAd = this.AdSchedule.NextAdTimestamp();
                if (nextAd > this.NextAdTimestamp)
                {
                    int nextAdMinutes = this.AdSchedule.NextAdMinutesFromNow();
                    if (nextAdMinutes <= ChannelSession.Settings.TwitchUpcomingAdCommandTriggerAmount && nextAdMinutes > 0)
                    {
                        this.NextAdTimestamp = nextAd;

                        Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                        eventCommandSpecialIdentifiers["adsnoozecount"] = this.AdSchedule.snooze_count.ToString();
                        eventCommandSpecialIdentifiers["adnextduration"] = this.AdSchedule.duration.ToString();
                        eventCommandSpecialIdentifiers["adnextminutes"] = nextAdMinutes.ToString();
                        eventCommandSpecialIdentifiers["adnexttime"] = nextAd.ToFriendlyTimeString();
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelAdUpcoming, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));
                    }
                }
            }

            foreach (var key in ChannelSession.Settings.TwitchVIPAutomaticRemovals.Keys.ToList())
            {
                if (ChannelSession.Settings.TwitchVIPAutomaticRemovals.TryGetValue(key, out DateTimeOffset removalTime) && removalTime < DateTimeOffset.Now)
                {
                    ChannelSession.Settings.TwitchVIPAutomaticRemovals.Remove(key);

                    await this.StreamerService.UnVIPUser(this.StreamerModel, key);

                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: key);
                    if (user != null)
                    {
                        user.Roles.Remove(UserRoleEnum.TwitchVIP);
                    }
                }
            }

            return new Result();
        }

        public override async Task<Result> SetStreamTitle(string title)
        {
            return await this.StreamerService.UpdateChannelInformation(this.StreamerModel, title: title);
        }

        public override async Task<Result> SetStreamCategory(string category)
        {
            GameModel game = null;
            IEnumerable<GameModel> games = await this.StreamerService.GetNewAPIGamesByName(category);
            if (games != null && games.Count() > 0)
            {
                game = games.FirstOrDefault(g => g.name.ToLower().Equals(category));
                if (game == null)
                {
                    game = games.First();
                }

                if (game != null)
                {
                    return await this.StreamerService.UpdateChannelInformation(this.StreamerModel, gameID: game?.id);
                }
            }

            return new Result(success: false);
        }

        public override async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.SendMessage(message, sendAsStreamer);
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false, string replyMessageID = null)
        {
            if (!ChannelSession.Settings.TwitchReplyToCommandChatMessages)
            {
                replyMessageID = null;
            }

            if (ChannelSession.Settings.TwitchSlashMeForAllChatMessages)
            {
                message = $"{SlashMeMessagePrefix} {message}";
            }

            foreach (string m in this.SplitLargeMessage(message))
            {
                if (sendAsStreamer || !this.IsBotConnected)
                {
                    await this.StreamerService.SendChatMessage(this.StreamerModel, this.StreamerModel, m, replyMessageID);
                }
                else
                {
                    await this.BotService.SendChatMessage(this.StreamerModel, this.BotModel, m, replyMessageID);
                }
            }
        }

        public override async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.StreamerService.DeleteChatMessage(this.StreamerModel, message.ID);
        }

        public override async Task ClearMessages()
        {
            await this.StreamerService.ClearChat(this.StreamerModel);
        }

        public override async Task TimeoutUser(UserV2ViewModel user, int durationInSeconds, string reason = null)
        {
            await this.StreamerService.TimeoutUser(this.StreamerModel, user.PlatformID, durationInSeconds, reason);
        }

        public override async Task ModUser(UserV2ViewModel user)
        {
            await this.StreamerService.ModUser(this.StreamerModel, user.PlatformID);
        }

        public override async Task UnmodUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnmodUser(this.StreamerModel, user.PlatformID);
        }

        public override async Task BanUser(UserV2ViewModel user, string reason = null)
        {
            await this.StreamerService.BanUser(this.StreamerModel, user.PlatformID, reason);
        }

        public override async Task UnbanUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnbanUser(this.StreamerModel, user.PlatformID);
        }

        public async Task SendWhisper(UserV2ViewModel user, string message, bool sendAsStreamer = false)
        {
            if (sendAsStreamer || !this.IsBotConnected)
            {
                await this.StreamerService.SendWhisper(this.StreamerModel, user.PlatformID, message);
            }
            else
            {
                await this.BotService.SendWhisper(this.BotModel, user.PlatformID, message);
            }
        }

        public async Task StreamOnline()
        {
            this.IsLive = true;
            this.StreamStart = DateTimeOffset.Now;

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        public async Task StreamOffline()
        {
            this.IsLive = false;
            this.Stream = null;
            this.StreamStart = DateTimeOffset.MinValue;
            this.StreamViewerCount = 0;

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.Twitch));
        }

        public async Task ChannelUpdated(ChannelUpdateNotification update)
        {
            if (!string.Equals(this.StreamCategoryID, update.category_id, StringComparison.OrdinalIgnoreCase))
            {
                GameModel game = await this.StreamerService.GetNewAPIGameByID(update.category_id);
                if (game != null)
                {
                    this.SetCategoryImageURL(game);
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

        public void AddGiftedSub(TwitchSubcriptionEventModel sub)
        {
            lock (this.pendingGiftedSubs)
            {
                this.pendingGiftedSubs.Add(sub);
            }
        }

        public async Task AddMassGiftedSub(TwitchMassGiftedSubcriptionsEventModel massGiftedSub)
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
            IEnumerable<ChatterModel> chatterModels = await this.StreamerService.GetChatters(this.StreamerModel);

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
                List<TwitchSubcriptionEventModel> tempGiftedSubs = new List<TwitchSubcriptionEventModel>();
                lock (this.pendingGiftedSubs)
                {
                    tempGiftedSubs.AddRange(this.pendingGiftedSubs.ToList().OrderBy(s => s.Processed));
                    this.pendingGiftedSubs.Clear();
                }

                List<TwitchMassGiftedSubcriptionsEventModel> tempMassGiftedSubs = new List<TwitchMassGiftedSubcriptionsEventModel>();
                lock (this.pendingMassGiftedSubs)
                {
                    tempMassGiftedSubs.AddRange(this.pendingMassGiftedSubs.ToList().OrderBy(s => s.Processed));
                }

                foreach (var giftedSub in tempGiftedSubs)
                {
                    TwitchMassGiftedSubcriptionsEventModel massGiftedSub = null;
                    if (giftedSub.IsAnonymous || string.IsNullOrEmpty(giftedSub.CommunityGiftID))
                    {
                        massGiftedSub = tempMassGiftedSubs.FirstOrDefault(ms => ms.IsAnonymous);
                    }
                    else
                    {
                        massGiftedSub = tempMassGiftedSubs.FirstOrDefault(ms => string.Equals(ms.CommunityGiftID, giftedSub.CommunityGiftID));
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

        private async Task ProcessGiftedSub(TwitchSubcriptionEventModel giftedSubEvent, bool fireEventCommand = true)
        {
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftedSubEvent.User.ID;
            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = giftedSubEvent.Cumulative;

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

        private async Task ProcessMassGiftedSub(TwitchMassGiftedSubcriptionsEventModel massGiftedSubEvent)
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

            foreach (TwitchSubcriptionEventModel sub in massGiftedSubEvent.Subs)
            {
                parameters.Arguments.Add(sub.User.Username);
            }

            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelMassSubscriptionsGifted, parameters);

            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(massGiftedSubEvent.Gifter, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGiftedTier, massGiftedSubEvent.Gifter.FullDisplayName, massGiftedSubEvent.TotalGifted, massGiftedSubEvent.TierName), ChannelSession.Settings.AlertMassGiftedSubColor));

            List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
            foreach (TwitchSubcriptionEventModel sub in massGiftedSubEvent.Subs)
            {
                subscriptions.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Twitch, sub.User, massGiftedSubEvent.Gifter, tier: massGiftedSubEvent.Tier));
            }
            EventService.MassSubscriptionsGiftedOccurred(subscriptions);
        }

        private void SetCategoryImageURL(GameModel game)
        {
            string image = game.box_art_url;
            image = image.Replace("{width}", "264");
            image = image.Replace("{height}", "352");
            this.StreamCategoryImageURL = image;
        }
    }
}
