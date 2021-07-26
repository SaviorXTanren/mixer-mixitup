using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trovo.Base.Models.Channels;
using Trovo.Base.Models.Users;

namespace MixItUp.Base.Services.Trovo
{
    public class TrovoSessionService : IStreamingPlatformSessionService
    {
        public TrovoPlatformService UserConnection { get; private set; }
        public TrovoPlatformService BotConnection { get; private set; }
        public PrivateUserModel User { get; private set; }
        public PrivateChannelModel Channel { get; private set; }
        public PrivateUserModel Bot { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }

        public async Task<Result> ConnectUser()
        {
            Result<TrovoPlatformService> result = await TrovoPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.User = await this.UserConnection.GetCurrentUser();
                if (this.User == null)
                {
                    return new Result("Failed to get Glimesh user data");
                }

                this.Channel = await this.UserConnection.GetCurrentChannel();
                if (this.Channel == null)
                {
                    return new Result("Failed to get Glimesh channel data");
                }
            }
            return result;
        }

        public async Task<Result> ConnectBot()
        {
            Result<TrovoPlatformService> result = await TrovoPlatformService.ConnectBot();
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
            if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].IsEnabled)
            {
                Result userResult = null;

                Result<TrovoPlatformService> trovoResult = await TrovoPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserOAuthToken);
                if (trovoResult.Success)
                {
                    this.UserConnection = trovoResult.Value;
                    userResult = trovoResult;
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
                        return new Result("Failed to get Trovo user data");
                    }

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotOAuthToken != null)
                    {
                        trovoResult = await TrovoPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotOAuthToken);
                        if (trovoResult.Success)
                        {
                            this.BotConnection = trovoResult.Value;
                            this.Bot = await this.BotConnection.GetCurrentUser();
                            if (this.Bot == null)
                            {
                                return new Result("Failed to get Trovo bot data");
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: "Failed to connect Trovo bot account, please manually reconnect");
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo] = null;
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

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo] = null;
        }

        public Task DisconnectBot(SettingsV3Model settings)
        {
            this.BotConnection = null;

            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotOAuthToken = null;
            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotID = null;

            return Task.CompletedTask;
        }

        public async Task<Result> InitializeUser(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                try
                {
                    PrivateChannelModel channel = await this.UserConnection.GetCurrentChannel();
                    if (channel != null)
                    {
                        this.Channel = channel;

                        if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Trovo))
                        {
                            if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserID) && !string.Equals(this.User.userId, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserID))
                            {
                                Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.User.userName} - {this.User.userId} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserID}");
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserOAuthToken.accessToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserOAuthToken.refreshToken = string.Empty;
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserOAuthToken.expiresIn = 0;
                                return new Result("The account you are logged in as on Trovo does not match the account for this settings. Please log in as the correct account on Trovo.");
                            }
                        }

                        TrovoChatEventService chatService = new TrovoChatEventService();

                        // Adding service before connecting due to race-condition where previous chat messages will begin getting
                        // sent in prior to full connect functionality finishing.
                        ServiceManager.Add(chatService);

                        List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                        platformServiceTasks.Add(chatService.ConnectUser());

                        await Task.WhenAll(platformServiceTasks);

                        if (platformServiceTasks.Any(c => !c.Result.Success))
                        {
                            ServiceManager.Remove<TrovoChatEventService>();

                            string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result("Failed to connect to Trovo services:" + Environment.NewLine + Environment.NewLine + errors);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result("Failed to connect to Trovo services. If this continues, please visit the Mix It Up Discord for assistance." +
                        Environment.NewLine + Environment.NewLine + "Error Details: " + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null && ServiceManager.Has<TrovoChatEventService>())
            {
                Result result = await ServiceManager.Get<TrovoChatEventService>().ConnectBot();
                if (!result.Success)
                {
                    return result;
                }
            }
            return new Result();
        }

        public async Task CloseUser()
        {
            if (ServiceManager.Has<TrovoChatEventService>())
            {
                await ServiceManager.Get<TrovoChatEventService>().DisconnectUser();
            }
        }

        public async Task CloseBot()
        {
            if (ServiceManager.Has<TrovoChatEventService>())
            {
                await ServiceManager.Get<TrovoChatEventService>().DisconnectBot();
            }
        }

        public void SaveSettings(SettingsV3Model settings)
        {
            if (this.UserConnection != null)
            {
                if (!settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Trovo))
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo] = new StreamingPlatformAuthenticationSettingsModel(StreamingPlatformTypeEnum.Trovo);
                }

                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserOAuthToken = this.UserConnection.Connection.GetOAuthTokenCopy();
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].UserID = this.User.userId;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].ChannelID = this.Channel.channel_id;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Trovo].BotID = this.Bot.userId;
                    }
                }
            }
        }

        public async Task RefreshUser()
        {
            if (this.UserConnection != null)
            {
                PrivateUserModel user = await this.UserConnection.GetCurrentUser();
                if (user != null)
                {
                    this.User = user;
                }
            }

            if (this.BotConnection != null)
            {
                PrivateUserModel bot = await this.BotConnection.GetCurrentUser();
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
                    PrivateChannelModel channel = await this.UserConnection.GetCurrentChannel();
                    if (channel != null)
                    {
                        this.Channel = channel;
                    }
                }
            }
        }
    }
}
