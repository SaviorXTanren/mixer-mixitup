using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Trovo.Base.Clients;
using Trovo.Base.Models.Chat;
using Trovo.Base.Models.Users;

namespace MixItUp.Base.Services.Trovo
{
    public class TrovoChatEventService : StreamingPlatformServiceBase
    {
        private const string RaidMessageRegexFormat = " is carrying \\d+ raiders to this channel.";
        private const string OnlyDigitsRegexReplacementFormat = "[^0-9]";

        private Dictionary<string, ChatEmoteModel> channelEmotes = new Dictionary<string, ChatEmoteModel>();
        private Dictionary<string, EventChatEmoteModel> eventEmotes = new Dictionary<string, EventChatEmoteModel>();
        private Dictionary<string, GlobalEmoteChatModel> globalEmotes = new Dictionary<string, GlobalEmoteChatModel>();

        private ChatClient userClient;
        private ChatClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private const int MaxMessageLength = 250;

        public TrovoChatEventService() { }

        public override string Name { get { return "Trovo Chat"; } }

        public IDictionary<string, ChatEmoteModel> ChannelEmotes { get { return this.channelEmotes; } }
        public IDictionary<string, EventChatEmoteModel> EventEmotes { get { return this.eventEmotes; } }
        public IDictionary<string, GlobalEmoteChatModel> GlobalEmotes { get { return this.globalEmotes; } }

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
                        this.userClient = new ChatClient(ServiceManager.Get<TrovoSessionService>().UserConnection.Connection);

                        string token = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetChatToken();
                        if (string.IsNullOrEmpty(token))
                        {
                            return new Result("Failed to get chat token from Trovo chat servers");
                        }

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred += Client_OnSentOccurred;
                            this.userClient.OnTextReceivedOccurred += UserClient_OnTextReceivedOccurred;
                        }
                        this.userClient.OnDisconnectOccurred += UserClient_OnDisconnectOccurred;

                        this.userClient.OnChatMessageReceived += UserClient_OnChatMessageReceived;

                        if (!await this.userClient.Connect(token))
                        {
                            return new Result("Failed to connect to Trovo chat servers");
                        }

                        ChatEmotePackageModel emotePackage = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetEmotes(ServiceManager.Get<TrovoSessionService>().Channel.channel_id);
                        if (emotePackage != null)
                        {
                            return new Result("Failed to get available Trovo emotes");
                        }

                        foreach (ChannelChatEmotesModel channel in emotePackage.customizedEmotes.channel)
                        {
                            foreach (ChatEmoteModel emote in channel.emotes)
                            {
                                this.ChannelEmotes[emote.name] = emote;
                            }
                        }

                        foreach (EventChatEmoteModel emote in emotePackage.eventEmotes)
                        {
                            this.EventEmotes[emote.name] = emote;
                        }

                        foreach (GlobalEmoteChatModel emote in emotePackage.globalEmotes)
                        {
                            this.GlobalEmotes[emote.name] = emote;
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
            return new Result("Trovo chat connection has not been established");
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

                        string token = await ServiceManager.Get<TrovoSessionService>().BotConnection.GetChatToken(ServiceManager.Get<TrovoSessionService>().Channel.channel_id);
                        if (string.IsNullOrEmpty(token))
                        {
                            return new Result("Failed to get chat token from Trovo chat servers");
                        }

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.botClient.OnSentOccurred += Client_OnSentOccurred;
                        }
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;

                        if (!await this.botClient.Connect(token))
                        {
                            return new Result("Failed to connect to Trovo chat servers");
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
            return new Result("Trovo chat connection has not been established");
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

        public async Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            return await this.GetChatClient(sendAsStreamer: true).DeleteMessage(ServiceManager.Get<TrovoSessionService>().Channel.channel_id, message.ID, message.User?.TrovoID);
        }

        public async Task<bool> ClearChat() { return await this.PerformChatCommand("clear"); }

        public async Task<bool> ModUser(UserViewModel user) { return await this.PerformChatCommand("mod @" + user.TrovoUsername); }

        public async Task<bool> UnmodUser(UserViewModel user) { return await this.PerformChatCommand("unmod @" + user.TrovoUsername); }

        public async Task<bool> TimeoutUser(UserViewModel user, int duration) { return await this.PerformChatCommand($"ban @{user.TrovoUsername} {duration}"); }

        public async Task<bool> BanUser(UserViewModel user) { return await this.PerformChatCommand("ban @" + user.TrovoUsername); }

