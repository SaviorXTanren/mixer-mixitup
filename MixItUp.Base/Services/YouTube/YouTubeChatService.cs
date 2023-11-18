using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YouTube.Base.Model;

namespace MixItUp.Base.Services.YouTube
{
    // https://stackoverflow.com/questions/64726611/how-to-get-a-list-of-youtube-channel-emojis

    public class YouTubeChatEmoteModel
    {
        public class YouTubeChatEmoteImageModel
        {
            public class YouTubeChatEmoteImageURLModel
            {
                public string url { get; set; }
            }

            public List<YouTubeChatEmoteImageURLModel> thumbnails { get; set; } = new List<YouTubeChatEmoteImageURLModel>();
        }

        public string emojiId { get; set; }
        public List<string> searchTerms { get; set; } = new List<string>();
        public List<string> shortcuts { get; set; } = new List<string>();

        public YouTubeChatEmoteImageModel image { get; set; } = null;
    }

    public class YouTubeMembershipsGiftedModel
    {
        public UserV2ViewModel Gifter { get; private set; }

        public int Amount { get; set; }

        public string Tier { get; private set; }

        public List<UserV2ViewModel> Receivers { get; private set; } = new List<UserV2ViewModel>();

        public YouTubeMembershipsGiftedModel(UserV2ViewModel user, LiveChatMembershipGiftingDetails giftingDetails)
        {
            this.Gifter = user;
            this.Amount = giftingDetails.GiftMembershipsCount.GetValueOrDefault();
            this.Tier = giftingDetails.GiftMembershipsLevelName;
        }
    }

    public class YouTubeChatService : StreamingPlatformServiceBase
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

        private string nextMessagesToken = null;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private CancellationTokenSource messageBackgroundPollingTokenSource;
        private int messagePollingInterval = 5000;

        public YouTubeChatService() { }

        public IEnumerable<YouTubeChatEmoteModel> Emotes { get; private set; } = new List<YouTubeChatEmoteModel>();
        public Dictionary<string, YouTubeChatEmoteModel> EmoteDictionary { get; private set; } = new Dictionary<string, YouTubeChatEmoteModel>();

        public IDictionary<string, BetterTTVEmoteModel> BetterTTVEmotes { get { return this.betterTTVEmotes; } }
        private Dictionary<string, BetterTTVEmoteModel> betterTTVEmotes = new Dictionary<string, BetterTTVEmoteModel>();

        private Dictionary<string, YouTubeMembershipsGiftedModel> userGiftedMembershipDictionary = new Dictionary<string, YouTubeMembershipsGiftedModel>();

        public override string Name { get { return "YouTube Chat"; } }

        public bool IsUserConnected { get; private set; }
        public bool IsBotConnected { get; private set; }

        public async Task<Result> ConnectUser()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        List<Task> initializationTasks = new List<Task>();

                        initializationTasks.Add(Task.Run(async () =>
                        {
                            this.EmoteDictionary.Clear();
                            this.Emotes = await this.GetChatEmotes();
                            if (this.Emotes != null)
                            {
                                foreach (YouTubeChatEmoteModel emote in this.Emotes)
                                {
                                    foreach (string shortcut in emote.shortcuts)
                                    {
                                        this.EmoteDictionary[shortcut] = emote;
                                    }
                                }
                            }
                        }));

                        if (ChannelSession.Settings.ShowBetterTTVEmotes)
                        {
                            initializationTasks.Add(ServiceManager.Get<BetterTTVService>().DownloadGlobalBetterTTVEmotes());
                            initializationTasks.Add(ServiceManager.Get<BetterTTVService>().DownloadYouTubeBetterTTVEmotes(ServiceManager.Get<YouTubeSessionService>().ChannelID));
                        }

                        await Task.WhenAll(initializationTasks);

