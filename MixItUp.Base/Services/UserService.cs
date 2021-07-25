using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
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
using GlimeshBase = Glimesh.Base;
using TrovoBase = Trovo.Base;

namespace MixItUp.Base.Services
{
    public class UserService
    {
        public static readonly HashSet<string> SpecialUserAccounts = new HashSet<string>() { "boomtvmod", "streamjar", "pretzelrocks", "scottybot", "streamlabs", "streamelements", "nightbot", "deepbot", "moobot", "coebot", "wizebot", "phantombot", "stay_hydrated_bot", "stayhealthybot", "anotherttvviewer", "commanderroot", "lurxx", "thecommandergroot", "moobot", "thelurxxer", "twitchprimereminder", "communityshowcase", "banmonitor", "wizebot" };

        public static string SanitizeUsername(string username) { return !string.IsNullOrEmpty(username) ? username.ToLower().Replace("@", "").Trim() : string.Empty; }

        private LockedDictionary<Guid, UserViewModel> activeUsers = new LockedDictionary<Guid, UserViewModel>();

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

        public UserViewModel GetActiveUserByID(Guid id)
        {
            if (this.activeUsers.ContainsKey(id))
            {
                return this.activeUsers[id];
            }
            return null;
        }

        public UserViewModel GetActiveUserByUsername(string username, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None)
        {
            if (!string.IsNullOrEmpty(username))
            {
                username = UserService.SanitizeUsername(username);
                if (platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
                {
                    foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.SupportedPlatforms)
                    {
                        UserViewModel user = this.GetActiveUserByUsername(username, p);
                        if (user != null)
                        {
                            return user;
                        }
                    }
                }
                else
                {
                    if (this.platformUsernameLookups[platform].ContainsKey(username))
                    {
                        return this.GetActiveUserByID(this.platformUsernameLookups[platform][username]);
                    }
                }
            }
            return null;
        }

        public UserViewModel GetActiveUserByPlatformID(StreamingPlatformTypeEnum platform, string id)
        {
            if (!string.IsNullOrEmpty(id) && this.platformUserIDLookups[platform].ContainsKey(id))
            {
                return this.GetActiveUserByID(this.platformUserIDLookups[platform][id]);
            }
            return null;
        }

        public async Task AddOrUpdateActiveUser(UserViewModel user)
        {
            if (!user.IsAnonymous)
            {
                bool newUser = !this.activeUsers.ContainsKey(user.ID);

                this.activeUsers[user.ID] = user;

                if (!string.IsNullOrEmpty(user.TwitchID))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Twitch][user.TwitchID] = user.ID;
                }
                if (!string.IsNullOrEmpty(user.TwitchUsername))
                {
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Twitch][user.TwitchUsername] = user.ID;
                }

