using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Settings;
using MixItUp.Base.Model.Twitch.Ads;
using MixItUp.Base.Model.Twitch.Channels;
using MixItUp.Base.Model.Twitch.Games;
using MixItUp.Base.Model.Twitch.Streams;
using MixItUp.Base.Model.Twitch.User;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.Twitch
{
    [Obsolete]
    public class TwitchSessionService : IStreamingPlatformSessionService
    {
        public TwitchPlatformService UserConnection { get; private set; }
        public TwitchPlatformService BotConnection { get; private set; }
        public HashSet<string> ChannelEditors { get; private set; } = new HashSet<string>();
        public UserModel User { get; set; }
        public UserModel Bot { get; set; }
        public ChannelInformationModel Channel { get; set; }
        public StreamModel Stream { get; set; }
        public List<ChannelContentClassificationLabelModel> ContentClassificationLabels { get; private set; } = new List<ChannelContentClassificationLabelModel>();
        public AdScheduleModel AdSchedule { get; set; }
        public DateTimeOffset NextAdTimestamp { get; set; } = DateTimeOffset.MinValue;

        public bool IsConnected { get { return this.UserConnection != null; } }
        public bool IsBotConnected { get { return this.BotConnection != null; } }

        public string UserID { get { return this.User?.id; } }
        public string Username { get { return this.User?.login; } }
        public string BotID { get { return this.Bot?.id; } }
        public string Botname { get { return this.Bot?.login; } }
        public string ChannelID { get { return this.User?.id; } }
        public string ChannelLink { get { return string.Format("twitch.tv/{0}", this.Username?.ToLower()); } }

        private StreamModel streamCache;

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
                return this.Stream != null || ServiceManager.Get<TwitchEventSubService>().StreamLiveStatus;
            }
        }

        public int ViewerCount { get { return (int)(this.Stream?.viewer_count ?? 0); } }

        public DateTimeOffset StreamStart
        {
            get
            {
                if (this.IsLive)
                {
                    return TwitchPlatformService.GetTwitchDateTime(this.Stream?.started_at);
                }
                return DateTimeOffset.MinValue;
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

                // If scopes don't match, re-auth the token
                if (string.Equals(string.Join(",", TwitchPlatformService.StreamerScopes), settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].UserOAuthToken.ScopeList, StringComparison.OrdinalIgnoreCase))
                {
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
                        Result<TwitchPlatformService> twitchResult = await TwitchPlatformService.Connect(settings.StreamingPlatformAuthentications[StreamingPlatformTypeEnum.Twitch].BotOAuthToken);
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
            await ServiceManager.Get<TwitchEventSubService>().Disconnect(true);
            await ServiceManager.Get<TwitchPubSubService>().Disconnect();

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

                        IEnumerable<ChannelContentClassificationLabelModel> contentClassificationLabels = await this.UserConnection.GetContentClassificationLabels(Languages.GetLanguageLocale());
                        if (contentClassificationLabels == null || contentClassificationLabels.Count() == 0)
                        {
                            contentClassificationLabels = await this.UserConnection.GetContentClassificationLabels();
                        }

                        if (contentClassificationLabels != null)
                        {
                            this.ContentClassificationLabels.AddRange(contentClassificationLabels.Where(l => !string.Equals(l.id, "MatureGame")));
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
                        platformServiceTasks.Add(ServiceManager.Get<TwitchPubSubService>().Connect());

                        await Task.WhenAll(platformServiceTasks);

                        if (platformServiceTasks.Any(c => !c.Result.Success))
                        {
                            string errors = string.Join(Environment.NewLine, platformServiceTasks.Where(c => !c.Result.Success).Select(c => c.Result.Message));
                            return new Result(MixItUp.Base.Resources.TwitchFailedToConnectHeader + Environment.NewLine + Environment.NewLine + errors);
                        }

                        // Let's start this in the background and not block
                        await ServiceManager.Get<TwitchEventSubService>().TryConnect();

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

            await ServiceManager.Get<TwitchEventSubService>().Disconnect(true);
            await ServiceManager.Get<TwitchPubSubService>().Disconnect();
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

                StreamModel newStream = await this.UserConnection.GetStream(this.User);
                if (newStream != null)
                {
                    this.Stream = this.streamCache = newStream;
                }
                else
                {
                    this.Stream = this.streamCache;
                    this.streamCache = null;
                }

                if (this.Stream != null)
                {
                    if (this.Stream.title != null && !string.Equals(newStream?.title, this.Stream?.title, StringComparison.OrdinalIgnoreCase))
                    {
                        ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.StreamUpdated, platform: StreamingPlatformTypeEnum.Twitch, description: this.Stream?.title);
                    }
                    if (this.Stream.game_name != null && !string.Equals(newStream?.game_name, this.Stream?.game_name, StringComparison.OrdinalIgnoreCase))
                    {
                        ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.StreamUpdated, platform: StreamingPlatformTypeEnum.Twitch, description: this.Stream?.game_name);
                    }
                }

                AdScheduleModel adSchedule = await this.UserConnection.GetAdSchedule(this.User);
                if (adSchedule != null)
                {
                    this.AdSchedule = adSchedule;
                }

                if (this.AdSchedule != null)
                {
                    DateTimeOffset nextAd = this.AdSchedule.NextAdTimestamp();
                    if (nextAd > this.NextAdTimestamp)
                    {
                        int nextAdMinutes = this.AdSchedule.NextAdMinutesFromNow();
                        if (nextAdMinutes <= ChannelSession.Settings.TwitchUpcomingAdCommandTriggerAmount && nextAdMinutes > 0)
                        {
                            this.NextAdTimestamp = nextAd;

                            Dictionary<string, string> eventCommandSpecialIdentifiers = new Dictionary<string, string>();
                            eventCommandSpecialIdentifiers["adsnoozecount"] = this.AdSchedule.snooze_count.ToString();
                            eventCommandSpecialIdentifiers["adnextduration"] = this.AdSchedule.duration.ToString();
                            eventCommandSpecialIdentifiers["adnextminutes"] = nextAdMinutes.ToString();
                            eventCommandSpecialIdentifiers["adnexttime"] = nextAd.ToFriendlyTimeString();
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.TwitchChannelAdUpcoming, new CommandParametersModel(ChannelSession.User, StreamingPlatformTypeEnum.Twitch, eventCommandSpecialIdentifiers));
                        }
                    }
                }

                foreach (var key in ChannelSession.Settings.TwitchVIPAutomaticRemovals.Keys.ToList())
                {
                    if (ChannelSession.Settings.TwitchVIPAutomaticRemovals.TryGetValue(key, out DateTimeOffset removalTime) && removalTime < DateTimeOffset.Now)
                    {
                        ChannelSession.Settings.TwitchVIPAutomaticRemovals.Remove(key);

                        await ServiceManager.Get<TwitchSessionService>().UserConnection.UnVIPUser(ServiceManager.Get<TwitchSessionService>().User, new UserModel() { id = key });

                        UserV2ViewModel user = ServiceManager.Get<UserService>().GetActiveUserByPlatform(StreamingPlatformTypeEnum.Twitch, platformID: key);
                        if (user != null)
                        {
                            user.Roles.Remove(UserRoleEnum.TwitchVIP);
                        }
                    }
                }
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
    }
}
