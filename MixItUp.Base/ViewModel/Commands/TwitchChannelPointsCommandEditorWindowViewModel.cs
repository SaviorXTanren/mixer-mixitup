using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TwitchChannelPointsCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public TwitchChannelPointsCommandEditorWindowViewModel(TwitchChannelPointsCommandModel existingCommand)
            : base(existingCommand)
        { }

        public TwitchChannelPointsCommandEditorWindowViewModel() : base(CommandTypeEnum.TwitchChannelPoints) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrEmpty(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> GetCommand() { return Task.FromResult<CommandModelBase>(new TwitchChannelPointsCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ChannelSession.TwitchChannelPointsCommands.Remove((TwitchChannelPointsCommandModel)this.existingCommand);
            ChannelSession.TwitchChannelPointsCommands.Add((TwitchChannelPointsCommandModel)command);
            return Task.FromResult(0);
        }
    }
}
