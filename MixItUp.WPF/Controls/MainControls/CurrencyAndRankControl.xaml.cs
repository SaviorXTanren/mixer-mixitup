using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Currency;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CurrencyControl.xaml
    /// </summary>
    public partial class CurrencyAndRankControl : MainCommandControlBase
    {
        private ObservableCollection<CurrencyDataControl> currencyControls = new ObservableCollection<CurrencyDataControl>();

        public CurrencyAndRankControl()
        {
            InitializeComponent();

            this.CurrenciesListView.ItemsSource = this.currencyControls;
        }

        public async Task RefreshList()
        {
            this.currencyControls.Clear();
            foreach (UserCurrencyViewModel currency in ChannelSession.Settings.Currencies.Values.OrderBy(c => c.Name))
            {
                CurrencyDataControl control = await this.AddCurrency(currency);
                control.Minimize();
            }
        }

        public async Task DeleteCurrency(UserCurrencyViewModel currency)
        {
            if (!string.IsNullOrEmpty(currency.Name))
            {
                ChannelSession.Settings.Currencies.Remove(currency.Name);
                foreach (UserDataViewModel userData in ChannelSession.Settings.UserData.Values.ToList())
                {
                    userData.ResetCurrency(currency);
                }
            }
            await this.RefreshList();
        }

        protected override async Task InitializeInternal()
        {
            await this.RefreshList();
            await base.InitializeInternal();
        }

        private async void AddNewCurrencyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            await this.AddCurrency(new UserCurrencyViewModel());
        }

        private async Task<CurrencyDataControl> AddCurrency(UserCurrencyViewModel currency)
        {
            CurrencyDataControl control = new CurrencyDataControl(this, currency);
            await control.Initialize(this.Window);
            this.currencyControls.Add(control);
            return control;
        }
    }
}
