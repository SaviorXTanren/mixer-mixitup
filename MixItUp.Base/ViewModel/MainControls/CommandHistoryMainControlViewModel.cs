using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class CommandHistoryMainControlViewModel : WindowControlViewModelBase
    {
        public event EventHandler UncheckSelectAll = delegate { };

        public IEnumerable<CommandTypeEnum> CommandTypes
        {
            get
            {
                List<CommandTypeEnum> types = new List<CommandTypeEnum>(EnumHelper.GetEnumList<CommandTypeEnum>());
                types.Remove(CommandTypeEnum.PreMade);
                types.Remove(CommandTypeEnum.UserOnlyChat);
#pragma warning disable CS0612 // Type or member is obsolete
                types.Insert(0, CommandTypeEnum.All);
#pragma warning restore CS0612 // Type or member is obsolete
                return types;
            }
        }

        public CommandTypeEnum SelectedCommandTypeFilter
        {
            get { return this.selectedCommandType; }
            set
            {
                this.selectedCommandType = value;
                this.NotifyPropertyChanged();

                this.RefreshList();
            }
        }
#pragma warning disable CS0612 // Type or member is obsolete
        private CommandTypeEnum selectedCommandType = CommandTypeEnum.All;
#pragma warning restore CS0612 // Type or member is obsolete

        public IEnumerable<CommandInstanceStateEnum> CommandStates
        {
            get
            {
                List<CommandInstanceStateEnum> states = new List<CommandInstanceStateEnum>(EnumHelper.GetEnumList<CommandInstanceStateEnum>());
#pragma warning disable CS0612 // Type or member is obsolete
                states.Insert(0, CommandInstanceStateEnum.All);
#pragma warning restore CS0612 // Type or member is obsolete
                return states;
            }
        }

        public CommandInstanceStateEnum SelectedCommandStateFilter
        {
            get { return this.selectedCommandStateFilter; }
            set
            {
                this.selectedCommandStateFilter = value;
                this.NotifyPropertyChanged();

                this.RefreshList();
            }
        }
#pragma warning disable CS0612 // Type or member is obsolete
        private CommandInstanceStateEnum selectedCommandStateFilter = CommandInstanceStateEnum.All;
#pragma warning restore CS0612 // Type or member is obsolete

        public string UsernameFilter
        {
            get { return this.usernameFilter; }
            set
            {
                this.usernameFilter = value;
                this.NotifyPropertyChanged();
            }
        }
        private string usernameFilter;

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

        public bool ShowPauseAllCommands { get { return !ServiceManager.Get<CommandService>().IsPaused; } }
        public bool ShowUnpauseAllCommands { get { return ServiceManager.Get<CommandService>().IsPaused; } }

        public ICommand CancelSelectedCommand { get; set; }
        public ICommand ReplaySelectedCommand { get; set; }
        public ICommand PauseAllCommandsCommand { get; set; }
        public ICommand UnpauseAllCommandsCommand { get; set; }

        private bool filterApplied = false;

        public CommandHistoryMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            this.CancelSelectedCommand = this.CreateCommand(() =>
            {
                foreach (CommandInstanceViewModel commandInstance in this.GetSelectedCommandInstances())
                {
                    ServiceManager.Get<CommandService>().Cancel(commandInstance.Model);
                    commandInstance.IsSelected = false;
                }
                this.ResetSelectedState();
            });

            this.ReplaySelectedCommand = this.CreateCommand(async () =>
            {
                foreach (CommandInstanceViewModel commandInstance in this.GetSelectedCommandInstances())
                {
                    await ServiceManager.Get<CommandService>().Replay(commandInstance.Model);
                    commandInstance.IsSelected = false;
                }
                this.ResetSelectedState();
            });

            this.PauseAllCommandsCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommandService>().Pause();
                this.NotifyPropertyChanged("ShowPauseAllCommands");
                this.NotifyPropertyChanged("ShowUnpauseAllCommands");
            });

            this.UnpauseAllCommandsCommand = this.CreateCommand(async () =>
            {
                await ServiceManager.Get<CommandService>().Unpause();
                this.NotifyPropertyChanged("ShowPauseAllCommands");
                this.NotifyPropertyChanged("ShowUnpauseAllCommands");
            });

            ServiceManager.Get<CommandService>().OnCommandInstanceAdded += Command_OnCommandInstanceAdded;

            this.RefreshList();
        }

        protected override Task OnVisibleInternal()
        {
            this.NotifyPropertyChanged("ShowPauseAllCommands");
            this.NotifyPropertyChanged("ShowUnpauseAllCommands");

            return base.OnVisibleInternal();
        }

        public void SetSelectedStateForAll(bool state)
        {
            foreach (CommandInstanceViewModel commandInstance in this.CommandInstances)
            {
                commandInstance.IsSelected = state;
            }
        }

        public void RefreshList()
        {
            IEnumerable<CommandInstanceModel> commandInstances = ServiceManager.Get<CommandService>().CommandInstances;

            commandInstances = commandInstances.Where(c => c.ShowInUI);

            this.filterApplied = false;

#pragma warning disable CS0612 // Type or member is obsolete
            if (this.SelectedCommandTypeFilter != CommandTypeEnum.All)
#pragma warning restore CS0612 // Type or member is obsolete
            {
                this.filterApplied = true;
                commandInstances = commandInstances.Where(c => c.QueueCommandType == this.SelectedCommandTypeFilter);
            }

#pragma warning disable CS0612 // Type or member is obsolete
            if (this.SelectedCommandStateFilter != CommandInstanceStateEnum.All)
#pragma warning restore CS0612 // Type or member is obsolete
            {
                this.filterApplied = true;
                commandInstances = commandInstances.Where(c => c.State == this.SelectedCommandStateFilter);
            }

            if (!string.IsNullOrEmpty(this.UsernameFilter))
            {
                this.filterApplied = true;
                commandInstances = commandInstances.Where(c => (c.Parameters?.User?.Username ?? string.Empty).Contains(this.UsernameFilter, StringComparison.OrdinalIgnoreCase));
            }

            this.CommandInstances.Clear();
            foreach (CommandInstanceModel commandInstance in commandInstances)
            {
                this.CommandInstances.Add(new CommandInstanceViewModel(commandInstance));
            }
        }

        private void Command_OnCommandInstanceAdded(object sender, CommandInstanceModel commandInstance)
        {
            if (!this.filterApplied && commandInstance.ShowInUI)
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
