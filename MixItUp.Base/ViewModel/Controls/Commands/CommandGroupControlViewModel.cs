using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Commands
{
    public class CommandGroupControlViewModel : ControlViewModelBase
    {
        public CommandGroupSettings GroupSettings { get; set; }

        public string GroupName { get { return (this.GroupSettings != null) ? this.GroupSettings.Name : null; } }
        public string DisplayName { get { return (!string.IsNullOrEmpty(this.GroupName)) ? this.GroupName : "Ungrouped"; } }

        public bool HasCommands { get { return this.Commands.Count > 0; } }

        public bool IsMinimized
        {
            get { return (this.GroupSettings != null) ? this.GroupSettings.IsMinimized : false; }
            set
            {
                if (this.GroupSettings != null)
                {
                    this.GroupSettings.IsMinimized = value;
                }
            }
        }

        public bool IsEnabled
        {
            get { return this.Commands.Any(c => c.IsEnabled); }
            set
            {
                bool newIsEnabledState = !this.IsEnabled;

                foreach (CommandBase command in ChannelSession.AllCommands)
                {
                    if (this.GroupName.Equals(command.GroupName))
                    {
                        command.IsEnabled = newIsEnabledState;
                    }
                }

                this.RefreshCommands();

                this.NotifyPropertyChanged();
            }
        }

        public bool IsEnableSwitchToggable { get { return !string.IsNullOrEmpty(this.GroupName); } }

        public SortableObservableCollection<CommandBase> Commands
        {
            get { return this.commands; }
            set
            {
                this.commands = value;
                this.NotifyPropertyChanged();
            }
        }
        private SortableObservableCollection<CommandBase> commands = new SortableObservableCollection<CommandBase>();

        private List<CommandBase> allCommands = new List<CommandBase>();

        public CommandGroupControlViewModel(CommandGroupSettings groupSettings, IEnumerable<CommandBase> commands)
        {
            this.GroupSettings = groupSettings;
            this.allCommands.AddRange(commands);
            this.RefreshCommands();
        }

        public void AddCommand(CommandBase command)
        {
            this.allCommands.Add(command);
            this.Commands.SortedInsert(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RemoveCommand(CommandBase command)
        {
            this.allCommands.Remove(command);
            this.Commands.Remove(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RefreshCommands(string filter = null)
        {
            this.Commands.Clear();

            foreach (CommandBase command in ((string.IsNullOrEmpty(filter)) ? this.allCommands : this.allCommands.Where(c => c.Name.ToLower().Contains(filter))))
            {
                this.Commands.SortedInsert(command);
            }
            this.NotifyPropertyChanged("HasCommands");
        }
    }
}
