using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Dialogs
{
    public class CommandSelectorDialogControlViewModel : UIViewModelBase
    {
        public IEnumerable<CommandTypeEnum> CommandTypes { get { return CommandModelBase.GetSelectableCommandTypes(); } }

        public CommandTypeEnum SelectedCommandType
        {
            get { return this.selectedCommandType; }
            set
            {
                this.selectedCommandType = value;
                this.NotifyPropertyChanged();

                this.Commands.Clear();
                foreach (CommandModelBase command in ChannelSession.AllCommands.Where(c => c.Type == this.SelectedCommandType).OrderBy(c => c.Name))
                {
                    this.Commands.Add(command);
                }
            }
        }
        private CommandTypeEnum selectedCommandType;

        public ObservableCollection<CommandModelBase> Commands { get; set; } = new ObservableCollection<CommandModelBase>();

        public CommandModelBase SelectedCommand
        {
            get { return this.selectedCommand; }
            set
            {
                this.selectedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase selectedCommand;

        public CommandSelectorDialogControlViewModel() { }
    }
}
