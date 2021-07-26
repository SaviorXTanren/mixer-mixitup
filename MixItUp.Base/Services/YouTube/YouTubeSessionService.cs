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
        public Channel Channel { get; private set; }
        public Channel Bot { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }

        public async Task<Result> ConnectUser()
        {
            Result<YouTubePlatformService> result = await YouTubePlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.Channel = await this.UserConnection.GetCurrentChannel();
                if (this.Channel == null)
                {
                    return new Result("Failed to get YouTube channel data");
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
                    return new Result("Failed to get YouTube bot data");
                }
            }
            return result;
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].IsEnabled)
            {
                Result userResult = null;

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

                if (userResult.Success)
                {
                    this.Channel = await this.UserConnection.GetCurrentChannel();
                    if (this.Channel == null)
                    {
                        return new Result("Failed to get YouTube channel data");
                    }

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken != null)
                    {
                        youtubeResult = await YouTubePlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken);
                        if (youtubeResult.Success)
                        {
                            this.BotConnection = youtubeResult.Value;
                            this.Bot = await this.BotConnection.GetCurrentChannel();
                            if (this.Bot == null)
                            {
                                return new Result("Failed to get YouTube bot data");
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: "Failed to connect YouTube bot account, please manually reconnect");
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube] = null;
                    return userResult;
                }

                return userResult;
            }
            return new Result();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            await this.DisconnectBot(settings);

            this.UserConnection = null;

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube] = null;
        }

        public Task DisconnectBot(SettingsV3Model settings)
        {
            this.BotConnection = null;

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken = null;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotID = null;

            return Task.CompletedTask;
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.YouTube))
                    {
                        if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID) && !string.Equals(this.Channel.Id, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID))
                        {
                            Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.Channel.Id} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID}");
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken.accessToken = string.Empty;
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken.refreshToken = string.Empty;
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserOAuthToken.expiresIn = 0;
                            return new Result("The account you are logged in as on YouTube does not match the account for this settings. Please log in as the correct account on YouTube.");
                        }
                    }

                    YouTubeChatService chatService = new YouTubeChatService();

                    List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                    platformServiceTasks.Add(chatService.ConnectUser());

                    await Task.WhenAll(platformServiceTasks);

                    if (platformServiceTasks.Any(c => !c.Result.Success))
                    {
                        ServiceManager.Remove<YouTubeChatService>();

                        string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                        return new Result("Failed to connect to YouTube services:" + Environment.NewLine + Environment.NewLine + errors);
                    }

                    ServiceManager.Add(chatService);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result("Failed to connect to YouTube services. If this continues, please visit the Mix It Up Discord for assistance." +
                        Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null && ServiceManager.Has<YouTubeChatService>())
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
            if (ServiceManager.Has<YouTubeChatService>())
            {
                await ServiceManager.Get<YouTubeChatService>().DisconnectUser();
            }
        }

        public async Task CloseBot()
        {
            if (ServiceManager.Has<YouTubeChatService>())
            {
                await ServiceManager.Get<YouTubeChatService>().DisconnectBot();
            }
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
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].UserID = this.Channel.Id;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].ChannelID = this.Channel.Id;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.YouTube].BotID = this.Channel.Id;
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
                    this.Channel = channel;
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
            return Task.CompletedTask;
        }
    }
}
