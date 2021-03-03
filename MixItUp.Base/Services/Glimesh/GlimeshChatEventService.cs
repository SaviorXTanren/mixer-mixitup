using Glimesh.Base.Clients;
using Glimesh.Base.Models.Clients.Channel;
using Glimesh.Base.Models.Clients.Chat;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.Glimesh;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Glimesh
{
    public interface IGlimeshChatEventService
    {
        bool IsUserConnected { get; }
        bool IsBotConnected { get; }

        Task<Result> ConnectUser();
        Task DisconnectUser();

        Task<Result> ConnectBot();
        Task DisconnectBot();

        Task SendMessage(string message, bool sendAsStreamer = false);

        Task ShortTimeoutUser(UserViewModel user);
        Task LongTimeoutUser(UserViewModel user);

        Task BanUser(UserViewModel user);
        Task UnbanUser(UserViewModel user);
    }

    public class GlimeshChatEventService : StreamingPlatformServiceBase, IGlimeshChatEventService
    {
        private ChatEventClient userClient;
        private ChatEventClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        private const int MaxMessageLength = 250;

        public GlimeshChatEventService() { }

        public override string Name { get { return "Glimesh Chat"; } }

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
                            return new Result("Failed to connect to Glimesh chat servers");
                        }

                        if (!await this.userClient.JoinChannelChat(ServiceManager.Get<GlimeshSessionService>().Channel?.id))
                        {
                            return new Result("Failed to join Glimesh channel chat");
                        }

                        if (!await this.userClient.JoinChannelEvents(ServiceManager.Get<GlimeshSessionService>().Channel?.id, ServiceManager.Get<GlimeshSessionService>().User?.id))
                        {
                            return new Result("Failed to join Glimesh channel events");
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
            return new Result("Glimesh chat connection has not been established");
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
                            return new Result("Failed to connect to Glimesh chat servers");
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
            return new Result("Glimesh chat connection has not been established");
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
                        await client.SendMessage(ServiceManager.Get<GlimeshSessionService>().Channel?.id, message);
                        message = subMessage;
                        await Task.Delay(500);
                    }
                    while (!string.IsNullOrEmpty(message));
                }
            });
        }

        public async Task ShortTimeoutUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.ShortTimeoutUser(ServiceManager.Get<GlimeshSessionService>().Channel?.id, user.GlimeshID);
                }
            });
        }

        public async Task LongTimeoutUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.LongTimeoutUser(ServiceManager.Get<GlimeshSessionService>().Channel?.id, user.GlimeshID);
                }
            });
        }

        public async Task BanUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.BanUser(ServiceManager.Get<GlimeshSessionService>().Channel?.id, user.GlimeshID);
                }
            });
        }

        public async Task UnbanUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.UnbanUser(ServiceManager.Get<GlimeshSessionService>().Channel?.id, user.GlimeshID);
                }
            });
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

        private async void UserClient_OnChatMessageReceived(object sender, ChatMessagePacketModel message)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message))
            {
                UserViewModel user = ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, message.User?.id);
                if (user == null)
                {
                    UserModel glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByName(message.User?.id);
                    if (glimeshUser != null)
                    {
                        user = await ServiceManager.Get<UserService>().AddOrUpdateUser(glimeshUser);
                    }
                }
                await ServiceManager.Get<ChatService>().AddMessage(new GlimeshChatMessageViewModel(message, user));
            }
        }

        private async void UserClient_OnChannelUpdated(object sender, ChannelUpdatePacketModel channel)
        {
            try
            {
                if (!ServiceManager.Get<GlimeshSessionService>().Channel.IsLive && channel.Channel.IsLive)
                {
                    EventTrigger trigger = new EventTrigger(EventTypeEnum.GlimeshChannelStreamStart, ChannelSession.GetCurrentUser());
                    if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(trigger);
                    }
                }
                else if (ServiceManager.Get<GlimeshSessionService>().Channel.IsLive && !channel.Channel.IsLive)
                {
                    EventTrigger trigger = new EventTrigger(EventTypeEnum.GlimeshChannelStreamStop, ChannelSession.GetCurrentUser());
                    if (ServiceManager.Get<EventService>().CanPerformEvent(trigger))
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(trigger);
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
                UserViewModel user = ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, follow.Follow.user.id);
                if (user == null)
                {
                    user = new UserViewModel(follow.Follow.user);
                }

                EventTrigger trigger = new EventTrigger(EventTypeEnum.GlimeshChannelFollowed, user);
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

                    await ServiceManager.Get<AlertsService>().AddAlert(new AlertChatMessageViewModel(StreamingPlatformTypeEnum.Glimesh, user, string.Format("{0} Followed", user.DisplayName), ChannelSession.Settings.AlertFollowColor));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async void UserClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Glimesh User Chat");

            Result result;
            await this.DisconnectUser();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectUser();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Glimesh User Chat");
        }

        private async void BotClient_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Glimesh Bot Chat");

            Result result;
            await this.DisconnectBot();
            do
            {
                await Task.Delay(2500);

                result = await this.ConnectBot();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred("Glimesh Bot Chat");
        }
    }
}
