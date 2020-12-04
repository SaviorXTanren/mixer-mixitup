using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.ViewModel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class GamesMainControlViewModel : WindowControlViewModelBase
    {
        public bool CurrenciesExist { get { return ChannelSession.Settings.Currency.Count() > 0; } }
        public bool NoCurrenciesMade { get { return !this.CurrenciesExist; } }

        public ObservableCollection<GameCommandModelBase> GameCommands { get; private set; } = new ObservableCollection<GameCommandModelBase>();

        public GamesMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.GameCommands.Clear();
            foreach (GameCommandModelBase gameCommand in ChannelSession.GameCommands)
            {
                this.GameCommands.Add(gameCommand);
            }

            this.NotifyPropertyChanges();
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

        private void NotifyPropertyChanges()
        {
            this.NotifyPropertyChanged("NoCurrenciesMade");
            this.NotifyPropertyChanged("CurrenciesExist");
        }
    }
}
