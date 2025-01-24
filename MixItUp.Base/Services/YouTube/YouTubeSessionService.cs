using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube
{
    [Obsolete]
    public class YouTubeSessionService : IStreamingPlatformSessionService
    {
        public YouTubePlatformService UserConnection { get; private set; }
        public YouTubePlatformService BotConnection { get; private set; }
        public Channel User { get; private set; }
        public Channel Bot { get; private set; }
        public LiveBroadcast Broadcast { get; private set; }
        public Video Video { get; private set; }
        public List<MembershipsLevel> MembershipLevels { get; private set; } = new List<MembershipsLevel>();

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return this.User?.Id; } }
        public string Username { get { return this.User?.Snippet?.Title; } }
        public string BotID { get { return this.Bot?.Id; } }
        public string Botname { get { return this.Bot?.Snippet?.Title; } }
        public string ChannelID { get { return this.User?.Id; } }
        public string ChannelLink { get { return this.User?.Snippet?.CustomUrl; } }
        public string StreamLink { get { return $"https://youtube.com/watch?v={ServiceManager.Get<YouTubeSessionService>().Broadcast?.Id}"; } }

        public bool HasMembershipCapabilities { get { return this.MembershipLevels.Count > 0; } }

        private DateTime launchDateTime = DateTime.Now;

        public StreamingPlatformAccountModel UserAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.UserID,
                    Username = this.Username,
                    AvatarURL = this.User?.Snippet?.Thumbnails?.Medium?.Url
                };
            }
        }
        public StreamingPlatformAccountModel BotAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.BotID,
                    Username = this.Botname,
                    AvatarURL = this.Bot?.Snippet?.Thumbnails?.Medium?.Url
                };
            }
        }

        public bool IsLive { get { return string.Equals(this.Broadcast?.Status?.LifeCycleStatus, "live", StringComparison.OrdinalIgnoreCase); } }

        public int ViewerCount { get { return (int)this.Video?.LiveStreamingDetails?.ConcurrentViewers.GetValueOrDefault(); } }

        public DateTimeOffset StreamStart
        {
            get
            {
                if (this.IsLive)
                {
                    if (this.Broadcast.Snippet.ActualStartTime.HasValue)
                    {
                        DateTime dt = this.Broadcast.Snippet.ActualStartTime.GetValueOrDefault();
                        if (dt.Kind == DateTimeKind.Unspecified)
                        {
                            dt = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                        }
                        return new DateTimeOffset(dt, (dt.Kind == DateTimeKind.Utc) ? TimeSpan.Zero : DateTimeOffset.Now.Offset);
                    }
                }
                return DateTimeOffset.MinValue;
            }
        }

        public async Task<Result> ConnectUser()
        {
            Result<YouTubePlatformService> result = await YouTubePlatformService.ConnectUser();
            if (result.Success)
            {
                this.User = await result.Value.GetCurrentChannel();
                if (this.User == null)
                {
                    return new Result(MixItUp.Base.Resources.YouTubeFailedToGetUserData);
                }
                this.UserConnection = result.Value;

                await this.RefreshChannel();
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<YouTubePlatformService> result = await YouTubePlatformService.ConnectBot();
            if (result.Success)
            {
                this.Bot = await result.Value.GetCurrentChannel();
                if (this.Bot == null)
                {
                    return new Result(MixItUp.Base.Resources.YouTubeFailedToGetBotData);
                }
                this.BotConnection = result.Value;
            }
            return result;
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].IsEnabled)
            {
                Result userResult = null;

                // If scopes don't match, re-auth the token
                if (string.Equals(string.Join(",", YouTubePlatformService.StreamerScopes), settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken.ScopeList, StringComparison.OrdinalIgnoreCase))
                {
                    Result<YouTubePlatformService> youtubeResult = await YouTubePlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken);
                    if (youtubeResult.Success)
                    {
                        this.UserConnection = youtubeResult.Value;
                        userResult = youtubeResult;
                    }
                    else
                    {
                        userResult = await this.ConnectUser();
                    }
                }
                else
                {
                    userResult = await this.ConnectUser();
                }

                if (userResult.Success)
                {
                    this.User = await this.UserConnection.GetCurrentChannel();
                    if (this.User == null)
                    {
                        return new Result(MixItUp.Base.Resources.YouTubeFailedToGetUserData);
                    }

                    await this.RefreshChannel();

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken != null)
                    {
                        Result<YouTubePlatformService> youtubeResult = await YouTubePlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken);
                        if (youtubeResult.Success)
                        {
                            this.BotConnection = youtubeResult.Value;
                            this.Bot = await this.BotConnection.GetCurrentChannel();
                            if (this.Bot == null)
                            {
                                return new Result(MixItUp.Base.Resources.YouTubeFailedToGetBotData);
                            }
                        }
                        else
                        {
                            return new Result(success: true, message: MixItUp.Base.Resources.YouTubeFailedToConnectBotAccount);
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].ClearUserData();
                    return userResult;
                }

                return userResult;
            }
            return new Result();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            await this.DisconnectBot(settings);

            await ServiceManager.Get<YouTubeChatService>().DisconnectUser();

            this.UserConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.YouTube, out var streamingPlatform))
            {
                streamingPlatform.ClearUserData();
            }
        }

        public async Task DisconnectBot(SettingsV3Model settings)
        {
            await ServiceManager.Get<YouTubeChatService>().DisconnectBot();

            this.BotConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.YouTube, out var streamingPlatform))
            {
                streamingPlatform.ClearBotData();
            }
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.YouTube))
                    {
                        if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID) && !string.Equals(this.UserID, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID))
                        {
                            Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.UserID} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID}");
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken.ResetToken();
                            return new Result(string.Format(MixItUp.Base.Resources.StreamingPlatformIncorrectAccount, StreamingPlatformTypeEnum.YouTube));
                        }
                    }

                    List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                    platformServiceTasks.Add(ServiceManager.Get<YouTubeChatService>().ConnectUser());
                    platformServiceTasks.Add(this.SetMembershipLevels());

                    await Task.WhenAll(platformServiceTasks);

                    if (platformServiceTasks.Any(c => !c.Result.Success))
                    {
                        string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                        return new Result(MixItUp.Base.Resources.YouTubeFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result(MixItUp.Base.Resources.YouTubeFailedToConnect +
                        Environment.NewLine + Environment.NewLine + MixItUp.Base.Resources.ErrorHeader + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null)
            {
                Result result = await ServiceManager.Get<YouTubeChatService>().ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser()
        {
            await ServiceManager.Get<YouTubeChatService>().DisconnectUser();
        }

        public async Task CloseBot()
        {
            await ServiceManager.Get<YouTubeChatService>().DisconnectBot();
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                if (!settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.YouTube))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube] = new StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum.YouTube);
                }

                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken = this.UserConnection.Connection.GetOAuthTokenCopy();
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID = this.UserID;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].ChannelID = this.ChannelID;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotID = this.BotID;
                    }
                }
            }
        }

        public async Task RefreshUser()
        {
            if (this.UserConnection != null)
            {
                Channel channel = await this.UserConnection.GetCurrentChannel();
                if (channel != null)
                {
                    this.User = channel;
                }
            }

            if (this.BotConnection != null)
            {
                Channel bot = await this.BotConnection.GetCurrentChannel();
                if (bot != null)
                {
                    this.Bot = bot;
                }
            }
        }

        public async Task RefreshChannel()
        {
            if (this.Broadcast == null)
            {
                this.Broadcast = await this.UserConnection.GetMyActiveBroadcast();
            }

            if (this.Broadcast != null)
            {
                LiveBroadcast broadcast = await this.UserConnection.GetBroadcastByID(this.Broadcast.Id);
                if (broadcast != null)
                {
                    if (broadcast?.Snippet?.Title != null && !string.Equals(this.Broadcast?.Snippet?.Title, broadcast?.Snippet?.Title, StringComparison.OrdinalIgnoreCase))
                    {
                        ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.StreamUpdated, platform: StreamingPlatformTypeEnum.YouTube, description: broadcast?.Snippet?.Title);
                    }
                    this.Broadcast = broadcast;
                }

                Video video = await this.UserConnection.GetVideoByID(this.Broadcast.Id);
                if (video != null)
                {
                    this.Video = video;
                }

                if (ChannelSession.User != null)
                {
                    if (this.Broadcast.Snippet.ActualStartTime.HasValue && this.launchDateTime < this.Broadcast.Snippet.ActualStartTime)
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStart, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                    }

                    if (this.Broadcast.Snippet.ActualEndTime.HasValue && this.launchDateTime < this.Broadcast.Snippet.ActualEndTime)
                    {
                        await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.YouTubeChannelStreamStop, new CommandParametersModel(StreamingPlatformTypeEnum.YouTube));
                    }
                }
            }
        }

        public Task<string> GetTitle() { return Task.FromResult<string>(this.Broadcast?.Snippet?.Title); }

        public async Task<bool> SetTitle(string title)
        {
            if (this.IsLive)
            {
                Video video = ServiceManager.Get<YouTubeSessionService>().Video;
                video = await ServiceManager.Get<YouTubeSessionService>().UserConnection.UpdateVideo(ServiceManager.Get<YouTubeSessionService>().Video, title: title, description: video.Snippet?.Description ?? title);
                if (video != null && string.Equals(video.Snippet.Title, title))
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        public Task<string> GetGame() { return Task.FromResult(string.Empty); }

        public Task<bool> SetGame(string gameName) { return Task.FromResult(true); }

        private async Task<Result> SetMembershipLevels()
        {
            try
            {
                IEnumerable<MembershipsLevel> membershipLevels = await this.UserConnection.GetMembershipLevels();
                if (membershipLevels != null && membershipLevels.Count() > 0)
                {
                    this.MembershipLevels.AddRange(membershipLevels);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result();
        }
    }
}
