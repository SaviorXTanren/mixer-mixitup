using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
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
        public IEnumerable<CommandInstanceModel> Instances { get { return this.instances; } }
        private List<CommandInstanceModel> instances = new List<CommandInstanceModel>();

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

        public async Task Run(CommandInstanceModel commandInstance)
        {
            this.instances.Add(commandInstance);

            CommandTypeEnum type = commandInstance.CommandType;
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
                    if (commandTypeTasks.ContainsKey(type) && commandTypeTasks[type] != null)
                    {
                        commandTypeInstances[type].Add(commandInstance);
                    }
                    else
                    {
                        commandTypeTasks[type] = AsyncRunner.RunAsync(() => this.RunCommandTypeInstance(type));
                    }
                    return Task.FromResult(0);
                });
            }
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
                    commandInstance.Parameters.SpecialIdentifiers[CommandModelBase.CommandNameSpecialIdentifier] = command.Name;
                    ChannelSession.Services.Telemetry.TrackCommand(command.Type);

                    if (await command.CustomValidation() && await command.ValidateRequirements(commandInstance.Parameters))
                    {
                        if (command.Requirements != null)
                        {
                            await command.PerformRequirements(commandInstance.Parameters);
                            runnerParameters = new List<CommandParametersModel>(command.GetPerformingUsers(commandInstance.Parameters));
                        }
                    }
                }

                Logger.Log(LogLevel.Debug, $"Starting command performing: {this}");

                commandInstance.State = CommandInstanceStateEnum.Running;

                foreach (CommandParametersModel p in runnerParameters)
                {
                    p.User.Data.TotalCommandsRun++;
                    await this.RunActionsDirectly(commandInstance, p);
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

        private async Task RunCommandTypeInstance(CommandTypeEnum type)
        {
            CommandInstanceModel instance;
            do
            {
                instance = await this.commandTypeLock.WaitAndRelease(() =>
                {
                    if (commandTypeInstances.ContainsKey(type))
                    {
                        return Task.FromResult(commandTypeInstances[type].FirstOrDefault());
                    }
                    else
                    {
                        commandTypeTasks[type] = null;
                        return Task.FromResult<CommandInstanceModel>(null);
                    }
                });

                if (instance != null)
                {
                    await this.RunDirectly(instance);
                }

            } while (instance != null);
        }

        private async Task RunActionsDirectly(CommandInstanceModel commandInstance, CommandParametersModel parameters)
        {
            CommandModelBase command = commandInstance.Command;
            if (command != null && command.HasCustomPerform)
            {
                await commandInstance.Command.CustomPerform(parameters);
            }
            else
            {
                List<ActionModelBase> actions = new List<ActionModelBase>(commandInstance.Actions);
                if (command != null)
                {
                    actions.AddRange(command.Actions);
                }

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
        }
    }
}
