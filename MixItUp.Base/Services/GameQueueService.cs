using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class GameQueueService
    {
        private const string QueuePositionSpecialIdentifier = "queueposition";

        public static event EventHandler OnGameQueueUpdated = delegate { };

        private LockedList<CommandParametersModel> queue = new LockedList<CommandParametersModel>();

        public GameQueueService() { }

        public IEnumerable<CommandParametersModel> Queue { get { return this.queue.ToList(); } }

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

        public async Task Join(CommandParametersModel parameters)
        {
            if (await this.ValidateJoin(parameters))
            {
                if (ChannelSession.Settings.GameQueueSubPriority)
                {
                    if (parameters.User.MeetsRole(UserRoleEnum.Subscriber))
                    {
                        int totalSubs = this.Queue.Count(u => u.User.MeetsRole(UserRoleEnum.Subscriber));
                        this.queue.Insert(totalSubs, parameters);
                    }
                    else
                    {
                        this.queue.Add(parameters);
                    }
                }
                else
                {
                    this.queue.Add(parameters);
                }

                parameters.SpecialIdentifiers[QueuePositionSpecialIdentifier] = this.GetUserPosition(parameters.User).ToString();
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserJoinedCommandID, parameters);
            }

            GameQueueService.OnGameQueueUpdated(this, new EventArgs());
        }

        public async Task JoinFront(CommandParametersModel parameters)
        {
            if (await this.ValidateJoin(parameters))
            {
                this.queue.Insert(0, parameters);

                parameters.SpecialIdentifiers[QueuePositionSpecialIdentifier] = this.GetUserPosition(parameters.User).ToString();
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserJoinedCommandID, parameters);
            }

            GameQueueService.OnGameQueueUpdated(this, new EventArgs());
        }

        public Task Leave(CommandParametersModel parameters)
        {
            CommandParametersModel p = this.FindCurrentParameters(parameters);
            if (p != null)
            {
                this.queue.Remove(p);
                GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            }
            return Task.CompletedTask;
        }

        public Task MoveUp(CommandParametersModel parameters)
        {
            CommandParametersModel p = this.FindCurrentParameters(parameters);
            if (p != null)
            {
                this.queue.MoveUp(p);
                GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            }
            return Task.CompletedTask;
        }

        public Task MoveDown(CommandParametersModel parameters)
        {
            CommandParametersModel p = this.FindCurrentParameters(parameters);
            if (p != null)
            {
                this.queue.MoveDown(p);
                GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            }
            return Task.CompletedTask;
        }

        public async Task SelectFirst()
        {
            if (this.queue.Count > 0)
            {
                CommandParametersModel parameters = this.queue.ElementAt(0);
                this.queue.Remove(parameters);
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, parameters);
                GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            }
        }

        public async Task SelectFirstType(RoleRequirementModel roleRequirement)
        {
            foreach (CommandParametersModel parameters in this.queue.ToList())
            {
                Result result = await roleRequirement.Validate(parameters);
                if (result.Success)
                {
                    this.queue.Remove(parameters);
                    await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, parameters);
                    GameQueueService.OnGameQueueUpdated(this, new EventArgs());
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
                CommandParametersModel parameters = this.queue.ElementAt(index);
                this.queue.Remove(parameters);
                await ServiceManager.Get<CommandService>().Queue(ChannelSession.Settings.GameQueueUserSelectedCommandID, parameters);
                GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            }
        }

        public int GetUserPosition(UserV2ViewModel user)
        {
            int position = -1;
            CommandParametersModel parameters = this.queue.FirstOrDefault(q => q.User.ID == user.ID);
            if (parameters != null)
            {
                position = this.queue.IndexOf(parameters);
            }
            return (position != -1) ? position + 1 : position;
        }

        public async Task PrintUserPosition(CommandParametersModel parameters)
        {
            int position = this.GetUserPosition(parameters.User);
            if (position != -1)
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.QueueYouAreInPosition, position), parameters);
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.QueueYouAreNotCurrentlyIn, parameters);
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
                    users.Add("@" + this.queue[i].User.Username);
                }

                message.Append(string.Join(", ", users));
                message.Append(".");
            }

            await ServiceManager.Get<ChatService>().SendMessage(message.ToString(), parameters);
        }

        public Task Clear()
        {
            this.queue.Clear();
            GameQueueService.OnGameQueueUpdated(this, new EventArgs());
            return Task.CompletedTask;
        }

        private async Task<bool> ValidateJoin(CommandParametersModel parameters)
        {
            int position = this.GetUserPosition(parameters.User);
            if (position != -1)
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.QueueYouAreInPosition, position), parameters);
                return false;
            }
            return true;
        }

        private CommandParametersModel FindCurrentParameters(CommandParametersModel parameters)
        {
            return this.queue.FirstOrDefault(p => p.User.ID == parameters.User.ID);
        }
    }
}
