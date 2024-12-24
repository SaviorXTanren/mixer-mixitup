using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.YouTube;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.YouTube;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using System.Threading;
using System.Linq;

namespace MixItUp.Base.Services.YouTube.New
{
    public class YouTubeSession : StreamingPlatformSessionBase
    {
        private const int MaxMessageLength = 200;

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

        public override IEnumerable<string> StreamerScopes { get; protected set; } = new List<string>()
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

        public override IEnumerable<string> BotScopes { get; protected set; } = new List<string>()
        {
            "email",
            "profile",
            "openid",
            "https://www.googleapis.com/auth/youtube",
            "https://www.googleapis.com/auth/youtube.force-ssl",
            "https://www.googleapis.com/auth/youtube.readonly",
        };

        public override string StreamerID { get { return this.Streamer?.Id; } }
        public override string StreamerUsername { get { return this.Streamer?.Snippet?.Title; } }
        public override string BotID { get { return this.Bot?.Id; } }
        public override string BotUsername { get { return this.Bot?.Snippet?.Title; } }
        public override string ChannelID { get { return this.Streamer?.Id; } }
        public override string ChannelLink { get { return this.Streamer?.Snippet?.CustomUrl; } }
        public override string StreamLink
        {
            get
            {
                var broadcast = this.LiveBroadcasts.FirstOrDefault();
                if (broadcast.Value != null)
                {
                    return $"https://youtube.com/watch?v={broadcast.Value.Id}";
                }
                return string.Empty;
            }
        }

        public Channel Streamer { get; private set; }
        public Channel Bot { get; private set; }
        public Dictionary<string, LiveBroadcast> LiveBroadcasts { get; private set; } = new Dictionary<string, LiveBroadcast>();
        public Dictionary<string, Video> Videos { get; private set; } = new Dictionary<string, Video>();

        public YouTubeService StreamerService { get; private set; }
        public YouTubeService BotService { get; private set; }

        public IEnumerable<YouTubeChatEmoteModel> Emotes { get; private set; } = new List<YouTubeChatEmoteModel>();
        public Dictionary<string, YouTubeChatEmoteViewModel> EmoteDictionary { get; private set; } = new Dictionary<string, YouTubeChatEmoteViewModel>();

        private string nextMessagesToken = null;

        private CancellationTokenSource messageBackgroundPollingTokenSource;
        private int messagePollingInterval = 5000;

        private Dictionary<string, YouTubeMembershipsGiftedModel> userGiftedMembershipDictionary = new Dictionary<string, YouTubeMembershipsGiftedModel>();

        private DateTime launchDateTime = DateTime.Now;

        public override async Task RefreshDetails()
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
                this.LiveBroadcasts = newBroadcasts;
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
                if (this.LiveBroadcasts.Any(b => this.launchDateTime < b.Value.Snippet.ActualStartTimeDateTimeOffset.GetValueOrDefault()))
                {
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                }

                if (this.LiveBroadcasts.Any(b => this.launchDateTime < b.Value.Snippet.ActualEndTimeDateTimeOffset.GetValueOrDefault()))
                {
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                }
            }
        }

        protected override async Task<Result> ConnectStreamer()
        {
            Result result = await StreamerService.Connect();
            if (!result.Success)
            {
                return result;
            }

            this.Streamer = await this.StreamerService.GetCurrentChannel();
            if (this.Streamer == null)
            {
                return new Result(Resources.YouTubeFailedToGetUserData);
            }

            this.EmoteDictionary.Clear();
            this.Emotes = await this.StreamerService.GetChatEmotes();
            if (this.Emotes != null)
            {
                foreach (YouTubeChatEmoteModel emote in this.Emotes)
                {
                    foreach (string shortcut in emote.shortcuts)
                    {
                        this.EmoteDictionary[shortcut] = new YouTubeChatEmoteViewModel(emote);
                    }
                }
            }

            this.messageBackgroundPollingTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(this.MessageBackgroundPolling, this.messageBackgroundPollingTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            return new Result();
        }

        protected override Task DisconnectStreamer()
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

        protected override async Task<Result> ConnectBot()
        {
            Result result = await BotService.Connect();
            if (!result.Success)
            {
                return result;
            }

            this.Bot = await this.BotService.GetCurrentChannel();
            if (this.Bot == null)
            {
                return new Result(Resources.YouTubeFailedToGetUserData);
            }

            return new Result();
        }

        protected override Task DisconnectBot()
        {
            return Task.CompletedTask;
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
                                        await this.ProcessMessages(newMessages);

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

        private async Task ProcessMessages(IEnumerable<LiveChatMessage> liveChatMessages)
        {
            foreach (LiveChatMessage liveChatMessage in liveChatMessages)
            {
                try
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        Logger.Log(LogLevel.Debug, string.Format("YouTube Chat Packet Received: {0}", JSONSerializerHelper.SerializeToString(liveChatMessage)));
                    }

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
                            YouTubeMembershipsGiftedModel membershipsGifted = new YouTubeMembershipsGiftedModel(user, liveChatMessage.Snippet.MembershipGiftingDetails);
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

                            if (this.userGiftedMembershipDictionary.TryGetValue(liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId, out YouTubeMembershipsGiftedModel membershipsGifted))
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
                            superChat.SetCommandParameterSpecialIdentifiers(parameters);

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
                            superChat.SetCommandParameterSpecialIdentifiers(parameters);

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
}