                if (!string.IsNullOrEmpty(user.YouTubeID) && !string.IsNullOrEmpty(user.YouTubeUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.YouTube][user.YouTubeID] = user.ID;
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.YouTube][user.YouTubeUsername] = user.ID;
                }

                if (!string.IsNullOrEmpty(user.GlimeshID) && !string.IsNullOrEmpty(user.GlimeshUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Glimesh][user.GlimeshID] = user.ID;
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Glimesh][user.GlimeshUsername] = user.ID;
                }

                if (!string.IsNullOrEmpty(user.TrovoID) && !string.IsNullOrEmpty(user.TrovoUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Trovo][user.TrovoID] = user.ID;
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Trovo][user.TrovoUsername] = user.ID;
                }

                if (UserService.SpecialUserAccounts.Contains(user.Username.ToLower()))
                {
                    user.IgnoreForQueries = true;
                }
                else if (ChannelSession.GetCurrentUser().ID.Equals(user.ID))
                {
                    user.IgnoreForQueries = true;
                }
                else if (ServiceManager.Get<TwitchSessionService>().BotNewAPI != null && ServiceManager.Get<TwitchSessionService>().BotNewAPI.id.Equals(user.TwitchID))
                {
                    user.IgnoreForQueries = true;
                }
                // TODO
                else
                {
                    user.IgnoreForQueries = false;

                    if (newUser)
                    {
                        if (user.Data.ViewingMinutes == 0)
                        {
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserFirstJoin, new CommandParametersModel(user));
                        }

                        CommandParametersModel parameters = new CommandParametersModel(user);
                        if (ServiceManager.Get<EventService>().CanPerformEvent(EventTypeEnum.ChatUserJoined, parameters))
                        {
                            user.LastSeen = DateTimeOffset.Now;
                            user.Data.TotalStreamsWatched++;
                            await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserJoined, parameters);
                        }
                    }
                }
            }
        }

        public async Task<UserViewModel> RemoveActiveUserByUsername(StreamingPlatformTypeEnum platform, string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                username = UserService.SanitizeUsername(username);
                if (this.platformUsernameLookups[platform].ContainsKey(username))
                {
                    UserViewModel user = this.GetActiveUserByID(this.platformUsernameLookups[platform][username]);
                    await this.RemoveActiveUser(user);
                    return user;
                }
            }
            return null;
        }

        public async Task<UserViewModel> RemoveActiveUserByID(Guid id)
        {
            if (this.activeUsers.ContainsKey(id))
            {
                UserViewModel user = this.activeUsers[id];
                await this.RemoveActiveUser(user);
                return user;
            }
            return null;
        }

        private async Task RemoveActiveUser(UserViewModel user)
        {
            if (user != null)
            {
                this.activeUsers.Remove(user.ID);

                if (!string.IsNullOrEmpty(user.TwitchID))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Twitch].Remove(user.TwitchID);
                }
                if (!string.IsNullOrEmpty(user.TwitchUsername))
                {
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Twitch].Remove(user.TwitchUsername);
                }

                if (!string.IsNullOrEmpty(user.YouTubeID) && !string.IsNullOrEmpty(user.YouTubeUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.YouTube].Remove(user.YouTubeID);
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.YouTube].Remove(user.YouTubeUsername);
                }

                if (!string.IsNullOrEmpty(user.GlimeshID) && !string.IsNullOrEmpty(user.GlimeshUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Glimesh].Remove(user.GlimeshID);
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Glimesh].Remove(user.GlimeshUsername);
                }

                if (!string.IsNullOrEmpty(user.TrovoID) && !string.IsNullOrEmpty(user.TrovoUsername))
                {
                    this.platformUserIDLookups[StreamingPlatformTypeEnum.Trovo].Remove(user.TrovoID);
                    this.platformUsernameLookups[StreamingPlatformTypeEnum.Trovo].Remove(user.TrovoUsername);
                }

                await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserLeft, new CommandParametersModel(user));
            }
        }

        public bool IsUserActive(Guid id) { return this.activeUsers.ContainsKey(id); }

        public IEnumerable<UserViewModel> GetAllActiveUsers() { return this.activeUsers.Values.ToList(); }

        public IEnumerable<UserViewModel> GetAllWorkableActiveUsers()
        {
            IEnumerable<UserViewModel> results = this.GetAllActiveUsers();
            return results.Where(u => !u.IgnoreForQueries);
        }

        public IEnumerable<UserViewModel> GetAllWorkableActiveUsers(StreamingPlatformTypeEnum platform)
        {
            IEnumerable<UserViewModel> results = this.GetAllWorkableActiveUsers();
            return results.Where(u => platform.HasFlag(u.Platform));
        }

        public UserViewModel GetRandomActiveUser(CommandParametersModel parameters, bool excludeCurrencyRankExempt = false)
        {
            List<UserViewModel> results = new List<UserViewModel>(this.GetAllWorkableActiveUsers(parameters.Platform));
            results.Remove(parameters.User);
            if (excludeCurrencyRankExempt) { results.RemoveAll(u => u.Data.IsCurrencyRankExempt); }
            return results.Random();
        }

        public int ActiveUserCount() { return this.activeUsers.Count; }

        public async Task<UserDataModel> GetUserDataByID(Guid id)
        {
            UserDataModel userData = null;

            if (id == Guid.Empty)
            {
                return userData;
            }

            if (ChannelSession.Settings.UserData.TryGetValue(id, out userData))
            {
                return userData;
            }

            IEnumerable<UserDataModel> results = await ChannelSession.Settings.LoadUserData("SELECT * FROM Users WHERE ID = @ID", new Dictionary<string, object>() { { "ID", id } });
            if (results.Count() > 0)
            {
                userData = results.First();
                this.SetUserData(userData);
            }
            return userData;
        }

        public async Task<UserDataModel> GetUserDataByPlatformID(StreamingPlatformTypeEnum platform, string platformID)
        {
            UserDataModel userData = null;

            if (string.IsNullOrEmpty(platformID) || platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                return userData;
            }

            if (this.platformUserIDLookups[platform].TryGetValue(platformID, out Guid id) && ChannelSession.Settings.UserData.TryGetValue(id, out userData))
            {
                return userData;
            }

            string columnName = null;
            switch (platform)
            {
                case StreamingPlatformTypeEnum.Twitch: columnName = "TwitchID"; break;
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                IEnumerable<UserDataModel> results = await ChannelSession.Settings.LoadUserData("SELECT * FROM Users WHERE " + columnName + " = @PlatformID", new Dictionary<string, object>() { { "PlatformID", platformID } });
                if (results.Count() > 0)
                {
                    userData = results.First();
                    this.SetUserData(userData);
                }
            }
            return userData;
        }

        public async Task<UserDataModel> GetUserDataByPlatformUsername(StreamingPlatformTypeEnum platform, string platformUsername)
        {
            UserDataModel userData = null;

            if (string.IsNullOrEmpty(platformUsername) || platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                return null;
            }

            if (this.platformUsernameLookups[platform].TryGetValue(platformUsername, out Guid id) && ChannelSession.Settings.UserData.TryGetValue(id, out userData))
            {
                return userData;
            }

            string columnName = null;
            switch (platform)
            {
                case StreamingPlatformTypeEnum.Twitch: columnName = "TwitchUsername"; break;
            }

            if (!string.IsNullOrEmpty(columnName))
            {
                IEnumerable<UserDataModel> results = await ChannelSession.Settings.LoadUserData("SELECT * FROM Users WHERE " + columnName + " LIKE @PlatformUsername", new Dictionary<string, object>() { { "PlatformUsername", platformUsername } });
                if (results.Count() > 0)
                {
                    userData = results.First();
                    this.SetUserData(userData);
                }
            }
            return userData;
        }

        public async Task LoadAllUserData()
        {
            if (!this.fullUserDataLoadOccurred)
            {
                this.fullUserDataLoadOccurred = true;

                foreach (UserDataModel userData in await ChannelSession.Settings.LoadUserData("SELECT * FROM Users", new Dictionary<string, object>()))
                {
                    this.SetUserData(userData);
                }
            }
        }

        public void SetUserData(UserDataModel userData, bool newData = false)
        {
            if (userData != null && userData.Platforms.Count() > 0)
            {
                lock (ChannelSession.Settings.UserData)
                {
                    if (!ChannelSession.Settings.UserData.ContainsKey(userData.ID))
                    {
                        ChannelSession.Settings.UserData[userData.ID] = userData;
                        if (newData)
                        {
                            ChannelSession.Settings.UserData.ManualValueChanged(userData.ID);
                        }
                        else
                        {
                            ChannelSession.Settings.UserData.ClearTracking(userData.ID);
                        }
                    }

                    if (!string.IsNullOrEmpty(userData.TwitchID))
                    {
                        this.platformUserIDLookups[StreamingPlatformTypeEnum.Twitch][userData.TwitchID] = userData.ID;
                    }
                    if (!string.IsNullOrEmpty(userData.TwitchUsername))
                    {
                        this.platformUsernameLookups[StreamingPlatformTypeEnum.Twitch][userData.TwitchUsername] = userData.ID;
                    }

                    if (!string.IsNullOrEmpty(userData.YouTubeID))
                    {
                        this.platformUserIDLookups[StreamingPlatformTypeEnum.YouTube][userData.YouTubeID] = userData.ID;
                    }
                    if (!string.IsNullOrEmpty(userData.YouTubeUsername))
                    {
                        this.platformUsernameLookups[StreamingPlatformTypeEnum.YouTube][userData.YouTubeUsername] = userData.ID;
                    }

                    if (!string.IsNullOrEmpty(userData.TrovoID))
                    {
                        this.platformUserIDLookups[StreamingPlatformTypeEnum.Trovo][userData.TrovoID] = userData.ID;
                    }
                    if (!string.IsNullOrEmpty(userData.TrovoUsername))
                    {
                        this.platformUsernameLookups[StreamingPlatformTypeEnum.Trovo][userData.TrovoUsername] = userData.ID;
                    }

                    if (!string.IsNullOrEmpty(userData.GlimeshID))
                    {
                        this.platformUserIDLookups[StreamingPlatformTypeEnum.Glimesh][userData.GlimeshID] = userData.ID;
                    }
                    if (!string.IsNullOrEmpty(userData.GlimeshUsername))
                    {
                        this.platformUsernameLookups[StreamingPlatformTypeEnum.Glimesh][userData.GlimeshUsername] = userData.ID;
                    }
                }
            }
        }

        public void ClearMixerUserData()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            foreach (UserDataModel userData in ChannelSession.Settings.UserData.Values.ToList())
            {
                if (userData.MixerID > 0)
                {
                    ChannelSession.Settings.UserData.Remove(userData.ID);
                }
            }
