using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YouTube.Base.Clients;

namespace MixItUp.Base.Services.YouTube
{
    public class YouTubeChatService : StreamingPlatformServiceBase
    {
        private const int MaxMessageLength = 250;

        private ChatClient userClient;
        private ChatClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        public YouTubeChatService() { }

        public override string Name { get { return "YouTube Chat"; } }

        public bool IsUserConnected { get { return this.userClient != null; } }
        public bool IsBotConnected { get { return this.botClient != null; } }

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

                        if (!await this.userClient.Connect())
                        {
                            return new Result("Failed to connect to YouTube chat servers");
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
            return new Result("YouTube chat connection has not been established");
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
                        LiveBroadcast broadcast = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetActiveBroadcast();
                        if (broadcast != null)
                        {
                            return new Result("No live broadcast currently");
                        }

                        this.botClient = new ChatClient(ServiceManager.Get<YouTubeSessionService>().BotConnection.Connection);

                        if (!await this.botClient.Connect(broadcast, listenForMessage: false))
                        {
                            return new Result("Failed to connect to YouTube chat servers");
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
            return new Result("YouTube chat connection has not been established");
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

        public async Task<LiveChatModerator> ModUser(UserViewModel user) { return await this.userClient.ModUser(new Channel() { Id = user.YouTubeID }); }

        public async Task<LiveChatBan> TimeoutUser(UserViewModel user, ulong duration) { return await this.userClient.TimeoutUser(new Channel() { Id = user.YouTubeID }, duration); }

        public async Task<LiveChatBan> BanUser(UserViewModel user) { return await this.userClient.BanUser(new Channel() { Id = user.YouTubeID }); }

        private ChatClient GetChatClient(bool sendAsStreamer = false) { return (this.botClient != null && !sendAsStreamer) ? this.botClient : this.userClient; }

        private async void UserClient_OnMessagesReceived(object sender, IEnumerable<LiveChatMessage> messages)
        {
            if (messages != null)
            {
                foreach (LiveChatMessage message in messages)
                {
                    if (ChannelSession.AppSettings.DiagnosticLogging)
                    {
                        Logger.Log(LogLevel.Debug, string.Format("YouTube Chat Packet Received: {0}", JSONSerializerHelper.SerializeToString(message)));
                    }

                    if (message.AuthorDetails?.ChannelId != null)
                    {
                        UserViewModel user = ServiceManager.Get<UserService>().GetUserByPlatformID(StreamingPlatformTypeEnum.YouTube, message.AuthorDetails.ChannelId);
                        if (user == null)
                        {
                            Channel youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(message.AuthorDetails.ChannelId);
                            if (youtubeUser != null)
                            {
                                user = await ServiceManager.Get<UserService>().AddOrUpdateUser(youtubeUser);
                            }
                        }
                        user.SetYouTubeChatDetails(message);

                        // https://developers.google.com/youtube/v3/live/docs/liveChatMessages#resource

                        if (message.Snippet.HasDisplayContent.GetValueOrDefault() && !string.IsNullOrEmpty(message.Snippet.DisplayMessage))
                        {
                            await ServiceManager.Get<ChatService>().AddMessage(new YouTubeChatMessageViewModel(message, user));
                        }
                    }
                }
            }
        }
    }
}
