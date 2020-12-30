using MixItUp.Base.Model.Commands;
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
                CommandGroupSettingsModel commandGroup = this.GetCommandGroup();
                if (commandGroup != null && commandGroup.TimerInterval != this.commandGroupTimerInterval)
                {
                    this.commandGroupTimerInterval = commandGroup.TimerInterval;
                }
                return this.commandGroupTimerInterval;
            }
            set
            {
                this.commandGroupTimerInterval = value;
                this.NotifyPropertyChanged();
            }
        }
        private int commandGroupTimerInterval;

        public TimerCommandEditorWindowViewModel(TimerCommandModel existingCommand) : base(existingCommand) { }

        public TimerCommandEditorWindowViewModel() : base(CommandTypeEnum.Timer) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new TimerCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ChannelSession.TimerCommands.Remove((TimerCommandModel)this.existingCommand);
            ChannelSession.TimerCommands.Add((TimerCommandModel)command);
            return Task.FromResult(0);
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
    }
}
