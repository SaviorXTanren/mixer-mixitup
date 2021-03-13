using Glimesh.Base.Models.Channels;
using Glimesh.Base.Models.Users;
using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Glimesh
{
    public class GlimeshSessionService : IStreamingPlatformSessionService
    {
        public GlimeshPlatformService UserConnection { get; private set; }
        public GlimeshPlatformService BotConnection { get; private set; }
        public UserModel User { get; private set; }
        public ChannelModel Channel { get; private set; }
        public UserModel Bot { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }

        public async Task<Result> ConnectUser()
        {
            Result<GlimeshPlatformService> result = await GlimeshPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.User = await this.UserConnection.GetCurrentUser();
                if (this.User == null)
                {
                    return new Result("Failed to get Glimesh user data");
                }

                this.Channel = await this.UserConnection.GetChannelByName(this.User.username);
                if (this.Channel == null)
                {
                    return new Result("Failed to get Glimesh channel data");
                }
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<GlimeshPlatformService> result = await GlimeshPlatformService.ConnectBot();
            if (result.Success)
            {
                this.BotConnection = result.Value;
                this.Bot = await this.BotConnection.GetCurrentUser();
                if (this.Bot == null)
                {
                    return new Result("Failed to get Glimesh bot data");
                }
            }
            return result;
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].IsEnabled)
            {
                Result userResult = null;

                Result<GlimeshPlatformService> glimeshResult = await GlimeshPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken);
                if (glimeshResult.Success)
                {
                    this.UserConnection = glimeshResult.Value;
                    userResult = glimeshResult;
                }
                else
                {
                    userResult = await this.ConnectUser();
                }

                if (userResult.Success)
                {
                    this.User = await this.UserConnection.GetCurrentUser();
                    if (this.User == null)
                    {
                        return new Result("Failed to get Glimesh user data");
                    }

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotOAuthToken != null)
                    {
                        glimeshResult = await GlimeshPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotOAuthToken);
                        if (glimeshResult.Success)
                        {
                            this.BotConnection = glimeshResult.Value;
                            this.Bot = await this.BotConnection.GetCurrentUser();
                            if (this.Bot == null)
                            {
                                return new Result("Failed to get Glimesh bot data");
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: "Failed to connect Glimesh bot account, please manually reconnect");
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh] = null;
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

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh] = null;
        }

        public Task DisconnectBot(SettingsV3Model settings)
        {
            this.BotConnection = null;

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotOAuthToken = null;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotID = null;

            return Task.FromResult(0);
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    ChannelModel channel = await this.UserConnection.GetChannelByName(this.User.username);
                    if (channel != null)
                    {
                        this.Channel = channel;

                        if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Glimesh))
                        {
                            if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID) && !string.Equals(this.User.id, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID))
                            {
                                Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.User.username} - {this.User.id} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID}");
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken.accessToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken.refreshToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken.expiresIn = 0;
                                return new Result("The account you are logged in as on Glimesh does not match the account for this settings. Please log in as the correct account on Glimesh.");
                            }
                        }

                        GlimeshChatEventService chatService = new GlimeshChatEventService();

                        List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                        platformServiceTasks.Add(chatService.ConnectUser());

                        await Task.WhenAll(platformServiceTasks);

                        if (platformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result("Failed to connect to Glimesh services:" + Environment.NewLine + Environment.NewLine + errors);
                        }

                        ServiceManager.Add(chatService);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result("Failed to connect to Glimesh services. If this continues, please visit the Mix It Up Discord for assistance." +
                        Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null && ServiceManager.Has<GlimeshChatEventService>())
            {
                Result result = await ServiceManager.Get<GlimeshChatEventService>().ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser()
        {
            if (ServiceManager.Has<GlimeshChatEventService>())
            {
                await ServiceManager.Get<GlimeshChatEventService>().DisconnectUser();
            }
        }

        public async Task CloseBot()
        {
            if (ServiceManager.Has<GlimeshChatEventService>())
            {
                await ServiceManager.Get<GlimeshChatEventService>().DisconnectBot();
            }
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                if (!settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Glimesh))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh] = new StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum.Glimesh);
                }

                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken = this.UserConnection.Connection.GetOAuthTokenCopy();
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID = this.User.id;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].ChannelID = this.Channel.id;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotID = this.Bot.id;
                    }
                }
            }
        }

        public async Task RefreshUser()
        {
            if (this.UserConnection != null)
            {
                UserModel user = await this.UserConnection.GetCurrentUser();
                if (user != null)
                {
                    this.User = user;
                }
            }

            if (this.BotConnection != null)
            {
                UserModel bot = await this.BotConnection.GetCurrentUser();
                if (bot != null)
                {
                    this.Bot = bot;
                }
            }
        }

        public async Task RefreshChannel()
        {
            if (this.UserConnection != null)
            {
                if (this.Channel != null)
                {
                    ChannelModel channel = await this.UserConnection.GetChannelByID(this.Channel.id);
                    if (channel != null)
                    {
                        this.Channel = channel;
                    }
                }
            }
        }
    }
}
