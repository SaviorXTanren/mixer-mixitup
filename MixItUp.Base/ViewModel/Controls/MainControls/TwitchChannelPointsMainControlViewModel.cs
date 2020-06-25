using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class TwitchChannelPointsMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<TwitchChannelPointsCommand> Commands { get; private set; } = new ObservableCollection<TwitchChannelPointsCommand>();

        public ObservableCollection<MixPlayCommand> MixPlayCommands { get; set; } = new ObservableCollection<MixPlayCommand>();
        public MixPlayCommand SelectedMixPlayCommand
        {
            get { return this.selectedMixPlayCommand; }
            set
            {
                this.selectedMixPlayCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private MixPlayCommand selectedMixPlayCommand;

        public bool ImportMixPlayCommandButtonVisible { get { return this.MixPlayCommands.Count > 0; } }

        public TwitchChannelPointsMainControlViewModel(WindowViewModelBase windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.Commands.Clear();
            foreach (TwitchChannelPointsCommand command in ChannelSession.Settings.TwitchChannelPointsCommands)
            {
                this.Commands.Add(command);
            }

            this.MixPlayCommands.Clear();
            foreach (MixPlayCommand command in ChannelSession.Settings.OldMixPlayCommands)
            {
                this.MixPlayCommands.Add(command);
            }
            this.SelectedMixPlayCommand = null;
        }

        protected override Task OnLoadedInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }

        protected override Task OnVisibleInternal()
        {
            this.Refresh();
            return base.OnVisibleInternal();
        }
    }
}
