using Glimesh.Base.Clients;
using Glimesh.Base.Models.Clients.Chat;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Glimesh;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Glimesh
{
    public interface IGlimeshChatService
    {
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

    public class GlimeshChatService : StreamingPlatformServiceBase, IGlimeshChatService
    {
        public event EventHandler<GlimeshChatMessageViewModel> OnMessageOccurred;

        private ChatEventClient userClient;
        private ChatEventClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        public GlimeshChatService() { }

        public override string Name { get { return "Glimesh Chat"; } }

        public async Task<Result> ConnectUser()
        {
            if (ChannelSession.GlimeshUserConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.userClient = await ChatEventClient.CreateWithToken(ChannelSession.GlimeshUserConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred += Client_OnSentOccurred;
                        }
                        this.userClient.OnDisconnectOccurred += UserClient_OnDisconnectOccurred;
                        this.userClient.OnChatMessageReceived += UserClient_OnChatMessageReceived;

                        if (await this.userClient.Connect())
                        {
                            return new Result("Failed to connect to Glimesh chat servers");
                        }

                        if (await this.userClient.JoinChannelChat(ChannelSession.GlimeshChannel.id))
                        {
                            return new Result("Failed to join Glimesh channel chat");
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
            if (ChannelSession.GlimeshBotConnection != null)
            {
                return await this.AttemptConnect((Func<Task<Result>>)(async () =>
                {
                    try
                    {
                        this.botClient = await ChatEventClient.CreateWithToken(ChannelSession.GlimeshBotConnection.Connection);

                        if (ChannelSession.AppSettings.DiagnosticLogging)
                        {
                            this.userClient.OnSentOccurred -= Client_OnSentOccurred;
                        }
                        this.botClient.OnDisconnectOccurred += BotClient_OnDisconnectOccurred;

                        if (await this.botClient.Connect())
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
                        message = ChatService.SplitLargeMessage(message, out subMessage);
                        await client.SendMessage(ChannelSession.GlimeshChannel.id, message);
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
                    await this.userClient.ShortTimeoutUser(ChannelSession.GlimeshChannel.id, user.GlimeshID);
                }
            });
        }

        public async Task LongTimeoutUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.LongTimeoutUser(ChannelSession.GlimeshChannel.id, user.GlimeshID);
                }
            });
        }

        public async Task BanUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.BanUser(ChannelSession.GlimeshChannel.id, user.GlimeshID);
                }
            });
        }

        public async Task UnbanUser(UserViewModel user)
        {
            await AsyncRunner.RunAsync(async () =>
            {
                if (this.userClient != null)
                {
                    await this.userClient.UnbanUser(ChannelSession.GlimeshChannel.id, user.GlimeshID);
                }
            });
        }

        private ChatEventClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private void Client_OnSentOccurred(object sender, string packet)
        {
            Logger.Log(LogLevel.Debug, string.Format("Glimesh Chat Packet Sent: {0}", packet));
        }

        private async void UserClient_OnChatMessageReceived(object sender, ChatMessagePacketModel message)
        {
            if (message != null && !string.IsNullOrEmpty(message.Message))
            {
                UserViewModel user = ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.Glimesh, message.User?.id);
                if (user == null)
                {
                    UserModel glimeshUser = await ChannelSession.GlimeshUserConnection.GetUserByName(message.User?.id);
                    if (glimeshUser != null)
                    {
                        user = await ServiceManager.Get<UserService>().AddOrUpdateUser(glimeshUser);
                    }
                }
                this.OnMessageOccurred?.Invoke(this, new GlimeshChatMessageViewModel(message, user));
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
