using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            get { return this.isMinimized; }
            set
            {
                this.isMinimized = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool isMinimized = true;

        public bool IsEnabled
        {
            get { return this.isEnabled; }
            set
            {
                bool newIsEnabledState = !this.IsEnabled;
                foreach (CommandModelBase command in this.Commands)
                {
                    command.IsEnabled = newIsEnabledState;
                    ChannelSession.Settings.Commands.ManualValueChanged(command.ID);
                }
                this.RefreshCommands();
                this.NotifyPropertyChanged();
            }
        }
        private bool isEnabled;

        public bool IsEnableSwitchToggable { get { return !string.IsNullOrEmpty(this.GroupName); } }

        public ObservableCollection<CommandModelBase> Commands
        {
            get { return this.commands; }
            set
            {
                this.commands = value;
                this.NotifyPropertyChanged();
            }
        }
        private ObservableCollection<CommandModelBase> commands = new ObservableCollection<CommandModelBase>();

        private Dictionary<Guid, CommandModelBase> commandLookup = new Dictionary<Guid, CommandModelBase>();

        public CommandGroupControlViewModel(CommandGroupSettingsModel groupSettings, IEnumerable<CommandModelBase> commands)
        {
            this.GroupSettings = groupSettings;
            foreach (CommandModelBase command in commands)
            {
                this.commandLookup[command.ID] = command;
            }
            this.RefreshCommands();
        }

        public bool HasCommand(CommandModelBase command) { return this.commandLookup.ContainsKey(command.ID); }

        public void AddCommand(CommandModelBase command)
        {
            if (this.commandLookup.ContainsKey(command.ID))
            {
                this.RemoveCommand(this.commandLookup[command.ID]);
            }
            this.commandLookup[command.ID] = command;
            this.Commands.SortedInsert(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RemoveCommand(CommandModelBase command)
        {
            this.commandLookup.Remove(command.ID);
            this.Commands.Remove(command);
            this.NotifyPropertyChanged("HasCommands");
        }

        public void RefreshCommands(string filter = null)
        {
            this.Commands.Clear();

            var matchedCommands = this.commandLookup.Values.ToList();
            if (!string.IsNullOrEmpty(filter))
            {
                matchedCommands = matchedCommands
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

            this.isEnabled = this.Commands.Any(c => c.IsEnabled);
            this.NotifyPropertyChanged("IsEnabled");
        }
    }
}
