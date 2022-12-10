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
        private const string GamingCategorySlug = "gaming";

        public GlimeshPlatformService UserConnection { get; private set; }
        public GlimeshPlatformService BotConnection { get; private set; }

        public UserModel User { get; private set; }
        public UserModel Bot { get; private set; }
        public ChannelModel Channel { get; private set; }

        public HashSet<string> Moderators { get; private set; } = new HashSet<string>();

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return this.User?.id; } }
        public string Username { get { return this.User?.username; } }
        public string BotID { get { return this.Bot?.id; } }
        public string Botname { get { return this.Bot?.username; } }
        public string ChannelID { get { return this.Channel?.id; } }
        public string ChannelLink { get { return string.Format("glimesh.tv/{0}", this.Username?.ToLower()); } }

        public StreamingPlatformAccountModel UserAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.UserID,
                    Username = this.Username,
                    AvatarURL = this.User?.avatarUrl
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
                    AvatarURL = this.Bot?.avatarUrl
                };
            }
        }

        public bool IsLive
        {
            get
            {
                bool? isLive = this.User?.channel?.IsLive;
                return isLive.GetValueOrDefault();
            }
        }

        public async Task<Result> ConnectUser()
        {
            Result<GlimeshPlatformService> result = await GlimeshPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.User = await this.UserConnection.GetCurrentUser();
                if (this.User == null)
                {
                    return new Result(MixItUp.Base.Resources.GlimeshFailedToGetUserData);
                }

                this.Channel = await this.UserConnection.GetChannelByID(this.User.channel.id);
                if (this.Channel == null)
                {
                    return new Result(MixItUp.Base.Resources.GlimeshFailedToGetUserData);
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
                    return new Result(MixItUp.Base.Resources.GlimeshFailedToGetBotData);
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
                        return new Result(MixItUp.Base.Resources.GlimeshFailedToGetUserData);
                    }

                    this.Channel = await this.UserConnection.GetChannelByID(this.User.channel.id);
                    if (this.Channel == null)
                    {
                        return new Result(MixItUp.Base.Resources.GlimeshFailedToGetUserData);
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
                                return new Result(MixItUp.Base.Resources.GlimeshFailedToGetBotData);
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: MixItUp.Base.Resources.GlimeshFailedToConnectBotAccount);
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].ClearUserData();
                    return userResult;
                }

                return userResult;
            }
            return new Result();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            await this.DisconnectBot(settings);

            await ServiceManager.Get<GlimeshChatEventService>().DisconnectUser();

            this.UserConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Glimesh, out var streamingPlatform))
            {
                streamingPlatform.ClearUserData();
            }
        }

        public async Task DisconnectBot(SettingsV3Model settings)
        {
            await ServiceManager.Get<GlimeshChatEventService>().DisconnectBot();

            this.BotConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Glimesh, out var streamingPlatform))
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
                    if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Glimesh))
                    {
                        if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID) && !string.Equals(this.UserID, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID))
                        {
                            Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.Username} - {this.UserID} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID}");
                            settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserOAuthToken.ResetToken();
                            return new Result(string.Format(MixItUp.Base.Resources.StreamingPlatformIncorrectAccount, StreamingPlatformTypeEnum.Glimesh));
                        }
                    }

                    List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                    platformServiceTasks.Add(ServiceManager.Get<GlimeshChatEventService>().ConnectUser());

                    await Task.WhenAll(platformServiceTasks);

                    if (platformServiceTasks.Any(c => !c.Result.Success))
                    {
                        string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                        return new Result(MixItUp.Base.Resources.GlimeshFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result(MixItUp.Base.Resources.GlimeshFailedToConnect +
                        Environment.NewLine + Environment.NewLine + MixItUp.Base.Resources.ErrorHeader + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null)
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
            await ServiceManager.Get<GlimeshChatEventService>().DisconnectUser();
        }

        public async Task CloseBot()
        {
            await ServiceManager.Get<GlimeshChatEventService>().DisconnectBot();
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
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].UserID = this.UserID;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].ChannelID = this.ChannelID;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Glimesh].BotID = this.BotID;
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
            var channel = await this.UserConnection.GetChannelByID(this.User.channel.id);
            if (channel != null)
            {
                this.Channel = channel;
            }

            if (this.Channel != null)
            {
                this.Moderators.Clear();
                foreach (ChannelModeratorModel moderator in this.Channel.moderators.Items)
                {
                    this.Moderators.Add(moderator.user.id);
                }
            }
        }

        public Task<string> GetTitle()
        {
            return Task.FromResult(this.Channel?.title);
        }

        public async Task<bool> SetTitle(string title)
        {
            await ServiceManager.Get<GlimeshSessionService>().UserConnection.UpdateStreamInfo(this.ChannelID, title);
            return true;
        }

        public Task<string> GetGame()
        {
            if (this.Channel?.subcategory != null)
            {
                if (GamingCategorySlug.Equals(this.Channel?.category?.slug))
                {
                    return Task.FromResult(this.Channel?.subcategory?.name);
                }
                else
                {
                    return Task.FromResult($"{this.Channel?.category?.name} - {this.Channel?.subcategory?.name}");
                }
            }
            else
            {
                return Task.FromResult(this.Channel?.category?.name);
            }
        }

        public Task<bool> SetGame(string gameName) { return Task.FromResult(false); }
    }
}
