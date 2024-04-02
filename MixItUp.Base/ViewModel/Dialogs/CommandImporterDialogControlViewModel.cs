using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MixItUp.Base.ViewModel.Dialogs
{
    public class CommandImporterDialogControlViewModel : UIViewModelBase
    {
        public bool IsNewCommandSelected
        {
            get { return this.isNewCommandSelected; }
            set
            {
                this.isNewCommandSelected = value;
                this.NotifyPropertyChanged();

                this.isExistingCommandSelected = false;
                this.NotifyPropertyChanged("IsExistingCommandSelected");
            }
        }
        private bool isNewCommandSelected = true;

        public IEnumerable<CommandTypeEnum> NewCommandTypes
        {
            get
            {
                List<CommandTypeEnum> commandTypes = new List<CommandTypeEnum>(CommandModelBase.GetSelectableCommandTypes());
                commandTypes.Remove(CommandTypeEnum.Event);
                commandTypes.Remove(CommandTypeEnum.Game);
                return commandTypes;
            }
        }

        public CommandTypeEnum SelectedNewCommandType
        {
            get { return this.selectedNewCommandType; }
            set
            {
                this.selectedNewCommandType = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandTypeEnum selectedNewCommandType = CommandTypeEnum.Chat;

        public bool IsExistingCommandSelected
        {
            get { return this.isExistingCommandSelected; }
            set
            {
                this.isExistingCommandSelected = value;
                this.NotifyPropertyChanged();

                this.isNewCommandSelected = false;
                this.NotifyPropertyChanged("IsNewCommandSelected");
            }
        }
        private bool isExistingCommandSelected;

        public IEnumerable<CommandTypeEnum> ExistingCommandTypes { get { return CommandModelBase.GetSelectableCommandTypes(); } }

        public CommandTypeEnum SelectedExistingCommandType
        {
            get { return this.selectedExistingCommandType; }
            set
            {
                this.selectedExistingCommandType = value;
                this.NotifyPropertyChanged();

                this.Commands.ClearAndAddRange(ServiceManager.Get<CommandService>().AllCommands.Where(c => c.Type == this.SelectedExistingCommandType).OrderBy(c => c.Name));
            }
        }
        private CommandTypeEnum selectedExistingCommandType;

        public ObservableCollection<CommandModelBase> Commands { get; set; } = new ObservableCollection<CommandModelBase>();

        public CommandModelBase SelectedExistingCommand
        {
            get { return this.selectedExistingCommand; }
            set
            {
                this.selectedExistingCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CommandModelBase selectedExistingCommand;

        public CommandImporterDialogControlViewModel() { }
    }
}