using MixItUp.Base.Model;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.Base.ViewModels;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Twitch.Base.Models.NewAPI.Channels;
using Twitch.Base.Models.NewAPI.Games;
using Twitch.Base.Models.NewAPI.Streams;
using Twitch.Base.Models.NewAPI.Tags;
using Twitch.Base.Models.NewAPI.Users;

namespace MixItUp.Base.Services.Twitch
{
    public class TwitchSessionService : IStreamingPlatformSessionService
    {
        public TwitchPlatformService UserConnection { get; private set; }
        public TwitchPlatformService BotConnection { get; private set; }
        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel User { get; set; }
        public UserModel Bot { get; set; }
        public ChannelInformationModel Channel { get; set; }
        public StreamModel Stream { get; set; }
        public IEnumerable<TwitchTagModel> StreamTags { get; private set; }

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return this.User?.id; } }
        public string Username { get { return this.User?.login; } }
        public string BotID { get { return this.Bot?.id; } }
        public string Botname { get { return this.Bot?.login; } }
        public string ChannelID { get { return this.User?.id; } }
        public string ChannelLink { get { return string.Format("twitch.tv/{0}", this.Username?.ToLower()); } }

        public StreamingPlatformAccountModel UserAccount
        {
            get
            {
                return new StreamingPlatformAccountModel()
                {
                    ID = this.UserID,
                    Username = this.Username,
                    AvatarURL = this.User?.profile_image_url
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
                    AvatarURL = this.Bot?.profile_image_url
                };
            }
        }

        public bool IsLive
        {
            get
            {
                return this.Stream != null;
            }
        }

        public async Task<Result> ConnectUser()
        {
            Result<TwitchPlatformService> result = await TwitchPlatformService.ConnectUser();
            if (result.Success)
            {
                this.UserConnection = result.Value;
                this.User = await this.UserConnection.GetNewAPICurrentUser();
                if (this.User == null)
                {
                    return new Result(MixItUp.Base.Resources.TwitchFailedToGetUserData);
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
                this.Bot = await this.BotConnection.GetNewAPICurrentUser();
                if (this.Bot == null)
                {
                    return new Result(MixItUp.Base.Resources.TwitchFailedToGetBotData);
                }

                if (ServiceManager.Get<TwitchChatService>().IsUserConnected)
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
                    this.User = await this.UserConnection.GetNewAPICurrentUser();
                    if (this.User == null)
                    {
                        return new Result(MixItUp.Base.Resources.TwitchFailedToGetUserData);
                    }

                    if (settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken != null)
                    {
                        twitchResult = await TwitchPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken);
                        if (twitchResult.Success)
                        {
                            this.BotConnection = twitchResult.Value;
                            this.Bot = await this.BotConnection.GetNewAPICurrentUser();
                            if (this.Bot == null)
                            {
                                return new Result(MixItUp.Base.Resources.TwitchFailedToGetBotData);
                            }
                        }
                        else
                        {

                            return new Result(success: true, message: MixItUp.Base.Resources.TwitchFailedToConnectBotAccount);
                        }
                    }
                }
                else
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].ClearUserData();
                    return userResult;
                }

                return userResult;
            }
            return new Result();
        }

        public async Task DisconnectUser(SettingsV3Model settings)
        {
            await this.DisconnectBot(settings);

            await ServiceManager.Get<TwitchChatService>().DisconnectUser();
            await ServiceManager.Get<TwitchEventService>().Disconnect();

            this.UserConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Twitch, out var streamingPlatform))
            {
                streamingPlatform.ClearUserData();
            }
        }

        public async Task DisconnectBot(SettingsV3Model settings)
        {
            await ServiceManager.Get<TwitchChatService>().DisconnectBot();

            this.BotConnection = null;

            if (settings.StreamingPlatformAuthentications.TryGetValue(StreamingPlatformTypeEnum.Twitch, out var streamingPlatform))
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
                    UserModel twitchChannelNew = await this.UserConnection.GetNewAPICurrentUser();
                    if (twitchChannelNew != null)
                    {
                        this.User = twitchChannelNew;
                        this.Stream = await this.UserConnection.GetStream(this.User);

                        IEnumerable<ChannelEditorUserModel> channelEditors = await this.UserConnection.GetChannelEditors(this.User);
                        if (channelEditors != null)
                        {
                            foreach (ChannelEditorUserModel channelEditor in channelEditors)
                            {
                                this.ChannelEditors.Add(channelEditor.user_id);
                            }
                        }

                        if (settings.StreamingPlatformAuthentications.ContainsKey(StreamingPlatformTypeEnum.Twitch))
                        {
                            if (!string.IsNullOrEmpty(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID) && !string.Equals(this.UserID, settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID))
                            {
                                Logger.Log(LogLevel.Error, $"Signed in account does not match settings account: {this.Username} - {this.UserID} - {settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID}");
                                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.ResetToken();
                                return new Result(string.Format(MixItUp.Base.Resources.StreamingPlatformIncorrectAccount, StreamingPlatformTypeEnum.Twitch));
                            }
                        }

                        List<Task<Result>> platformServiceTasks = new List<Task<Result>>();
                        platformServiceTasks.Add(ServiceManager.Get<TwitchChatService>().ConnectUser());
                        platformServiceTasks.Add(ServiceManager.Get<TwitchEventService>().Connect());
                        platformServiceTasks.Add(ServiceManager.Get<TwitchSessionService>().SetStreamTagsCache());

                        await Task.WhenAll(platformServiceTasks);

                        if (platformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result(MixItUp.Base.Resources.TwitchFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
                        }

                        await ServiceManager.Get<TwitchChatService>().Initialize();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    return new Result(MixItUp.Base.Resources.TwitchFailedToConnect +
                        Environment.NewLine + Environment.NewLine + MixItUp.Base.Resources.ErrorHeader + ex.Message);
                }
            }
            return new Result();
        }

        public async Task<Result> InitializeBot(SettingsV3Model settings)
        {
            if (this.BotConnection != null)
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
            await ServiceManager.Get<TwitchChatService>().DisconnectUser();

            await ServiceManager.Get<TwitchEventService>().Disconnect();
        }

        public async Task CloseBot()
        {
            await ServiceManager.Get<TwitchChatService>().DisconnectBot();
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
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserID = this.UserID;
                settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].ChannelID = this.ChannelID;

                if (this.BotConnection != null)
                {
                    settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken = this.BotConnection.Connection.GetOAuthTokenCopy();
                    if (this.Bot != null)
                    {
                        settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotID = this.BotID;
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
                    this.User = twitchUserNewAPI;
                }
            }

            if (this.BotConnection != null)
            {
                UserModel botUserNewAPI = await this.BotConnection.GetNewAPICurrentUser();
                if (botUserNewAPI != null)
                {
                    this.Bot = botUserNewAPI;
                }
            }
        }

        public async Task RefreshChannel()
        {
            if (this.UserConnection != null && this.User != null)
            {
                this.Channel = await this.UserConnection.GetChannelInformation(this.User);

                this.Stream = await this.UserConnection.GetStream(this.User);
            }
        }

        public Task<string> GetTitle()
        {
            return Task.FromResult(this.Channel?.title);
        }

        public async Task<bool> SetTitle(string title)
        {
            return await this.UserConnection.UpdateChannelInformation(this.User, title: title);
        }

        public Task<string> GetGame()
        {
            return Task.FromResult(this.Channel?.game_name);
        }

        public async Task<bool> SetGame(string gameName)
        {
            IEnumerable<GameModel> games = await this.UserConnection.GetNewAPIGamesByName(gameName);
            if (games != null && games.Count() > 0)
            {
                GameModel game = games.FirstOrDefault(g => g.name.ToLower().Equals(gameName));
                if (game == null)
                {
                    game = games.First();
                }

                if (this.IsConnected && game != null)
                {
                    await this.UserConnection.UpdateChannelInformation(this.User, gameID: game.id);
                    return true;
                }
            }
            return false;
        }

        public async Task<Result> SetStreamTagsCache()
        {
            SortedList<string, TwitchTagModel> tags = new SortedList<string, TwitchTagModel>();
            foreach (TagModel tag in await ServiceManager.Get<TwitchSessionService>().UserConnection.GetStreamTags())
            {
                if (!tag.is_auto)
                {
                    TwitchTagModel tagModel = new TwitchTagModel(tag);
                    if (!string.IsNullOrEmpty(tagModel.Name) && !tags.ContainsKey(tagModel.Name))
                    {
                        tags.Add(tagModel.Name, tagModel);
                    }
                }
            }
            this.StreamTags = tags.Values.ToList();

            return new Result();
        }
    }

    public static class TwitchNewAPIUserModelExtensions
    {
        public static bool IsAffiliate(this UserModel twitchUser)
        {
            return string.Equals(twitchUser.broadcaster_type, "affiliate", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPartner(this UserModel twitchUser)
        {
            return string.Equals(twitchUser.broadcaster_type, "partner", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsStaff(this UserModel twitchUser)
        {
            return string.Equals(twitchUser.type, "staff", StringComparison.OrdinalIgnoreCase) || string.Equals(twitchUser.type, "admin", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsGlobalMod(this UserModel twitchUser)
        {
            return string.Equals(twitchUser.type, "global_mod", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class TwitchTagModel
    {
        public TwitchTagModel(TagModel tag)
        {
            this.Tag = tag;

            string languageLocale = Languages.GetLanguageLocale().ToLower();
            if (this.Tag.localization_names.ContainsKey(languageLocale))
            {
                this.Name = (string)this.Tag.localization_names[languageLocale];
            }
            else
            {
                languageLocale = Languages.GetLanguageLocale(LanguageOptions.Default).ToLower();
                if (this.Tag.localization_names.ContainsKey(languageLocale))
                {
                    this.Name = (string)this.Tag.localization_names[languageLocale];
                }
            }
        }

        public TagModel Tag { get; private set; }

        public string ID { get { return this.Tag.tag_id; } }

        public string Name { get; private set; }

        public bool IsDeletable { get { return !this.Tag.is_auto; } }
    }
}
