using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.YouTube
{
    public class YouTubeSessionService : IStreamingPlatformSessionService
    {
        public YouTubePlatformService UserConnection { get; private set; }
        public YouTubePlatformService BotConnection { get; private set; }
        public Channel User { get; private set; }
        public Channel Bot { get; private set; }
        public LiveBroadcast Broadcast { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return this.User?.Id; } }
        public string Username { get { return this.User?.Snippet?.Title; } }
        public string BotID { get { return this.Bot?.Id; } }
        public string Botname { get { return this.Bot?.Snippet?.Title; } }
        public string ChannelID { get { return this.User?.Id; } }
        public string ChannelLink { get { return this.User?.Snippet?.CustomUrl; } }

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

        public async Task<Result> ConnectUser()
        {
            Result<YouTubePlatformService> result = await YouTubePlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.User = await this.UserConnection.GetCurrentChannel();
                if (this.User == null)
                {
                    return new Result(MixItUp.Base.Resources.YouTubeFailedToGetUserData);
                }
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<YouTubePlatformService> result = await YouTubePlatformService.ConnectBot();
            if (result.Success)
            {
                this.BotConnection = result.Value;
                this.Bot = await this.BotConnection.GetCurrentChannel();
                if (this.Bot == null)
                {
                    return new Result(MixItUp.Base.Resources.YouTubeFailedToGetBotData);
                }
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

                    await Task.WhenAll(platformServiceTasks);

                    if (platformServiceTasks.Any(c => !c.Result.Success))
                    {
                        ServiceManager.Remove<YouTubeChatService>();

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

        public Task RefreshChannel()
        {
            this.Broadcast = ServiceManager.Get<YouTubeChatService>().Broadcast;
            return Task.CompletedTask;
        }

        public Task<string> GetTitle() { return Task.FromResult(string.Empty); }

        public Task<bool> SetTitle(string title) { return Task.FromResult(false); }

        public Task<string> GetGame() { return Task.FromResult(string.Empty); }

        public Task<bool> SetGame(string gameName) { return Task.FromResult(false); }
    }
}
