using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommandHistoryMainControlViewModel : WindowControlViewModelBase
    {
        public event EventHandler UncheckSelectAll = delegate { };

        public ThreadSafeObservableCollection<CommandInstanceViewModel> CommandInstances { get; set; } = new ThreadSafeObservableCollection<CommandInstanceViewModel>();

        public bool SelectAll
        {
            get { return this.selectAll; }
            set
            {
                this.selectAll = value;
                this.NotifyPropertyChanged();

                foreach (CommandInstanceViewModel commandInstance in this.CommandInstances)
                {
                    commandInstance.IsSelected = this.SelectAll;
                }
            }
        }
        private bool selectAll;

        public ICommand CancelSelectedCommand { get; set; }

        public ICommand ReplaySelectedCommand { get; set; }

        private bool filterApplied = false;

        public CommandHistoryMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            ChannelSession.Services.Command.OnCommandInstanceAdded += Command_OnCommandInstanceAdded;

            this.RefreshList();

            this.CancelSelectedCommand = this.CreateCommand(() =>
            {
                foreach (CommandInstanceViewModel commandInstance in this.GetSelectedCommandInstances())
                {
                    ChannelSession.Services.Command.Cancel(commandInstance.Model);
                    commandInstance.IsSelected = false;
                }
                this.ResetSelectedState();
            });

            this.ReplaySelectedCommand = this.CreateCommand(async () =>
            {
                foreach (CommandInstanceViewModel commandInstance in this.GetSelectedCommandInstances())
                {
                    await ChannelSession.Services.Command.Replay(commandInstance.Model);
                    commandInstance.IsSelected = false;
                }
                this.ResetSelectedState();
            });
        }

        public void SetSelectedStateForAll(bool state)
        {
            foreach (CommandInstanceViewModel commandInstance in this.CommandInstances)
            {
                commandInstance.IsSelected = state;
            }
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

        private IEnumerable<CommandInstanceViewModel> GetSelectedCommandInstances()
        {
            return this.CommandInstances.ToList().Where(c => c.IsSelected);
        }

        private void ResetSelectedState()
        {
            this.UncheckSelectAll(this, new EventArgs());
        }
    }
}
