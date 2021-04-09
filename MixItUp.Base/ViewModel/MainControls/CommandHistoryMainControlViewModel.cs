using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommandHistoryMainControlViewModel : WindowControlViewModelBase
    {
        public ThreadSafeObservableCollection<CommandInstanceViewModel> CommandInstances { get; set; } = new ThreadSafeObservableCollection<CommandInstanceViewModel>();

        private bool filterApplied = false;

        public CommandHistoryMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            ChannelSession.Services.Command.OnCommandInstanceAdded += Command_OnCommandInstanceAdded;

            this.RefreshList();
        }

        private void RefreshList()
        {
            IEnumerable<CommandInstanceModel> commandInstances = ChannelSession.Services.Command.CommandInstances;

            if (this.filterApplied)
            {

            }

            foreach (CommandInstanceModel commandInstance in commandInstances)
            {
                this.CommandInstances.Add(new CommandInstanceViewModel(commandInstance));
            }
        }

        private void Command_OnCommandInstanceAdded(object sender, CommandInstanceModel commandInstance)
        {
            if (!this.filterApplied)
            {
                this.CommandInstances.Insert(0, new CommandInstanceViewModel(commandInstance));
            }
        }
    }
}
