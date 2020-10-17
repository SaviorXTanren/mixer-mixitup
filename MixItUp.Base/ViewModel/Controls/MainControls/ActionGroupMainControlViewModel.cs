using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class ActionGroupMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public ActionGroupMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.ActionGroupCommands.ToList();
        }
    }
}
