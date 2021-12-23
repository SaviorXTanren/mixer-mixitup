using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TimerCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public int CommandGroupTimerInterval
        {
            get
            {
                return this.commandGroupTimerInterval;
            }
            set
            {
                this.commandGroupTimerInterval = value;
                this.NotifyPropertyChanged();
            }
        }
        private int commandGroupTimerInterval;

        public bool IsCommandGroupTimerIntervalEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(SelectedCommandGroup);
            }
        }

        public TimerCommandEditorWindowViewModel(TimerCommandModel existingCommand) : base(existingCommand)
        {
            this.SelectedCommandGroupChanged();
        }

        public TimerCommandEditorWindowViewModel() : base(CommandTypeEnum.Timer) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }

            if (this.CommandGroupTimerInterval < 0)
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.CommandGroupTimerIntervalMustBePositive));
            }

            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new TimerCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().TimerCommands.Remove((TimerCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().TimerCommands.Add((TimerCommandModel)command);
            return Task.CompletedTask;
        }

        protected override async Task UpdateCommandGroup()
        {
            await base.UpdateCommandGroup();

            CommandGroupSettingsModel commandGroup = this.GetCommandGroup();
            if (commandGroup != null)
            {
                commandGroup.TimerInterval = this.CommandGroupTimerInterval;
            }
        }

        protected override void SelectedCommandGroupChanged()
        {
            CommandGroupSettingsModel commandGroup = this.GetCommandGroup();
            if (commandGroup != null && commandGroup.TimerInterval != this.commandGroupTimerInterval)
            {
                this.CommandGroupTimerInterval = commandGroup.TimerInterval;
            }
        }
    }
}
