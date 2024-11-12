using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Trovo
{
    public class TrovoClient
    {
        public const string TrovoChatConnectionURL = "wss://open-chat.trovo.live/chat";

        private const string TreasureBoxUnleashedActivityTopic = "item_drop_box_unleash";

        private const int MaxMessageLength = 500;

        private TrovoService service;

        private CancellationTokenSource userCancellationTokenSource;
        private CancellationTokenSource botCancellationTokenSource;

        private AdvancedClientWebSocket userWebSocket;
        private AdvancedClientWebSocket botWebSocket;

        private bool processMessages;
        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private HashSet<string> messagesProcessed = new HashSet<string>();
        private Dictionary<Guid, int> userSubsGiftedInstanced = new Dictionary<Guid, int>();

        private readonly Dictionary<string, ChatPacketModel> replyIDListeners = new Dictionary<string, ChatPacketModel>();

        private HashSet<string> previousViewers = new HashSet<string>();

        public bool IsUserConnected { get { return this.userWebSocket != null && this.userWebSocket.IsOpen(); } }
        public bool IsBotConnected { get { return this.botWebSocket != null && this.botWebSocket.IsOpen(); } }

        public TrovoClient(TrovoService service)
        {
            this.service = service;

            this.userWebSocket = new AdvancedClientWebSocket();
            if (ChannelSession.AppSettings.DiagnosticLogging)
            {
                this.userWebSocket.PacketSent += UserWebSocket_PacketSent;
            }
            this.userWebSocket.PacketReceived += UserWebSocket_PacketReceived;
            this.userWebSocket.Disconnected += UserWebSocket_Disconnected;

            this.botWebSocket = new AdvancedClientWebSocket();
            if (ChannelSession.AppSettings.DiagnosticLogging)
            {
                this.botWebSocket.PacketSent += BotWebSocket_PacketSent;
            }
            this.botWebSocket.Disconnected += BotWebSocket_Disconnected;
        }

        public async Task<Result> ConnectUser()
        {
            this.processMessages = false;

            string chatToken = await this.service.GetUserChatToken();
            if (string.IsNullOrEmpty(chatToken))
            {
                return new Result(Resources.TrovoFailedToGetChatToken);
            }

            if (await this.userWebSocket.Connect(TrovoChatConnectionURL))
            {
                ChatPacketModel authReply = await this.SendAndListen(this.userWebSocket, new ChatPacketModel("AUTH", new JObject() { { "token", chatToken } }));
                if (authReply != null && string.IsNullOrEmpty(authReply.error))
                {
                    this.userCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(this.UserBackgroundPing, this.userCancellationTokenSource.Token);

                    AsyncRunner.RunAsyncBackground(this.ChatterJoinLeaveBackground, this.userCancellationTokenSource.Token, 60000);

                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await Task.Delay(2000);
                        this.processMessages = true;
                    }, this.userCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    ChannelSession.ReconnectionOccurred(Resources.TrovoUserChat);

                    return new Result();
                }
            }

            return new Result(success: false);
        }

        public async Task DisconnectUser()
        {
            if (this.userCancellationTokenSource != null)
            {
                this.userCancellationTokenSource.Cancel();
                this.userCancellationTokenSource = null;
            }

            this.processMessages = false;

            await this.userWebSocket.Disconnect();
        }

        public async Task<Result> ConnectBot()
        {
            string chatToken = await this.service.GetBotChatToken();
            if (string.IsNullOrEmpty(chatToken))
            {
                return new Result(Resources.TrovoFailedToGetChatToken);
            }

            if (await this.botWebSocket.Connect(TrovoChatConnectionURL))
            {
                ChatPacketModel authReply = await this.SendAndListen(this.botWebSocket, new ChatPacketModel("AUTH", new JObject() { { "token", chatToken } }));
                if (authReply != null && string.IsNullOrEmpty(authReply.error))
                {
                    this.botCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(this.BotBackgroundPing, this.userCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    ChannelSession.ReconnectionOccurred(Resources.TrovoBotChat);

                    return new Result();
                }
            }

            return new Result(success: false);
        }

        public async Task DisconnectBot()
        {
            if (this.botCancellationTokenSource != null)
            {
                this.botCancellationTokenSource.Cancel();
                this.botCancellationTokenSource = null;
            }

            await this.botWebSocket.Disconnect();
        }

        public async Task SendMessage(string message, bool sendAsStreamer = false)
        {
            try
            {
                await this.messageSemaphore.WaitAsync();

                string subMessage = null;
                do
                {
                    message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);

                    if (sendAsStreamer || !this.IsBotConnected)
                    {
                        await this.service.SendUserMessage(message);
                    }
                    else
                    {
                        await this.service.SendBotMessage(message);
                    }

                    message = subMessage;
                    await Task.Delay(500);
                }
                while (!string.IsNullOrEmpty(message));
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
            return await this.service.DeleteMessage(this.service.ChannelID, message.ID, message.User?.PlatformID);
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
            string result = await this.service.PerformChatCommand(this.service.ChannelID, command);
            if (!string.IsNullOrEmpty(result))
            {
                await ServiceManager.Get<ChatService>().SendMessage(result, StreamingPlatformTypeEnum.Trovo);
                return false;
            }
            return true;
        }

        public async Task<ChatPacketModel> Ping(AdvancedClientWebSocket webSocket)
        {
            return await this.SendAndListen(webSocket, new ChatPacketModel("PING"));
        }

        private async Task UserBackgroundPing(CancellationToken token) { await this.BackgroundPing(this.userWebSocket, token); }

        private async Task BotBackgroundPing(CancellationToken token) { await this.BackgroundPing(this.botWebSocket, token); }

        private async Task BackgroundPing(AdvancedClientWebSocket webSocket, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int delay = 30;
                try
                {
                    ChatPacketModel reply = await this.Ping(webSocket);
                    if (reply != null && reply.data != null && reply.data.ContainsKey("gap"))
                    {
                        int.TryParse(reply.data["gap"].ToString(), out delay);
                    }
                    await Task.Delay(delay * 1000);
                }
                catch (ThreadAbortException) { return; }
                catch (OperationCanceledException) { return; }
                catch (Exception ex) { Logger.Log(ex); }
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

        private async void UserWebSocket_PacketReceived(object sender, string packet)
        {
            if (ChannelSession.IsDebug())
            {
                Logger.Log(LogLevel.Debug, "Trovo Chat Packet Received: " + packet);
            }

            ChatPacketModel response = JSONSerializerHelper.DeserializeFromString<ChatPacketModel>(packet);
            if (response != null && !string.IsNullOrEmpty(response.type))
            {
                if (string.Equals(response.type, "RESPONSE"))
                {
                    if (this.replyIDListeners.ContainsKey(response.nonce))
                    {
                        this.replyIDListeners[response.nonce] = response;
                    }
                }
                else if (string.Equals(response.type, "CHAT"))
                {
                    if (!this.processMessages)
                    {
                        return;
                    }

                    ChatMessageContainerModel messageContainer = response.data.ToObject<ChatMessageContainerModel>();
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
            }
        }

        private void UserWebSocket_PacketSent(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Chat Packet Sent: {0}", packet));
        }

        private void UserWebSocket_Disconnected(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred(Resources.TrovoUserChat);
        }

        private void BotWebSocket_PacketSent(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Bot Chat Packet Sent: {0}", packet));
        }

        private void BotWebSocket_Disconnected(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred(Resources.TrovoBotChat);
        }

        protected async Task<ChatPacketModel> SendAndListen(AdvancedClientWebSocket webSocket, ChatPacketModel packet)
        {
            ChatPacketModel replyPacket = null;
            this.replyIDListeners[packet.nonce] = null;

            await webSocket.Send(JSONSerializerHelper.SerializeToString(packet));

            await AsyncRunner.WaitForSuccess(() =>
            {
                if (this.replyIDListeners.ContainsKey(packet.nonce) && this.replyIDListeners[packet.nonce] != null)
                {
                    replyPacket = this.replyIDListeners[packet.nonce];
                    return true;
                }
                return false;
            }, secondsToWait: 5);

            this.replyIDListeners.Remove(packet.nonce);
            return replyPacket;
        }
    }
}
