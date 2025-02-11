using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
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

namespace MixItUp.Base.Services.Trovo.New
{
    public class TrovoClient : ServiceClientBase
    {
        public const string TrovoChatConnectionURL = "wss://open-chat.trovo.live/chat";

        private const string TreasureBoxUnleashedActivityTopic = "item_drop_box_unleash";

        public string ChatToken { get; set; }

        private CancellationTokenSource cancellationTokenSource;

        private AdvancedClientWebSocket webSocket;

        private bool processMessages;

        private HashSet<string> messagesProcessed = new HashSet<string>();
        private Dictionary<Guid, int> userSubsGiftedInstanced = new Dictionary<Guid, int>();

        private readonly Dictionary<string, ChatPacketModel> replyIDListeners = new Dictionary<string, ChatPacketModel>();

        private HashSet<string> previousViewers = new HashSet<string>();

        public override bool IsConnected { get { return webSocket != null && webSocket.IsOpen(); } }

        public TrovoClient()
        {
            this.webSocket = new AdvancedClientWebSocket();

            this.webSocket.PacketSent += WebSocket_PacketSent;
            this.webSocket.PacketReceived += UserWebSocket_PacketReceived;
            this.webSocket.Disconnected += WebSocket_Disconnected;
        }

        public override async Task<Result> Connect()
        {
            processMessages = false;

            if (string.IsNullOrWhiteSpace(this.ChatToken))
            {
                return new Result("No chat token set for Trovo client");
            }

            if (await this.webSocket.Connect(TrovoChatConnectionURL, CancellationToken.None))
            {
                ChatPacketModel authReply = await SendAndListen(new ChatPacketModel("AUTH", new JObject() { { "token", this.ChatToken } }));
                if (authReply != null && string.IsNullOrEmpty(authReply.error))
                {
                    cancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(BackgroundPing, cancellationTokenSource.Token);

                    AsyncRunner.RunAsyncBackground(ChatterJoinLeaveBackground, cancellationTokenSource.Token, 60000);

                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await Task.Delay(2000);
                        processMessages = true;
                    }, cancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    ChannelSession.ReconnectionOccurred(Resources.TrovoUserChat);

                    return new Result();
                }
            }

            return new Result(success: false);
        }

