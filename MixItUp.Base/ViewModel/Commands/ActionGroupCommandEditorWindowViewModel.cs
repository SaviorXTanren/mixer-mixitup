using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class ActionGroupCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public ActionGroupCommandEditorWindowViewModel(ActionGroupCommandModel existingCommand)
            : base(existingCommand)
        { }

        public ActionGroupCommandEditorWindowViewModel() : base(CommandTypeEnum.ActionGroup) { }

        public override Task<Result> Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Name))
            {
                return Task.FromResult(new Result(MixItUp.Base.Resources.ACommandNameMustBeSpecified));
            }
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new ActionGroupCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            ServiceManager.Get<CommandService>().ActionGroupCommands.Remove((ActionGroupCommandModel)this.existingCommand);
            ServiceManager.Get<CommandService>().ActionGroupCommands.Add((ActionGroupCommandModel)command);
            return Task.CompletedTask;
        }
    }
}
