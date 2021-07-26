using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Commands
{
    public class CustomCommandEditorWindowViewModel : CommandEditorWindowViewModelBase
    {
        public CustomCommandEditorWindowViewModel(CustomCommandModel existingCommand) : base(existingCommand) { }

        public CustomCommandEditorWindowViewModel() : base(CommandTypeEnum.Custom) { }

        public override bool CheckActionCount { get { return false; } }

        public override Task<Result> Validate()
        {
            return Task.FromResult(new Result());
        }

        public override Task<CommandModelBase> CreateNewCommand() { return Task.FromResult<CommandModelBase>(new CustomCommandModel(this.Name)); }

        public override Task SaveCommandToSettings(CommandModelBase command) { return Task.CompletedTask; }

        public override Dictionary<string, string> GetTestSpecialIdentifiers() { return CustomCommandModel.GetCustomTestSpecialIdentifiers(this.Name); }
    }
}
