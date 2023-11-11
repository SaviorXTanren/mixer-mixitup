using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class TrovoSpellsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ICommand CustomSpellsEditorCommand { get; set; }

        public TrovoSpellsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;

            this.CustomSpellsEditorCommand = this.CreateCommand(() =>
            {
                ServiceManager.Get<IProcessService>().LaunchLink($"https://studio.trovo.live/mychannel/customization");
            });
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ServiceManager.Get<CommandService>().TrovoSpellCommands.ToList();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.TrovoSpell)
            {
                this.AddCommand(command);
            }
        }
    }
}
