using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Actions;
using MixItUp.Base.ViewModel.Requirements;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Commands
{
    public abstract class CommandEditorWindowViewModelBase : ActionEditorListControlViewModel
    {
        public const string MixItUpCommandFileExtension = ".miucommand";

        public static async Task<CommandModelBase> ImportCommandFromFile()
        {
            string fileName = ChannelSession.Services.FileService.ShowOpenFileDialog(string.Format("Mix It Up Command (*.{0})|*.{0}|All files (*.*)|*.*", MixItUpCommandFileExtension));
            if (!string.IsNullOrEmpty(fileName))
            {
                return await FileSerializerHelper.DeserializeFromFile<CommandModelBase>(fileName);
            }
            return null;
        }

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

        public RequirementsSetViewModel Requirements { get; set; } = new RequirementsSetViewModel();

        public ICommand SaveCommand { get; private set; }
        public ICommand TestCommand { get; private set; }
        public ICommand ExportCommand { get; private set; }
        public ICommand ImportCommand { get; private set; }

        public event EventHandler<CommandModelBase> CommandSaved = delegate { };

        private CommandModelBase existingCommand;

        public CommandEditorWindowViewModelBase(CommandModelBase existingCommand)
            : this()
        {
            this.existingCommand = existingCommand;

            this.Name = this.existingCommand.Name;
            this.Requirements = new RequirementsSetViewModel(this.existingCommand.Requirements);
        }

        public CommandEditorWindowViewModelBase()
        {
            this.SaveCommand = this.CreateCommand(async (parameter) =>
            {
                CommandModelBase command = await this.ValidateAndBuildCommand();
                if (command != null)
                {
                    ChannelSession.Settings.SetCommand(command);
                    await ChannelSession.SaveSettings();

                    await this.SaveCommandToSettings(command);

                    this.CommandSaved(this, command);
                }
            });

            this.TestCommand = this.CreateCommand(async (parameter) =>
            {
                CommandModelBase command = await this.ValidateAndBuildCommand();
                if (command != null)
                {
                    await command.TestPerform();
                }
            });

            this.ExportCommand = this.CreateCommand(async (parameter) =>
            {
                CommandModelBase command = await this.ValidateAndBuildCommand();
                if (command != null)
                {
                    string fileName = ChannelSession.Services.FileService.ShowSaveFileDialog(this.Name + MixItUpCommandFileExtension);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        await FileSerializerHelper.SerializeToFile(fileName, command);
                    }
                }
            });

            this.ImportCommand = this.CreateCommand(async (parameter) =>
            {
                try
                {
                    CommandModelBase command = await CommandEditorWindowViewModelBase.ImportCommandFromFile();
                    if (command != null)
                    {
                        // TODO Check if the imported command type matches the currently edited command. If so, import additional information.
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

        public abstract Task<Result> Validate();

        public abstract Task<CommandModelBase> GetCommand();

        public abstract Task SaveCommandToSettings(CommandModelBase command);

        protected override async Task OnLoadedInternal()
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

            if (results.Any(r => !r.Success))
            {
                StringBuilder error = new StringBuilder();
                error.AppendLine(MixItUp.Base.Resources.TheFollowingErrorsMustBeFixed);
                error.AppendLine();
                foreach (Result result in results)
                {
                    if (!result.Success)
                    {
                        error.AppendLine(" - " + result.Message);
                    }
                }
                await DialogHelper.ShowMessage(error.ToString());
                return null;
            }

            CommandModelBase command = await this.GetCommand();
            if (command == null)
            {
                return null;
            }

            if (this.existingCommand != null)
            {
                command.ID = this.existingCommand.ID;
            }
            command.Requirements = this.Requirements.GetRequirements();

            IEnumerable<ActionModelBase> actions = await this.GetActions();
            if (actions == null)
            {
                return null;
            }
            command.Actions = new List<ActionModelBase>(actions);

            return command;
        }

        public override async Task<Dictionary<string, string>> GetUniqueSpecialIdentifiers()
        {
            CommandModelBase command = await this.ValidateAndBuildCommand();
            if (command != null)
            {
                return command.GetUniqueSpecialIdentifiers();
            }
            return null;
        }
    }
}
