using MixItUp.Base.Commands;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class CommandGroupControlViewModel : ViewModelBase
    {
        public string GroupName { get; set; }
        public string DisplayName { get { return (!string.IsNullOrEmpty(this.GroupName)) ? this.GroupName : "Ungrouped"; } }

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

        public CommandGroupControlViewModel(IEnumerable<CommandBase> commands)
        {
            foreach (CommandBase command in commands)
            {
                this.Commands.Add(command);
            }

            this.GroupName = this.Commands.First().GroupName;
        }
    }
}
