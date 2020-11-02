using MixItUp.Base.Model.Commands;
using MixItUp.Base.ViewModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class TwitchChannelPointsMainControlViewModel : WindowControlViewModelBase
    {
        public ObservableCollection<TwitchChannelPointsCommandModel> Commands { get; private set; } = new ObservableCollection<TwitchChannelPointsCommandModel>();

        public TwitchChannelPointsMainControlViewModel(UIViewModelBase windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.Commands.Clear();
            foreach (TwitchChannelPointsCommandModel command in ChannelSession.TwitchChannelPointsCommands)
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
