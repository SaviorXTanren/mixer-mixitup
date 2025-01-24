using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.Base.ViewModel.Requirements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Commands
{
    public abstract class CommandEditorWindowViewModelBase : ActionEditorListControlViewModel
    {
        public const string MixItUpCommandFileExtension = ".miucommand";

        public static string OpenCommandFileBrowser()
        {
            return ServiceManager.Get<IFileService>().ShowOpenFileDialog(string.Format("Mix It Up Command (*{0})|*{0}|All files (*.*)|*.*", MixItUpCommandFileExtension));
        }

        public static async Task<CommandModelBase> ImportCommandFromFile(string filePath)
        {
            return await ImportCommandFromFile<CommandModelBase>(filePath);
        }

        public static async Task<T> ImportCommandFromFile<T>(string filePath) where T : CommandModelBase
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                T command = await FileSerializerHelper.DeserializeFromFile<T>(filePath);
                SettingsV3Upgrader.UpdateActionsV7(ChannelSession.Settings, command.Actions);
                return command;
            }
            return null;
        }

        public static async Task<bool> ExportCommandToFile(CommandModelBase command)
        {
            if (command != null)
            {
                string fileName = ServiceManager.Get<IFileService>().ShowSaveFileDialog(command.Name + MixItUpCommandFileExtension, MixItUp.Base.Resources.MixItUpCommandFileFormatFilter);
                if (!string.IsNullOrEmpty(fileName))
                {
                    await FileSerializerHelper.SerializeToFile(fileName, command);
                    return true;
                }
            }
            return false;
        }

        public static async Task TestCommandWithTestCommandParameters(CommandModelBase command)
        {
            Dictionary<string, string> testSpecialIdentifiers = command.GetTestSpecialIdentifiers();
            CommandParametersModel parameters = CommandParametersModel.GetTestParameters(testSpecialIdentifiers);
            if (testSpecialIdentifiers != null && testSpecialIdentifiers.Count > 0)
            {
                parameters = await DialogHelper.ShowEditTestCommandParametersDialog(parameters);
                if (parameters == null)
                {
                    return;
                }
            }

            if (parameters.UseCommandLocks)
            {
                await ServiceManager.Get<CommandService>().Queue(new CommandInstanceModel(command, parameters));
            }
            else
            {
                await ServiceManager.Get<CommandService>().RunDirectly(new CommandInstanceModel(command, parameters));
                if (command.Requirements.Cooldown != null)
                {
                    command.Requirements.Cooldown.Reset();
                }
            }
        }

        public static async Task TestCommandWithTestCommandParameters(IEnumerable<ActionModelBase> actions, Dictionary<string, string> testSpecialIdentifiers)
        {
            CommandParametersModel parameters = CommandParametersModel.GetTestParameters(testSpecialIdentifiers);
            if (testSpecialIdentifiers != null && testSpecialIdentifiers.Count > 0)
            {
                parameters = await DialogHelper.ShowEditTestCommandParametersDialog(parameters);
                if (parameters == null)
                {
                    return;
                }
            }

            if (parameters.UseCommandLocks)
            {
                await ServiceManager.Get<CommandService>().Queue(new CommandInstanceModel(actions, parameters));
            }
            else
            {
                await ServiceManager.Get<CommandService>().RunDirectly(new CommandInstanceModel(actions, parameters));
            }
        }

        public CommandTypeEnum Type
        {
            get { return this.type; }
            set
            {
                this.type = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandTypeEnum type;

        public string Name
        {
            get { return this.name; }
            set
            {
                this.name = value;
                this.NotifyPropertyChanged();
            }
        }
        private string name;

        public IEnumerable<string> CommandGroups { get { return ChannelSession.Settings.CommandGroups.Keys.ToList().OrderBy(cg => cg); } }

        public string SelectedCommandGroup
        {
            get { return this.selectedCommandGroup; }
            set
            {
                this.selectedCommandGroup = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("CommandGroupTimerInterval");
                this.NotifyPropertyChanged("IsCommandGroupTimerIntervalEnabled");

                SelectedCommandGroupChanged();
            }
        }
        private string selectedCommandGroup;

        public bool Unlocked
        {
            get { return this.unlocked; }
            set
            {
                this.unlocked = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool unlocked;

        public RequirementsSetViewModel Requirements { get; set; } = new RequirementsSetViewModel();

        public ICommand SaveCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }

        public virtual bool IsExistingCommand { get { return this.existingCommand != null; } }

        public event EventHandler<CommandModelBase> CommandSaved = delegate { };

        protected CommandModelBase existingCommand;

        public CommandEditorWindowViewModelBase(CommandModelBase existingCommand)
            : this(existingCommand.Type)
        {
            this.existingCommand = existingCommand;

            this.Name = this.existingCommand.Name;
            this.SelectedCommandGroup = this.existingCommand.GroupName;
            this.Unlocked = this.existingCommand.Unlocked;
            this.Requirements = new RequirementsSetViewModel(this.existingCommand.Requirements);
        }

        public CommandEditorWindowViewModelBase(CommandTypeEnum type)
        {
            this.Type = type;

            this.SaveCommand = this.CreateCommand(async () =>
            {
                CommandModelBase command = await this.ValidateAndBuildCommand();
                if (command != null)
                {
                    if (!command.IsEmbedded)
                    {
                        ChannelSession.Settings.SetCommand(command);
                        await ChannelSession.SaveSettings();
                    }
                    await this.SaveCommandToSettings(command);
                    this.CommandSaved(this, command);
                }
            });

            this.TestCommand = this.CreateCommand(async () =>
            {
                if (!await this.CheckForResultErrors(await this.ValidateActions()))
                {
                    IEnumerable<ActionModelBase> actions = await this.GetActions();
                    if (actions != null)
                    {
                        await CommandEditorWindowViewModelBase.TestCommandWithTestCommandParameters(actions, this.GetTestSpecialIdentifiers());
                    }
                }
            });

            this.ExportCommand = this.CreateCommand(async () =>
            {
                await CommandEditorWindowViewModelBase.ExportCommandToFile(await this.ValidateAndBuildCommand());
            });

            this.ImportCommand = this.CreateCommand(async () =>
            {
                try
                {
                    string filename = CommandEditorWindowViewModelBase.OpenCommandFileBrowser();

                    if (string.IsNullOrEmpty(filename))
                    {
                        return;
                    }

                    CommandModelBase command = null;

                    // Check if the imported command type matches the currently edited command. If so, import additional information.
                    switch (this.Type)
                    {
                        case CommandTypeEnum.Webhook:
                            WebhookCommandModel webhookCommandModel = await CommandEditorWindowViewModelBase.ImportCommandFromFile<WebhookCommandModel>(filename);
                            command = webhookCommandModel;

                            WebhookCommandEditorWindowViewModel thisType = this as WebhookCommandEditorWindowViewModel;
                            if (webhookCommandModel != null && thisType != null)
                            {
                                // Merge JSON Mapping
                                foreach (var map in webhookCommandModel.JSONParameters)
                                {
                                    if (!thisType.JSONParameters.Any(j => j.JSONParameterName == map.JSONParameterName || j.SpecialIdentifierName == map.SpecialIdentifierName))
                                    {
                                        thisType.JSONParameters.Add(new WebhookJSONParameterViewModel(thisType) { JSONParameterName = map.JSONParameterName, SpecialIdentifierName = map.SpecialIdentifierName });
                                    }
                                }
                            }

                            break;
                    }

                    if (command == null)
                    {
                        command = await CommandEditorWindowViewModelBase.ImportCommandFromFile(filename);
                    }

                    if (command != null)
                    {
                        foreach (ActionModelBase action in command.Actions)
                        {
                            await this.AddAction(action);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    await DialogHelper.ShowMessage(MixItUp.Base.Resources.FailedToImportCommand);
                }
            });
        }

        public virtual bool CanBeUnlocked { get { return true; } }

        public virtual bool AddRequirementsToCommand { get { return false; } }

        public virtual bool CheckActionCount { get { return true; } }

        public abstract Task<Result> Validate();

        public abstract Task<CommandModelBase> CreateNewCommand();

        public virtual Task UpdateExistingCommand(CommandModelBase command)
        {
            command.Name = this.Name;
            return Task.CompletedTask;
        }

        public abstract Task SaveCommandToSettings(CommandModelBase command);

        protected override async Task OnOpenInternal()
        {
            if (this.existingCommand != null)
            {
                foreach (ActionModelBase action in this.existingCommand.Actions)
                {
                    await this.AddAction(action);
                }
            }
        }

        public async Task<CommandModelBase> ValidateAndBuildCommand()
        {
            List<Result> results = new List<Result>();

            results.Add(await this.Validate());
            results.AddRange(await this.Requirements.Validate());
            results.AddRange(await this.ValidateActions());
            if (this.CheckActionCount && this.Actions.Count == 0)
            {
                results.Add(new Result(MixItUp.Base.Resources.CommandMustHaveAtLeast1Action));
            }
            
            if (await this.CheckForResultErrors(results))
            {
                return null;
            }

            CommandModelBase command = this.existingCommand;
            if (command != null)
            {
                await this.UpdateExistingCommand(command);
            }
            else
            {
                command = await this.CreateNewCommand();
            }

            if (command == null)
            {
                return null;
            }
            command.Unlocked = this.Unlocked;

            if (!string.IsNullOrEmpty(this.SelectedCommandGroup))
            {
                if (!ChannelSession.Settings.CommandGroups.ContainsKey(this.SelectedCommandGroup))
                {
                    ChannelSession.Settings.CommandGroups[this.SelectedCommandGroup] = new CommandGroupSettingsModel(this.SelectedCommandGroup);
                }
                command.GroupName = this.SelectedCommandGroup;
                await this.UpdateCommandGroup();
            }
            else
            {
                command.GroupName = null;
            }

            if (this.AddRequirementsToCommand)
            {
                command.Requirements = this.Requirements.GetRequirements();
            }

            IEnumerable<ActionModelBase> actions = await this.GetActions();
            if (actions == null)
            {
                return null;
            }
            command.Actions = new List<ActionModelBase>(actions);

            return command;
        }

        protected virtual Task UpdateCommandGroup() { return Task.CompletedTask; }
        protected virtual void SelectedCommandGroupChanged() { }

        protected CommandGroupSettingsModel GetCommandGroup()
        {
            if (!string.IsNullOrEmpty(this.SelectedCommandGroup) && ChannelSession.Settings.CommandGroups.ContainsKey(this.SelectedCommandGroup))
            {
                return ChannelSession.Settings.CommandGroups[this.SelectedCommandGroup];
            }
            return null;
        }

        private async Task<bool> CheckForResultErrors(IEnumerable<Result> results)
        {
            if (results.Any(r => !r.Success))
            {
                await DialogHelper.ShowFailedResults(results);
                return true;
            }
            return false;
        }
    }
}
