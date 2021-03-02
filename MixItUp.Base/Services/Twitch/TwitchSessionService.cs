using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;
using TwitchV5API = Twitch.Base.Models.V5;

namespace MixItUp.Base.Services.Twitch
{
    public class TwitchSessionService : IStreamingPlatformSessionService
    {
        public TwitchPlatformService UserConnection { get; private set; }
        public TwitchPlatformService BotConnection { get; private set; }
        public TwitchV5API.Users.UserModel UserV5 { get; private set; }
        public TwitchV5API.Channel.ChannelModel ChannelV5 { get; private set; }
        public TwitchV5API.Streams.StreamModel StreamV5 { get; private set; }
        public HashSet<string> ChannelEditorsV5 { get; private set; } = new HashSet<string>();
        public TwitchNewAPI.Users.UserModel UserNewAPI { get; set; }
        public TwitchNewAPI.Users.UserModel BotNewAPI { get; set; }
        public TwitchNewAPI.Streams.StreamModel StreamNewAPI { get; set; }
        public bool StreamIsLive { get { return this.StreamV5 != null && this.StreamV5.IsLive; } }

        public async Task<Result> ConnectUser(SettingsV3Model settings)
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

                this.UserV5 = await this.UserConnection.GetV5APIUserByLogin(this.UserNewAPI.login);
                if (this.UserV5 == null)
                {
                    return new Result("Failed to get V5 API Twitch user data");
                }

                this.SaveSettings(settings);
            }
            return result;
        }

        public async Task<Result> ConnectBot(SettingsV3Model settings)
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

                if (ServiceManager.Get<ChatService>().TwitchChatService != null && ServiceManager.Get<ChatService>().TwitchChatService.IsUserConnected)
                {
                    return await ServiceManager.Get<ChatService>().TwitchChatService.ConnectBot();
                }

                this.SaveSettings(settings);
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
                    userResult = await this.ConnectUser(settings);
                }

                if (userResult.Success)
                {
                    this.UserNewAPI = await this.UserConnection.GetNewAPICurrentUser();
                    if (this.UserNewAPI == null)
                    {
                        return new Result("Failed to get Twitch user data");
                    }

                    this.UserV5 = await this.UserConnection.GetV5APIUserByLogin(this.UserNewAPI.login);
                    if (this.UserV5 == null)
                    {
                        return new Result("Failed to get V5 API Twitch user data");
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
                    TwitchNewAPI.Users.UserModel twitchChannelNew = await this.UserConnection.GetNewAPICurrentUser();
                    TwitchV5API.Channel.ChannelModel twitchChannelv5 = await this.UserConnection.GetCurrentV5APIChannel();
                    if (twitchChannelNew != null && twitchChannelv5 != null)
                    {
                        this.UserNewAPI = twitchChannelNew;
                        this.ChannelV5 = twitchChannelv5;
                        this.StreamNewAPI = await this.UserConnection.GetStream(this.UserNewAPI);
                        this.StreamV5 = await this.UserConnection.GetV5LiveStream(this.ChannelV5);

                        IEnumerable<TwitchV5API.Users.UserModel> channelEditors = await this.UserConnection.GetV5APIChannelEditors(this.ChannelV5);
                        if (channelEditors != null)
                        {
                            foreach (TwitchV5API.Users.UserModel channelEditor in channelEditors)
                            {
                                this.ChannelEditorsV5.Add(channelEditor.id);
                            }
                        }

                        if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID) && !string.Equals(this.UserNewAPI.id, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID))
                        {
                            Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.UserNewAPI.login} - {this.UserNewAPI.id} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID}");
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.accessToken = string.Empty;
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.refreshToken = string.Empty;
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.expiresIn = 0;
                            return new Result("The account you are logged in as on Twitch does not match the account for this settings. Please log in as the correct account on Twitch.");
                        }

                        this.SaveSettings(settings);

                        TwitchChatService twitchChatService = new TwitchChatService();
                        TwitchEventService twitchEventService = new TwitchEventService();

                        List<Task<Result>> twitchPlatformServiceTasks = new List<Task<Result>>();
                        twitchPlatformServiceTasks.Add(twitchChatService.ConnectUser());
                        twitchPlatformServiceTasks.Add(twitchEventService.Connect());

                        await Task.WhenAll(twitchPlatformServiceTasks);

                        if (twitchPlatformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, twitchPlatformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result("Failed to connect to Twitch services:" + Environment.NewLine + Environment.NewLine + errors);
                        }

                        await ServiceManager.Get<ChatService>().Initialize(twitchChatService);
                        await ServiceManager.Get<EventService>().Initialize(twitchEventService);
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
            if (this.BotConnection != null)
            {
                Result result = await ServiceManager.Get<ChatService>().TwitchChatService.ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser(SettingsV3Model settings)
        {
            if (ServiceManager.Get<ChatService>().TwitchChatService != null)
            {
                await ServiceManager.Get<ChatService>().TwitchChatService.DisconnectUser();
            }
        }

        public async Task CloseBot(SettingsV3Model settings)
        {
            if (ServiceManager.Get<ChatService>().TwitchChatService != null)
            {
                await ServiceManager.Get<ChatService>().TwitchChatService.DisconnectBot();
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
            if (this.UserNewAPI != null)
            {
                TwitchNewAPI.Users.UserModel twitchUserNewAPI = await this.UserConnection.GetNewAPICurrentUser();
                if (twitchUserNewAPI != null)
                {
                    this.UserNewAPI = twitchUserNewAPI;

                    TwitchV5API.Users.UserModel twitchUserV5 = await this.UserConnection.GetV5APIUserByLogin(this.UserNewAPI.login);
                    if (twitchUserV5 != null)
                    {
                        this.UserV5 = twitchUserV5;
                    }
                }
            }
        }

        public async Task RefreshChannel()
        {
            if (this.ChannelV5 != null)
            {
                TwitchV5API.Channel.ChannelModel twitchChannel = await this.UserConnection.GetV5APIChannel(this.ChannelV5.id);
                if (twitchChannel != null)
                {
                    this.ChannelV5 = twitchChannel;
                    this.StreamV5 = await this.UserConnection.GetV5LiveStream(this.ChannelV5);
                }
            }

            if (this.UserNewAPI != null)
            {
                this.StreamNewAPI = await this.UserConnection.GetStream(this.UserNewAPI);
            }
        }
    }
}