                        this.messageBackgroundPollingTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(this.MessageBackgroundPolling, this.messageBackgroundPollingTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        this.IsUserConnected = true;

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                }));
            }
            return new Result(MixItUp.Base.Resources.YouTubeCouldNotEstablishChatConnection);
        }

        public Task DisconnectUser()
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

            this.IsUserConnected = false;

            return Task.CompletedTask;
        }

        public async Task<Result> ConnectBot()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected && ServiceManager.Get<YouTubeSessionService>().BotConnection != null)
            {
                await Task.Delay(1);

                this.IsBotConnected = true;

                return new Result();
            }
            return new Result(MixItUp.Base.Resources.YouTubeCouldNotEstablishChatConnection);
        }

        public Task DisconnectBot()
        {
            this.IsBotConnected = false;
            return Task.CompletedTask;
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsLive)
            {
                await this.messageSemaphore.WaitAsync();

                YouTubePlatformService connection = this.GetConnection(sendAsStreamer);
                if (connection != null)
                {
                    string subMessage = null;
                    do
                    {
                        message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);
                        await connection.SendChatMessage(ServiceManager.Get<YouTubeSessionService>().Broadcast, message);
                        message = subMessage;
                        await Task.Delay(1000);
                    }
                    while (!string.IsNullOrEmpty(message));
                }

                this.messageSemaphore.Release();
            }
        }

        public async Task DeleteMessage(ChatMessageViewModel message)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsLive)
            {
                await this.GetConnection(sendAsStreamer: true).DeleteChatMessage(new LiveChatMessage() { Id = message.ID });
            }
        }

        public async Task<LiveChatModerator> ModUser(UserV2ViewModel user)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsLive)
            {
                await this.GetConnection(sendAsStreamer: true).ModChatUser(ServiceManager.Get<YouTubeSessionService>().Broadcast, new Channel() { Id = user.PlatformID });
            }
            return null;
        }

        public async Task<LiveChatBan> TimeoutUser(UserV2ViewModel user, ulong duration)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsLive)
            {
                await this.GetConnection(sendAsStreamer: true).TimeoutChatUser(ServiceManager.Get<YouTubeSessionService>().Broadcast, new Channel() { Id = user.PlatformID }, duration);
            }
            return null;
        }

        public async Task<LiveChatBan> BanUser(UserV2ViewModel user)
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsLive)
            {
                await this.GetConnection(sendAsStreamer: true).BanChatUser(ServiceManager.Get<YouTubeSessionService>().Broadcast, new Channel() { Id = user.PlatformID });
            }
            return null;
        }

        public async Task<IEnumerable<YouTubeChatEmoteModel>> GetChatEmotes()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync("https://www.gstatic.com/youtube/img/emojis/emojis-svg-8.json");
                    if (response.IsSuccessStatusCode)
                    {
                        return JSONSerializerHelper.DeserializeFromString<List<YouTubeChatEmoteModel>>(await response.Content.ReadAsStringAsync());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<YouTubeChatEmoteModel>();
        }

        private YouTubePlatformService GetConnection(bool sendAsStreamer = false) { return (sendAsStreamer || !ServiceManager.Get<YouTubeSessionService>().IsBotConnected) ? ServiceManager.Get<YouTubeSessionService>().UserConnection : ServiceManager.Get<YouTubeSessionService>().BotConnection; }

        private async Task MessageBackgroundPolling()
        {
            while (!this.messageBackgroundPollingTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (ServiceManager.Get<YouTubeSessionService>().IsLive)
                    {
                        LiveChatMessagesResultModel result = await this.GetConnection(sendAsStreamer: true).GetChatMessages(ServiceManager.Get<YouTubeSessionService>().Broadcast, this.nextMessagesToken);
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
                        UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.YouTube, liveChatMessage.AuthorDetails.ChannelId);
                        if (user == null)
                        {
                            Channel youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(liveChatMessage.AuthorDetails.ChannelId);
                            if (youtubeUser != null)
                            {
                                user = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(youtubeUser));
                            }
                            else
                            {
                                user = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(liveChatMessage));
                            }
                            await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);
                        }
                        user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).SetMessageProperties(liveChatMessage);

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

                                GlobalEvents.SubscribeOccurred(user);
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

                                GlobalEvents.ResubscribeOccurred(new Tuple<UserV2ViewModel, int>(user, months));
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
                            UserV2ViewModel gifter = await ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.YouTube, liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId, performPlatformSearch: true);
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
                                GlobalEvents.SubscriptionGiftedOccurred(gifter, receiver);
                            }
                        }
                        else if (SuperChatEventMessageType.Equals(liveChatMessage.Snippet.Type))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatUserData] = user.ID;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatAmountData] = liveChatMessage.Snippet.SuperChatDetails.AmountDisplayString;

                            double amount = Math.Round((double)liveChatMessage.Snippet.SuperChatDetails.AmountMicros.GetValueOrDefault() / 1000000.0, 2);
                            int amountDigits = (int)amount * 100;

                            CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                            parameters.SpecialIdentifiers["amountnumberdigits"] = amountDigits.ToString();
                            parameters.SpecialIdentifiers["amountnumber"] = amount.ToString();
                            parameters.SpecialIdentifiers["amount"] = liveChatMessage.Snippet.SuperChatDetails.AmountDisplayString;
                            parameters.SpecialIdentifiers["tier"] = liveChatMessage.Snippet.SuperChatDetails.Tier.GetValueOrDefault().ToString();
                            parameters.SpecialIdentifiers["message"] = liveChatMessage.Snippet.SuperChatDetails.UserComment;

                            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                            {
                                if (parameters.User.MeetsRole(streamPass.UserPermission))
                                {
                                    streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.DonationBonus * amount));
                                }
                            }

                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelSuperChat, parameters);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertYouTubeSuperChat, user.FullDisplayName, liveChatMessage.Snippet.SuperChatDetails.AmountDisplayString), ChannelSession.Settings.AlertYouTubeSuperChatColor));
                        }
                        else if (SuperStickerEventMessageType.Equals(liveChatMessage.Snippet.Type))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatUserData] = user.ID;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatAmountData] = liveChatMessage.Snippet.SuperStickerDetails.AmountDisplayString;

                            double amount = Math.Round((double)liveChatMessage.Snippet.SuperStickerDetails.AmountMicros.GetValueOrDefault() / 1000000.0, 2);
                            int amountDigits = (int)amount * 100;

                            CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.YouTube);
                            parameters.SpecialIdentifiers["amountnumberdigits"] = amountDigits.ToString();
                            parameters.SpecialIdentifiers["amountnumber"] = amount.ToString();
                            parameters.SpecialIdentifiers["amount"] = liveChatMessage.Snippet.SuperStickerDetails.AmountDisplayString;
                            parameters.SpecialIdentifiers["tier"] = liveChatMessage.Snippet.SuperStickerDetails.Tier.GetValueOrDefault().ToString();
                            parameters.SpecialIdentifiers["message"] = liveChatMessage.Snippet.SuperStickerDetails.SuperStickerMetadata.AltText;

                            foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                            {
                                if (parameters.User.MeetsRole(streamPass.UserPermission))
                                {
                                    streamPass.AddAmount(user, (int)Math.Ceiling(streamPass.DonationBonus * amount));
                                }
                            }

                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelSuperChat, parameters);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertYouTubeSuperChat, user.FullDisplayName, liveChatMessage.Snippet.SuperStickerDetails.AmountDisplayString), ChannelSession.Settings.AlertYouTubeSuperChatColor));
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes the resources of the object.
        /// </summary>
        /// <param name="disposing">Whether disposal is taking place</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.messageBackgroundPollingTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        /// <summary>
        /// Disposes the resources of the object.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
