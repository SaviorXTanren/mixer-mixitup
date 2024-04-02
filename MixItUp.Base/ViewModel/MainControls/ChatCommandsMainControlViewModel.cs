using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class ChatCommandsMainControlViewModel : GroupedCommandsMainControlViewModelBase
    {
        public bool CustomCommandsSelected
        {
            get { return this.customCommandsSelected; }
            set
            {
                this.customCommandsSelected = value;
                this.NotifyPropertyChanged();
                this.NotifyPropertyChanged("PreMadeCommandSelected");
            }
        }
        private bool customCommandsSelected = true;

        public bool PreMadeCommandSelected { get { return !this.CustomCommandsSelected; } }

        public ObservableCollection<PreMadeChatCommandControlViewModel> PreMadeChatCommands { get; set; } = new ObservableCollection<PreMadeChatCommandControlViewModel>();
        private List<PreMadeChatCommandControlViewModel> allPreMadeChatCommands = new List<PreMadeChatCommandControlViewModel>();

        public ICommand SwitchToPreMadeCommands { get; set; }
        public ICommand SwitchToCustomCommands { get; set; }

        public ChatCommandsMainControlViewModel(MainWindowViewModel windowViewModel)
            : base(windowViewModel)
        {
            GroupedCommandsMainControlViewModelBase.OnCommandAddedEdited += GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited;

            this.SwitchToPreMadeCommands = this.CreateCommand(() =>
            {
                this.CustomCommandsSelected = false;
            });

            this.SwitchToCustomCommands = this.CreateCommand(() =>
            {
                this.CustomCommandsSelected = true;
            });
        }

        protected override async Task OnOpenInternal()
        {
            await base.OnOpenInternal();

            foreach (PreMadeChatCommandModelBase command in ServiceManager.Get<CommandService>().PreMadeChatCommands.OrderBy(c => c.Name))
            {
                this.allPreMadeChatCommands.Add(new PreMadeChatCommandControlViewModel(command));
            }

            this.PreMadeChatCommands.ClearAndAddRange(this.allPreMadeChatCommands);

            if (this.CommandGroups.Count == 0)
            {
                this.CustomCommandsSelected = false;
            }
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ServiceManager.Get<CommandService>().ChatCommands.ToList();
        }

        protected override void FilterCommands()
        {
            List<PreMadeChatCommandControlViewModel> commands = new List<PreMadeChatCommandControlViewModel>();
            foreach (PreMadeChatCommandControlViewModel command in ((string.IsNullOrEmpty(this.NameFilter)) ? this.allPreMadeChatCommands : this.allPreMadeChatCommands.Where(c => c.Name.ToLower().Contains(this.NameFilter.ToLower()))).OrderBy(c => c.Name))
            {
                commands.Add(command);
            }
            this.PreMadeChatCommands.ClearAndAddRange(commands);

            base.FilterCommands();
        }

        private void GroupedCommandsMainControlViewModelBase_OnCommandAddedEdited(object sender, CommandModelBase command)
        {
            if (command.Type == CommandTypeEnum.Chat)
            {
                this.AddCommand(command);
            }
        }
    }
}
