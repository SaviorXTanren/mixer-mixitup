using Mixer.Base.Model.MixPlay;
using Mixer.Base.Model.User;
using MixItUp.Base.Commands;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.User
{
    public class UserContainerViewModel
    {
        public static readonly List<string> SpecialUserAccounts = new List<string>() { "HypeBot", "boomtvmod", "StreamJar", "PretzelRocks", "ScottyBot" };

        private Dictionary<uint, UserViewModel> users = new Dictionary<uint, UserViewModel>();

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        public UserContainerViewModel() { }

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


        public async Task<IEnumerable<UserViewModel>> GetUsersByID(uint[] userIDs)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult<IEnumerable<UserViewModel>>(this.users.Where(x => userIDs.Contains(x.Key)).Select(x => x.Value).ToList());
            });
        }

        public async Task<UserViewModel> GetUserByParticipantID(string participantID)
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult(this.users.Values.FirstOrDefault(u => u.InteractiveIDs.ContainsKey(participantID)));
            });
        }

        public async Task<UserViewModel> AddOrUpdateUser(UserViewModel user)
        {
            if (!user.IsAnonymous)
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

        public async Task<UserViewModel> AddOrUpdateUser(ChatUserModel chatUser)
        {
            await this.AddOrUpdateUsers(new List<ChatUserModel>() { chatUser });
            return await this.GetUserByID(chatUser.userId.GetValueOrDefault());
        }

        public async Task<IEnumerable<UserViewModel>> AddOrUpdateUsers(IEnumerable<ChatUserModel> chatUsers)
        {
            Dictionary<uint, UserViewModel> allProcessedUsers = new Dictionary<uint, UserViewModel>();
            List<UserViewModel> newUsers = new List<UserViewModel>();
            await this.semaphore.WaitAndRelease(() =>
            {
                foreach (ChatUserModel chatUser in chatUsers)
                {
                    if (chatUser.userId.HasValue)
                    {
                        UserViewModel user = null;
                        if (this.users.ContainsKey(chatUser.userId.GetValueOrDefault()))
                        {
                            user = this.users[chatUser.userId.GetValueOrDefault()];
                        }
                        else
                        {
                            user = new UserViewModel(chatUser);
                            newUsers.Add(user);
                        }

                        if (user != null)
                        {
                            allProcessedUsers[user.ID] = user;
                            this.users[user.ID] = user;
                            user.SetChatDetails(chatUser);
                        }
                    }
                }

                return Task.FromResult(0);
            });

            await this.PerformUserFirstJoins(newUsers);

            foreach (UserViewModel user in allProcessedUsers.Values)
            {
                if (user.IsInChat)
                {
                    if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserJoined)))
                    {
                        await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserJoined), user);
                    }
                }
            }

            return allProcessedUsers.Values;
        }

        public async Task<UserViewModel> AddOrUpdateUser(MixPlayParticipantModel interactiveUser)
        {
            await this.AddOrUpdateUsers(new List<MixPlayParticipantModel>() { interactiveUser });
            return await this.GetUserByID(interactiveUser.userID);
        }

        public async Task<IEnumerable<UserViewModel>> AddOrUpdateUsers(IEnumerable<MixPlayParticipantModel> interactiveUsers)
        {
            Dictionary<uint, UserViewModel> allProcessedUsers = new Dictionary<uint, UserViewModel>();
            List<UserViewModel> newUsers = new List<UserViewModel>();
            await this.semaphore.WaitAndRelease(() =>
            {
                foreach (MixPlayParticipantModel interactiveUser in interactiveUsers)
                {
                    if (interactiveUser.userID > 0)
                    {
                        UserViewModel user = null;
                        if (this.users.ContainsKey(interactiveUser.userID))
                        {
                            user = this.users[interactiveUser.userID];
                        }
                        else
                        {
                            user = new UserViewModel(interactiveUser);
                            newUsers.Add(user);
                        }

                        if (user != null)
                        {
                            allProcessedUsers[user.ID] = user;
                            this.users[user.ID] = user;
                            user.SetInteractiveDetails(interactiveUser);
                        }
                    }
                }

                return Task.FromResult(0);
            });
            await this.PerformUserFirstJoins(newUsers);
            return allProcessedUsers.Values;
        }

        public async Task RemoveInteractiveUser(MixPlayParticipantModel interactiveUser)
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
            IEnumerable<UserViewModel> results = await this.RemoveUsers(new List<uint>() { userID });
            return (results.Count() > 0) ? results.First() : null;
        }

        public async Task<IEnumerable<UserViewModel>> RemoveUsers(IEnumerable<uint> userIDs)
        {
            List<UserViewModel> results = new List<UserViewModel>();
            await this.semaphore.WaitAndRelease(() =>
            {
                foreach (uint userID in userIDs)
                {
                    if (this.users.ContainsKey(userID))
                    {
                        UserViewModel user = this.users[userID];
                        this.users.Remove(userID);
                        results.Add(user);
                    }
                }
                return Task.FromResult(0);
            });

            foreach (UserViewModel user in results)
            {
                if (user.IsInChat)
                {
                    if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserLeft)))
                    {
                        await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserLeft), user);
                    }
                }
            }

            return results;
        }

        public async Task<IEnumerable<UserViewModel>> GetAllUsers()
        {
            return await this.semaphore.WaitAndRelease(() =>
            {
                return Task.FromResult(this.users.Values.Where(u => u.IsInChat).ToList());
            });
        }

        public async Task<IEnumerable<UserViewModel>> GetAllWorkableUsers()
        {
            List<UserViewModel> users = new List<UserViewModel>(await this.GetAllUsers());
            users.RemoveAll(u => UserContainerViewModel.SpecialUserAccounts.Contains(u.UserName));
            if (ChannelSession.MixerBotUser != null)
            {
                users.RemoveAll(u => ChannelSession.MixerBotUser.username.Equals(u.UserName));
            }
            return users.Where(u => u.IsInChat);
        }

        public async Task Clear()
        {
            await this.semaphore.WaitAndRelease(() =>
            {
                this.users.Clear();
                return Task.FromResult(0);
            });
        }

        public async Task<int> Count() { return (await this.GetAllUsers()).Count(); }

        private async Task PerformUserFirstJoins(IEnumerable<UserViewModel> users)
        {
            try
            {
                foreach (UserViewModel user in users)
                {
                    if (user.Data.ViewingMinutes == 0)
                    {
                        if (EventCommand.CanUserRunEvent(user, EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserFirstJoin)))
                        {
                            await EventCommand.FindAndRunEventCommand(EnumHelper.GetEnumName(OtherEventTypeEnum.MixerUserFirstJoin), user);
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }
    }
}
