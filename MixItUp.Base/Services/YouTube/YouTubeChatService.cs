using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YouTube.Base.Clients;

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
        public UserV2ViewModel User { get; private set; }

        public int Amount { get; private set; }

        public string Tier { get; private set; }

        public List<UserV2ViewModel> Receivers { get; private set; } = new List<UserV2ViewModel>();

        public YouTubeMembershipsGiftedModel(UserV2ViewModel user, LiveChatMembershipGiftingDetails giftingDetails)
        {
            this.User = user;
            this.Amount = giftingDetails.GiftMembershipsCount.GetValueOrDefault();
            this.Tier = giftingDetails.GiftMembershipsLevelName;
        }

        public YouTubeMembershipsGiftedModel(UserV2ViewModel user, LiveChatGiftMembershipReceivedDetails giftReceivedDetails)
        {
            this.User = user;
            this.Amount = 1;
            this.Tier = giftReceivedDetails.MemberLevelName;
        }
    }

    public class YouTubeChatService : StreamingPlatformServiceBase
    {
        private const int MaxMessageLength = 200;

        private const string TextMessageEventMessageType = "textMessageEvent";
        private const string NewMemberEventMessageType = "newSponsorEvent";
        private const string MemberMilestoneEventMessageType = "memberMilestoneChatEvent";
        private const string MembershipGiftingEventMessageType = "membershipGiftingEvent";
        private const string GiftMembershipReceivedEventMessageType = "giftMembershipReceivedEvent";
        private const string SuperChatEventMessageType = "superChatEvent";
        private const string SuperStickerEventMessageType = "superStickerEvent";
        private const string MessageDeletedEventMessageType = "messageDeletedEvent";
        private const string UserBannedEventMessageType = "userBannedEvent";

        private ChatClient userClient;
        private ChatClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        public YouTubeChatService() { }

        public IEnumerable<YouTubeChatEmoteModel> Emotes { get; private set; } = new List<YouTubeChatEmoteModel>();
        public Dictionary<string, YouTubeChatEmoteModel> EmoteDictionary { get; private set; } = new Dictionary<string, YouTubeChatEmoteModel>();

        private Dictionary<string, YouTubeMembershipsGiftedModel> userGiftedMembershipDictionary = new Dictionary<string, YouTubeMembershipsGiftedModel>();

        public override string Name { get { return "YouTube Chat"; } }

        public bool IsUserConnected { get { return this.userClient != null; } }
        public bool IsBotConnected { get { return this.botClient != null; } }

        public LiveBroadcast Broadcast { get { return (this.IsUserConnected) ? this.userClient.Broadcast : null; } }

        public async Task<Result> ConnectUser()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.userClient = new ChatClient(ServiceManager.Get<YouTubeSessionService>().UserConnection.Connection);

                        this.userClient.OnMessagesReceived += UserClient_OnMessagesReceived;

                        int pollDelay = 5000;
                        if (RandomHelper.GenerateProbability() > 50)
                        {
                            pollDelay = 10000;
                        }

                        Logger.Log(LogLevel.Information, $"YouTube Chat polling delay set to {pollDelay} milliseconds");

                        if (!await this.userClient.Connect(minimumPollTimeMilliseconds: pollDelay))
                        {
                            return new Result(MixItUp.Base.Resources.YouTubeFailedToConnectToChat);
                        }

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

        public async Task DisconnectUser()
        {
            try
            {
                if (this.userClient != null)
                {
                    this.userClient.OnMessagesReceived -= UserClient_OnMessagesReceived;

                    await this.userClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.userClient = null;
        }

        public async Task<Result> ConnectBot()
        {
            if (ServiceManager.Get<YouTubeSessionService>().IsConnected && ServiceManager.Get<YouTubeSessionService>().BotConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        LiveBroadcast broadcast = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetMyActiveBroadcast();
                        if (broadcast != null)
                        {
                            return new Result(MixItUp.Base.Resources.YouTubeNoLiveBroadcast);
                        }

                        this.botClient = new ChatClient(ServiceManager.Get<YouTubeSessionService>().BotConnection.Connection);

                        if (!await this.botClient.Connect(broadcast, listenForMessage: false))
                        {
                            return new Result(MixItUp.Base.Resources.YouTubeFailedToConnectToChat);
                        }

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

        public async Task DisconnectBot()
        {
            try
            {
                if (this.botClient != null)
                {
                    await this.botClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.botClient = null;
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            await this.messageSemaphore.WaitAndRelease(async () =>
            {
                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    string subMessage = null;
                    do
                    {
                        message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);
                        await client.SendMessage(message);
                        message = subMessage;
                        await Task.Delay(500);
                    }
                    while (!string.IsNullOrEmpty(message));
                }
            });
        }

        public async Task DeleteMessage(ChatMessageViewModel message) { await this.userClient.DeleteMessage(new LiveChatMessage() { Id = message.ID }); }

        public async Task<LiveChatModerator> ModUser(UserV2ViewModel user) { return await this.userClient.ModUser(new Channel() { Id = user.PlatformID }); }

        public async Task<LiveChatBan> TimeoutUser(UserV2ViewModel user, ulong duration) { return await this.userClient.TimeoutUser(new Channel() { Id = user.PlatformID }, duration); }

        public async Task<LiveChatBan> BanUser(UserV2ViewModel user) { return await this.userClient.BanUser(new Channel() { Id = user.PlatformID }); }

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

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private async void UserClient_OnMessagesReceived(object sender, IEnumerable<LiveChatMessage> liveChatMessages)
        {
            if (liveChatMessages != null)
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
                                CommandParametersModel parameters = new CommandParametersModel(user);
                                if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.YouTubeChannelNewMember, parameters))
                                {
                                    // TODO
                                    parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.NewSponsorDetails.MemberLevelName;

                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                                    user.Roles.Add(UserRoleEnum.YouTubeMember);
                                    // TODO
                                    //user.SubscriberTier = subMessage.Tier;
                                    user.SubscribeDate = DateTimeOffset.Now;

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

                                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelNewMember, parameters);

                                    GlobalEvents.SubscribeOccurred(user);
                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscribed, user.DisplayName), ChannelSession.Settings.AlertSubColor));
                                }
                            }
                            else if (MemberMilestoneEventMessageType.Equals(liveChatMessage.Snippet.Type))
                            {
                                CommandParametersModel parameters = new CommandParametersModel(user);
                                if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.YouTubeChannelMemberMilestone, parameters))
                                {
                                    int months = (int)liveChatMessage.Snippet.MemberMilestoneChatDetails.MemberMonth.GetValueOrDefault();

                                    // TODO
                                    parameters.SpecialIdentifiers["message"] = liveChatMessage.Snippet.MemberMilestoneChatDetails.UserComment;
                                    parameters.SpecialIdentifiers["usersubmonths"] = months.ToString();
                                    parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.MemberMilestoneChatDetails.MemberLevelName;

                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                                    user.Roles.Add(UserRoleEnum.YouTubeMember);
                                    // TODO
                                    //user.SubscriberTier = subMessage.Tier;
                                    if (!user.SubscribeDate.HasValue)
                                    {
                                        user.SubscribeDate = DateTimeOffset.Now.SubtractMonths(months);
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

                                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelMemberMilestone, parameters);

                                    GlobalEvents.ResubscribeOccurred(new Tuple<UserV2ViewModel, int>(user, months));
                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertResubscribed, user.DisplayName, months), ChannelSession.Settings.AlertSubColor));
                                }
                            }
                            else if (MembershipGiftingEventMessageType.Equals(liveChatMessage.Snippet.Type))
                            {
                                YouTubeMembershipsGiftedModel membershipsGifted = new YouTubeMembershipsGiftedModel(user, liveChatMessage.Snippet.MembershipGiftingDetails);
                                this.userGiftedMembershipDictionary[membershipsGifted.User.PlatformID] = membershipsGifted;

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

                                if (membershipsGifted.Amount > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                                {
                                    // TODO
                                    CommandParametersModel parameters = new CommandParametersModel(user);
                                    parameters.SpecialIdentifiers["subsgiftedamount"] = membershipsGifted.Amount.ToString();
                                    parameters.SpecialIdentifiers["usersubplan"] = membershipsGifted.Tier;

                                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelMassSubscriptionsGifted, parameters);

                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGiftedTier, user.FullDisplayName, membershipsGifted.Amount, membershipsGifted.Tier), ChannelSession.Settings.AlertMassGiftedSubColor));
                                }
                            }
                            else if (GiftMembershipReceivedEventMessageType.Equals(liveChatMessage.Snippet.Type))
                            {
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                                user.Roles.Add(UserRoleEnum.YouTubeMember);
                                user.SubscribeDate = DateTimeOffset.Now;
                                // TODO
                                //user.SubscriberTier = giftedSubEvent.PlanTierNumber;
                                user.TotalSubsReceived++;
                                user.TotalMonthsSubbed++;

                                if (this.userGiftedMembershipDictionary.TryGetValue(liveChatMessage.Snippet.GiftMembershipReceivedDetails.GifterChannelId, out YouTubeMembershipsGiftedModel membershipsGifted))
                                {
                                    if (membershipsGifted.Amount > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                                    {
                                        // Skip ones that are filtered by the mass gifted subs amount
                                        continue;
                                    }
                                }
                                else
                                {
                                    membershipsGifted = new YouTubeMembershipsGiftedModel(user, liveChatMessage.Snippet.GiftMembershipReceivedDetails);
                                }
                                membershipsGifted.Receivers.Add(user);

                                CommandParametersModel parameters = new CommandParametersModel(membershipsGifted.User);
                                // TODO
                                parameters.SpecialIdentifiers["usersubplan"] = liveChatMessage.Snippet.GiftMembershipReceivedDetails.MemberLevelName;
                                parameters.Arguments.Add(user.Username);
                                parameters.TargetUser = user;
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelMembershipGifted, parameters);

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(membershipsGifted.User, string.Format(MixItUp.Base.Resources.AlertSubscriptionGiftedTier, membershipsGifted.User.FullDisplayName, liveChatMessage.Snippet.GiftMembershipReceivedDetails.MemberLevelName, user.FullDisplayName), ChannelSession.Settings.AlertGiftedSubColor));
                                GlobalEvents.SubscriptionGiftedOccurred(membershipsGifted.User, user);
                            }
                            else if (SuperChatEventMessageType.Equals(liveChatMessage.Snippet.Type))
                            {
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatUserData] = user.ID;
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSuperChatAmountData] = liveChatMessage.Snippet.SuperChatDetails.AmountDisplayString;

                                double amount = Math.Round((double)liveChatMessage.Snippet.SuperChatDetails.AmountMicros.GetValueOrDefault() / 1000000.0, 2);

                                CommandParametersModel parameters = new CommandParametersModel(user);
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

                                CommandParametersModel parameters = new CommandParametersModel(user);
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
        }
    }
}
