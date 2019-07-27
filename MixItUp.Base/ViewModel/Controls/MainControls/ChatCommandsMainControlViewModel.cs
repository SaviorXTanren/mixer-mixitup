using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Controls.Chat;
using MixItUp.Base.ViewModel.Window;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.MainControls
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
            this.SwitchToPreMadeCommands = this.CreateCommand((parameter) =>
            {
                this.CustomCommandsSelected = false;
                return Task.FromResult(0);
            });

            this.SwitchToCustomCommands = this.CreateCommand((parameter) =>
            {
                this.CustomCommandsSelected = true;
                return Task.FromResult(0);
            });
        }

        protected override async Task OnLoadedInternal()
        {
            await base.OnLoadedInternal();

            foreach (PreMadeChatCommand command in ChannelSession.PreMadeChatCommands.OrderBy(c => c.Name))
            {
                this.allPreMadeChatCommands.Add(new PreMadeChatCommandControlViewModel(command));
            }

            foreach (PreMadeChatCommandControlViewModel command in this.allPreMadeChatCommands)
            {
                this.PreMadeChatCommands.Add(command);
            }

            if (this.CommandGroups.Count == 0)
            {
                this.CustomCommandsSelected = false;
            }
        }

        protected override async Task OnVisibleInternal()
        {
            if (ChannelSession.Settings.ChatCommands.Count != this.CommandGroups.Sum(cg => cg.Commands.Count))
            {
                this.FullRefresh();
            }
            await base.OnVisibleInternal();
        }

        protected override IEnumerable<CommandBase> GetCommands()
        {
            return ChannelSession.Settings.ChatCommands.ToList();
        }

        protected override void FilterCommands()
        {
            this.PreMadeChatCommands.Clear();
            foreach (PreMadeChatCommandControlViewModel command in ((string.IsNullOrEmpty(this.NameFilter)) ? this.allPreMadeChatCommands : this.allPreMadeChatCommands.Where(c => c.Name.Contains(this.NameFilter))).OrderBy(c => c.Name))
            {
                this.PreMadeChatCommands.Add(command);
            }

            base.FilterCommands();
        }
    }
}