        public async Task<bool> UnbanUser(UserViewModel user) { return await this.PerformChatCommand("unban @" + user.TrovoUsername); }

        public async Task<bool> PerformChatCommand(string command)
        {
            string result = await this.GetChatClient(sendAsStreamer: true).PerformChatCommand(ServiceManager.Get<TrovoSessionService>().Channel.channel_id, command);
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

        private async void UserClient_OnChatMessageReceived(object sender, ChatMessageContainerModel messageContainer)
        {
            if (messageContainer != null && messageContainer.chats != null && messageContainer.chats.Count > 0)
            {
                UserViewModel user = ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Trovo, messageContainer.chats.First().sender_id);
                if (user == null)
                {
                    UserModel trovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(messageContainer.chats.First().nick_name);
                    if (trovoUser != null)
                    {
                        user = await ServiceManager.Get<UserService>().AddOrUpdateUser(trovoUser);
                    }
                }

                user.SetTrovoChatDetails(messageContainer);

                TrovoChatMessageViewModel message = new TrovoChatMessageViewModel(messageContainer, user);

                await ServiceManager.Get<ChatService>().AddMessage(message);

                foreach (ChatMessageModel token in messageContainer.chats)
                {
                    if (!string.IsNullOrEmpty(token.content))
                    {
                        if (token.type == ChatMessageTypeEnum.FollowAlert)
                        {
                            EventTrigger trigger = new EventTrigger(EventTypeEnum.TrovoChannelFollowed, user);
                            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
                            {
                                user.FollowDate = DateTimeOffset.Now;

                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestFollowerUserData] = user.ID;

                                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                                {
                                    currency.AddAmount(user.Data, currency.OnFollowBonus);
                                }

                                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                                {
                                    if (user.HasPermissionsTo(streamPass.Permission))
                                    {
                                        streamPass.AddAmount(user.Data, streamPass.FollowBonus);
                                    }
                                }

                                await ServiceManager.Get<EventService>().PerformEvent(trigger);

                                GlobalEvents.FollowOccurred(user);

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Trovo, user, string.Format("{0} Followed", user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                            }
                        }
                        else if (token.type == ChatMessageTypeEnum.SubscriptionAlert)
                        {
                            EventTrigger trigger = new EventTrigger(EventTypeEnum.TrovoChannelSubscribed, user);
                            if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
                            {
                                trigger.SpecialIdentifiers["message"] = message.PlainTextMessage;
                                //trigger.SpecialIdentifiers["usersubmonths"] = months.ToString();
                                //trigger.SpecialIdentifiers["usersubplanname"] = !string.IsNullOrEmpty(packet.sub_plan_name) ? packet.sub_plan_name : TwitchEventService.GetSubTierNameFromText(packet.sub_plan);
                                //trigger.SpecialIdentifiers["usersubplan"] = planTier;
                                //trigger.SpecialIdentifiers["usersubstreak"] = packet.streak_months.ToString();

                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                                //ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = months;

                                user.SubscribeDate = DateTimeOffset.Now;
                                //user.Data.TwitchSubscriberTier = TwitchEventService.GetSubTierNumberFromText(packet.sub_plan);
                                //user.Data.TotalMonthsSubbed++;

                                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                                {
                                    currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                                }

                                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                                {
                                    if (trigger.User.HasPermissionsTo(streamPass.Permission))
                                    {
                                        streamPass.AddAmount(user.Data, streamPass.SubscribeBonus);
                                    }
                                }

                                if (string.IsNullOrEmpty(await ServiceManager.Get<ModerationService>().ShouldTextBeModerated(user, trigger.SpecialIdentifiers["message"])))
                                {
                                    await ServiceManager.Get<EventService>().PerformEvent(trigger);
                                }
                            }

                            GlobalEvents.ResubscribeOccurred(new Tuple<UserViewModel, int>(user, 1));
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Trovo, user, string.Format("{0} Subscribed", user.DisplayName), ChannelSession.Settings.AlertSubColor));
                        }
                        else if (token.type == ChatMessageTypeEnum.GiftedSubscriptionSentMessage)
                        {
                            int totalGifted = 1;
                            int.TryParse(token.content, out totalGifted);

                            EventTrigger trigger = new EventTrigger(EventTypeEnum.TrovoChannelMassSubscriptionsGifted, user);
                            trigger.SpecialIdentifiers["subsgiftedamount"] = totalGifted.ToString();
                            //trigger.SpecialIdentifiers["subsgiftedlifetimeamount"] = massGiftedSubEvent.LifetimeGifted.ToString();
                            //trigger.SpecialIdentifiers["usersubplan"] = massGiftedSubEvent.PlanTier;
                            //trigger.SpecialIdentifiers["isanonymous"] = massGiftedSubEvent.IsAnonymous.ToString();
                            await ServiceManager.Get<EventService>().PerformEvent(trigger);

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Trovo, user, string.Format("{0} Gifted {1} Subs", user.DisplayName, totalGifted), ChannelSession.Settings.AlertMassGiftedSubColor));

                        }
                        else if (token.type == ChatMessageTypeEnum.GiftedSubscriptionMessage)
                        {
                            string[] splits = token.content.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length == 2)
                            {
                                string gifteeUsername = splits[1];
                                UserViewModel giftee = ServiceManager.Get<UserService>().GetUserByUsername(gifteeUsername, StreamingPlatformTypeEnum.Trovo);
                                if (giftee == null)
                                {
                                    UserModel gifteeTrovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(gifteeUsername);
                                    if (giftee == null)
                                    {
                                        giftee = user;
                                    }
                                    else
                                    {
                                        giftee = new UserViewModel(gifteeTrovoUser);
                                    }
                                }

                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftee.ID;
                                //ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = giftedSubEvent.MonthsGifted;

                                giftee.SubscribeDate = DateTimeOffset.Now;
                                //giftedSubEvent.Receiver.Data.TwitchSubscriberTier = giftedSubEvent.PlanTierNumber;
                                user.Data.TotalSubsGifted++;
                                giftee.Data.TotalSubsReceived++;
                                //giftedSubEvent.Receiver.Data.TotalMonthsSubbed += (uint)giftedSubEvent.MonthsGifted;

                                foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values)
                                {
                                    currency.AddAmount(user.Data, currency.OnSubscribeBonus);
                                }

