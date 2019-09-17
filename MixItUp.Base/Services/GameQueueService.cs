using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IGameQueueService
    {
        IEnumerable<UserViewModel> Queue { get; }

        bool IsEnabled { get; }

        Task Enable();
        Task Disable();

        Task Join(UserViewModel user);
        Task JoinFront(UserViewModel user);

        Task Leave(UserViewModel user);

        Task MoveUp(UserViewModel user);
        Task MoveDown(UserViewModel user);

        Task SelectFirst();
        Task SelectFirstType(RoleRequirementViewModel requirement);
        Task SelectRandom();

        int GetUserPosition(UserViewModel user);
        Task PrintUserPosition(UserViewModel user);

        Task PrintStatus();

        Task Clear();
    }

    public class GameQueueService : IGameQueueService
    {
        private LockedList<UserViewModel> queue = new LockedList<UserViewModel>();

        public GameQueueService() { }

        public IEnumerable<UserViewModel> Queue { get { return this.queue.ToList(); } }

        public bool IsEnabled { get; private set; }

        public async Task Enable()
        {
            this.IsEnabled = true;
            await this.Clear();
        }

        public async Task Disable()
        {
            this.IsEnabled = false;
            await this.Clear();
        }

        public async Task Join(UserViewModel user)
        {
            if (await this.ValidateJoin(user))
            {
                if (ChannelSession.Settings.GameQueueSubPriority)
                {
                    if (user.HasPermissionsTo(MixerRoleEnum.Subscriber))
                    {
                        int totalSubs = this.Queue.Count(u => u.HasPermissionsTo(MixerRoleEnum.Subscriber));
                        this.queue.Insert(totalSubs, user);
                    }
                    else
                    {
                        this.queue.Add(user);
                    }
                }
                else
                {
                    this.queue.Add(user);
                }

                int position = this.queue.IndexOf(user);
                await ChannelSession.Settings.GameQueueUserJoinedCommand.Perform(user, arguments: null, extraSpecialIdentifiers: new Dictionary<string, string>() { { "queueposition", this.GetUserPosition(user).ToString() } });
            }
            GlobalEvents.GameQueueUpdated();
        }

        public async Task JoinFront(UserViewModel user)
        {
            if (await this.ValidateJoin(user))
            {
                this.queue.Insert(0, user);
                await ChannelSession.Settings.GameQueueUserJoinedCommand.Perform(user, arguments: null, extraSpecialIdentifiers: new Dictionary<string, string>() { { "queueposition", this.GetUserPosition(user).ToString() } });
            }
            GlobalEvents.GameQueueUpdated();
        }

        public Task Leave(UserViewModel user)
        {
            this.queue.Remove(user);
            GlobalEvents.GameQueueUpdated();
            return Task.FromResult(0);
        }

        public Task MoveUp(UserViewModel user)
        {
            this.queue.MoveUp(user);
            GlobalEvents.GameQueueUpdated();
            return Task.FromResult(0);
        }

        public Task MoveDown(UserViewModel user)
        {
            this.queue.MoveDown(user);
            GlobalEvents.GameQueueUpdated();
            return Task.FromResult(0);
        }

        public async Task SelectFirst()
        {
            if (this.queue.Count > 0)
            {
                UserViewModel user = this.queue.ElementAt(0);
                this.queue.Remove(user);
                await ChannelSession.Settings.GameQueueUserSelectedCommand.Perform(user);
                GlobalEvents.GameQueueUpdated();
            }
        }

        public async Task SelectFirstType(RoleRequirementViewModel requirement)
        {
            UserViewModel user = this.queue.FirstOrDefault(u => requirement.DoesMeetRequirement(u));
            if (user != null)
            {
                await this.SelectFirst();
            }
            else
            {
                this.queue.Remove(user);
                await ChannelSession.Settings.GameQueueUserSelectedCommand.Perform(user);
                GlobalEvents.GameQueueUpdated();
            }
        }

        public async Task SelectRandom()
        {
            if (this.queue.Count > 0)
            {
                int index = RandomHelper.GenerateRandomNumber(this.queue.Count());
                UserViewModel user = this.queue.ElementAt(index);
                this.queue.Remove(user);
                await ChannelSession.Settings.GameQueueUserSelectedCommand.Perform(user);
                GlobalEvents.GameQueueUpdated();
            }
        }

        public int GetUserPosition(UserViewModel user)
        {
            int position = this.queue.IndexOf(user);
            return (position != -1) ? position + 1 : position;
        }

        public async Task PrintUserPosition(UserViewModel user)
        {
            int position = this.GetUserPosition(user);
            if (position != -1)
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You are #{0} in the queue to play", position));
            }
            else
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, "You are not currently in the queue to play");
            }
        }

        public async Task PrintStatus()
        {
            StringBuilder message = new StringBuilder();
            message.Append(string.Format("There are currently {0} waiting to play.", this.queue.Count()));

            if (this.queue.Count() > 0)
            {
                message.Append(" The following users are next up to play: ");

                List<string> users = new List<string>();
                for (int i = 0; i < this.queue.Count() && i < 5; i++)
                {
                    users.Add("@" + this.queue[i].UserName);
                }

                message.Append(string.Join(", ", users));
                message.Append(".");
            }

            await ChannelSession.Services.Chat.SendMessage(message.ToString());
        }

        public Task Clear()
        {
            this.queue.Clear();
            GlobalEvents.GameQueueUpdated();
            return Task.FromResult(0);
        }

        private async Task<bool> ValidateJoin(UserViewModel user)
        {
            int position = this.GetUserPosition(user);
            if (position != -1)
            {
                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You are already #{0} in the queue", position));
                return false;
            }
            return true;
        }
    }
}
