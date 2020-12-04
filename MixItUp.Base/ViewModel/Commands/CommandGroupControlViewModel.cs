using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Commands
{
    public class CommandGroupControlViewModel : ControlViewModelBase
    {
        public CommandGroupSettingsModel GroupSettings { get; set; }

        public string GroupName { get { return (this.GroupSettings != null) ? this.GroupSettings.Name : null; } }
        public string DisplayName { get { return (!string.IsNullOrEmpty(this.GroupName)) ? this.GroupName : MixItUp.Base.Resources.Ungrouped; } }

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

                foreach (CommandModelBase command in ChannelSession.AllCommands)
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

        public SortableObservableCollection<CommandModelBase> Commands
        {
            get { return this.commands; }
            set
            {
                this.commands = value;
                this.NotifyPropertyChanged();
            }
        }
        private SortableObservableCollection<CommandModelBase> commands = new SortableObservableCollection<CommandModelBase>();

        private List<CommandModelBase> allCommands = new List<CommandModelBase>();

        public CommandGroupControlViewModel(CommandGroupSettingsModel groupSettings, IEnumerable<CommandModelBase> commands)
        {
            this.GroupSettings = groupSettings;
            this.allCommands.AddRange(commands);
            this.RefreshCommands();
        }

        public void AddCommand(CommandModelBase command)
        {
            this.allCommands.Add(command);
            this.Commands.SortedInsert(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RemoveCommand(CommandModelBase command)
        {
            this.allCommands.Remove(command);
            this.Commands.Remove(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RefreshCommands(string filter = null)
        {
            this.Commands.Clear();

            var matchedCommands = this.allCommands;
            if (!string.IsNullOrEmpty(filter))
            {
                matchedCommands = this.allCommands
                    .Where(
                        c => c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.Triggers.Any(t => t.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(c.GroupName) && c.GroupName.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            foreach (CommandModelBase command in matchedCommands)
            {
                this.Commands.SortedInsert(command);
            }
            this.NotifyPropertyChanged("HasCommands");
        }
    }
}
