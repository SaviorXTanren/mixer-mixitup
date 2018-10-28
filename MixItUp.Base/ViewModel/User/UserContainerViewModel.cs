using Mixer.Base.Model.Interactive;
using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public class UserContainerViewModel
    {
        public const string HypeBotUserName = "HypeBot";
        public const string BoomTVUserName = "boomtvmod";
        public const string StreamJarUserName = "StreamJar";
        public const string PretzelRocksUserName = "PretzelRocks";
        public const string ScottyBotUserName = "ScottyBot";

        private Dictionary<uint, UserViewModel> users = new Dictionary<uint, UserViewModel>();

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private Dictionary<uint, DateTimeOffset?> subscribers = new Dictionary<uint, DateTimeOffset?>();

        public UserContainerViewModel() { }

        public async Task Initialize()
        {
            foreach (UserWithGroupsModel userGroup in await ChannelSession.Connection.GetUsersWithRoles(ChannelSession.Channel, MixerRoleEnum.Subscriber))
            {
                subscribers[userGroup.id] = userGroup.GetSubscriberDate();
            }
        }

        public async Task<bool> HasUser(uint userID)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult<bool>(this.users.ContainsKey(userID));
            });
        }

        public async Task<UserViewModel> GetUserByUsername(string username)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult<UserViewModel>(this.users.Values.FirstOrDefault(u => u.UserName.Equals(username, StringComparison.CurrentCultureIgnoreCase)));
            });
        }

        public async Task<UserViewModel> GetUserByID(uint userID)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                if (this.users.ContainsKey(userID))
                {
                    return Task.FromResult(this.users[userID]);
                }
                return Task.FromResult<UserViewModel>(null);
            });
        }

        public async Task<UserViewModel> GetUserByID(string interactiveParticipantID)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult(this.users.Values.FirstOrDefault(u => u.InteractiveIDs.Contains(interactiveParticipantID)));
            });
        }

        public async Task<UserViewModel> AddOrUpdateUser(UserViewModel user)
        {
            if (user.ID > 0)
            {
                await this.semaphore.WaitAndRelease(() =>
                {
                    this.users[user.ID] = user;
                    return Task.FromResult(0);
                });
                return await this.GetUserByID(user.ID);
            }
            return null;
        }

        public async Task AddOrUpdateUsers(IEnumerable<ChatUserModel> chatUsers)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (ChatUserModel chatUser in chatUsers)
            {
                if (chatUser.userId.HasValue)
                {
                    UserViewModel user = new UserViewModel(chatUser);
                    user.SetChatDetails(chatUser);
                    users.Add(user);
                }
            }
            await this.AddOrUpdateUsers(users);
        }

        public async Task<UserViewModel> AddOrUpdateUser(ChatUserModel chatUser)
        {
            await this.AddOrUpdateUsers(new List<ChatUserModel>() { chatUser });
            return await this.GetUserByID(chatUser.userId.GetValueOrDefault());
        }

        public async Task AddOrUpdateUsers(IEnumerable<InteractiveParticipantModel> interactiveUsers)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (InteractiveParticipantModel interactiveUser in interactiveUsers)
            {
                UserViewModel user = new UserViewModel(interactiveUser);
                user.SetInteractiveDetails(interactiveUser);
                users.Add(user);
            }
            await this.AddOrUpdateUsers(users);
        }

        public async Task<UserViewModel> AddOrUpdateUser(InteractiveParticipantModel interactiveUser)
        {
            await this.AddOrUpdateUsers(new List<InteractiveParticipantModel>() { interactiveUser });
            return await this.GetUserByID(interactiveUser.userID);
        }

        public async Task RemoveInteractiveUser(InteractiveParticipantModel interactiveUser)
        {
            await this.semaphore.WaitAndRelease(() =>
            {
                if (this.users.ContainsKey(interactiveUser.userID))
                {
                    UserViewModel user = this.users[interactiveUser.userID];
                    user.RemoveInteractiveDetails(interactiveUser);
                }
                return Task.FromResult(0);
            });
        }

        public async Task<UserViewModel> RemoveUser(uint userID)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                UserViewModel user = null;
                if (this.users.ContainsKey(userID))
                {
                    user = this.users[userID];
                    this.users.Remove(userID);
                }
                return Task.FromResult(user);
            });
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            return await this.semaphore.WaitAndRelease(() => Task.FromResult(this.users.Values.ToList()));
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsersSorted()
        {
            return (await this.GetAllUsers()).OrderByDescending(u => u.PrimarySortableRole).ThenBy(u => u.UserName);
        }

        public async Task<IEnumerable<UserViewModel>> GetAllWorkableUsers()
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                List<UserViewModel> users = this.users.Values.ToList();
                users.RemoveAll(u => UserContainerViewModel.HypeBotUserName.Equals(u.UserName));
                users.RemoveAll(u => UserContainerViewModel.BoomTVUserName.Equals(u.UserName));
                users.RemoveAll(u => UserContainerViewModel.StreamJarUserName.Equals(u.UserName));
                users.RemoveAll(u => UserContainerViewModel.PretzelRocksUserName.Equals(u.UserName));
                users.RemoveAll(u => UserContainerViewModel.ScottyBotUserName.Equals(u.UserName));
                if (ChannelSession.BotUser != null)
                {
                    users.RemoveAll(u => ChannelSession.BotUser.username.Equals(u.UserName));
                }
                return Task.FromResult(users);
            });
        }

        public async Task Clear()
        {
            await this.semaphore.WaitAndRelease(() =>
            {
                this.users.Clear();
                return Task.FromResult(0);
            });
        }

        public async Task<int> Count()
        {
            return await this.semaphore.WaitAndRelease(() => Task.FromResult(this.users.Count));
        }

        private async Task AddOrUpdateUsers(IEnumerable<UserViewModel> users)
        {
            List<UserViewModel> firstJoinUsers = new List<UserViewModel>();
            List<UserViewModel> refreshUsers = new List<UserViewModel>();

            await this.semaphore.WaitAndRelease(() =>
            {
                foreach (UserViewModel user in users)
                {
                    if (!ChannelSession.Settings.UserData.ContainsKey(user.ID))
                    {
                        firstJoinUsers.Add(user);
                    }

                    if (!this.users.ContainsKey(user.ID))
                    {
                        this.users[user.ID] = user;
                        refreshUsers.Add(user);
                    }
                }

                return Task.FromResult(0);
            });

            Dictionary<uint, DateTimeOffset?> follows = await ChannelSession.Connection.CheckIfFollows(ChannelSession.Channel, users.Select(u => u.GetModel()));
            foreach (UserViewModel user in refreshUsers)
            {
                if (follows.ContainsKey(user.ID))
                {
                    user.FollowDate = follows[user.ID];
                }
                if (subscribers.ContainsKey(user.ID))
                {
                    user.SubscribeDate = subscribers[user.ID];
                }
            }

            foreach (UserViewModel user in firstJoinUsers)
            {
                await this.PerformUserFirstJoin(user);
            }
        }

        private async Task PerformUserFirstJoin(UserViewModel user)
        {
            if (ChannelSession.Constellation.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserFirstJoin)))
            {
                ChannelSession.Constellation.LogUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserFirstJoin));
                await ChannelSession.Constellation.RunEventCommand(ChannelSession.Constellation.FindMatchingEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserFirstJoin)), user);
            }
        }
    }
}
