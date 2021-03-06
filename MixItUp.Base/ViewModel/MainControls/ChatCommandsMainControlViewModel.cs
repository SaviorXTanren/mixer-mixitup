using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System.Collections.Generic;
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

        public ThreadSafeObservableCollection<PreMadeChatCommandControlViewModel> PreMadeChatCommands { get; set; } = new ThreadSafeObservableCollection<PreMadeChatCommandControlViewModel>();
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

            foreach (PreMadeChatCommandModelBase command in ChannelSession.PreMadeChatCommands.OrderBy(c => c.Name))
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
            if (ChannelSession.ChatCommands.Count != this.CommandGroups.Sum(cg => cg.Commands.Count))
            {
                this.FullRefresh();
            }
            await base.OnVisibleInternal();
        }

        protected override IEnumerable<CommandModelBase> GetCommands()
        {
            return ChannelSession.ChatCommands.ToList();
        }

        protected override void FilterCommands()
        {
            this.PreMadeChatCommands.Clear();
            foreach (PreMadeChatCommandControlViewModel command in ((string.IsNullOrEmpty(this.NameFilter)) ? this.allPreMadeChatCommands : this.allPreMadeChatCommands.Where(c => c.Name.ToLower().Contains(this.NameFilter.ToLower()))).OrderBy(c => c.Name))
            {
                this.PreMadeChatCommands.Add(command);
            }

            base.FilterCommands();
        }
    }
}
