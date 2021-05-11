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
    public enum CommandServiceLockTypeEnum
    {
        PerCommandType,
        PerActionType,
        VisualAudioActions,
        Singular,
        CommandInstances
    }

    public class CommandService
    {
        private const int MaxCommandInstancesTracked = 200;

        public event EventHandler<CommandInstanceModel> OnCommandInstanceAdded = delegate { };

        public IEnumerable<CommandInstanceModel> CommandInstances { get { return this.commandInstances.ToList(); } }
        private List<CommandInstanceModel> commandInstances = new List<CommandInstanceModel>();

        private SemaphoreSlim commandQueueLock = new SemaphoreSlim(1);

        private Dictionary<CommandTypeEnum, Task> perCommandTypeTasks = new Dictionary<CommandTypeEnum, Task>();
        private Dictionary<CommandTypeEnum, List<CommandInstanceModel>> perCommandTypeInstances = new Dictionary<CommandTypeEnum, List<CommandInstanceModel>>();

        private HashSet<ActionTypeEnum> perActionTypeInUse = new HashSet<ActionTypeEnum>();
        private List<Task> perActionTypeTasks = new List<Task>();
        private List<CommandInstanceModel> perActionTypeInstances = new List<CommandInstanceModel>();

        private Task singularTask = null;
        private List<CommandInstanceModel> singularInstances = new List<CommandInstanceModel>();

        public CommandService()
        {
            foreach (CommandTypeEnum type in EnumHelper.GetEnumList<CommandTypeEnum>())
            {
                perCommandTypeTasks[type] = null;
                perCommandTypeInstances[type] = new List<CommandInstanceModel>();
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
            CommandModelBase command = commandInstance.Command;
            if (command != null)
            {
                if (!command.IsEnabled || !command.HasWork)
                {
                    return;
                }

                await this.ValidateCommand(commandInstance);
            }

            lock (this.commandInstances)
            {
                this.commandInstances.Insert(0, commandInstance);
                while (this.commandInstances.Count > MaxCommandInstancesTracked)
                {
                    this.commandInstances.RemoveAt(this.commandInstances.Count - 1);
                }
            }

            if (commandInstance.State == CommandInstanceStateEnum.Pending)
            {
                CommandTypeEnum type = commandInstance.QueueCommandType;
                if (commandInstance.DontQueue)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsync(() => this.RunDirectly(commandInstance));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
                else
                {
                    await this.commandQueueLock.WaitAndRelease(() =>
                    {
                        if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerCommandType)
                        {
                            perCommandTypeInstances[type].Add(commandInstance);
                            if (perCommandTypeTasks[type] == null)
                            {
                                perCommandTypeTasks[type] = AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type));
                            }
                        }
                        else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerActionType)
                        {
                            this.perActionTypeInstances.Add(commandInstance);
                            if (this.CanCommandBeRunBasedOnActions(commandInstance))
                            {
                                this.perActionTypeTasks.Add(AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type)));
                            }
                        }
                        else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.VisualAudioActions)
                        {
                            HashSet<ActionTypeEnum> actionTypes = commandInstance.GetActionTypes();
                            if (actionTypes.Contains(ActionTypeEnum.Overlay) || actionTypes.Contains(ActionTypeEnum.OvrStream) || actionTypes.Contains(ActionTypeEnum.Sound) ||
                                actionTypes.Contains(ActionTypeEnum.StreamingSoftware) || actionTypes.Contains(ActionTypeEnum.TextToSpeech))
                            {
                                singularInstances.Add(commandInstance);
                                if (singularTask == null)
                                {
                                    singularTask = AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type));
                                }
                            }
                            else
                            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                AsyncRunner.RunAsync(() => this.RunDirectly(commandInstance));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            }
                        }
                        else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.Singular)
                        {
                            singularInstances.Add(commandInstance);
                            if (singularTask == null)
                            {
                                singularTask = AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type));
                            }
                        }
                        return Task.FromResult(0);
                    });
                }
            }

            this.OnCommandInstanceAdded(this, commandInstance);
        }

        public async Task RunDirectlyWithValidation(CommandInstanceModel commandInstance)
        {
            await this.ValidateCommand(commandInstance);
            await this.RunDirectly(commandInstance);
        }

        public async Task RunDirectly(CommandInstanceModel commandInstance)
        {
            try
            {
                if (commandInstance.State == CommandInstanceStateEnum.Canceled || commandInstance.State == CommandInstanceStateEnum.Completed)
                {
                    return;
                }

                CommandModelBase command = commandInstance.Command;
                if (command != null)
                {
                    if (!command.IsEnabled || !command.HasWork)
                    {
                        commandInstance.State = CommandInstanceStateEnum.Canceled;
                        return;
                    }

                    commandInstance.Parameters.SpecialIdentifiers[CommandModelBase.CommandNameSpecialIdentifier] = command.Name;

                    command.TrackTelemetry();
                }

                if (commandInstance.RunnerParameters.Count == 0)
                {
                    commandInstance.RunnerParameters = new List<CommandParametersModel>() { commandInstance.Parameters };
                }

                Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                commandInstance.State = CommandInstanceStateEnum.Running;

                foreach (CommandParametersModel p in commandInstance.RunnerParameters)
                {
                    p.User.Data.TotalCommandsRun++;
                    await this.RunDirectlyInternal(commandInstance, p);
                }

                if (commandInstance.State == CommandInstanceStateEnum.Running)
                {
                    commandInstance.State = CommandInstanceStateEnum.Completed;
                }
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
            await this.Queue(commandInstance.Duplicate());
        }

        private async Task<Result> ValidateCommand(CommandInstanceModel commandInstance)
        {
            Result validationResult = new Result();
            CommandModelBase command = commandInstance.Command;
            if (command != null)
            {
                validationResult = await command.CustomValidation(commandInstance.Parameters);
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
                        commandInstance.RunnerParameters = new List<CommandParametersModel>(command.GetPerformingUsers(commandInstance.Parameters));
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
                }
            }
            return validationResult;
        }

        private async Task BackgroundCommandTypeRunner(CommandTypeEnum type)
        {
            CommandInstanceModel instance = null;
            do
            {
                instance = await this.commandQueueLock.WaitAndRelease(() =>
                {
                    CommandInstanceModel commandInstance = null;
                    if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerCommandType)
                    {
                        if (perCommandTypeInstances.ContainsKey(type) && perCommandTypeInstances[type].Count > 0)
                        {
                            commandInstance = perCommandTypeInstances[type].RemoveFirst();
                        }
                        else
                        {
                            perCommandTypeTasks[type] = null;
                        }
                    }
                    else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerActionType)
                    {
                        if (instance != null)
                        {
                            foreach (ActionTypeEnum actionType in instance.GetActionTypes())
                            {
                                this.perActionTypeInUse.Remove(actionType);
                            }
                        }

                        commandInstance = this.perActionTypeInstances.FirstOrDefault(c => this.CanCommandBeRunBasedOnActions(c));
                        if (commandInstance != null)
                        {
                            this.perActionTypeInstances.Remove(commandInstance);
                            foreach (ActionTypeEnum actionType in commandInstance.GetActionTypes())
                            {
                                this.perActionTypeInUse.Add(actionType);
                            }
                        }
                    }
                    else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.VisualAudioActions ||
                        ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.Singular)
                    {
                        if (singularInstances.Count > 0)
                        {
                            commandInstance = singularInstances.RemoveFirst();
                        }
                        else
                        {
                            singularTask = null;
                        }
                    }
                    return Task.FromResult(commandInstance);
                });

                if (instance != null && instance.State == CommandInstanceStateEnum.Pending)
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

        private bool CanCommandBeRunBasedOnActions(CommandInstanceModel commandInstance)
        {
            HashSet<ActionTypeEnum> actionTypes = commandInstance.GetActionTypes();
            foreach (ActionTypeEnum actionType in actionTypes)
            {
                if (this.perActionTypeInUse.Contains(actionType))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
