using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.MainControls
{
    public class GamesMainControlViewModel : WindowControlViewModelBase
    {
        public bool NoCurrenciesExist { get { return ChannelSession.Settings.Currency.Count(c => !c.Value.IsRank) == 0; } }

        public CurrencyModel PrimaryCurrency
        {
            get
            {
                CurrencyModel currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => c.IsPrimary && !c.IsRank);
                if (currency == null)
                {
                    currency = ChannelSession.Settings.Currency.Values.FirstOrDefault(c => !c.IsRank);
                }
                return currency;
            }
        }

        public ObservableCollection<GameCommandModelBase> GameCommands { get; private set; } = new ObservableCollection<GameCommandModelBase>();

        public GamesMainControlViewModel(MainWindowViewModel windowViewModel) : base(windowViewModel) { }

        public void Refresh()
        {
            this.GameCommands.ClearAndAddRange(ServiceManager.Get<CommandService>().GameCommands);
            this.NotifyPropertyChanged("NoCurrenciesExist");
        }

        protected override Task OnOpenInternal()
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
