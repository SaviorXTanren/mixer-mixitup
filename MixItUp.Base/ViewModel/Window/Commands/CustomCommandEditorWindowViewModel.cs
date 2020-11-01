using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Window.Commands
{
    public class CustomCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public CustomCommandEditorWindowViewModel(CustomCommandModel existingCommand)
            : base(existingCommand)
        {

        }

        public CustomCommandEditorWindowViewModel(string name)
            : this()
        {
            this.Name = name;
        }

        public CustomCommandEditorWindowViewModel() : base() { }

        public override Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> GetCommand() { return Task.FromResult<CommandModelBase>(new CustomCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command)
        {
            return Task.FromResult(0);
        }
    }
}
