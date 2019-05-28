using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class ActionGroupMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ActionGroupMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override IEnumerable<CommandBase> GetCommands()
        {
            return ChannelSession.Settings.ActionGroupCommands.ToList();
        }
    }
}
