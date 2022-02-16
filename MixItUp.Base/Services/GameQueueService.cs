using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class GameQueueService
    {
        private const string QueuePositionSpecialIdentifier = "queueposition";

        private LockedList<UserV2ViewModel> queue = new LockedList<UserV2ViewModel>();

        public GameQueueService() { }

        public IEnumerable<UserV2ViewModel> Queue { get { return this.queue.ToList(); } }

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

        public async Task Join(UserV2ViewModel user)
        {
            if (await this.ValidateJoin(user))
            {
                if (ChannelSession.Settings.GameQueueSubPriority)
                {
                    if (user.MeetsRole(UserRoleEnum.Subscriber))
                    {
                        int totalSubs = this.Queue.Count(u => u.MeetsRole(UserRoleEnum.Subscriber));
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
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserJoinedCommandID, new CommandParametersModel(user, new Dictionary<string, string>() { { QueuePositionSpecialIdentifier, this.GetUserPosition(user).ToString() } }));
            }
            GlobalEvents.GameQueueUpdated();
        }

        public async Task JoinFront(UserV2ViewModel user)
        {
            if (await this.ValidateJoin(user))
            {
                this.queue.Insert(0, user);
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserJoinedCommandID, new CommandParametersModel(user, new Dictionary<string, string>() { { QueuePositionSpecialIdentifier, this.GetUserPosition(user).ToString() } }));
            }
            GlobalEvents.GameQueueUpdated();
        }

        public Task Leave(UserV2ViewModel user)
        {
            this.queue.Remove(user);
            GlobalEvents.GameQueueUpdated();
            return Task.CompletedTask;
        }

        public Task MoveUp(UserV2ViewModel user)
        {
            this.queue.MoveUp(user);
            GlobalEvents.GameQueueUpdated();
            return Task.CompletedTask;
        }

        public Task MoveDown(UserV2ViewModel user)
        {
            this.queue.MoveDown(user);
            GlobalEvents.GameQueueUpdated();
            return Task.CompletedTask;
        }

        public async Task SelectFirst()
        {
            if (this.queue.Count > 0)
            {
                UserV2ViewModel user = this.queue.ElementAt(0);
                this.queue.Remove(user);
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, new CommandParametersModel(user));
                GlobalEvents.GameQueueUpdated();
            }
        }

        public async Task SelectFirstType(RoleRequirementModel roleRequirement)
        {
            foreach (UserV2ViewModel user in this.queue.ToList())
            {
                Result result = await roleRequirement.Validate(new CommandParametersModel(user));
                if (result.Success)
                {
                    this.queue.Remove(user);
                    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, new CommandParametersModel(user));
                    GlobalEvents.GameQueueUpdated();
                    return;
                }
            }
            await this.SelectFirst();
        }

        public async Task SelectRandom()
        {
            if (this.queue.Count > 0)
            {
                int index = RandomHelper.GenerateRandomNumber(this.queue.Count());
                UserV2ViewModel user = this.queue.ElementAt(index);
                this.queue.Remove(user);
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, new CommandParametersModel(user));
                GlobalEvents.GameQueueUpdated();
            }
        }

        public int GetUserPosition(UserV2ViewModel user)
        {
            int position = this.queue.IndexOf(user);
            return (position != -1) ? position + 1 : position;
        }

        public async Task PrintUserPosition(UserV2ViewModel user)
        {
            int position = this.GetUserPosition(user);
            if (position != -1)
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.QueueYouAreInPosition, position), user.Platform);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.QueueYouAreNotCurrentlyIn, user.Platform);
            }
        }

        public async Task PrintStatus(CommandParametersModel parameters)
        {
            StringBuilder message = new StringBuilder();
            message.Append(string.Format(MixItUp.Base.Resources.QueueCurrentCount, this.queue.Count()));

            if (this.queue.Count() > 0)
            {
                message.Append(" " + MixItUp.Base.Resources.QueueUserListHeader);

                List<string> users = new List<string>();
                for (int i = 0; i < this.queue.Count() && i < 5; i++)
                {
                    users.Add("@" + this.queue[i].Username);
                }

                message.Append(string.Join(", ", users));
                message.Append(".");
            }

            await ServiceManager.Get<ChatService>().SendMessage(message.ToString(), parameters.Platform);
        }

        public Task Clear()
        {
            this.queue.Clear();
            GlobalEvents.GameQueueUpdated();
            return Task.CompletedTask;
        }

        private async Task<bool> ValidateJoin(UserV2ViewModel user)
        {
            int position = this.GetUserPosition(user);
            if (position != -1)
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.QueueYouAreInPosition, position), user.Platform);
                return false;
            }
            return true;
        }
    }
}
