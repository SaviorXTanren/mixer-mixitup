using MixItUp.Base.Model;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;

namespace MixItUp.Base.Services
{
    public interface IUserService
    {
        UserViewModel GetUserByID(Guid id);

        UserViewModel GetUserByUsername(string username, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None);

        UserViewModel GetUserByTwitchID(string id);

        Task<UserViewModel> AddOrUpdateUser(TwitchNewAPI.Users.UserModel twitchChatUser);

        Task<UserViewModel> RemoveUserByTwitchLogin(string twitchLogin);

        void Clear();

        IEnumerable<UserViewModel> GetAllUsers();

        IEnumerable<UserViewModel> GetAllWorkableUsers();

        int Count();
    }

    public class UserService : IUserService
    {
        public static readonly HashSet<string> SpecialUserAccounts = new HashSet<string>() { "boomtvmod", "streamjar", "pretzelrocks", "scottybot", "streamlabs", "streamelements", "nightbot", "deepbot", "moobot", "coebot", "wizebot", "phantombot", "stay_hydrated_bot" };

        private LockedDictionary<Guid, UserViewModel> usersByID = new LockedDictionary<Guid, UserViewModel>();

        private LockedDictionary<string, UserViewModel> usersByTwitchID = new LockedDictionary<string, UserViewModel>();
        private LockedDictionary<string, UserViewModel> usersByTwitchLogin = new LockedDictionary<string, UserViewModel>();

        public UserViewModel GetUserByID(Guid id)
        {
            if (this.usersByID.ContainsKey(id))
            {
                return this.usersByID[id];
            }
            return null;
        }

        public UserViewModel GetUserByUsername(string username, StreamingPlatformTypeEnum platform = StreamingPlatformTypeEnum.None)
        {
            username = username.ToLower().Replace("@", "");
            UserViewModel user = null;

            if (platform.HasFlag(StreamingPlatformTypeEnum.Twitch) || platform == StreamingPlatformTypeEnum.None)
            {
                if (this.usersByTwitchLogin.TryGetValue(username.ToLower(), out user))
                {
                    return user;
                }
            }
            return null;
        }

        public UserViewModel GetUserByTwitchID(string id)
        {
            if (this.usersByTwitchID.TryGetValue(id, out UserViewModel user))
            {
                return user;
            }
            return null;
        }

        public async Task<UserViewModel> AddOrUpdateUser(TwitchNewAPI.Users.UserModel twitchChatUser)
        {
            if (!string.IsNullOrEmpty(twitchChatUser.id) && !string.IsNullOrEmpty(twitchChatUser.login))
            {
                UserViewModel user = new UserViewModel(twitchChatUser);
                if (this.usersByTwitchID.ContainsKey(twitchChatUser.id))
                {
                    user = this.usersByTwitchID[twitchChatUser.id];
                }
                await this.AddOrUpdateUser(user);
                return user;
            }
            return null;
        }

        private async Task AddOrUpdateUser(UserViewModel user)
        {
            if (!user.IsAnonymous)
            {
                this.usersByID[user.ID] = user;

                if (!string.IsNullOrEmpty(user.TwitchID) && !string.IsNullOrEmpty(user.TwitchUsername))
                {
                    this.usersByTwitchID[user.TwitchID] = user;
                    this.usersByTwitchLogin[user.TwitchUsername] = user;
                }

                if (UserService.SpecialUserAccounts.Contains(user.Username.ToLower()))
                {
                    user.IgnoreForQueries = true;
                }
                else
                {
                    user.IgnoreForQueries = false;
                    if (user.Data.ViewingMinutes == 0)
                    {
                        await ChannelSession.Services.Events.PerformEvent(new EventTrigger(EventTypeEnum.ChatUserFirstJoin, user));
                    }

                    if (ChannelSession.Services.Events.CanPerformEvent(new EventTrigger(EventTypeEnum.ChatUserJoined, user)))
                    {
                        user.LastSeen = DateTimeOffset.Now;
                        user.Data.TotalStreamsWatched++;
                        await ChannelSession.Services.Events.PerformEvent(new EventTrigger(EventTypeEnum.ChatUserJoined, user));
                    }
                }
            }
        }

        public async Task<UserViewModel> RemoveUserByTwitchLogin(string twitchLogin)
        {
            if (this.usersByTwitchLogin.TryGetValue(twitchLogin, out UserViewModel user))
            {
                await this.RemoveUser(user);
                return user;
            }
            return null;
        }

        private async Task RemoveUser(UserViewModel user)
        {
            this.usersByID.Remove(user.ID);

            if (!string.IsNullOrEmpty(user.TwitchID) && !string.IsNullOrEmpty(user.TwitchUsername))
            {
                this.usersByTwitchID.Remove(user.TwitchID);
                this.usersByTwitchLogin.Remove(user.TwitchUsername);
            }

            await ChannelSession.Services.Events.PerformEvent(new EventTrigger(EventTypeEnum.ChatUserLeft, user));
        }

        public void Clear()
        {
            this.usersByID.Clear();

            this.usersByTwitchID.Clear();
            this.usersByTwitchLogin.Clear();
        }

        public IEnumerable<UserViewModel> GetAllUsers() { return this.usersByID.Values; }

        public IEnumerable<UserViewModel> GetAllWorkableUsers()
        {
            IEnumerable<UserViewModel> results = this.GetAllUsers();
            return results.Where(u => !u.IgnoreForQueries);
        }

        public int Count() { return this.usersByID.Count; }
    }
}
