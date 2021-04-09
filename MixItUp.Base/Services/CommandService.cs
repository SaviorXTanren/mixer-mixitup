using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public class CommandService
    {
        private const int MaxCommandInstancesTracked = 200;

        public event EventHandler<CommandInstanceModel> OnCommandInstanceAdded = delegate { };

        public IEnumerable<CommandInstanceModel> CommandInstances { get { return this.commandInstances.ToList(); } }
        private List<CommandInstanceModel> commandInstances = new List<CommandInstanceModel>();

        private SemaphoreSlim commandTypeLock = new SemaphoreSlim(1);
        private Dictionary<CommandTypeEnum, Task> commandTypeTasks = new Dictionary<CommandTypeEnum, Task>();
        private Dictionary<CommandTypeEnum, List<CommandInstanceModel>> commandTypeInstances = new Dictionary<CommandTypeEnum, List<CommandInstanceModel>>();

        public CommandService()
        {
            foreach (CommandTypeEnum type in EnumHelper.GetEnumList<CommandTypeEnum>())
            {
                commandTypeTasks[type] = null;
                commandTypeInstances[type] = new List<CommandInstanceModel>();
            }
        }

        public async Task Queue(Guid commandID) { await this.Queue(ChannelSession.Settings.GetCommand(commandID)); }

        public async Task Queue(Guid commandID, CommandParametersModel parameters) { await this.Queue(ChannelSession.Settings.GetCommand(commandID), parameters); }

        public async Task Queue(CommandModelBase command)
        {
            if (command != null)
            {
                await this.Queue(new CommandInstanceModel(command));
            }
        }

        public async Task Queue(CommandModelBase command, CommandParametersModel parameters)
        {
            if (command != null && parameters != null)
            {
                await this.Queue(new CommandInstanceModel(command, parameters));
            }
        }

        public async Task Queue(CommandInstanceModel commandInstance)
        {
            lock (CommandInstances)
            {
                this.commandInstances.Insert(0, commandInstance);
                while (this.commandInstances.Count > MaxCommandInstancesTracked)
                {
                    this.commandInstances.RemoveAt(this.commandInstances.Count - 1);
                }
            }

            CommandTypeEnum type = commandInstance.QueueCommandType;
            if (commandInstance.DontQueue)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsync(() => this.RunDirectly(commandInstance));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                await this.commandTypeLock.WaitAndRelease(() =>
                {
                    commandTypeInstances[type].Add(commandInstance);
                    if (commandTypeTasks[type] == null)
                    {
                        commandTypeTasks[type] = AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type));
                    }
                    return Task.FromResult(0);
                });
            }

            this.OnCommandInstanceAdded(this, commandInstance);
        }

        public async Task RunDirectly(CommandInstanceModel commandInstance)
        {
            try
            {
                if (commandInstance.State == CommandInstanceStateEnum.Canceled)
                {
                    return;
                }

                Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                List<CommandParametersModel> runnerParameters = new List<CommandParametersModel>() { commandInstance.Parameters };

                CommandModelBase command = commandInstance.Command;
                if (command != null)
                {
                    if (!command.IsEnabled || !command.HasWork)
                    {
                        return;
                    }

                    commandInstance.Parameters.SpecialIdentifiers[CommandModelBase.CommandNameSpecialIdentifier] = command.Name;

                    command.TrackTelemetry();

                    Result validationResult = await command.CustomValidation(commandInstance.Parameters);
                    if (validationResult.Success)
                    {
                        validationResult = await command.ValidateRequirements(commandInstance.Parameters);
                        if (!validationResult.Success && ChannelSession.Settings.RequirementErrorsCooldownType != RequirementErrorCooldownTypeEnum.PerCommand)
                        {
                            command.CommandErrorCooldown = RequirementModelBase.UpdateErrorCooldown();
                        }
                    }
                    else
                    {
                        if (ChannelSession.Settings.RequirementErrorsCooldownType != RequirementErrorCooldownTypeEnum.None)
                        {
                            if (!string.IsNullOrEmpty(validationResult.Message) && validationResult.DisplayMessage)
                            {
                                await ChannelSession.Services.Chat.SendMessage(validationResult.Message);
                            }
                        }
                    }

                    if (validationResult.Success)
                    {
                        if (command.Requirements != null)
                        {
                            await command.PerformRequirements(commandInstance.Parameters);
                            runnerParameters = new List<CommandParametersModel>(command.GetPerformingUsers(commandInstance.Parameters));
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(validationResult.Message))
                        {
                            commandInstance.ErrorMessage = validationResult.Message;
                            commandInstance.State = CommandInstanceStateEnum.Failed;
                        }
                        else
                        {
                            commandInstance.State = CommandInstanceStateEnum.Completed;
                        }
                        return;
                    }
                }

                Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                commandInstance.State = CommandInstanceStateEnum.Running;

                foreach (CommandParametersModel p in runnerParameters)
                {
                    p.User.Data.TotalCommandsRun++;
                    await this.RunDirectlyInternal(commandInstance, p);
                }

                commandInstance.State = CommandInstanceStateEnum.Completed;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void Cancel(CommandInstanceModel commandInstance)
        {
            if (commandInstance.State == CommandInstanceStateEnum.Pending || commandInstance.State == CommandInstanceStateEnum.Running)
            {
                commandInstance.State = CommandInstanceStateEnum.Canceled;
            }
        }

        public async Task Replay(CommandInstanceModel commandInstance)
        {
            await this.Queue(commandInstance);
        }

        private async Task BackgroundCommandTypeRunner(CommandTypeEnum type)
        {
            CommandInstanceModel instance;
            do
            {
                instance = await this.commandTypeLock.WaitAndRelease(() =>
                {
                    CommandInstanceModel commandInstance = null;
                    if (commandTypeInstances.ContainsKey(type) && commandTypeInstances[type].Count > 0)
                    {
                        commandInstance = commandTypeInstances[type].FirstOrDefault();
                        commandTypeInstances[type].Remove(commandInstance);
                    }
                    else
                    {
                        commandTypeTasks[type] = null;
                    }
                    return Task.FromResult(commandInstance);
                });

                if (instance != null)
                {
                    await this.RunDirectly(instance);
                }

            } while (instance != null);
        }

        private async Task RunDirectlyInternal(CommandInstanceModel commandInstance, CommandParametersModel parameters)
        {
            CommandModelBase command = commandInstance.Command;
            if (command != null)
            {
                await command.PreRun(parameters);
            }

            if (command != null && command.HasCustomRun)
            {
                await commandInstance.Command.CustomRun(parameters);
            }
            else
            {
                List<ActionModelBase> actions = commandInstance.GetActions();
                for (int i = 0; i < actions.Count; i++)
                {
                    if (commandInstance.State == CommandInstanceStateEnum.Canceled)
                    {
                        return;
                    }

                    ActionModelBase action = actions[i];
                    if (action is OverlayActionModel && ChannelSession.Services.Overlay.IsConnected)
                    {
                        ChannelSession.Services.Overlay.StartBatching();
                    }

                    await action.Perform(parameters);

                    if (action is OverlayActionModel && ChannelSession.Services.Overlay.IsConnected)
                    {
                        if (i == (actions.Count - 1) || !(actions[i + 1] is OverlayActionModel))
                        {
                            await ChannelSession.Services.Overlay.EndBatching();
                        }
                    }
                }
            }

            if (commandInstance.State == CommandInstanceStateEnum.Canceled)
            {
                return;
            }

            if (command != null)
            {
                await command.PostRun(parameters);
            }
        }
    }
}
