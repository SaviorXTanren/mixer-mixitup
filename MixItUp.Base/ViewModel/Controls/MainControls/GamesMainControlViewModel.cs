using MixItUp.Base.Commands;
using MixItUp.Base.ViewModel.Window;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.MainControls
{
    public class GamesMainControlViewModel : MainControlViewModelBase
    {
        public bool CurrenciesExist { get { return ChannelSession.Settings.Currencies.Values.Any(c => !c.IsRank); } }
        public bool NoCurrenciesMade { get { return !this.CurrenciesExist; } }

        public ObservableCollection<GameCommandBase> GameCommands { get; private set; } = new ObservableCollection<GameCommandBase>();

        public GamesMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.GameCommands.Clear();
            foreach (GameCommandBase gameCommand in ChannelSession.Settings.GameCommands)
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
