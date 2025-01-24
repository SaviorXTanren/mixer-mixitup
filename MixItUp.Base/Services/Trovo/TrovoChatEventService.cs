using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.Trovo.API;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo
{
    [Obsolete]
    public class TrovoSubscriptionMessageModel
    {
        private const string SubscriptionRenewedMessageText = "has renewed subscription";
        private const string SubscriptionTierMessageFormatText = "Tier \\d+";
        private const string SubscriptionMonthsMessageFormatText = "\\d+ months";

        public bool IsResub { get; private set; } = false;

        public int Months { get; private set; } = 1;

        public int Tier { get; private set; } = 1;

        public ChatMessageModel Message { get; private set; }

        public TrovoSubscriptionMessageModel(ChatMessageModel message)
        {
            this.Message = message;

            if (!string.IsNullOrEmpty(message.sub_lv) && int.TryParse(message.sub_lv.Replace("L", string.Empty), out int tier))
            {
                this.Tier = tier;

                Match match = Regex.Match(message.content, SubscriptionTierMessageFormatText);
                if (match != null && match.Success)
                {
                    string[] splits = match.Value.Split(new char[] { ' ' });
                    if (splits != null && splits.Length > 1 && int.TryParse(splits[1], out tier))
                    {
                        this.Months = tier;
                    }
                }
            }

            this.IsResub = message.content.Contains(SubscriptionRenewedMessageText, StringComparison.OrdinalIgnoreCase);

            if (this.IsResub)
            {
                Match match = Regex.Match(message.content, SubscriptionMonthsMessageFormatText);
                if (match != null && match.Success)
                {
                    string[] splits = match.Value.Split(new char[] { ' ' });
                    if (splits != null && splits.Length > 0 && int.TryParse(splits[0], out int months))
                    {
                        this.Months = months;
                    }
                }
            }
            else
            {
                this.Months = 1;
            }
        }
    }

    [Obsolete]
    public class TrovoChatEventService : StreamingPlatformServiceBase
    {
        private const string TreasureBoxUnleashedActivityTopic = "item_drop_box_unleash";

        private const int MaxMessageLength = 500;

        private Dictionary<string, TrovoChatEmoteViewModel> channelEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();
        private Dictionary<string, TrovoChatEmoteViewModel> eventEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();
        private Dictionary<string, TrovoChatEmoteViewModel> globalEmotes = new Dictionary<string, TrovoChatEmoteViewModel>();

        private ChatClient userClient;
        private ChatClient botClient;

        private CancellationTokenSource cancellationTokenSource;

        private bool processMessages = false;
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private HashSet<string> messagesProcessed = new HashSet<string>();
        private Dictionary<Guid, int> userSubsGiftedInstanced = new Dictionary<Guid, int>();

        private HashSet<string> previousViewers = new HashSet<string>();

        public TrovoChatEventService() { }

        public override string Name { get { return MixItUp.Base.Resources.TrovoChat; } }

        public IDictionary<string, TrovoChatEmoteViewModel> ChannelEmotes { get { return this.channelEmotes; } }
        public IDictionary<string, TrovoChatEmoteViewModel> EventEmotes { get { return this.eventEmotes; } }
        public IDictionary<string, TrovoChatEmoteViewModel> GlobalEmotes { get { return this.globalEmotes; } }

        public bool IsUserConnected { get { return this.userClient != null && this.userClient.IsOpen(); } }
        public bool IsBotConnected { get { return this.botClient != null && this.botClient.IsOpen(); } }

        public async Task<Result> ConnectUser()
        {
            if (ServiceManager.Get<TrovoSessionService>().IsConnected)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.cancellationTokenSource = new CancellationTokenSource();

                        this.userClient = new ChatClient(ServiceManager.Get<TrovoSessionService>().UserConnection.Connection);

                        string token = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetChatToken();
                        if (string.IsNullOrEmpty(token))
                        {
                            return new Result(MixItUp.Base.Resources.TrovoFailedToGetChatToken);
                        }

                        ChatEmotePackageModel emotePackage = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetPlatformAndChannelEmotes(ServiceManager.Get<TrovoSessionService>().ChannelID);
                        if (emotePackage != null)
                        {
                            if (emotePackage.customizedEmotes?.channel != null)
                            {
                                foreach (ChannelChatEmotesModel channel in emotePackage.customizedEmotes.channel)
                                {
                                    foreach (ChatEmoteModel emote in channel.emotes)
                                    {
                                        this.ChannelEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                                    }
                                }
                            }

                            if (emotePackage.eventEmotes != null)
                            {
                                foreach (EventChatEmoteModel emote in emotePackage.eventEmotes)
                                {
                                    this.EventEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                                }
                            }

                            if (emotePackage.globalEmotes != null)
                            {
                                foreach (GlobalChatEmoteModel emote in emotePackage.globalEmotes)
                                {
                                    this.GlobalEmotes[emote.name] = new TrovoChatEmoteViewModel(emote);
                                }
                            }
                        }
                        else
                        {
                            Logger.Log(LogLevel.Error, "Failed to get available Trovo emotes");
                        }

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred += Client_OnSentOccurred;
                            this.userClient.OnTextReceivedOccurred += UserClient_OnTextReceivedOccurred;
                        }
                        this.userClient.OnDisconnectOccurred += UserClient_OnDisconnectOccurred;

                        this.userClient.OnChatMessageReceived += UserClient_OnChatMessageReceived;

                        this.processMessages = false;
                        if (!await this.userClient.Connect(token))
                        {
                            return new Result(MixItUp.Base.Resources.TrovoFailedToConnectToChat);
                        }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                        {
                            await Task.Delay(2000);
                            this.processMessages = true;
                        }, this.cancellationTokenSource.Token);

                        AsyncRunner.RunAsyncBackground(this.ChatterJoinLeaveBackground, this.cancellationTokenSource.Token, 60000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        return new Result();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                        return new Result(ex);
                    }
                }));
            }
            return new Result(MixItUp.Base.Resources.TrovoChatConnectionCouldNotBeEstablished);
        }

        public async Task DisconnectUser()
        {
            try
            {
                if (this.userClient != null)
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        this.userClient.OnSentOccurred -= Client_OnSentOccurred;
                        this.userClient.OnTextReceivedOccurred -= UserClient_OnTextReceivedOccurred;
                    }
                    this.userClient.OnDisconnectOccurred -= UserClient_OnDisconnectOccurred;

                    this.userClient.OnChatMessageReceived -= UserClient_OnChatMessageReceived;

                    await this.userClient.Disconnect();
                }

                if (this.cancellationTokenSource != null)
                {
                    this.cancellationTokenSource.Cancel();
                    this.cancellationTokenSource = null;
                }

                this.processMessages = false;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            this.userClient = null;
        }

        public async Task<Result> ConnectBot()
        {
            if (ServiceManager.Get<TrovoSessionService>().IsConnected && ServiceManager.Get<TrovoSessionService>().BotConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.botClient = new ChatClient(ServiceManager.Get<TrovoSessionService>().BotConnection.Connection);

                        string token = await ServiceManager.Get<TrovoSessionService>().BotConnection.GetChatToken(ServiceManager.Get<TrovoSessionService>().ChannelID);
                        if (string.IsNullOrEmpty(token))
                        {
                            return new Result(MixItUp.Base.Resources.TrovoFailedToGetChatToken);
                        }

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.botClient.OnSentOccurred += Client_OnSentOccurred;
                            this.botClient.OnTextReceivedOccurred += BotClient_OnTextReceivedOccurred;
                        }
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;

                        if (!await this.botClient.Connect(token))
                        {
                            return new Result(MixItUp.Base.Resources.TrovoFailedToConnectToChat);
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
            return new Result(MixItUp.Base.Resources.TrovoChatConnectionCouldNotBeEstablished);
        }

        public async Task DisconnectBot()
        {
            try
            {
                if (this.botClient != null)
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        this.botClient.OnSentOccurred -= Client_OnSentOccurred;
                        this.botClient.OnTextReceivedOccurred -= BotClient_OnTextReceivedOccurred;
                    }
                    this.botClient.OnDisconnectOccurred -= BotClient_OnDisconnectOccurred;

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
            try
            {
                await this.messageSemaphore.WaitAsync();

                ChatClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    string subMessage = null;
                    do
                    {
                        message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);
                        if (client == this.botClient)
                        {
                            await client.SendMessage(ServiceManager.Get<TrovoSessionService>().ChannelID, message);
                        }
                        else
                        {
                            await client.SendMessage(message);
                        }
                        message = subMessage;
                        await Task.Delay(500);
                    }
                    while (!string.IsNullOrEmpty(message));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.messageSemaphore.Release();
            }
        }

        public async Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            return await this.GetChatClient(sendAsStreamer: true).DeleteMessage(ServiceManager.Get<TrovoSessionService>().ChannelID, message.ID, message.User?.PlatformID);
        }

        public async Task<bool> ClearChat() { return await this.PerformChatCommand("clear"); }

        public async Task<bool> ModUser(string username) { return await this.PerformChatCommand("mod " + username); }

        public async Task<bool> UnmodUser(string username) { return await this.PerformChatCommand("unmod " + username); }

        public async Task<bool> TimeoutUser(string username, int duration) { return await this.PerformChatCommand($"ban {username} {duration}"); }

        public async Task<bool> BanUser(string username) { return await this.PerformChatCommand("ban " + username); }

        public async Task<bool> UnbanUser(string username) { return await this.PerformChatCommand("unban " + username); }

        public async Task<bool> HostUser(string username) { return await this.PerformChatCommand("host " + username); }

        public async Task<bool> SlowMode(int seconds = 0)
        {
            if (seconds > 0)
            {
                return await this.PerformChatCommand("slow " + seconds);
            }
            else
            {
                return await this.PerformChatCommand("slowoff");
            }
        }

        public async Task<bool> FollowersMode(bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand("followers");
            }
            else
            {
                return await this.PerformChatCommand("followersoff");
            }
        }

        public async Task<bool> SubscriberMode(bool enable)
        {
            if (enable)
            {
                return await this.PerformChatCommand("subscribers");
            }
            else
            {
                return await this.PerformChatCommand("subscribersoff");
            }
        }

        public async Task<bool> AddRole(string username, string role) { return await this.PerformChatCommand($"addrole {role} {username}"); }

        public async Task<bool> RemoveRole(string username, string role) { return await this.PerformChatCommand($"removerole {role} {username}"); }

        public async Task<bool> FastClip() { return await this.PerformChatCommand("fastclip"); }

        public async Task<bool> PerformChatCommand(string command)
        {
            string result = await this.GetChatClient(sendAsStreamer: true).PerformChatCommand(ServiceManager.Get<TrovoSessionService>().ChannelID, command);
            if (!string.IsNullOrEmpty(result))
            {
                await ServiceManager.Get<ChatService>().SendMessage(result, StreamingPlatformTypeEnum.Trovo);
                return false;
            }
            return true;
        }

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private void Client_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Chat Packet Sent: {0}", packet));
        }

        private void UserClient_OnTextReceivedOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Chat Packet Received: {0}", packet));
        }

        private void BotClient_OnTextReceivedOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Bot Chat Packet Received: {0}", packet));
        }

        private async void UserClient_OnChatMessageReceived(object sender, ChatMessageContainerModel messageContainer)
        {
            if (!this.processMessages)
            {
                return;
            }

            foreach (ChatMessageModel message in messageContainer.chats)
            {
                if (this.messagesProcessed.Contains(message.message_id))
                {
                    continue;
                }
                this.messagesProcessed.Add(message.message_id);

                if (message.sender_id == 0 || string.IsNullOrEmpty(message.user_name))
                {
                    continue;
                }

                UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformID: message.sender_id.ToString(), platformUsername: message.user_name, performPlatformSearch: true);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new TrovoUserPlatformV2Model(message));
                }
                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);

                user.GetPlatformData<TrovoUserPlatformV2Model>(StreamingPlatformTypeEnum.Trovo).SetUserProperties(message);

                if (message.type == ChatMessageTypeEnum.StreamOnOff && !string.IsNullOrEmpty(message.content))
                {
                    if (message.content.Equals("stream_on", StringComparison.OrdinalIgnoreCase))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.Trovo));
                    }
                    else if (message.content.Equals("stream_off", StringComparison.OrdinalIgnoreCase))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.Trovo));
                    }
                }
                else if (message.type == ChatMessageTypeEnum.FollowAlert)
                {
                    user.FollowDate = DateTimeOffset.Now;

                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                    if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelFollowed, parameters))
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

                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelFollowed, parameters);

                        EventService.FollowOccurred(user);

                        await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertFollow, user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                    }
                }
                else if (message.type == ChatMessageTypeEnum.SubscriptionAlert)
                {
                    TrovoSubscriptionMessageModel subMessage = new TrovoSubscriptionMessageModel(message);

                    EventTypeEnum subEventType = EventTypeEnum.TrovoChannelSubscribed;
                    if (subMessage.IsResub)
                    {
                        subEventType = EventTypeEnum.TrovoChannelResubscribed;
                    }

                    user.Roles.Add(UserRoleEnum.Subscriber);
                    user.SubscriberTier = subMessage.Tier;
                    if (!subMessage.IsResub)
                    {
                        user.SubscribeDate = DateTimeOffset.Now;
                    }

                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                    parameters.SpecialIdentifiers["message"] = message.content;
                    parameters.SpecialIdentifiers["usersubmonths"] = subMessage.Months.ToString();
                    parameters.SpecialIdentifiers["usersubplan"] = $"{MixItUp.Base.Resources.Tier} {subMessage.Tier}";

                    if (await ServiceManager.Get<EventService>().PerformEvent(subEventType, parameters))
                    {
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = subMessage.Months;

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

                        if (subMessage.IsResub)
                        {
                            EventService.ResubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, user, months: subMessage.Months, tier: subMessage.Tier));
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertResubscribed, user.DisplayName, subMessage.Months), ChannelSession.Settings.AlertSubColor));
                        }
                        else
                        {
                            EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, user, tier: subMessage.Tier));
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscribed, user.DisplayName), ChannelSession.Settings.AlertSubColor));
                        }
                    }
                }
                else if (message.type == ChatMessageTypeEnum.GiftedSubscriptionSentMessage)
                {
                    TrovoSubscriptionMessageModel subMessage = new TrovoSubscriptionMessageModel(message);

                    int totalGifted = 1;
                    int.TryParse(message.content, out totalGifted);

                    this.userSubsGiftedInstanced[user.ID] = totalGifted;

                    if (ChannelSession.Settings.MassGiftedSubsFilterAmount == 0 || totalGifted > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                    {
                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                        parameters.SpecialIdentifiers["subsgiftedamount"] = totalGifted.ToString();
                        parameters.SpecialIdentifiers["usersubplan"] = $"{MixItUp.Base.Resources.Tier} {subMessage.Tier}";
                        parameters.SpecialIdentifiers["isanonymous"] = false.ToString();
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelMassSubscriptionsGifted, parameters);

                        List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
                        for (int i = 0; i < totalGifted; i++)
                        {
                            subscriptions.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, user, tier: subMessage.Tier));
                        }

                        EventService.MassSubscriptionsGiftedOccurred(subscriptions);
                    }
                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertMassSubscriptionsGifted, user.DisplayName, totalGifted), ChannelSession.Settings.AlertMassGiftedSubColor));
                }
                else if (message.type == ChatMessageTypeEnum.GiftedSubscriptionMessage)
                {
                    string[] splits = message.content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    if (splits.Length == 2)
                    {
                        string gifteeUsername = splits[1];
                        UserV2ViewModel giftee = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformUsername: gifteeUsername, performPlatformSearch: true);
                        if (giftee == null)
                        {
                            giftee = await ServiceManager.Get<UserService>().CreateUser(new TrovoUserPlatformV2Model("-1", gifteeUsername, gifteeUsername));
                        }

                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftee.ID;
                        ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                        giftee.Roles.Add(UserRoleEnum.Subscriber);
                        giftee.SubscriberTier = 1;
                        giftee.SubscribeDate = DateTimeOffset.Now;
                        //giftedSubEvent.Receiver.Data.TwitchSubscriberTier = giftedSubEvent.PlanTierNumber;
                        user.TotalSubsGifted++;
                        giftee.TotalSubsReceived++;
                        //giftedSubEvent.Receiver.Data.TotalMonthsSubbed += (uint)giftedSubEvent.MonthsGifted;

                        foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                        {
                            currency.AddAmount(user, currency.OnSubscribeBonus);
                        }

                        foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                        {
                            if (user.MeetsRole(streamPass.UserPermission))
                            {
                                streamPass.AddAmount(user, streamPass.SubscribeBonus);
                            }
                        }

                        this.userSubsGiftedInstanced.TryGetValue(user.ID, out int totalGifted);
                        if (ChannelSession.Settings.MassGiftedSubsFilterAmount == 0 || totalGifted <= ChannelSession.Settings.MassGiftedSubsFilterAmount)
                        {
                            CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                            parameters.Arguments.Add(giftee.Username);
                            parameters.TargetUser = giftee;
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelSubscriptionGifted, parameters);
                        }

                        await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscriptionGifted, user.DisplayName, giftee.DisplayName), ChannelSession.Settings.AlertGiftedSubColor));

                        EventService.SubscriptionGiftedOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, giftee, user));
                    }
                }
                else if (message.type == ChatMessageTypeEnum.WelcomeMessageFromRaid)
                {
                    if (message.content_data != null && message.content_data.TryGetValue("raiderNum", out JToken raiderNum))
                    {
                        int raidCount = raiderNum.ToObject<int>();
                        CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                        parameters.SpecialIdentifiers["raidviewercount"] = raidCount.ToString();

                        if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelRaided, parameters))
                        {
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidUserData] = user.ID;
                            ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidViewerCountData] = raidCount.ToString();

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

                            EventService.RaidOccurred(user, raidCount);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertRaid, user.DisplayName, raidCount), ChannelSession.Settings.AlertRaidColor));
                        }
                    }
                }
                else if (message.type == ChatMessageTypeEnum.Spell || message.type == ChatMessageTypeEnum.CustomSpell)
                {
                    TrovoChatSpellViewModel spell = new TrovoChatSpellViewModel(user, message);
                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo, spell.GetSpecialIdentifiers());

                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelSpellCast, parameters);

                    TrovoSpellCommandModel command = ServiceManager.Get<CommandService>().TrovoSpellCommands.FirstOrDefault(c => string.Equals(c.Name, spell.Name, StringComparison.CurrentCultureIgnoreCase));
                    if (command != null)
                    {
                        await ServiceManager.Get<CommandService>().Queue(command, parameters);
                    }

                    EventService.TrovoSpellCastOccurred(spell);

                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertTrovoSpellFormat, user.DisplayName, spell.Name, spell.ValueTotal, spell.ValueType), ChannelSession.Settings.AlertTrovoSpellCastColor));
                }
                else if (message.type == ChatMessageTypeEnum.ActivityEventMessage)
                {
                    if (message.content_data != null && message.content_data.TryGetValue("activity_topic", out JToken activity_topic))
                    {
                        if (string.Equals(activity_topic.ToString(), TreasureBoxUnleashedActivityTopic, StringComparison.OrdinalIgnoreCase))
                        {
                            // TODO: https://trello.com/c/iwEcqHvG/1199-trovo-treasure-chest-messages-require-formatting
                        }
                    }
                }

                if (TrovoChatMessageViewModel.ApplicableMessageTypes.Contains(message.type) && !string.IsNullOrEmpty(message.content))
                {
                    TrovoChatMessageViewModel chatMessage = new TrovoChatMessageViewModel(message, user);

                    await ServiceManager.Get<ChatService>().AddMessage(chatMessage);

                    if (message.type == ChatMessageTypeEnum.MagicChatBulletScreenChat || message.type == ChatMessageTypeEnum.MagicChatColorfulChat ||
                        message.type == ChatMessageTypeEnum.MagicChatSuperCapChat || message.type == ChatMessageTypeEnum.MagicChatBulletScreenChat)
                    {
                        CommandParametersModel parameters = new CommandParametersModel(chatMessage);

                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelMagicChat, parameters);
                    }
                }
            }
        }

        private async Task ChatterJoinLeaveBackground(CancellationToken cancellationToken)
        {
            ChatViewersModel viewers = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetViewers(ServiceManager.Get<TrovoSessionService>().ChannelID);
            if (viewers != null)
            {
                List<UserV2ViewModel> userJoins = new List<UserV2ViewModel>();
                List<UserV2ViewModel> userLeaves = new List<UserV2ViewModel>();

                HashSet<string> currentViewers = new HashSet<string>();
                foreach (string viewer in viewers.all.viewers)
                {
                    currentViewers.Add(viewer);
                    if (!previousViewers.Contains(viewer))
                    {
                        UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformUsername: viewer, performPlatformSearch: true);
                        if (user != null)
                        {
                            userJoins.Add(user);
                        }
                    }
                }

                await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(userJoins);

                foreach (string viewer in previousViewers)
                {
                    if (!currentViewers.Contains(viewer))
                    {
                        UserV2ViewModel user = await ServiceManager.Get<UserService>().GetUserByPlatform(StreamingPlatformTypeEnum.Trovo, platformUsername: viewer, performPlatformSearch: true);
                        if (user != null)
                        {
                            userLeaves.Add(user);
                        }
                    }
                }

                await ServiceManager.Get<UserService>().RemoveActiveUsers(userLeaves);

                userLeaves.Clear();
                foreach (UserV2ViewModel user in ServiceManager.Get<UserService>().GetActiveUsers(StreamingPlatformTypeEnum.Trovo))
                {
                    if (!currentViewers.Contains(user.Username))
                    {
                        userLeaves.Add(user);
                    }
                }

                await ServiceManager.Get<UserService>().RemoveActiveUsers(userLeaves);

                previousViewers.Clear();
                foreach (string viewer in currentViewers)
                {
                    previousViewers.Add(viewer);
                }
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TrovoUserChat);

            Result result;
            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TrovoUserChat);
        }

        private async void BotClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TrovoBotChat);

            Result result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TrovoBotChat);
        }
    }
}
