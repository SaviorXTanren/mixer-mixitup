using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Commands
{
    public class TimerCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public TimerCommandEditorWindowViewModel(TimerCommandModel existingCommand)
            : base(existingCommand)
        {

        }

        public TimerCommandEditorWindowViewModel() : base() { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> GetCommand() { return Task.FromResult<CommandModelBase>(new TimerCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            TimerCommandModel c = (TimerCommandModel)command;
            if (!ChannelSession.TimerCommands.Contains(c))
            {
                ChannelSession.TimerCommands.Add(c);
            }
            return Task.FromResult(0);
        }
    }
}
