using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class GamesMainControlViewModel : WindowControlViewModelBase
    {
        public bool NoCurrenciesExist { get { return ChannelSession.Settings.Currency.Count() == 0; } }

        public CurrencyModel PrimaryCurrency
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.IsPrimary);
                if (currency != null)
                {
                    currency = ChannelSession.Settings.Currency.Values.FirstOrDefault();
                }
                return currency;
            }
        }

        public ObservableCollection<GameCommandModelBase> GameCommands { get; private set; } = new ObservableCollection<GameCommandModelBase>();

        public GamesMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.GameCommands.Clear();
            foreach (GameCommandModelBase gameCommand in ChannelSession.GameCommands)
            {
                this.GameCommands.Add(gameCommand);
            }
            this.NotifyPropertyChanged("NoCurrenciesExist");
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
