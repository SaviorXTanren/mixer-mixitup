using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.Glimesh;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class UserService
    {
        public static readonly HashSet<string> SpecialUserAccounts = new HashSet<string>() { "boomtvmod", "streamjar", "pretzelrocks", "scottybot", "streamlabs", "streamelements", "nightbot", "deepbot", "moobot", "coebot", "wizebot", "phantombot", "stay_hydrated_bot", "stayhealthybot", "anotherttvviewer", "commanderroot", "lurxx", "thecommandergroot", "moobot", "thelurxxer", "twitchprimereminder", "communityshowcase", "banmonitor", "wizebot" };

        public static string SanitizeUsername(string username) { return !string.IsNullOrEmpty(username) ? username.ToLower().Replace("@", "").Trim() : string.Empty; }

        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUserIDLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();
        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUsernameLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();

        private bool fullUserDataLoadOccurred = false;

        public UserService()
        {
            foreach (StreamingPlatformTypeEnum platform in StreamingPlatforms.SupportedPlatforms)
            {
                this.platformUserIDLookups[platform] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                this.platformUsernameLookups[platform] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private Dictionary<Guid, UserV2ViewModel> activeUsers = new Dictionary<Guid, UserV2ViewModel>();

        public async Task AddOrUpdateActiveUser(UserV2ViewModel user)
        {
            if (user == null)
            {
                return;
            }

            bool newUser = !this.activeUsers.ContainsKey(user.ID);

            this.activeUsers[user.ID] = user;

            this.SetUserData(user.Model);

            // TODO
            // Add IgnoreForQueries logic

            if (newUser)
            {
                if (user.OnlineViewingMinutes == 0)
                {
                    // TODO
                    //await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserFirstJoin, new CommandParametersModel(user));
                }

                // TODO
                //CommandParametersModel parameters = new CommandParametersModel(user);
                //if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.ChatUserJoined, parameters))
                //{
                //    user.UpdateLastActivity();
                //    user.Model.TotalStreamsWatched++;
                //    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserJoined, parameters);
                //}
            }
        }

        public async Task RemoveActiveUser(StreamingPlatformTypeEnum platform, string platformUsername)
        {
            if (this.platformUsernameLookups.ContainsKey(platform) && this.platformUsernameLookups[platform].TryGetValue(platformUsername, out Guid id) && this.activeUsers.TryGetValue(id, out UserV2ViewModel user))
            {
                await this.RemoveActiveUser(user);
            }
        }

        public async Task RemoveActiveUser(UserV2ViewModel user)
        {
            if (user != null && this.activeUsers.ContainsKey(user.ID))
            {
                this.activeUsers.Remove(user.ID);

                // TODO
                //await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserLeft, new CommandParametersModel(user));
            }
        }

        public IEnumerable<UserV2ViewModel> GetActiveUsers() { return this.activeUsers.Values.ToList(); }

        public async Task<UserV2ViewModel> GetUserByID(Guid id)
        {
            if (this.activeUsers.TryGetValue(id, out UserV2ViewModel user))
            {
                return user;
            }

            IEnumerable<UserV2Model> results = await ChannelSession.Settings.LoadUserV2Data("SELECT * FROM Users WHERE ID = @ID", new Dictionary<string, object>() { { "ID", id } });
            if (results.Count() > 0)
            {
                this.SetUserData(results.First());
                return new UserV2ViewModel(ChannelSession.Settings.DefaultStreamingPlatform, results.First());
            }

            return null;
        }

        public async Task<UserV2ViewModel> GetUserByPlatformID(StreamingPlatformTypeEnum platform, string platformID, bool performPlatformSearch = false)
        {
            UserV2ViewModel user = null;

            if (string.IsNullOrEmpty(platformID))
            {
                return user;
            }

            if (platform == StreamingPlatformTypeEnum.None)
            {
                return user;
            }

            if (this.platformUserIDLookups.ContainsKey(platform) && this.platformUserIDLookups[platform].TryGetValue(platformID, out Guid id))
            {
                user = await this.GetUserByID(id);
                if (user != null)
                {
                    return user;
                }
            }

            if (performPlatformSearch)
            {
                UserPlatformV2ModelBase platformModel = null;
                if (platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().UserConnection != null)
                {
                    var twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(platformID);
                    if (twitchUser != null)
                    {
                        platformModel = new TwitchUserPlatformV2Model(twitchUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSessionService>().UserConnection != null)
                {
                    var youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByID(platformID);
                    if (youtubeUser != null)
                    {
                        platformModel = new YouTubeUserPlatformV2Model(youtubeUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().UserConnection != null)
                {
                    var glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByID(platformID);
                    if (glimeshUser != null)
                    {
                        platformModel = new GlimeshUserPlatformV2Model(glimeshUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().UserConnection != null)
                {
                    throw new InvalidOperationException("Trovo does not support user look-up by user ID");
                }

                return this.CreateUser(platformModel);
            }

            return null;
        }

        public async Task<UserV2ViewModel> GetUserByPlatformUsername(StreamingPlatformTypeEnum platform, string platformUsername, bool performPlatformSearch = false)
        {
            UserV2ViewModel user = null;

            if (string.IsNullOrEmpty(platformUsername))
            {
                return user;
            }

            if (platform == StreamingPlatformTypeEnum.None)
            {
                foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.SupportedPlatforms)
                {
                    user = await this.GetUserByPlatformUsername(p, platformUsername, performPlatformSearch);
                    if (user != null)
                    {
                        return user;
                    }
                }
                return null;
            }

            if (this.platformUsernameLookups.ContainsKey(platform) && this.platformUsernameLookups[platform].TryGetValue(platformUsername, out Guid id))
            {
                user = await this.GetUserByID(id);
                if (user != null)
                {
                    return user;
                }
            }

            if (performPlatformSearch)
            {
                UserPlatformV2ModelBase platformModel = null;
                if (platform == StreamingPlatformTypeEnum.Twitch && ServiceManager.Get<TwitchSessionService>().UserConnection != null)
                {
                    var twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(platformUsername);
                    if (twitchUser != null)
                    {
                        platformModel = new TwitchUserPlatformV2Model(twitchUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.YouTube && ServiceManager.Get<YouTubeSessionService>().UserConnection != null)
                {
                    var youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByUsername(platformUsername);
                    if (youtubeUser != null)
                    {
                        platformModel = new YouTubeUserPlatformV2Model(youtubeUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Glimesh && ServiceManager.Get<GlimeshSessionService>().UserConnection != null)
                {
                    var glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByName(platformUsername);
                    if (glimeshUser != null)
                    {
                        platformModel = new GlimeshUserPlatformV2Model(glimeshUser);
                    }
                }
                else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().UserConnection != null)
                {
                    var trovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(platformUsername);
                    if (trovoUser != null)
                    {
                        platformModel = new TrovoUserPlatformV2Model(trovoUser);
                    }
                }

                return this.CreateUser(platformModel);
            }

            return null;
        }

        public UserV2ViewModel CreateUser(UserPlatformV2ModelBase platformModel)
        {
            if (platformModel != null)
            {
                UserV2Model userModel = new UserV2Model();
                userModel.AddPlatformData(platformModel);
                UserV2ViewModel user = new UserV2ViewModel(platformModel.Platform, userModel);

                this.SetUserData(userModel, newData: true);
                return user;
            }
            return null;
        }

        private void SetUserData(UserV2Model userData, bool newData = false)
        {
            if (userData != null && userData.GetPlatforms().Count() > 0)
            {
                lock (ChannelSession.Settings.Users)
                {
                    if (!ChannelSession.Settings.Users.ContainsKey(userData.ID))
                    {
                        ChannelSession.Settings.Users[userData.ID] = userData;
                        ChannelSession.Settings.Users.ManualValueChanged(userData.ID);
                    }

                    foreach (StreamingPlatformTypeEnum platform in userData.GetPlatforms())
                    {
                        UserPlatformV2ModelBase platformModel = userData.GetPlatformData<UserPlatformV2ModelBase>(platform);
                        this.platformUserIDLookups[platform][platformModel.ID] = userData.ID;
                        this.platformUsernameLookups[platform][platformModel.Username] = userData.ID;
                    }
                }
            }
        }
    }
}
