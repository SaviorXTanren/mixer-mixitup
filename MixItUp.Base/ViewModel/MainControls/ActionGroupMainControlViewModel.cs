using MixItUp.Base.Model.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class ActionGroupMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public static event EventHandler<ActionGroupCommandModel> OnActionGroupAddedEdited = delegate { };

        public static void ActionGroupAddedEdited(ActionGroupCommandModel command)
        {
            ActionGroupMainControlViewModel.OnActionGroupAddedEdited(null, command);
        }

        public ActionGroupMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            ActionGroupMainControlViewModel.OnActionGroupAddedEdited += ActionGroupMainControlViewModel_OnActionGroupAddedEdited;
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.ActionGroupCommands.ToList();
        }

        private void ActionGroupMainControlViewModel_OnActionGroupAddedEdited(object sender, ActionGroupCommandModel command)
        {
            this.AddCommand(command);
        }
    }
}
