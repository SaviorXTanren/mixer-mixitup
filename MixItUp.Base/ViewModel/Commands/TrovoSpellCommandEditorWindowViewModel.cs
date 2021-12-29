using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class TrovoSpellCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public TrovoSpellCommandEditorWindowViewModel(TrovoSpellCommandModel existingCommand)
            : base(existingCommand)
        { }

        public TrovoSpellCommandEditorWindowViewModel() : base(CommandTypeEnum.TrovoSpell) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new TrovoSpellCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().TrovoSpellCommands.Remove((TrovoSpellCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().TrovoSpellCommands.Add((TrovoSpellCommandModel)command);
            return Task.CompletedTask;
        }
    }
}
