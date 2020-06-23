using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class TwitchChannelPointsMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<TwitchChannelPointsCommand> Commands { get; private set; } = new ObservableCollection<TwitchChannelPointsCommand>();

        public TwitchChannelPointsMainControlViewModel(WindowViewModelBase windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.Commands.Clear();
            foreach (TwitchChannelPointsCommand command in ChannelSession.Settings.TwitchChannelPointsCommands)
            {
                this.Commands.Add(command);
            }
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
