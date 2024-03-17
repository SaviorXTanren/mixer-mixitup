using MixItUp.Base.Model;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class UserService
    {
        public static string SanitizeUsername(string username) { return !string.IsNullOrEmpty(username) ? username.ToLower().Replace("@", "").Trim() : string.Empty; }

        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUserIDLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();
        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformUsernameLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();
        private Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>> platformDisplayNameLookups { get; set; } = new Dictionary<StreamingPlatformTypeEnum, LockedDictionary<string, Guid>>();

        private Dictionary<Guid, UserV2ViewModel> activeUsers = new Dictionary<Guid, UserV2ViewModel>();

        public int ActiveUserCount { get { return this.activeUsers.Count; } }

        public IEnumerable<UserV2ViewModel> DisplayUsers
        {
            get
            {
                lock (displayUsersLock)
                {
                    return this.displayUsers.Values.ToList().Take(ChannelSession.Settings.MaxUsersShownInChat);
                }
            }
        }
        private SortedList<string, UserV2ViewModel> displayUsers = new SortedList<string, UserV2ViewModel>();
        private object displayUsersLock = new object();

        public event EventHandler DisplayUsersUpdated = delegate { };

        private bool fullUserDataLoadOccurred = false;

        public UserService()
        {
            StreamingPlatforms.ForEachPlatform(p =>
            {
                this.platformUserIDLookups[p] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                this.platformUsernameLookups[p] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
                this.platformDisplayNameLookups[p] = new LockedDictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            });
        }

        #region Users

        public async Task<UserV2ViewModel> GetUserByID(Guid id)
        {
            UserV2ViewModel user = this.GetCachedUserByID(id);
            if (user == null)
            {
                user = await this.GetDatabaseUserByID(id);
            }
            return user;
        }

        public async Task<UserV2ViewModel> GetUserByPlatform(StreamingPlatformTypeEnum platform, string platformID = null, string platformUsername = null, bool performPlatformSearch = false)
        {
            UserV2ViewModel user = null;

            if (string.IsNullOrEmpty(platformID) && string.IsNullOrEmpty(platformUsername))
            {
                throw new ArgumentException("Neither PlatformID or PlatformUsername were specified");
            }

            if (platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                user = await this.GetUserByPlatform(ChannelSession.Settings.DefaultStreamingPlatform, platformID, platformUsername);
                if (user != null)
                {
                    return user;
                }

                foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.GetConnectedPlatforms().Where(p => p != ChannelSession.Settings.DefaultStreamingPlatform))
                {
                    user = await this.GetUserByPlatform(p, platformID, platformUsername);
                    if (user != null)
                    {
                        return user;
                    }
                }
                return null;
            }

            user = this.GetCachedUserByPlatform(platform, platformID: platformID, platformUsername: platformUsername);
            if (user == null)
            {
                user = await this.GetDatabaseUserByPlatform(platform, platformID: platformID, platformUsername: platformUsername);
            }

            if (user == null)
            {
                UserPlatformV2ModelBase platformModel = null;
                if (performPlatformSearch)
                {
                    if (!string.IsNullOrEmpty(platformID))
                    {
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
                        else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().UserConnection != null)
                        {
                            // Trovo does not support user look-up by user ID
                        }
                    }

                    if (!string.IsNullOrEmpty(platformUsername))
                    {
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
                        else if (platform == StreamingPlatformTypeEnum.Trovo && ServiceManager.Get<TrovoSessionService>().UserConnection != null)
                        {
                            var trovoUser = await ServiceManager.Get<TrovoSessionService>().UserConnection.GetUserByName(platformUsername);
                            if (trovoUser != null)
                            {
                                platformModel = new TrovoUserPlatformV2Model(trovoUser);
                            }
                        }
                    }

                    if (platformModel != null)
                    {
                        user = await this.CreateUserInternal(platformModel);
                    }
                }
            }

            return user;
        }

        public async Task<UserV2ViewModel> CreateUser(UserPlatformV2ModelBase platformModel)
        {
            if (platformModel != null && !string.IsNullOrEmpty(platformModel.ID))
            {
                UserV2ViewModel user = await this.GetUserByPlatform(platformModel.Platform, platformModel.ID, platformModel.Username);
                if (user == null)
                {
                    return await this.CreateUserInternal(platformModel);
                }
                return user;
            }
            return null;
        }

        public async Task LoadAllUserData()
        {
            if (!this.fullUserDataLoadOccurred)
            {
                this.fullUserDataLoadOccurred = true;

                foreach (UserV2Model userData in await ChannelSession.Settings.LoadUserV2Data("SELECT * FROM Users", new Dictionary<string, object>()))
                {
                    this.SetUserData(userData);
                }
            }
        }

        public void DeleteUserData(Guid id)
        {
            if (ChannelSession.Settings.Users.TryGetValue(id, out UserV2Model user))
            {
                foreach (UserPlatformV2ModelBase pUser in user.GetAllPlatformData())
                {
                    this.platformUserIDLookups[pUser.Platform].Remove(pUser.ID);
                    this.platformUsernameLookups[pUser.Platform].Remove(pUser.Username.ToLower());
                    if (!string.IsNullOrEmpty(pUser.DisplayName))
                    {
                        this.platformDisplayNameLookups[pUser.Platform].Remove(pUser.DisplayName.ToLower());
                    }
                }
                this.activeUsers.Remove(user.ID);

                ChannelSession.Settings.Users.Remove(user.ID);
            }
            ChannelSession.Settings.Users.ManualValueDeleted(id);
        }

        public async Task ClearUserDataRange(int days)
        {
            StreamingPlatforms.ForEachPlatform(p =>
            {
                this.platformUserIDLookups[p].Clear();
                this.platformUsernameLookups[p].Clear();
                this.platformDisplayNameLookups[p].Clear();
            });
            this.activeUsers.Clear();

            await this.LoadAllUserData();

            List<Guid> usersToRemove = new List<Guid>();
            foreach (var kvp in ChannelSession.Settings.Users)
            {
                if (kvp.Value.LastActivity.TotalDaysFromNow() > days)
                {
                    usersToRemove.Add(kvp.Key);
                }
            }

            foreach (Guid userID in usersToRemove)
            {
                ChannelSession.Settings.Users.Remove(userID);
            }
        }

        public async Task ClearAllUserData()
        {
            StreamingPlatforms.ForEachPlatform(p =>
            {
                this.platformUserIDLookups[p].Clear();
                this.platformUsernameLookups[p].Clear();
                this.platformDisplayNameLookups[p].Clear();
            });
            this.activeUsers.Clear();

            ChannelSession.Settings.Users.Clear();
            ChannelSession.Settings.Users.ClearTracking();

            await ServiceManager.Get<IDatabaseService>().Write(ChannelSession.Settings.DatabaseFilePath, "DELETE FROM Users");
            await ServiceManager.Get<IDatabaseService>().Write(ChannelSession.Settings.DatabaseFilePath, "DELETE FROM ImportedUsers");
        }

        public UserV2Model SetUserData(UserV2Model userData)
        {
            if (userData != null && userData.ID != Guid.Empty && userData.GetPlatforms().Count() > 0 && !userData.HasPlatformData(StreamingPlatformTypeEnum.None))
            {
                UserV2Model userDataToDelete = null;
                lock (ChannelSession.Settings.Users)
                {
                    if (!ChannelSession.Settings.Users.ContainsKey(userData.ID))
                    {
                        ChannelSession.Settings.Users[userData.ID] = userData;
                        if (ChannelSession.Settings.ModerationResetStrikesOnLaunch)
                        {
                            userData.ModerationStrikes = 0;
                        }

                        ChannelSession.Settings.Users.ManualValueChanged(userData.ID);
                    }

                    foreach (StreamingPlatformTypeEnum platform in userData.GetPlatforms())
                    {
                        UserPlatformV2ModelBase platformModel = userData.GetPlatformData<UserPlatformV2ModelBase>(platform);
                        if (platformModel != null)
                        {
                            // Check if there is more than 1 user record and if so, merge them together
                            try
                            {
                                if (this.platformUserIDLookups[platform].TryGetValue(platformModel.ID, out Guid existingUserID) &&
                                    existingUserID != userData.ID &&
                                    ChannelSession.Settings.Users.TryGetValue(existingUserID, out UserV2Model existingUserData))
                                {
                                    existingUserData.MergeUserData(userData);
                                    userDataToDelete = userData;
                                    userData = existingUserData;
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex);
                            }

                            this.platformUserIDLookups[platform][platformModel.ID] = userData.ID;
                            if (!string.IsNullOrEmpty(platformModel.Username))
                            {
                                this.platformUsernameLookups[platform][platformModel.Username.ToLower()] = userData.ID;
                            }
                            if (!string.IsNullOrEmpty(platformModel.DisplayName))
                            {
                                this.platformDisplayNameLookups[platform][platformModel.DisplayName.ToLower()] = userData.ID;
                            }
                        }
                    }
                }

                if (userDataToDelete != null)
                {
                    this.DeleteUserData(userDataToDelete.ID);
                }
            }
            return userData;
        }

        private UserV2ViewModel GetCachedUserByID(Guid id)
        {
            if (this.activeUsers.TryGetValue(id, out UserV2ViewModel user))
            {
                return user;
            }

            if (ChannelSession.Settings.Users.ContainsKey(id))
            {
                return new UserV2ViewModel(ChannelSession.Settings.DefaultStreamingPlatform, ChannelSession.Settings.Users[id]);
            }

            return null;
        }

        private async Task<UserV2ViewModel> GetDatabaseUserByID(Guid id)
        {
            IEnumerable<UserV2Model> results = await ChannelSession.Settings.LoadUserV2Data("SELECT * FROM Users WHERE ID = $ID", new Dictionary<string, object>() { { "$ID", id.ToString() } });
            if (results.Count() > 0)
            {
                UserV2Model userData = this.SetUserData(results.First());
                return new UserV2ViewModel(ChannelSession.Settings.DefaultStreamingPlatform, userData);
            }
            return null;
        }

        private UserV2ViewModel GetCachedUserByPlatform(StreamingPlatformTypeEnum platform, string platformID = null, string platformUsername = null)
        {
            UserV2ViewModel user = null;

            if (platform == StreamingPlatformTypeEnum.None || platform == StreamingPlatformTypeEnum.All)
            {
                foreach (StreamingPlatformTypeEnum p in StreamingPlatforms.GetConnectedPlatforms())
                {
                    user = GetCachedUserByPlatform(p, platformID, platformUsername);
                    if (user != null)
                    {
                        return user;
                    }
                }
                return null;
            }

            Guid userId;
            if (!string.IsNullOrEmpty(platformID))
            {
                if (this.platformUserIDLookups.ContainsKey(platform) && this.platformUserIDLookups[platform].TryGetValue(platformID, out userId))
                {
                    user = this.GetCachedUserByID(userId);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }

            if (!string.IsNullOrEmpty(platformUsername))
            {
                platformUsername = UserService.SanitizeUsername(platformUsername);
                if (this.platformUsernameLookups.ContainsKey(platform) && this.platformUsernameLookups[platform].TryGetValue(platformUsername, out userId))
                {
                    user = this.GetCachedUserByID(userId);
                    if (user != null)
                    {
                        return user;
                    }
                }

                if (this.platformDisplayNameLookups.ContainsKey(platform) && this.platformDisplayNameLookups[platform].TryGetValue(platformUsername, out userId))
                {
                    user = this.GetCachedUserByID(userId);
                    if (user != null)
                    {
                        return user;
                    }
                }
            }

            return null;
        }

        private async Task<UserV2ViewModel> GetDatabaseUserByPlatform(StreamingPlatformTypeEnum platform, string platformID = null, string platformUsername = null)
        {
            if (!string.IsNullOrEmpty(platformID))
            {
                IEnumerable<UserV2Model> results = await ChannelSession.Settings.LoadUserV2Data($"SELECT * FROM Users WHERE {platform.ToString()}ID = $PlatformID", new Dictionary<string, object>() { { "$PlatformID", platformID } });
                if (results.Count() > 0)
                {
                    // Check if there is more than 1 user record and if so, merge them together
                    UserV2Model userData = results.First();
                    if (results.Count() > 1)
                    {
                        userData = this.MergeData(results);
                    }

                    userData = this.SetUserData(userData);
                    return new UserV2ViewModel(platform, userData);
                }
            }

            if (!string.IsNullOrEmpty(platformUsername))
            {
                IEnumerable<UserV2Model> results = await ChannelSession.Settings.LoadUserV2Data($"SELECT * FROM Users WHERE {platform}Username = $PlatformUsername", new Dictionary<string, object>() { { "$PlatformUsername", platformUsername } });
                if (results.Count() > 0)
                {
                    // Check if there is more than 1 user record and if so, merge them together
                    UserV2Model userData = results.First();
                    if (results.Count() > 1)
                    {
                        userData = this.MergeData(results);
                    }

                    userData = this.SetUserData(userData);
                    return new UserV2ViewModel(platform, userData);
                }
            }

            return null;
        }

        private UserV2Model MergeData(IEnumerable<UserV2Model> users)
        {
            UserV2Model primaryUserData = users.First();
            foreach (UserV2Model duplicateUserData in users.Skip(1))
            {
                primaryUserData.MergeUserData(duplicateUserData);
                this.DeleteUserData(duplicateUserData.ID);
            }
            ChannelSession.Settings.Users.ManualValueChanged(primaryUserData.ID);
            return primaryUserData;
        }

        private async Task<UserV2ViewModel> CreateUserInternal(UserPlatformV2ModelBase platformModel)
        {
            if (platformModel != null && !string.IsNullOrEmpty(platformModel.ID))
            {
                UserV2Model userModel = new UserV2Model(platformModel);
                UserV2ViewModel user = null;
                if (platformModel.Platform != StreamingPlatformTypeEnum.None)
                {
                    userModel = this.SetUserData(userModel);
                    user = new UserV2ViewModel(platformModel.Platform, userModel);

                    UserImportModel import = await ChannelSession.Settings.LoadUserImportData(platformModel.Platform, platformModel.ID, platformModel.Username);
                    if (import != null)
                    {
                        user.MergeUserData(import);
                        ChannelSession.Settings.ImportedUsers.Remove(import.ID);
                    }
                }
                else
                {
                    user = new UserV2ViewModel(platformModel.Platform, userModel);
                }

                return user;
            }
            return null;
        }

        #endregion Users

        #region Active Users

        public UserV2ViewModel GetActiveUserByID(Guid id)
        {
            if (this.activeUsers.TryGetValue(id, out UserV2ViewModel user))
            {
                return user;
            }
            return null;
        }

        public UserV2ViewModel GetActiveUserByPlatform(StreamingPlatformTypeEnum platform, string platformID = null, string platformUsername = null)
        {
            UserV2ViewModel user = this.GetCachedUserByPlatform(platform, platformID: platformID, platformUsername: platformUsername);
            if (user != null)
            {
                return this.GetActiveUserByID(user.ID);
            }
            return null;
        }

        public bool IsUserActive(Guid userID) { return this.GetActiveUserByID(userID) != null; }

        public async Task AddOrUpdateActiveUser(IEnumerable<UserV2ViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            bool usersAdded = false;
            foreach (UserV2ViewModel user in users)
            {
                if (user == null || user.ID == Guid.Empty)
                {
                    return;
                }

                lock (this.activeUsers)
                {
                    if (this.activeUsers.ContainsKey(user.ID))
                    {
                        continue;
                    }
                    this.activeUsers[user.ID] = user;
                }

                usersAdded = true;

                lock (displayUsersLock)
                {
                    this.displayUsers[user.SortableID] = user;
                }

                if (user.OnlineViewingMinutes == 0)
                {
                    await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserFirstJoin, new CommandParametersModel(user));
                }

                CommandParametersModel parameters = new CommandParametersModel(user);
                if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserJoined, parameters))
                {
                    user.Model.TotalStreamsWatched++;

                    alerts.Add(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.UserJoinedChat, user.FullDisplayName), ChannelSession.Settings.AlertUserJoinLeaveColor));
                }
            }

            if (usersAdded)
            {
                this.DisplayUsersUpdated(this, new EventArgs());

                if (alerts.Count() < 5)
                {
                    foreach (AlertChatMessageViewModel alert in alerts)
                    {
                        await ServiceManager.Get<AlertsService>().AddAlert(alert);
                    }
                }
            }
        }

        public async Task AddOrUpdateActiveUser(UserV2ViewModel user)
        {
            if (user == null || user.ID == Guid.Empty)
            {
                return;
            }
            await this.AddOrUpdateActiveUser(new List<UserV2ViewModel>() { user });
        }

        public async Task<UserV2ViewModel> RemoveActiveUser(StreamingPlatformTypeEnum platform, string platformUsername)
        {
            platformUsername = platformUsername.ToLower();

            if (this.platformUsernameLookups.ContainsKey(platform) && this.platformUsernameLookups[platform].TryGetValue(platformUsername, out Guid id))
            {
                return await this.RemoveActiveUser(id);
            }

            if (this.platformDisplayNameLookups.ContainsKey(platform) && this.platformDisplayNameLookups[platform].TryGetValue(platformUsername, out id))
            {
                return await this.RemoveActiveUser(id);
            }
            return null;
        }

        public async Task<UserV2ViewModel> RemoveActiveUser(Guid id)
        {
            if (this.activeUsers.TryGetValue(id, out UserV2ViewModel user))
            {
                await this.RemoveActiveUser(user);
                return user;
            }
            return null;
        }

        public async Task<UserV2ViewModel> RemoveActiveUser(UserV2ViewModel user)
        {
            if (user != null && this.activeUsers.ContainsKey(user.ID))
            {
                await this.RemoveActiveUsers(new List<UserV2ViewModel>() { user });
            }
            return user;
        }

        public async Task RemoveActiveUsers(IEnumerable<UserV2ViewModel> users)
        {
            List<AlertChatMessageViewModel> alerts = new List<AlertChatMessageViewModel>();

            bool userRemoved = false;
            foreach (UserV2ViewModel user in users)
            {
                if (this.activeUsers.Remove(user.ID))
                {
                    userRemoved = true;

                    lock (displayUsersLock)
                    {
                        if (!this.displayUsers.Remove(user.SortableID))
                        {
                            int index = this.displayUsers.IndexOfValue(user);
                            if (index >= 0)
                            {
                                this.displayUsers.RemoveAt(index);
                            }
                        }
                    }

                    CommandParametersModel parameters = new CommandParametersModel(user);
                    if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.ChatUserLeft, parameters))
                    {
                        alerts.Add(new AlertChatMessageViewModel(user, string.Format(MixItUp.Base.Resources.UserLeftChat, user.FullDisplayName), ChannelSession.Settings.AlertUserJoinLeaveColor));
                    }
                }
            }

            if (userRemoved)
            {
                this.DisplayUsersUpdated(this, new EventArgs());

                if (users.Count() < 5)
                {
                    foreach (AlertChatMessageViewModel alert in alerts)
                    {
                        await ServiceManager.Get<AlertsService>().AddAlert(alert);
                    }
                }
            }
        }

        public IEnumerable<UserV2ViewModel> GetActiveUsers(StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.All, bool excludeSpecialtyExcluded = true)
        {
            IEnumerable<UserV2ViewModel> users = this.activeUsers.Values.ToList();

            if (platform != StreamingPlatformTypeEnum.None && platform != StreamingPlatformTypeEnum.All)
            {
                users = users.Where(u => u.Platform == platform);
            }

            if (excludeSpecialtyExcluded)
            {
                users = users.Where(u => !u.IsSpecialtyExcluded);
            }

            return users;
        }

        public int GetActiveUserCount() { return this.activeUsers.Count; }

        #endregion Active Users
    }
}