        public override async Task Disconnect()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource = null;
            }

            processMessages = false;

            await this.webSocket.Disconnect();
        }

        public async Task<ChatPacketModel> Ping()
        {
            return await SendAndListen(new ChatPacketModel("PING"));
        }

        private async Task<ChatPacketModel> SendAndListen(ChatPacketModel packet)
        {
            ChatPacketModel replyPacket = null;
            this.replyIDListeners[packet.nonce] = null;

            await this.webSocket.Send(JSONSerializerHelper.SerializeToString(packet));

            await AsyncRunner.WaitForSuccess(() =>
            {
                if (replyIDListeners.ContainsKey(packet.nonce) && replyIDListeners[packet.nonce] != null)
                {
                    replyPacket = replyIDListeners[packet.nonce];
                    return true;
                }
                return false;
            }, secondsToWait: 5);

            replyIDListeners.Remove(packet.nonce);
            return replyPacket;
        }

        private async Task BackgroundPing(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int delay = 30;
                try
                {
                    ChatPacketModel reply = await this.Ping();
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
            ChatViewersModel viewers = await ServiceManager.Get<TrovoSession>().StreamerService.GetViewers(ServiceManager.Get<TrovoSession>().ChannelID);
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
            Logger.Log(LogLevel.Debug, "Trovo Chat Packet Received: " + packet);

            ChatPacketModel response = JSONSerializerHelper.DeserializeFromString<ChatPacketModel>(packet);
            if (response != null && !string.IsNullOrEmpty(response.type))
            {
                if (string.Equals(response.type, "RESPONSE"))
                {
                    if (replyIDListeners.ContainsKey(response.nonce))
                    {
                        replyIDListeners[response.nonce] = response;
                    }
                }
                else if (string.Equals(response.type, "CHAT"))
                {
                    if (!processMessages)
                    {
                        return;
                    }

                    ChatMessageContainerModel messageContainer = response.data.ToObject<ChatMessageContainerModel>();
                    foreach (ChatMessageModel message in messageContainer.chats)
                    {
                        if (messagesProcessed.Contains(message.message_id))
                        {
                            continue;
                        }
                        messagesProcessed.Add(message.message_id);

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

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertFollow, user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                            }
                        }
                        else if (message.type == ChatMessageTypeEnum.SubscriptionAlert)
                        {
                            MixItUp.Base.Model.Trovo.Subscriptions.TrovoSubscriptionMessageModel subMessage = new MixItUp.Base.Model.Trovo.Subscriptions.TrovoSubscriptionMessageModel(message);

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
                            parameters.SpecialIdentifiers["usersubplan"] = $"{Resources.Tier} {subMessage.Tier}";

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
                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertResubscribed, user.DisplayName, subMessage.Months), ChannelSession.Settings.AlertSubColor));
                                }
                                else
                                {
                                    EventService.SubscribeOccurred(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, user, tier: subMessage.Tier));
                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertSubscribed, user.DisplayName), ChannelSession.Settings.AlertSubColor));
                                }
                            }
                        }
                        else if (message.type == ChatMessageTypeEnum.GiftedSubscriptionSentMessage)
                        {
                            MixItUp.Base.Model.Trovo.Subscriptions.TrovoSubscriptionMessageModel subMessage = new MixItUp.Base.Model.Trovo.Subscriptions.TrovoSubscriptionMessageModel(message);

                            int totalGifted = 1;
                            int.TryParse(message.content, out totalGifted);

                            userSubsGiftedInstanced[user.ID] = totalGifted;

                            if (ChannelSession.Settings.MassGiftedSubsFilterAmount == 0 || totalGifted > ChannelSession.Settings.MassGiftedSubsFilterAmount)
                            {
                                CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                                parameters.SpecialIdentifiers["subsgiftedamount"] = totalGifted.ToString();
                                parameters.SpecialIdentifiers["usersubplan"] = $"{Resources.Tier} {subMessage.Tier}";
                                parameters.SpecialIdentifiers["isanonymous"] = false.ToString();
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelMassSubscriptionsGifted, parameters);

                                List<SubscriptionDetailsModel> subscriptions = new List<SubscriptionDetailsModel>();
                                for (int i = 0; i < totalGifted; i++)
                                {
                                    subscriptions.Add(new SubscriptionDetailsModel(StreamingPlatformTypeEnum.Trovo, user, tier: subMessage.Tier));
                                }

                                EventService.MassSubscriptionsGiftedOccurred(subscriptions);
                            }
                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertMassSubscriptionsGifted, user.DisplayName, totalGifted), ChannelSession.Settings.AlertMassGiftedSubColor));
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

                                userSubsGiftedInstanced.TryGetValue(user.ID, out int totalGifted);
                                if (ChannelSession.Settings.MassGiftedSubsFilterAmount == 0 || totalGifted <= ChannelSession.Settings.MassGiftedSubsFilterAmount)
                                {
                                    CommandParametersModel parameters = new CommandParametersModel(user, StreamingPlatformTypeEnum.Trovo);
                                    parameters.Arguments.Add(giftee.Username);
                                    parameters.TargetUser = giftee;
                                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelSubscriptionGifted, parameters);
                                }

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertSubscriptionGifted, user.DisplayName, giftee.DisplayName), ChannelSession.Settings.AlertGiftedSubColor));

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

                                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertRaid, user.DisplayName, raidCount), ChannelSession.Settings.AlertRaidColor));
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

                            await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(Resources.AlertTrovoSpellFormat, user.DisplayName, spell.Name, spell.ValueTotal, spell.ValueType), ChannelSession.Settings.AlertTrovoSpellCastColor));
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

        private void WebSocket_PacketSent(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Trovo Chat Packet Sent: {0}", packet));
        }

        private void WebSocket_Disconnected(object sender, WebSocketCloseStatus closeStatus)
        {
            ChannelSession.DisconnectionOccurred(Resources.TrovoUserChat);

            Task.Run(this.Reconnect);
        }
    }
}
