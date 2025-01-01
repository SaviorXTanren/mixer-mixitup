using MixItUp.Base.Model.Actions;
using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Actions
{
    public class CommandActionEditorControlViewModel : ActionEditorControlViewModelBase
    {
        public override ActionTypeEnum Type { get { return ActionTypeEnum.Command; } }

        public IEnumerable<CommandActionTypeEnum> ActionTypes { get { return EnumHelper.GetEnumList<CommandActionTypeEnum>(); } }

        public CommandActionTypeEnum SelectedActionType
        {
            get { return this.selectedActionType; }
            set
            {
                this.selectedActionType = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged(nameof(this.ShowCommandsSection));
                this.NotifyPropertyChanged(nameof(this.ShowCommandRunSection));
                this.NotifyPropertyChanged(nameof(this.ShowCommandGroupsSection));
                this.NotifyPropertyChanged(nameof(this.ShowCommandCooldownSection));
            }
        }
        private CommandActionTypeEnum selectedActionType;

        public bool ShowCommandsSection
        {
            get
            {
                return this.SelectedActionType == CommandActionTypeEnum.RunCommand || this.SelectedActionType == CommandActionTypeEnum.EnableCommand ||
                    this.SelectedActionType == CommandActionTypeEnum.DisableCommand || this.SelectedActionType == CommandActionTypeEnum.ToggleCommand ||
                    this.SelectedActionType == CommandActionTypeEnum.SetCommandCooldown;
            }
        }

        public IEnumerable<CommandTypeEnum> CommandTypes
        {
            get
            {
                List<CommandTypeEnum> types = new List<CommandTypeEnum>(EnumHelper.GetEnumList<CommandTypeEnum>());
                types.Remove(CommandTypeEnum.UserOnlyChat);
                types.Remove(CommandTypeEnum.Custom);
                return types.OrderBy(c => EnumLocalizationHelper.GetLocalizedName(c));
            }
        }

        public CommandTypeEnum SelectedCommandType
        {
            get { return this.selectedCommandType; }
            set
            {
                this.selectedCommandType = value;
                this.SelectedCommand = null;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("Commands");
            }
        }
        private CommandTypeEnum selectedCommandType;

        public IEnumerable<CommandModelBase> Commands
        {
            get
            {
                if (this.SelectedCommandType == CommandTypeEnum.PreMade)
                {
                    return ServiceManager.Get<CommandService>().PreMadeChatCommands.OrderBy(c => c.Name);
                }
                else
                {
                    return ServiceManager.Get<CommandService>().AllCommands.Where(c => c.Type == this.SelectedCommandType).OrderBy(c => c.Name);
                }
            }
        }

        public CommandModelBase SelectedCommand
        {
            get { return this.selectedCommand; }
            set
            {
                this.selectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase selectedCommand;

        public bool ShowCommandRunSection { get { return this.SelectedActionType == CommandActionTypeEnum.RunCommand; } }

        public string CommandArguments
        {
            get { return this.commandArguments; }
            set
            {
                this.commandArguments = value;
                this.NotifyPropertyChanged();
            }
        }
        private string commandArguments;

        public bool WaitForCommandToFinish
        {
            get { return this.waitForCommandToFinish; }
            set
            {
                this.waitForCommandToFinish = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool waitForCommandToFinish = true;

        public bool IgnoreUsageRequirements
        {
            get { return this.ignoreUsageRequirements; }
            set
            {
                this.ignoreUsageRequirements = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool ignoreUsageRequirements = true;

        public bool ShowCommandCooldownSection { get { return this.SelectedActionType == CommandActionTypeEnum.SetCommandCooldown; } }

        public bool ShowCommandGroupsSection { get { return this.SelectedActionType == CommandActionTypeEnum.EnableCommandGroup || this.SelectedActionType == CommandActionTypeEnum.DisableCommandGroup; } }

        public IEnumerable<string> CommandGroups { get { return ChannelSession.Settings.CommandGroups.Keys.ToList(); } }

        public string SelectedCommandGroup
        {
            get { return this.selectedCommandGroup; }
            set
            {
                this.selectedCommandGroup = value;
                this.NotifyPropertyChanged();
            }
        }
        private string selectedCommandGroup;

        public CommandActionEditorControlViewModel(CommandActionModel action)
            : base(action)
        {
            this.SelectedActionType = action.ActionType;
            if (this.ShowCommandsSection)
            {
                if (action.Command != null)
                {
                    this.SelectedCommandType = action.Command.Type;
                    this.SelectedCommand = action.Command;
                }
                this.CommandArguments = action.Arguments;
                this.WaitForCommandToFinish = action.WaitForCommandToFinish;
                this.IgnoreUsageRequirements = action.IgnoreUsageRequirements;
            }
            else if (this.ShowCommandGroupsSection)
            {
                this.SelectedCommandGroup = action.CommandGroupName;
            }
        }

        public CommandActionEditorControlViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (this.ShowCommandsSection)
            {
                if (this.SelectedCommand == null)
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.CommandActionSelectCommand));
                }
            }
            else if (this.ShowCommandGroupsSection)
            {
                if (string.IsNullOrEmpty(this.SelectedCommandGroup))
                {
                    return Task.FromResult(new Result(MixItUp.Base.Resources.CommandActionSelectCommandGroup));
                }
            }
            return Task.FromResult(new Result());
        }

        protected override Task<ActionModelBase> GetActionInternal()
        {
            if (this.ShowCommandsSection)
            {
                return Task.FromResult<ActionModelBase>(new CommandActionModel(this.SelectedActionType, this.SelectedCommand, this.CommandArguments, this.WaitForCommandToFinish, this.IgnoreUsageRequirements));
            }
            else if (this.ShowCommandGroupsSection)
            {
                return Task.FromResult<ActionModelBase>(new CommandActionModel(this.SelectedActionType, this.SelectedCommandGroup));
            }
            else
            {
                return Task.FromResult<ActionModelBase>(new CommandActionModel(this.SelectedActionType));
            }
        }
    }
}
