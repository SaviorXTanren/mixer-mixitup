using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.Commands
{
    public class CommandGroupControlViewModel : ControlViewModelBase
    {
        public CommandGroupSettings GroupSettings { get; set; }

        public string GroupName { get { return (this.GroupSettings != null) ? this.GroupSettings.Name : null; } }
        public string DisplayName { get { return (!string.IsNullOrEmpty(this.GroupName)) ? this.GroupName : "Ungrouped"; } }

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

                this.NotifyPropertyChanged();
            }
        }

        public bool IsEnableSwitchToggable { get { return !string.IsNullOrEmpty(this.GroupName); } }

        public ObservableCollection<CommandBase> Commands
        {
            get { return this.commands; }
            set
            {
                this.commands = value;
                this.NotifyPropertyChanged();
            }
        }
        private ObservableCollection<CommandBase> commands = new ObservableCollection<CommandBase>();

        public CommandGroupControlViewModel(CommandGroupSettings groupSettings, IEnumerable<CommandBase> commands)
        {
            this.GroupSettings = groupSettings;
            foreach (CommandBase command in commands)
            {
                this.Commands.Add(command);
            }
        }
    }
}
