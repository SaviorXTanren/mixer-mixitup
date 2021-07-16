using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Models.NewAPI.Streams;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public class TwitchSessionService : IStreamingPlatformSessionService
    {
        public TwitchPlatformService UserConnection { get; private set; }
        public TwitchPlatformService BotConnection { get; private set; }
        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel UserNewAPI { get; set; }
        public UserModel BotNewAPI { get; set; }
        public StreamModel StreamNewAPI { get; set; }
        public bool StreamIsLive { get { return this.StreamNewAPI != null; } }

        public bool IsConnected { get { return this.UserConnection != null; } }

        public async Task<Result> ConnectUser()
        {
            Result<TwitchPlatformService> result = await TwitchPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.UserNewAPI = await this.UserConnection.GetNewAPICurrentUser();
                if (this.UserNewAPI == null)
                {
                    return new Result("Failed to get New API Twitch user data");
                }
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<TwitchPlatformService> result = await TwitchPlatformService.ConnectBot();
            if (result.Success)
            {
                this.BotConnection = result.Value;
                this.BotNewAPI = await this.BotConnection.GetNewAPICurrentUser();
                if (this.BotNewAPI == null)
                {
                    return new Result("Failed to get Twitch bot data");
                }

                if (ServiceManager.Has<TwitchChatService>() && ServiceManager.Get<TwitchChatService>().IsUserConnected)
                {
                    return await ServiceManager.Get<TwitchChatService>().ConnectBot();
                }
            }
            return result;
        }

        public async Task<Result> Connect(SettingsV3Model settings)
        {
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].IsEnabled)
            {
                Result userResult = null;

                Result<TwitchPlatformService> twitchResult = await TwitchPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken);
                if (twitchResult.Success)
                {
                    this.UserConnection = twitchResult.Value;
                    userResult = twitchResult;
                }
                else
                {
                    userResult = await this.ConnectUser();
                }

                if (userResult.Success)
                {
                    this.UserNewAPI = await this.UserConnection.GetNewAPICurrentUser();
                    if (this.UserNewAPI == null)
                    {
                        return new Result("Failed to get Twitch user data");
                    }

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken != null)
                    {
                        twitchResult = await TwitchPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken);
                        if (twitchResult.Success)
                        {
                            this.BotConnection = twitchResult.Value;
                            this.BotNewAPI = await this.BotConnection.GetNewAPICurrentUser();
                            if (this.BotNewAPI == null)
                            {
                                return new Result("Failed to get Twitch bot data");
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: "Failed to connect Twitch bot account, please manually reconnect");
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch] = null;
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

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch] = null;
        }

        public Task DisconnectBot(SettingsV3Model settings)
        {
            this.BotConnection = null;

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = null;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotID = null;

            return Task.FromResult(0);
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    UserModel twitchChannelNew = await this.UserConnection.GetNewAPICurrentUser();
                    if (twitchChannelNew != null)
                    {
                        this.UserNewAPI = twitchChannelNew;
                        this.StreamNewAPI = await this.UserConnection.GetStream(this.UserNewAPI);

                        IEnumerable<ChannelEditorUserModel> channelEditors = await this.UserConnection.GetChannelEditors(this.UserNewAPI);
                        if (channelEditors != null)
                        {
                            foreach (ChannelEditorUserModel channelEditor in channelEditors)
                            {
                                this.ChannelEditors.Add(channelEditor.user_id);
                            }
                        }

                        if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Twitch))
                        {
                            if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID) && !string.Equals(this.UserNewAPI.id, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID))
                            {
                                Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.UserNewAPI.login} - {this.UserNewAPI.id} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID}");
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.accessToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.refreshToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.expiresIn = 0;
                                return new Result("The account you are logged in as on Twitch does not match the account for this settings. Please log in as the correct account on Twitch.");
                            }
                        }

                        TwitchChatService chatService = new TwitchChatService();
                        TwitchEventService eventService = new TwitchEventService();

                        List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                        platformServiceTasks.Add(chatService.ConnectUser());
                        platformServiceTasks.Add(eventService.Connect());

                        await Task.WhenAll(platformServiceTasks);

                        if (platformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result("Failed to connect to Twitch services:" + Environment.NewLine + Environment.NewLine + errors);
                        }

                        ServiceManager.Add(chatService);
                        ServiceManager.Add(eventService);

                        await ServiceManager.Get<TwitchChatService>().Initialize();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result("Failed to connect to Twitch services. If this continues, please visit the Mix It Up Discord for assistance." +
                        Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null && ServiceManager.Has<TwitchChatService>())
            {
                Result result = await ServiceManager.Get<TwitchChatService>().ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser()
        {
            if (ServiceManager.Has<TwitchChatService>())
            {
                await ServiceManager.Get<TwitchChatService>().DisconnectUser();
            }

            if (ServiceManager.Has<TwitchEventService>())
            {
                await ServiceManager.Get<TwitchEventService>().Disconnect();
            }
        }

        public async Task CloseBot()
        {
            if (ServiceManager.Has<TwitchChatService>())
            {
                await ServiceManager.Get<TwitchChatService>().DisconnectBot();
            }
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                if (!settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Twitch))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch] = new StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum.Twitch);
                }

                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken = this.UserConnection.Connection.GetOAuthTokenCopy();
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID = this.UserNewAPI.id;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].ChannelID = this.UserNewAPI.id;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.BotNewAPI != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotID = this.BotNewAPI.id;
                    }
                }
            }
        }

        public async Task RefreshUser()
        {
            if (this.UserConnection != null)
            {
                UserModel twitchUserNewAPI = await this.UserConnection.GetNewAPICurrentUser();
                if (twitchUserNewAPI != null)
                {
                    this.UserNewAPI = twitchUserNewAPI;
                }
            }

            if (this.BotConnection != null)
            {
                UserModel botUserNewAPI = await this.BotConnection.GetNewAPICurrentUser();
                if (botUserNewAPI != null)
                {
                    this.BotNewAPI = botUserNewAPI;
                }
            }
        }

        public async Task RefreshChannel()
        {
            if (this.UserConnection != null && this.UserNewAPI != null)
            {
                this.StreamNewAPI = await this.UserConnection.GetStream(this.UserNewAPI);
            }
        }
    }
}
