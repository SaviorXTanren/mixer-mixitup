using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Chat.YouTube;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTube.Base.Clients;

namespace MixItUp.Base.Services.YouTube
{
    public class YouTubeChatEmoteModel
    {
        public class YouTubeChatEmoteImageModel
        {
            public List<string> thumbnails { get; set; } = new List<string>();
        }

        public string emojiId { get; set; }
        public List<string> searchTerms { get; set; } = new List<string>();
        public List<string> shortcuts { get; set; } = new List<string>();

        private YouTubeChatEmoteImageModel image = null;

        public string ImageURL { get { return this.image?.thumbnails?.FirstOrDefault(); } }
    }

    public class YouTubeChatService : StreamingPlatformServiceBase
    {
        private const int MaxMessageLength = 200;

        private ChatClient userClient;
        private ChatClient botClient;

        private SemaphoreSlim messageSemaphore = new SemaphoreSlim(1);

        public YouTubeChatService() { }

        public IEnumerable<YouTubeChatEmoteModel> Emotes { get; private set; } = new List<YouTubeChatEmoteModel>();

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

                        if (!await this.userClient.Connect())
                        {
                            return new Result(MixItUp.Base.Resources.YouTubeFailedToConnectToChat);
                        }

                        this.Emotes = await this.GetChatEmotes();

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
            using (AdvancedHttpClient client = new AdvancedHttpClient())
            {
                return await client.GetAsync<List<YouTubeChatEmoteModel>>("https://www.gstatic.com/youtube/img/emojis/emojis-svg-5.json");
            }
        }

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
                        UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.YouTube, message.AuthorDetails.ChannelId);
                        if (user == null)
                        {
                            Channel youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(message.AuthorDetails.ChannelId);
                            if (youtubeUser != null)
                            {
                                user = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(youtubeUser));
                            }
                            else
                            {
                                user = await ServiceManager.Get<UserService>().CreateUser(new YouTubeUserPlatformV2Model(message));
                            }
                            await ServiceManager.Get<UserService>().AddOrUpdateActiveUser(user);
                        }
                        user.GetPlatformData<YouTubeUserPlatformV2Model>(StreamingPlatformTypeEnum.YouTube).SetMessageProperties(message);

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