#pragma warning restore CS0612 // Type or member is obsolete
        }

        public async Task ClearAllUserData()
        {
            ChannelSession.Settings.UserData.Clear();
            await ServiceManager.Get<IDatabaseService>().Write(ChannelSession.Settings.DatabaseFilePath, "DELETE FROM Users");
        }

        public async Task<UserViewModel> GetUserFullSearch(StreamingPlatformTypeEnum platform, string userID = null, string username = null)
        {
            UserViewModel user = null;
            if (!string.IsNullOrEmpty(userID))
            {
                if (platform.HasFlag(StreamingPlatformTypeEnum.Twitch) && user == null)
                {
                    user = ServiceManager.Get<UserService>().GetActiveUserByPlatformID(StreamingPlatformTypeEnum.Twitch, userID);
                }

                if (user == null)
                {
                    UserDataModel userData = await this.GetUserDataByPlatformID(StreamingPlatformTypeEnum.Twitch, userID);
                    if (userData != null)
                    {
                        user = new UserViewModel(userData);
                    }
                    else
                    {
                        if (platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                        {
                            var twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByID(userID);
                            if (twitchUser != null)
                            {
                                user = await UserViewModel.Create(twitchUser);
                            }
                        }
                    }
                }
            }

            if (user == null && !string.IsNullOrEmpty(username))
            {
                username = UserService.SanitizeUsername(username);
                user = ServiceManager.Get<UserService>().GetActiveUserByUsername(username);
                if (user == null)
                {
                    UserDataModel userData = await this.GetUserDataByPlatformUsername(StreamingPlatformTypeEnum.Twitch, username);
                    if (userData != null)
                    {
                        user = new UserViewModel(userData);
                    }
                    else
                    {
                        if (platform.HasFlag(StreamingPlatformTypeEnum.Twitch) && ServiceManager.Get<TwitchSessionService>().UserConnection != null)
                        {
                            var twitchUser = await ServiceManager.Get<TwitchSessionService>().UserConnection.GetNewAPIUserByLogin(username);
                            if (twitchUser != null)
                            {
                                user = await UserViewModel.Create(twitchUser);
                            }
                        }

                        if (platform.HasFlag(StreamingPlatformTypeEnum.YouTube) && ServiceManager.Get<YouTubeSessionService>().UserConnection != null)
                        {
                            Google.Apis.YouTube.v3.Data.Channel youtubeUser = await ServiceManager.Get<YouTubeSessionService>().UserConnection.GetChannelByUsername(username);
                            if (youtubeUser != null)
                            {
                                return await UserViewModel.Create(youtubeUser);
                            }
                        }

                        if (platform.HasFlag(StreamingPlatformTypeEnum.Glimesh) && ServiceManager.Get<GlimeshSessionService>().UserConnection != null)
                        {
                            GlimeshBase.Models.Users.UserModel glimeshUser = await ServiceManager.Get<GlimeshSessionService>().UserConnection.GetUserByName(username);
                            if (glimeshUser != null)
                            {
                                return await UserViewModel.Create(glimeshUser);
                            }
                        }

                        if (platform.HasFlag(StreamingPlatformTypeEnum.Trovo) && ServiceManager.Get<TrovoSessionService>().UserConnection != null)
                        {
                            TrovoBase.Models.Users.UserModel trovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(username);
                            if (trovoUser != null)
                            {
                                return await UserViewModel.Create(trovoUser);
                            }
                        }
                    }

                    if (user == null)
                    {
                        user = UserViewModel.Create(username);
                    }
                }
            }

            return user;
        }
    }
}