                                foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                                {
                                    if (user.HasPermissionsTo(streamPass.Permission))
                                    {
                                        streamPass.AddAmount(user.Data, streamPass.SubscribeBonus);
                                    }
                                }

                                // TODO : Add same logic that Twitch uses for determine which event command to fire for gifted subs
                                EventTrigger trigger = new EventTrigger(EventTypeEnum.TrovoChannelSubscriptionGifted, user);
                                trigger.Arguments.Add(giftee.Username);
                                trigger.TargetUser = giftee;
                                await ServiceManager.Get<EventService>().PerformEvent(trigger);

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Trovo, user, string.Format("{0} Gifted A Subscription To {1}", user.DisplayName, giftee.DisplayName), ChannelSession.Settings.AlertGiftedSubColor));

                                GlobalEvents.SubscriptionGiftedOccurred(user, giftee);
                            }
                        }
                        else if (token.type == ChatMessageTypeEnum.WelcomeMessageFromRaid)
                        {
                            Match match = Regex.Match(token.content, RaidMessageRegexFormat);
                            if (match.Success)
                            {
                                int raidCount = 0;
                                int.TryParse(Regex.Replace(match.Value, OnlyDigitsRegexReplacementFormat, string.Empty), out raidCount);

                                EventTrigger trigger = new EventTrigger(EventTypeEnum.TrovoChannelRaided, user);
                                if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
                                {
                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidUserData] = user.ID;
                                    ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestRaidViewerCountData] = raidCount.ToString();

                                    foreach (CurrencyModel currency in ChannelSession.Settings.Currency.Values.ToList())
                                    {
                                        currency.AddAmount(user.Data, currency.OnHostBonus);
                                    }

                                    foreach (StreamPassModel streamPass in ChannelSession.Settings.StreamPass.Values)
                                    {
                                        if (user.HasPermissionsTo(streamPass.Permission))
                                        {
                                            streamPass.AddAmount(user.Data, streamPass.HostBonus);
                                        }
                                    }

                                    GlobalEvents.RaidOccurred(user, raidCount);

                                    trigger.SpecialIdentifiers["raidviewercount"] = raidCount.ToString();
                                    await ServiceManager.Get<EventService>().PerformEvent(trigger);

                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Trovo, user, string.Format("{0} raided with {1} viewers", user.DisplayName, raidCount), ChannelSession.Settings.AlertRaidColor));
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Trovo User Chat");

            Result result;
            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Trovo User Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Trovo Bot Chat");

            Result result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Trovo Bot Chat");
        }
    }
}
