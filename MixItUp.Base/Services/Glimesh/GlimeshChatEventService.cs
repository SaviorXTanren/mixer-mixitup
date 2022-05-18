using Glimesh.Base.Clients;
using Glimesh.Base.Models.Clients.Channel;
using Glimesh.Base.Models.Clients.Chat;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Glimesh;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Glimesh
{
    public class GlimeshChatEventService : StreamingPlatformServiceBase
    {
        private const string SubscriptionMessageContents = " just subscribed!";
        private const string GiftedSubscriptionMessageContentsFormat = " just gifted a subscription to \\w+!";
        private const string DonationMessageContentsFormat = " just donated \\$[\\d.]+!";

        private ChatEventClient userClient;
        private ChatEventClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private const int MaxMessageLength = 250;

        public GlimeshChatEventService() { }

        public override string Name { get { return MixItUp.Base.Resources.GlimeshChat; } }

        public bool IsUserConnected { get { return this.userClient != null && this.userClient.IsOpen(); } }
        public bool IsBotConnected { get { return this.botClient != null && this.botClient.IsOpen(); } }

        public async Task<Result> ConnectUser()
        {
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.userClient = await ChatEventClient.CreateWithToken(ServiceManager.Get<GlimeshSessionService>().UserConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred += Client_OnSentOccurred;
                            this.userClient.OnTextReceivedOccurred += UserClient_OnTextReceivedOccurred;
                        }
                        this.userClient.OnDisconnectOccurred += UserClient_OnDisconnectOccurred;

                        this.userClient.OnChatMessageReceived += UserClient_OnChatMessageReceived;

                        this.userClient.OnChannelUpdated += UserClient_OnChannelUpdated;
                        this.userClient.OnFollowOccurred += UserClient_OnFollowOccurred;

                        if (!await this.userClient.Connect())
                        {
                            return new Result(MixItUp.Base.Resources.GlimeshFailedToConnectToChat);
                        }

                        if (!await this.userClient.JoinChannelChat(ServiceManager.Get<GlimeshSessionService>().ChannelID))
                        {
                            return new Result(MixItUp.Base.Resources.GlimeshFailedToJoinChat);
                        }

                        if (!await this.userClient.JoinChannelEvents(ServiceManager.Get<GlimeshSessionService>().ChannelID, ServiceManager.Get<GlimeshSessionService>().UserID))
                        {
                            return new Result(MixItUp.Base.Resources.GlimeshFailedToJoinEvents);
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
            return new Result(MixItUp.Base.Resources.GlimeshChatConnectionCouldNotBeEstablished);
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

                    this.userClient.OnChannelUpdated -= UserClient_OnChannelUpdated;
                    this.userClient.OnFollowOccurred -= UserClient_OnFollowOccurred;

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
            if (ServiceManager.Get<GlimeshSessionService>().IsConnected && ServiceManager.Get<GlimeshSessionService>().BotConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.botClient = await ChatEventClient.CreateWithToken(ServiceManager.Get<GlimeshSessionService>().BotConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.botClient.OnSentOccurred += Client_OnSentOccurred;
                        }
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;

                        if (!await this.botClient.Connect())
                        {
                            return new Result(MixItUp.Base.Resources.GlimeshFailedToConnectToChat);
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
            return new Result(MixItUp.Base.Resources.GlimeshChatConnectionCouldNotBeEstablished);
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
                ChatEventClient client = this.GetChatClient(sendAsStreamer);
                if (client != null)
                {
                    string subMessage = null;
                    do
                    {
                        message = ChatService.SplitLargeMessage(message, MaxMessageLength, out subMessage);
                        await client.SendMessage(ServiceManager.Get<GlimeshSessionService>().ChannelID, message);
                        message = subMessage;
                        await Task.Delay(500);
                    }
                    while (!string.IsNullOrEmpty(message));
                }
            });
        }

        public async Task ShortTimeoutUser(UserV2ViewModel user)
        {
            await this.GetChatClient(sendAsStreamer: true).ShortTimeoutUser(ServiceManager.Get<GlimeshSessionService>().ChannelID, user.PlatformID);
        }

        public async Task LongTimeoutUser(UserV2ViewModel user)
        {
            await this.GetChatClient(sendAsStreamer: true).LongTimeoutUser(ServiceManager.Get<GlimeshSessionService>().ChannelID, user.PlatformID);
        }

        public async Task BanUser(UserV2ViewModel user)
        {
            await this.GetChatClient(sendAsStreamer: true).BanUser(ServiceManager.Get<GlimeshSessionService>().ChannelID, user.PlatformID);
        }

        public async Task UnbanUser(UserV2ViewModel user)
        {
            await this.GetChatClient(sendAsStreamer: true).UnbanUser(ServiceManager.Get<GlimeshSessionService>().ChannelID, user.PlatformID);
        }

        public async Task<bool> DeleteMessage(ChatMessageViewModel message)
        {
            return await this.GetChatClient(sendAsStreamer: true).DeleteMessage(ServiceManager.Get<GlimeshSessionService>().ChannelID, message.ID);
        }

        private ChatEventClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private void Client_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Glimesh Chat Packet Sent: {0}", packet));
        }

        private void UserClient_OnTextReceivedOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Glimesh Chat Packet Received: {0}", packet));
        }

        private async void UserClient_OnChatMessageReceived(object sender, ChatMessagePacketModel messagePacket)
        {
            try
            {
                if (messagePacket != null && !string.IsNullOrEmpty(messagePacket.Message))
                {
                    UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, messagePacket.User?.id);
                    if (user == null)
                    {
                        UserModel glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByID(messagePacket.User?.id);
                        if (glimeshUser != null)
                        {
                            user = await ServiceManager.Get<UserService>().CreateUser(new GlimeshUserPlatformV2Model(glimeshUser));
                        }
                        else
                        {
                            user = await ServiceManager.Get<UserService>().CreateUser(new GlimeshUserPlatformV2Model(messagePacket));
                        }
                        await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);
                    }

                    user.GetPlatformData<GlimeshUserPlatformV2Model>(StreamingPlatformTypeEnum.Glimesh).SetUserProperties(messagePacket);

                    GlimeshChatMessageViewModel message = new GlimeshChatMessageViewModel(messagePacket, user);

                    if (messagePacket.IsFollowedMessage)
                    {
                        // Ignore follow messages
                    }
                    else if (messagePacket.IsSubscriptionMessage)
                    {
                        if (messagePacket.Message.Contains(SubscriptionMessageContents, StringComparison.OrdinalIgnoreCase))
                        {
                            user.Roles.Add(UserRoleEnum.Subscriber);
                            user.SubscribeDate = DateTimeOffset.Now;

                            CommandParametersModel parameters = new CommandParametersModel(user);
                            if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.GlimeshChannelSubscribed, parameters))
                            {
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = user.ID;
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

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

                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.GlimeshChannelSubscribed, parameters);

                                GlobalEvents.SubscribeOccurred(user);

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscribed, user.DisplayName), ChannelSession.Settings.AlertSubColor));
                            }
                        }
                        else if (Regex.IsMatch(messagePacket.Message, GiftedSubscriptionMessageContentsFormat, RegexOptions.IgnoreCase))
                        {
                            Match match = Regex.Match(messagePacket.Message, GiftedSubscriptionMessageContentsFormat, RegexOptions.IgnoreCase);
                            string[] splits = match.Value.Split(new char[] { ' ', '!' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length > 0)
                            {
                                string gifteeName = splits.Last();
                                UserV2ViewModel giftee = await ServiceManager.Get<UserService>().GetUserByPlatformUsername(StreamingPlatformTypeEnum.Glimesh, gifteeName);
                                if (giftee == null)
                                {
                                    UserModel gifteeGlimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByName(gifteeName);
                                    if (giftee != null)
                                    {
                                        giftee = await ServiceManager.Get<UserService>().CreateUser(new GlimeshUserPlatformV2Model(gifteeGlimeshUser));
                                    }
                                }

                                if (giftee == null)
                                {
                                    Logger.Log(LogLevel.Error, "Glimesh Gifted Subscription processing failed: " + message.ToString());
                                    return;
                                }

                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberUserData] = giftee.ID;
                                ChannelSession.Settings.LatestSpecialIdentifiersData[SpecialIdentifierStringBuilder.LatestSubscriberSubMonthsData] = 1;

                                giftee.Roles.Add(UserRoleEnum.Subscriber);
                                giftee.SubscriberTier = 1;
                                giftee.SubscribeDate = DateTimeOffset.Now;
                                user.TotalSubsGifted++;
                                giftee.TotalSubsReceived++;

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

                                CommandParametersModel parameters = new CommandParametersModel(user);
                                parameters.Arguments.Add(giftee.Username);
                                parameters.TargetUser = giftee;
                                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TrovoChannelSubscriptionGifted, parameters);

                                await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertSubscriptionGifted, user.DisplayName, giftee.DisplayName), ChannelSession.Settings.AlertGiftedSubColor));

                                GlobalEvents.SubscriptionGiftedOccurred(user, giftee);
                            }
                        }
                        else if (Regex.IsMatch(messagePacket.Message, DonationMessageContentsFormat, RegexOptions.IgnoreCase))
                        {
                            Match match = Regex.Match(messagePacket.Message, GiftedSubscriptionMessageContentsFormat, RegexOptions.IgnoreCase);
                            string[] splits = match.Value.Split(new char[] { ' ', '!' }, StringSplitOptions.RemoveEmptyEntries);
                            if (splits.Length > 0)
                            {
                                string amountText = string.Concat(splits.Last().Where(c => char.IsDigit(c) || c == '.'));
                                if (double.TryParse(amountText, out double amount))
                                {
                                    await EventService.ProcessDonationEvent(EventTypeEnum.GlimeshChannelDonation, new UserDonationModel()
                                    {
                                        Source = UserDonationSourceEnum.Glimesh,

                                        ID = message.ID,

                                        User = user,
                                        Username = user.Username,
                                        Message = message.PlainTextMessage,

                                        Amount = Math.Round(amount, 2),

                                        DateTime = DateTimeOffset.Now,
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().AddMessage(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex + " - " + JSONSerializerHelper.SerializeToString(messagePacket));
            }
        }

        private async void UserClient_OnChannelUpdated(object sender, ChannelUpdatePacketModel channel)
        {
            try
            {
                if (!ServiceManager.Get<GlimeshSessionService>().User.channel.IsLive && channel.Channel.IsLive)
                {
                    CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Glimesh);
                    if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.GlimeshChannelStreamStart, parameters))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.GlimeshChannelStreamStart, parameters);
                    }
                }
                else if (ServiceManager.Get<GlimeshSessionService>().User.channel.IsLive && !channel.Channel.IsLive)
                {
                    CommandParametersModel parameters = new CommandParametersModel(StreamingPlatformTypeEnum.Glimesh);
                    if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.GlimeshChannelStreamStop, parameters))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.GlimeshChannelStreamStop, parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async void UserClient_OnFollowOccurred(object sender, FollowPacketModel follow)
        {
            try
            {
                UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, follow.Follow.user.id);
                if (user == null)
                {
                    user = await ServiceManager.Get<UserService>().CreateUser(new GlimeshUserPlatformV2Model(follow.Follow.user));
                }

                user.Roles.Add(UserRoleEnum.Follower);
                user.FollowDate = DateTimeOffset.Now;

                CommandParametersModel parameters = new CommandParametersModel(user);
                if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.GlimeshChannelFollowed, parameters))
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

                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.GlimeshChannelFollowed, parameters);

                    GlobalEvents.FollowOccurred(user);

                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.AlertFollow, user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.GlimeshUserChat);

            Result result;
            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.GlimeshUserChat);
        }

        private async void BotClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.GlimeshBotChat);

            Result result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.GlimeshBotChat);
        }
    }
}
