using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Util;
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
        None
    }

    public class CommandService
    {
        private static readonly HashSet<ActionTypeEnum> VisualAudioActionTypes = new HashSet<ActionTypeEnum>()
        {
            ActionTypeEnum.Overlay, ActionTypeEnum.OvrStream, ActionTypeEnum.PolyPop, ActionTypeEnum.Sound, ActionTypeEnum.StreamingSoftware, ActionTypeEnum.TextToSpeech,
            ActionTypeEnum.MusicPlayer, ActionTypeEnum.TITS, ActionTypeEnum.Voicemod, ActionTypeEnum.VTubeStudio, ActionTypeEnum.VTSPog, ActionTypeEnum.LumiaStream,
            ActionTypeEnum.MtionStudio,
        };

        public static HashSet<ActionTypeEnum> GetActionTypesForActions(IEnumerable<ActionModelBase> actions, HashSet<Guid> commandIDs = null)
        {
            HashSet<ActionTypeEnum> actionTypes = new HashSet<ActionTypeEnum>();

            foreach (ActionModelBase action in actions)
            {
                if (action is GroupActionModel)
                {
                    GroupActionModel groupAction = (GroupActionModel)action;
                    foreach (ActionTypeEnum a in GetActionTypesForActions(groupAction.Actions, commandIDs))
                    {
                        actionTypes.Add(a);
                    }
                }

                if (action.Type == ActionTypeEnum.Command)
                {
                    CommandActionModel commandAction = (CommandActionModel)action;
                    if (commandAction.WaitForCommandToFinish)
                    {
                        CommandModelBase subCommand = ChannelSession.Settings.GetCommand(commandAction.CommandID);
                        if (subCommand != null)
                        {
                            foreach (ActionTypeEnum subActionType in subCommand.GetActionTypesInCommand(commandIDs))
                            {
                                actionTypes.Add(subActionType);
                            }
                        }
                    }
                }
                else if (action.Type == ActionTypeEnum.Overlay)
                {
                    OverlayActionModel overlayAction = (OverlayActionModel)action;
                    if (overlayAction.OverlayItemV3 != null)
                    {
                        if (overlayAction.OverlayItemV3.Type == OverlayItemV3Type.Video || overlayAction.OverlayItemV3.Type == OverlayItemV3Type.YouTube)
                        {
                            actionTypes.Add(ActionTypeEnum.Sound);
                        }
                    }
                }
                else if (action.Type == ActionTypeEnum.TextToSpeech)
                {
                    actionTypes.Add(ActionTypeEnum.Sound);
                }
                else if (action.Type == ActionTypeEnum.PolyPop || action.Type == ActionTypeEnum.StreamingSoftware || action.Type == ActionTypeEnum.MeldStudio)
                {
                    actionTypes.Add(ActionTypeEnum.Sound);
                    actionTypes.Add(ActionTypeEnum.Overlay);
                }
                actionTypes.Add(action.Type);
            }

            actionTypes.Remove(ActionTypeEnum.Command);
            actionTypes.Remove(ActionTypeEnum.Wait);

            return actionTypes;
        }

        public bool IsPaused { get; private set; }

        public event EventHandler<CommandInstanceModel> OnCommandInstanceAdded = delegate { };

        public List<PreMadeChatCommandModelBase> PreMadeChatCommands { get; private set; } = new List<PreMadeChatCommandModelBase>();
        public List<ChatCommandModel> ChatCommands { get; set; } = new List<ChatCommandModel>();
        public List<EventCommandModel> EventCommands { get; set; } = new List<EventCommandModel>();
        public List<TimerCommandModel> TimerCommands { get; set; } = new List<TimerCommandModel>();
        public List<ActionGroupCommandModel> ActionGroupCommands { get; set; } = new List<ActionGroupCommandModel>();
        public List<GameCommandModelBase> GameCommands { get; set; } = new List<GameCommandModelBase>();
        public List<TwitchChannelPointsCommandModel> TwitchChannelPointsCommands { get; set; } = new List<TwitchChannelPointsCommandModel>();
        public List<StreamlootsCardCommandModel> StreamlootsCardCommands { get; set; } = new List<StreamlootsCardCommandModel>();
        public List<WebhookCommandModel> WebhookCommands { get; set; } = new List<WebhookCommandModel>();
        public List<TrovoSpellCommandModel> TrovoSpellCommands { get; set; } = new List<TrovoSpellCommandModel>();
        public List<TwitchBitsCommandModel> TwitchBitsCommands { get; set; } = new List<TwitchBitsCommandModel>();
        public List<CrowdControlEffectCommandModel> CrowdControlEffectCommands { get; set; } = new List<CrowdControlEffectCommandModel>();

        public IEnumerable<CommandModelBase> AllEnabledChatAccessibleCommands
        {
            get
            {
                List<CommandModelBase> commands = new List<CommandModelBase>();
                commands.AddRange(this.PreMadeChatCommands.Where(c => c.IsEnabled));
                commands.AddRange(this.ChatCommands.Where(c => c.IsEnabled));
                commands.AddRange(this.GameCommands.Where(c => c.IsEnabled));
                return commands;
            }
        }

        public IEnumerable<CommandModelBase> AllCommands
        {
            get
            {
                List<CommandModelBase> commands = new List<CommandModelBase>();
                commands.AddRange(this.PreMadeChatCommands);
                commands.AddRange(this.ChatCommands);
                commands.AddRange(this.GameCommands);
                commands.AddRange(this.EventCommands);
                commands.AddRange(this.TimerCommands);
                commands.AddRange(this.ActionGroupCommands);
                commands.AddRange(this.TwitchChannelPointsCommands);
                commands.AddRange(this.StreamlootsCardCommands);
                commands.AddRange(this.WebhookCommands);
                commands.AddRange(this.TrovoSpellCommands);
                commands.AddRange(this.TwitchBitsCommands);
                commands.AddRange(this.CrowdControlEffectCommands);
                return commands;
            }
        }

        private List<CommandInstanceModel> pauseQueue = new List<CommandInstanceModel>();

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

            foreach (PreMadeChatCommandModelBase command in ReflectionHelper.CreateInstancesOfImplementingType<PreMadeChatCommandModelBase>())
            {
                this.PreMadeChatCommands.Add(command);
            }
        }

        public Task Initialize()
        {
            this.ChatCommands.Clear();
            this.EventCommands.Clear();
            this.TimerCommands.Clear();
            this.ActionGroupCommands.Clear();
            this.GameCommands.Clear();
            this.TwitchChannelPointsCommands.Clear();
            this.StreamlootsCardCommands.Clear();
            this.WebhookCommands.Clear();
            this.TrovoSpellCommands.Clear();
            this.TwitchBitsCommands.Clear();
            this.CrowdControlEffectCommands.Clear();

            foreach (CommandModelBase command in ChannelSession.Settings.Commands.Values.ToList())
            {
                if (command is ChatCommandModel)
                {
                    if (command is GameCommandModelBase) { this.GameCommands.Add((GameCommandModelBase)command); }
                    else if (command is UserOnlyChatCommandModel) { }
                    else { this.ChatCommands.Add((ChatCommandModel)command); }
                }
                else if (command is EventCommandModel) { this.EventCommands.Add((EventCommandModel)command); }
                else if (command is TimerCommandModel) { this.TimerCommands.Add((TimerCommandModel)command); }
                else if (command is ActionGroupCommandModel) { this.ActionGroupCommands.Add((ActionGroupCommandModel)command); }
                else if (command is TwitchChannelPointsCommandModel) { this.TwitchChannelPointsCommands.Add((TwitchChannelPointsCommandModel)command); }
                else if (command is StreamlootsCardCommandModel) { this.StreamlootsCardCommands.Add((StreamlootsCardCommandModel)command); }
                else if (command is WebhookCommandModel) { this.WebhookCommands.Add((WebhookCommandModel)command); }
                else if (command is TrovoSpellCommandModel) { this.TrovoSpellCommands.Add((TrovoSpellCommandModel)command); }
                else if (command is TwitchBitsCommandModel) { this.TwitchBitsCommands.Add((TwitchBitsCommandModel)command); }
                else if (command is CrowdControlEffectCommandModel) { this.CrowdControlEffectCommands.Add((CrowdControlEffectCommandModel)command); }
            }

            foreach (PreMadeChatCommandSettingsModel commandSetting in ChannelSession.Settings.PreMadeChatCommandSettings)
            {
                PreMadeChatCommandModelBase command = this.PreMadeChatCommands.FirstOrDefault(c => c.Name.Equals(commandSetting.Name));
                if (command != null)
                {
                    command.UpdateFromSettings(commandSetting);
                }
            }

            ServiceManager.Get<ChatService>().RebuildCommandTriggers();

            return Task.CompletedTask;
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
            if (commandInstance.Parameters.User != null)
            {
                commandInstance.Parameters.User.UpdateLastActivity();
            }

            CommandModelBase command = commandInstance.Command;
            if (command != null)
            {
                if (!command.IsEnabled || !command.HasWork)
                {
                    Logger.Log(LogLevel.Debug, $"Command is not enabled/has not work: {command.ID} - {command.Name}");
                    return;
                }

                await this.ValidateCommand(commandInstance);
            }

            lock (this.commandInstances)
            {
                this.commandInstances.Insert(0, commandInstance);
            }
            this.OnCommandInstanceAdded(this, commandInstance);


            if (commandInstance.State == CommandInstanceStateEnum.Pending)
            {
                if (this.IsPaused)
                {
                    this.pauseQueue.Add(commandInstance);
                }
                else
                {
                    await this.QueueInternal(commandInstance);
                }
            }
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
                if (commandInstance.State != CommandInstanceStateEnum.Pending)
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

                    Logger.Log(LogLevel.Debug, $"Starting command performing: {command.ID} - {command.Name}");
                }

                if (commandInstance.RunnerParameters.Count == 0)
                {
                    commandInstance.RunnerParameters = new List<CommandParametersModel>() { commandInstance.Parameters };
                }

                commandInstance.State = CommandInstanceStateEnum.Running;

                foreach (CommandParametersModel p in commandInstance.RunnerParameters)
                {
                    p.User.TotalCommandsRun++;
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

        public async Task Pause()
        {
            await this.commandQueueLock.WaitAsync();

            this.IsPaused = true;

            this.commandQueueLock.Release();
        }

        public async Task Unpause()
        {
            await this.commandQueueLock.WaitAsync();

            this.IsPaused = false;

            this.commandQueueLock.Release();

            foreach (CommandInstanceModel commandInstance in this.pauseQueue)
            {
                await this.QueueInternal(commandInstance);
            }
            this.pauseQueue.Clear();
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
                    if (!commandInstance.Parameters.IgnoreRequirements)
                    {
                        validationResult = await command.ValidateRequirements(commandInstance.Parameters);
                        if (!validationResult.Success && ChannelSession.Settings.RequirementErrorsCooldownType != RequirementErrorCooldownTypeEnum.PerCommand)
                        {
                            command.CommandErrorCooldown = RequirementModelBase.UpdateErrorCooldown();
                        }
                    }
                }
                else
                {
                    if (ChannelSession.Settings.RequirementErrorsCooldownType != RequirementErrorCooldownTypeEnum.None)
                    {
                        if (!string.IsNullOrEmpty(validationResult.Message) && validationResult.DisplayMessage)
                        {
                            await ServiceManager.Get<ChatService>().SendMessage(validationResult.Message, commandInstance.Parameters);
                        }
                    }
                }

                if (validationResult.Success)
                {
                    if (command.Requirements != null && !commandInstance.Parameters.IgnoreRequirements)
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

            Logger.Log(LogLevel.Debug, $"Command validation status: {command.ID} - {command.Name} - {validationResult.ToString()}");

            return validationResult;
        }

        private async Task QueueInternal(CommandInstanceModel commandInstance)
        {
            Logger.Log(LogLevel.Debug, $"Starting command queuing: {commandInstance.CommandID} - {commandInstance.Name}");

            CommandTypeEnum type = commandInstance.QueueCommandType;
            if (commandInstance.DontQueue)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsync(() => this.RunDirectly(commandInstance));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            else
            {
                try
                {
                    await this.commandQueueLock.WaitAsync();

                    if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerCommandType)
                    {
                        this.perCommandTypeInstances[type].Add(commandInstance);
                        if (this.perCommandTypeTasks[type] == null)
                        {
                            this.perCommandTypeTasks[type] = AsyncRunner.RunAsync(() => this.BackgroundCommandTypeRunner(type));
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
                        if (actionTypes.Any(a => VisualAudioActionTypes.Contains(a)))
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
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    this.commandQueueLock.Release();
                }
            }
        }

        private async Task BackgroundCommandTypeRunner(CommandTypeEnum type)
        {
            CommandInstanceModel currentInstance = null;
            do
            {
                try
                {
                    await this.commandQueueLock.WaitAsync();

                    CommandInstanceModel newInstance = null;
                    if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerCommandType)
                    {
                        if (this.perCommandTypeInstances.ContainsKey(type) && this.perCommandTypeInstances[type].Count > 0)
                        {
                            newInstance = this.perCommandTypeInstances[type].RemoveFirst();
                        }
                        else
                        {
                            this.perCommandTypeTasks[type] = null;
                        }
                    }
                    else if (ChannelSession.Settings.CommandServiceLockType == CommandServiceLockTypeEnum.PerActionType)
                    {
                        if (currentInstance != null)
                        {
                            foreach (ActionTypeEnum actionType in currentInstance.GetActionTypes())
                            {
                                this.perActionTypeInUse.Remove(actionType);
                            }
                        }

                        newInstance = this.perActionTypeInstances.FirstOrDefault(c => this.CanCommandBeRunBasedOnActions(c));
                        if (newInstance != null)
                        {
                            this.perActionTypeInstances.Remove(newInstance);
                            foreach (ActionTypeEnum actionType in newInstance.GetActionTypes())
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
                            newInstance = this.singularInstances.RemoveFirst();
                        }
                        else
                        {
                            this.singularTask = null;
                        }
                    }

                    currentInstance = newInstance;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
                finally
                {
                    this.commandQueueLock.Release();
                }

                if (currentInstance != null && currentInstance.State == CommandInstanceStateEnum.Pending)
                {
                    if (this.IsPaused)
                    {
                        this.pauseQueue.Add(currentInstance);
                    }
                    else
                    {
                        await this.RunDirectly(currentInstance);
                    }
                }

            } while (currentInstance != null);
        }

        private async Task RunDirectlyInternal(CommandInstanceModel commandInstance, CommandParametersModel parameters)
        {
            ServiceManager.Get<StatisticsService>().LogStatistic(StatisticItemTypeEnum.Command, description: commandInstance.ID.ToString());

            CommandModelBase command = commandInstance.Command;
            if (command != null)
            {
                if (parameters.InitialCommandID == Guid.Empty)
                {
                    parameters.InitialCommandID = command.ID;
                }

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
                    if (ServiceManager.Get<OverlayV3Service>().IsConnected)
                    {
                        if (action is OverlayActionModel)
                        {
                            ServiceManager.Get<OverlayV3Service>().StartBatching();
                        }
                        else
                        {
                            await ServiceManager.Get<OverlayV3Service>().EndBatching();
                        }
                    }

                    await action.Perform(parameters);

                    if (parameters.ExitCommand)
                    {
                        break;
                    }
                }

                if (ServiceManager.Get<OverlayV3Service>().IsConnected)
                {
                    await ServiceManager.Get<OverlayV3Service>().EndBatching();
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
