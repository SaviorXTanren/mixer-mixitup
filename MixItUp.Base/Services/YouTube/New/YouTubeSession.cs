using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube.New
{
    public static class LiveBroadcastExtensions
    {
        public static string GetStreamURL(this LiveBroadcast liveBroadcast) { return $"https://youtube.com/watch?v={liveBroadcast.Id}"; }
    }

    public class YouTubeSession : StreamingPlatformSessionBase
    {
        public static DateTimeOffset GetYouTubeDateTime(string dateTime)
        {
            return DateTimeOffsetExtensions.FromGeneralString(dateTime);
        }

        private const int MinMessagePollingInterval = 5000;
        private const int MaxMessagePollingInterval = 10000;

        private const string TextMessageEventMessageType = "textMessageEvent";
        private const string NewMemberEventMessageType = "newSponsorEvent";
        private const string MemberMilestoneEventMessageType = "memberMilestoneChatEvent";
        private const string MembershipGiftingEventMessageType = "membershipGiftingEvent";
        private const string GiftMembershipReceivedEventMessageType = "giftMembershipReceivedEvent";
        private const string SuperChatEventMessageType = "superChatEvent";
        private const string SuperStickerEventMessageType = "superStickerEvent";
        private const string MessageDeletedEventMessageType = "messageDeletedEvent";
        private const string UserBannedEventMessageType = "userBannedEvent";

        public static readonly IEnumerable<string> StreamerScopes = new List<string>()
        {
            "email",
            "profile",
            "openid",
            "https://www.googleapis.com/auth/youtube",
            "https://www.googleapis.com/auth/youtube.force-ssl",
            "https://www.googleapis.com/auth/youtube.channel-memberships.creator",
            "https://www.googleapis.com/auth/youtube.readonly",
            "https://www.googleapis.com/auth/youtubepartner",
            "https://www.googleapis.com/auth/yt-analytics.readonly",
        };

        public static readonly IEnumerable<string> BotScopes = new List<string>()
        {
            "email",
            "profile",
            "openid",
            "https://www.googleapis.com/auth/youtube",
            "https://www.googleapis.com/auth/youtube.force-ssl",
            "https://www.googleapis.com/auth/youtube.readonly",
        };

        public override int MaxMessageLength { get { return 200; } }
        public override StreamingPlatformTypeEnum Platform { get { return StreamingPlatformTypeEnum.YouTube; } }

        public override string StreamLink
        {
            get
            {
                var broadcast = this.LiveBroadcasts.FirstOrDefault();
                if (broadcast.Value != null)
                {
                    return broadcast.Value.GetStreamURL();
                }
                return string.Empty;
            }
        }

        public override OAuthServiceBase StreamerOAuthService { get { return this.StreamerService; } }
        public override OAuthServiceBase BotOAuthService { get { return this.BotService; } }

        public YouTubeService StreamerService { get; private set; } = new YouTubeService(StreamerScopes);
        public YouTubeService BotService { get; private set; } = new YouTubeService(BotScopes, isBotService: true);

        public Channel StreamerModel { get; private set; }
        public Channel BotModel { get; private set; }

        public Dictionary<string, LiveBroadcast> LiveBroadcasts
        {
            get
            {
                Dictionary<string, LiveBroadcast> broadcasts = new Dictionary<string, LiveBroadcast>();
                foreach (var kvp in this.AutomaticLiveBroadcasts)
                {
                    broadcasts[kvp.Key] = kvp.Value;
                }
                foreach (var kvp in this.ManualLiveBroadcasts)
                {
                    broadcasts[kvp.Key] = kvp.Value;
                }
                return broadcasts;
            }
        }
        public Dictionary<string, LiveBroadcast> AutomaticLiveBroadcasts { get; private set; } = new Dictionary<string, LiveBroadcast>();
        public Dictionary<string, LiveBroadcast> ManualLiveBroadcasts { get; private set; } = new Dictionary<string, LiveBroadcast>();

        public Dictionary<string, Video> Videos { get; private set; } = new Dictionary<string, Video>();

        public IEnumerable<Model.YouTube.YouTubeChatEmoteModel> Emotes { get; private set; } = new List<Model.YouTube.YouTubeChatEmoteModel>();
        public Dictionary<string, YouTubeChatEmoteViewModel> EmoteDictionary { get; private set; } = new Dictionary<string, YouTubeChatEmoteViewModel>();

        public List<MembershipsLevel> MembershipLevels { get; private set; } = new List<MembershipsLevel>();
        public bool HasMembershipCapabilities { get { return this.MembershipLevels.Count > 0; } }

        private string nextMessagesToken = null;

        private CancellationTokenSource messageBackgroundPollingTokenSource;
        private int messagePollingInterval = 5000;

        private Dictionary<string, MixItUp.Base.Model.YouTube.YouTubeMembershipsGiftedModel> userGiftedMembershipDictionary = new Dictionary<string, MixItUp.Base.Model.YouTube.YouTubeMembershipsGiftedModel>();

        private DateTime launchDateTime = DateTime.Now;

        private SearchResult latestNonStreamVideo;
        private Video latestShort;

        private HashSet<string> messageIDsToIgnore = new HashSet<string>();

        protected override async Task<Result> InitializeStreamerInternal()
        {
            this.StreamerModel = await this.StreamerService.GetCurrentChannel();
            if (this.StreamerModel == null)
            {
                return new Result(Resources.YouTubeFailedToGetChannelData);
            }

            this.StreamerID = this.StreamerModel?.Id;
            this.StreamerUsername = this.StreamerModel?.Snippet?.Title;
            this.StreamerAvatarURL = this.StreamerModel?.Snippet?.Thumbnails?.Medium?.Url;

            this.ChannelID = this.StreamerModel?.Id;
            this.ChannelLink = this.StreamerModel?.Snippet?.CustomUrl;

            this.Streamer = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: this.StreamerID);
            if (this.Streamer == null)
            {
                this.Streamer = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(this.StreamerModel));
            }

            List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
            platformServiceTasks.Add(this.SetChatEmotes());
            platformServiceTasks.Add(this.SetMembershipLevels());
            platformServiceTasks.Add(this.StreamerService.ValidateAccountIsEnabledForLiveStreaming());

            await Task.WhenAll(platformServiceTasks);

            if (platformServiceTasks.Any(c => !c.Result.Success))
            {
                string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                return new Result(MixItUp.Base.Resources.YouTubeFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
            }

            this.messageBackgroundPollingTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.MessageBackgroundPolling, this.messageBackgroundPollingTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return new Result();
        }

        protected override Task DisconnectStreamerInternal()
        {
            try
            {
                if (this.messageBackgroundPollingTokenSource != null)
                {
                    this.messageBackgroundPollingTokenSource.Cancel();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return Task.CompletedTask;
        }

        protected override async Task<Result> InitializeBotInternal()
        {
            this.BotModel = await this.BotService.GetCurrentChannel();
            if (this.BotModel == null)
            {
                return new Result(Resources.YouTubeFailedToGetChannelData);
            }

            this.BotID = this.BotModel?.Id;
            this.BotUsername = this.BotModel?.Snippet?.Title;
            this.BotAvatarURL = this.BotModel?.Snippet?.Thumbnails?.Medium?.Url;

            this.Bot = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: this.BotID);
            if (this.Bot == null)
            {
                this.Bot = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(this.BotModel));
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
            IEnumerable<LiveBroadcast> broadcasts = await this.StreamerService.GetActiveBroadcasts();
            if (broadcasts != null)
            {
                Dictionary<string, LiveBroadcast> newBroadcasts = new Dictionary<string, LiveBroadcast>();
                foreach (LiveBroadcast broadcast in broadcasts)
                {
                    if (broadcast.IsLive())
                    {
                        newBroadcasts[broadcast.Id] = broadcast;
                    }
                }
                this.AutomaticLiveBroadcasts = newBroadcasts;
            }

            this.IsLive = this.LiveBroadcasts.Count > 0;
            if (this.IsLive)
            {
                IEnumerable<Video> videos = await this.StreamerService.GetVideosByID(this.LiveBroadcasts.Keys.ToList());
                if (videos != null)
                {
                    Dictionary<string, Video> newVideos = new Dictionary<string, Video>();
                    foreach (Video video in videos)
                    {
                        newVideos[video.Id] = video;
                    }
                    this.Videos = newVideos;
                }
            }

            this.StreamViewerCount = this.Videos.Sum(v => (int)v.Value?.LiveStreamingDetails?.ConcurrentViewers.GetValueOrDefault());

            if (ChannelSession.User != null)
            {
                if (this.LiveBroadcasts.Any(b => this.launchDateTime < YouTubeSession.GetYouTubeDateTime(b.Value.Snippet.ActualStartTimeRaw)))
                {
                    this.StreamStart = DateTimeOffset.Now;
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                }

                if (this.LiveBroadcasts.Any(b => this.launchDateTime < YouTubeSession.GetYouTubeDateTime(b.Value.Snippet.ActualEndTimeRaw)))
                {
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                }
            }

            return new Result();
        }

        public override async Task<Result> SetStreamTitle(string title)
        {
            return await this.UpdateStreamTitleAndDescription(title, description: null);
        }

        public override Task<Result> SetStreamCategory(string category)
        {
            return Task.FromResult(new Result());
        }

        public override async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            List<LiveChatMessage> messagesToProcess = new List<LiveChatMessage>();
            foreach (string m in this.SplitLargeMessage(message))
            {
                bool processedMessage = false;
                foreach (LiveBroadcast broadcast in this.LiveBroadcasts.Values.ToList())
                {
                    LiveChatMessage resultMessage = null;
                    if (sendAsStreamer || !this.IsBotConnected)
                    {
                        resultMessage = await this.StreamerService.SendMessage(broadcast, m);
                    }
                    else
                    {
                        resultMessage = await this.BotService.SendMessage(broadcast, m);
                    }

                    if (resultMessage != null)
                    {
                        messageIDsToIgnore.Add(resultMessage.Id);
                    }

                    if (!processedMessage)
                    {
                        processedMessage = true;

                        if (sendAsStreamer || !this.IsBotConnected)
                        {
                            resultMessage.AuthorDetails = new LiveChatMessageAuthorDetails()
                            {
                                ChannelId = this.ChannelID,
                                ChannelUrl = this.ChannelLink,
                                DisplayName = this.Streamer.DisplayName,
                                IsChatOwner = true,
                                ProfileImageUrl = this.StreamerAvatarURL,
                            };
                        }
                        else
                        {
                            resultMessage.AuthorDetails = new LiveChatMessageAuthorDetails()
                            {
                                ChannelId = this.BotModel?.Id,
                                ChannelUrl = this.BotModel?.Snippet?.CustomUrl,
                                DisplayName = this.Bot.DisplayName,
                                IsChatOwner = true,
                                ProfileImageUrl = this.BotAvatarURL,
                            };
                        }

                        messagesToProcess.Add(resultMessage);
                    }
                }
            }

            foreach (LiveChatMessage messageToProcess in messagesToProcess)
            {
                await this.ProcessMessage(messageToProcess);
            }
        }

        public override async Task DeleteMessage(ChatMessageViewModel message)
        {
            await this.StreamerService.DeleteMessage(message.ID);
        }

        public override Task ClearMessages()
        {
            return Task.CompletedTask;
        }

        public override async Task TimeoutUser(UserV2ViewModel user, int durationInSeconds, string reason = null)
        {
            foreach (LiveBroadcast broadcast in this.LiveBroadcasts.Values.ToList())
            {
                await this.StreamerService.TimeoutUser(broadcast, user, durationInSeconds);
            }
        }

        public override async Task ModUser(UserV2ViewModel user)
        {
            LiveBroadcast broadcast = this.LiveBroadcasts.Values.FirstOrDefault();
            if (broadcast != null)
            {
                await this.StreamerService.ModUser(broadcast, user);
            }
        }

        public override async Task UnmodUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnmodUser(user);
        }

        public override async Task BanUser(UserV2ViewModel user, string reason = null)
        {
            LiveBroadcast broadcast = this.LiveBroadcasts.Values.FirstOrDefault();
            if (broadcast != null)
            {
                await this.StreamerService.BanUser(broadcast, user);
            }
        }

        public override async Task UnbanUser(UserV2ViewModel user)
        {
            await this.StreamerService.UnbanUser(user);
        }

        public async Task<Result> UpdateStreamTitleAndDescription(string title, string description)
        {
            Result result = new Result();
            foreach (LiveBroadcast liveBroadcast in this.LiveBroadcasts.Values.ToList())
            {
                LiveBroadcast lb = await this.StreamerService.UpdateVideo(liveBroadcast, title: title, description: description);
                if (lb == null)
                {
                    result.Append(string.Format(Resources.YouTubeFailedToUpdateVideoID, liveBroadcast.Id));
                    result.Success = false;
                }
                else
                {
                    this.LiveBroadcasts[lb.Id] = lb;
                }
            }
            return result;
        }

        public async Task<SearchResult> GetLatestNonStreamVideo()
        {
            if (this.latestNonStreamVideo == null)
            {
                IEnumerable<SearchResult> searchResults = await this.StreamerService.GetLatestVideos(this.ChannelID, maxResults: 100);

                HashSet<string> broadcastIDs = new HashSet<string>();
                foreach (LiveBroadcast broadcast in await this.StreamerService.GetLatestBroadcasts())
                {
                    broadcastIDs.Add(broadcast.Id);
                }

                IEnumerable<string> shortIDs = await this.StreamerService.GetLatestShortIDs(this.ChannelID);

                foreach (SearchResult searchResult in searchResults)
                {
                    if (!broadcastIDs.Contains(searchResult.Id.VideoId) && !shortIDs.Contains(searchResult.Id.VideoId))
                    {
                        this.latestNonStreamVideo = searchResult;
                        break;
                    }
                }
            }
            return this.latestNonStreamVideo;
        }

        public async Task<Video> GetLatestShort()
        {
            if (this.latestShort == null)
            {
                IEnumerable<string> shortIDs = await this.StreamerService.GetLatestShortIDs(this.ChannelID);
                foreach (string shortID in shortIDs)
                {
                    IEnumerable<Video> video = await this.StreamerService.GetVideosByID(new List<string>() { shortID });
                    if (video != null && video.Count() > 0)
                    {
                        this.latestShort = video.First();
                        break;
                    }
                }
            }
            return this.latestShort;
        }

        private async Task<Result> SetChatEmotes()
        {
            this.EmoteDictionary.Clear();
            this.Emotes = await this.StreamerService.GetChatEmotes();
            if (this.Emotes != null)
            {
                foreach (MixItUp.Base.Model.YouTube.YouTubeChatEmoteModel emote in this.Emotes)
                {
                    foreach (string shortcut in emote.shortcuts)
                    {
                        this.EmoteDictionary[shortcut] = new YouTubeChatEmoteViewModel(emote);
                    }
                }
            }
            return new Result();
        }

        private async Task<Result> SetMembershipLevels()
        {
            try
            {
                IEnumerable<MembershipsLevel> membershipLevels = await this.StreamerService.GetMembershipLevels();
                if (membershipLevels != null && membershipLevels.Count() > 0)
                {
                    this.MembershipLevels.AddRange(membershipLevels);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result();
        }

        private async Task MessageBackgroundPolling()
        {
            while (!this.messageBackgroundPollingTokenSource.IsCancellationRequested)
            {
                try
                {
                    IEnumerable<LiveBroadcast> broadcasts = this.LiveBroadcasts.Values.ToList();
                    if (broadcasts.Count() > 0)
                    {
                        foreach (LiveBroadcast broadcast in broadcasts)
                        {
                            LiveChatMessagesResultModel result = await this.StreamerService.GetChatMessages(broadcast, this.nextMessagesToken);
                            if (result != null)
                            {
                                // Only process messages after the first time polling chat so we don't re-trigger commands on old messages
                                if (this.nextMessagesToken != null)
                                {
                                    List<LiveChatMessage> newMessages = new List<LiveChatMessage>();
                                    foreach (LiveChatMessage message in result.Messages)
                                    {
                                        newMessages.Add(message);
                                    }

                                    if (newMessages.Count > 0)
                                    {
                                        foreach (LiveChatMessage message in newMessages)
                                        {
                                            if (!messageIDsToIgnore.Contains(message.Id))
                                            {
                                                await this.ProcessMessage(message);
                                            }
                                        }

                                        this.messagePollingInterval = Math.Max((int)result.PollingInterval, MinMessagePollingInterval);
                                    }
                                    else
                                    {
                                        this.messagePollingInterval = Math.Min(this.messagePollingInterval + 1000, MaxMessagePollingInterval);
                                    }
                                }

                                this.nextMessagesToken = result.NextResultsToken;

                                await Task.Delay(this.messagePollingInterval);
                            }
                            else
                            {
                                await Task.Delay(MaxMessagePollingInterval);
                            }
                        }
                    }
                    else
                    {
                        await Task.Delay(60000);
                    }
                }
                catch (TaskCanceledException) { }
                catch (Exception ex) { Logger.Log(ex); }
            }
        }

        private async Task ProcessMessage(LiveChatMessage liveChatMessage)
        {
            try
            {
                Logger.Log(LogLevel.Debug, string.Format("YouTube Chat Packet Received: {0}", JSONSerializerHelper.SerializeToString(liveChatMessage)));

                if (liveChatMessage.AuthorDetails?.ChannelId != null)
                {
                    UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: liveChatMessage.AuthorDetails.ChannelId, liveChatMessage.AuthorDetails.DisplayName, performPlatformSearch: true);
                    if (user == null)
                    {
                        user = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(liveChatMessage));
                    }
                    else
                    {
                        user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).SetMessageProperties(liveChatMessage);
                    }
                    await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);

                    // https://developers.google.com/youtube/v3/live/docs/liveChatMessages#resource
                    if (TextMessageEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        if (liveChatMessage.Snippet.HasDisplayContent.GetValueOrDefault() && !string.IsNullOrEmpty(liveChatMessage.Snippet.DisplayMessage))
                        {
                            await ServiceManager.Get<ChatService>().AddMessage(new YouTubeChatMessageViewModel(liveChatMessage, user));
                        }
                    }
                    else if (NewMemberEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        user.Roles.Add(UserRoleEnum.YouTubeMember);
                        user.Roles.Add(UserRoleEnum.Subscriber);
                        user.SubscribeDate = DateTimeOffset.Now;
                        user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).MemberLevels.Clear();
                        user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).MemberLevels.Add(liveChatMessage.Snippet.NewSponsorDetails.MemberLevelName);

                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                        parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.NewSponsorDetails.MemberLevelName;
                        if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelNewMember, parameters))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

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

                            EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, user, youTubeMembershipTier: liveChatMessage.Snippet.NewSponsorDetails.MemberLevelName));
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscribed, user.DisplayName), ChannelSession.Settings.AlertSubColor));
                        }
                    }
                    else if (MemberMilestoneEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        int months = (int)liveChatMessage.Snippet.MemberMilestoneChatDetails.MemberMonth.GetValueOrDefault();

                        user.Roles.Add(UserRoleEnum.YouTubeMember);
                        // TODO
                        //user.SubscriberTier = subMessage.Tier;
                        if (!user.SubscribeDate.HasValue)
                        {
                            user.SubscribeDate = DateTimeOffset.Now.SubtractMonths(months);
                        }

                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                        parameters.SpecialIdentifiers["message"] = liveChatMessage.Snippet.MemberMilestoneChatDetails.UserComment;
                        parameters.SpecialIdentifiers["usersubmonths"] = months.ToString();
                        parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.MemberMilestoneChatDetails.MemberLevelName;

                        if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelMemberMilestone, parameters))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

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

                            EventService.ResubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, user, months: months, youTubeMembershipTier: liveChatMessage.Snippet.MemberMilestoneChatDetails.MemberLevelName));
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertResubscribed, user.DisplayName, months), ChannelSession.Settings.AlertSubColor));
                        }
                    }
                    else if (MembershipGiftingEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        MixItUp.Base.Model.YouTube.YouTubeMembershipsGiftedModel membershipsGifted = new MixItUp.Base.Model.YouTube.YouTubeMembershipsGiftedModel(user, liveChatMessage.Snippet.MembershipGiftingDetails);
                        if (membershipsGifted.Amount > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                        {
                            this.userGiftedMembershipDictionary[membershipsGifted.Gifter.PlatformID] = membershipsGifted;

                            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                            {
                                for (int i = 0; i < membershipsGifted.Amount; i++)
                                {
                                    currency.AddAmount(user, currency.OnSubscribeBonus * membershipsGifted.Amount);
                                }
                            }

                            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                            {
                                if (user.MeetsRole(streamPass.UserPermission))
                                {
                                    streamPass.AddAmount(user, streamPass.SubscribeBonus * membershipsGifted.Amount);
                                }
                            }

                            CommandParametersModel parameters = new CommandParametersModel(user);
                            parameters.SpecialIdentifiers["subsgiftedamount"] = membershipsGifted.Amount.ToString();
                            parameters.SpecialIdentifiers["usersubplan"] = membershipsGifted.Tier;

                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelMassMembershipGifted, parameters);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGiftedTier, user.FullDisplayName, membershipsGifted.Amount, membershipsGifted.Tier), ChannelSession.Settings.AlertMassGiftedSubColor));
                        }
                    }
                    else if (GiftMembershipReceivedEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        UserV2ViewModel gifter = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.YouTube, platformID: liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId, performPlatformSearch: true);
                        if (gifter == null)
                        {
                            await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(liveChatMessage));
                        }
                        UserV2ViewModel receiver = user;

                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = receiver.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                        receiver.Roles.Add(UserRoleEnum.YouTubeMember);
                        receiver.SubscribeDate = DateTimeOffset.Now;
                        receiver.TotalSubsReceived++;
                        receiver.TotalMonthsSubbed++;

                        if (this.userGiftedMembershipDictionary.TryGetValue(liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId, out MixItUp.Base.Model.YouTube.YouTubeMembershipsGiftedModel membershipsGifted))
                        {
                            membershipsGifted.Amount--;
                            if (membershipsGifted.Amount == 0)
                            {
                                this.userGiftedMembershipDictionary.Remove(liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId);
                            }
                        }
                        else
                        {
                            foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                            {
                                currency.AddAmount(receiver, currency.OnSubscribeBonus);
                            }

                            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                            {
                                if (receiver.MeetsRole(streamPass.UserPermission))
                                {
                                    streamPass.AddAmount(receiver, streamPass.SubscribeBonus);
                                }
                            }

                            CommandParametersModel parameters = new CommandParametersModel(gifter, StreamingPlatformTypeEnum.YouTube);
                            // TODO
                            parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.GiftMembershipReceivedDetails.MemberLevelName;
                            parameters.Arguments.Add(receiver.Username);
                            parameters.TargetUser = receiver;
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelMembershipGifted, parameters);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(gifter, string.Format(MixItUp.Base.Resources.AlertSubscriptionGiftedTier, gifter.FullDisplayName, liveChatMessage.Snippet.GiftMembershipReceivedDetails.MemberLevelName, receiver.FullDisplayName), ChannelSession.Settings.AlertGiftedSubColor));
                            EventService.SubscriptionGiftedOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.YouTube, user, gifter, youTubeMembershipTier: liveChatMessage.Snippet.GiftMembershipReceivedDetails.MemberLevelName));
                        }
                    }
                    else if (SuperChatEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        YouTubeSuperChatViewModel superChat = new YouTubeSuperChatViewModel(liveChatMessage.Snippet.SuperChatDetails, user);

                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatUserData] = user.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatAmountData] = superChat.AmountDisplay;

                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                        superChat.SetCommandParameterData(parameters);

                        foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                        {
                            if (parameters.User.MeetsRole(streamPass.UserPermission))
                            {
                                streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.DonationBonus * superChat.Amount));
                            }
                        }

                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelSuperChat, parameters);

                        await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertYouTubeSuperChat, user.FullDisplayName, superChat.AmountDisplay), ChannelSession.Settings.AlertYouTubeSuperChatColor));

                        EventService.YouTubeSuperChatOccurred(superChat);
                    }
                    else if (SuperStickerEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        YouTubeSuperChatViewModel superChat = new YouTubeSuperChatViewModel(liveChatMessage.Snippet.SuperChatDetails, user);

                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatUserData] = user.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatAmountData] = superChat.AmountDisplay;

                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                        superChat.SetCommandParameterData(parameters);

                        foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                        {
                            if (parameters.User.MeetsRole(streamPass.UserPermission))
                            {
                                streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.DonationBonus * superChat.Amount));
                            }
                        }

                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelSuperChat, parameters);

                        await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertYouTubeSuperChat, user.FullDisplayName, superChat.AmountDisplay), ChannelSession.Settings.AlertYouTubeSuperChatColor));

                        EventService.YouTubeSuperChatOccurred(superChat);
                    }
                    else if (MessageDeletedEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        await ServiceManager.Get<ChatService>().DeleteMessage(liveChatMessage.Snippet.MessageDeletedDetails.DeletedMessageId);
                    }
                    else if (UserBannedEventMessageType.Equals(liveChatMessage.Snippet.Type))
                    {
                        // TODO
                        if (liveChatMessage.Snippet.UserBannedDetails.BanDurationSeconds.HasValue)
                        {
                            // Timeout
                        }
                        else
                        {
                            // Ban
                        }
                        //message.Snippet.UserBannedDetails.BannedUserDetails.ChannelId
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }
}
