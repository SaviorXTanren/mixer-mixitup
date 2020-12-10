using MixItUp.Base.Model.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.MainControls
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
