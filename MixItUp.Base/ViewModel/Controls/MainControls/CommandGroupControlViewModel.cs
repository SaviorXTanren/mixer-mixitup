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
            get { return this.isEnabled; }
            set
            {
                this.isEnabled = value;
                this.NotifyPropertyChanged();
                foreach (CommandBase command in this.Commands)
                {
                    command.IsEnabled = this.IsEnabled;
                }
            }
        }
        private bool isEnabled;

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
            this.GroupName = commands.First().GroupName;

            this.isEnabled = this.Commands.Any(c => c.IsEnabled);

            foreach (CommandBase command in commands)
            {
                this.Commands.Add(command);
            }
        }
    }
}
