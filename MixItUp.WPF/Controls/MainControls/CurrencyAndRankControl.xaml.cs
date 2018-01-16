using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Windows.Currency;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.MainControls
{
    /// <summary>
    /// Interaction logic for CurrencyControl.xaml
    /// </summary>
    public partial class CurrencyAndRankControl : MainControlBase
    {
        private ObservableCollection<UserCurrencyViewModel> currencies = new ObservableCollection<UserCurrencyViewModel>();
        private ObservableCollection<UserCurrencyViewModel> ranks = new ObservableCollection<UserCurrencyViewModel>();

        public CurrencyAndRankControl()
        {
            InitializeComponent();

            this.CurrenciesDataGrid.ItemsSource = this.currencies;
            this.RanksDataGrid.ItemsSource = this.ranks;
        }

        public void RefreshList()
        {
            this.currencies.Clear();
            this.ranks.Clear();
            foreach (var kvp in ChannelSession.Settings.Currencies.ToDictionary())
            {
                if (kvp.Value.IsRank)
                {
                    this.ranks.Add(kvp.Value);
                }
                else
                {
                    this.currencies.Add(kvp.Value);
                }
            }
        }

        public void DeleteCurrency(UserCurrencyViewModel currency)
        {
            if (!string.IsNullOrEmpty(currency.Name))
            {
                ChannelSession.Settings.Currencies.Remove(currency.Name);
                currency.Reset();
            }
            this.RefreshList();
        }

        protected override async Task InitializeInternal()
        {
            this.RefreshList();
            await base.InitializeInternal();
        }

        private void CurrencyEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserCurrencyViewModel currency = (UserCurrencyViewModel)button.DataContext;
            CurrencyWindow window = new CurrencyWindow(currency);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void CurrencyDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserCurrencyViewModel currency = (UserCurrencyViewModel)button.DataContext;
            this.DeleteCurrency(currency);
        }

        private void AddNewCurrencyButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            CurrencyWindow window = new CurrencyWindow(isRank: false);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void RankEditButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserCurrencyViewModel currency = (UserCurrencyViewModel)button.DataContext;
            CurrencyWindow window = new CurrencyWindow(currency);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void RankDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            UserCurrencyViewModel currency = (UserCurrencyViewModel)button.DataContext;
            this.DeleteCurrency(currency);
        }

        private void AddNewRankButton_Click(object sender, RoutedEventArgs e)
        {
            CurrencyWindow window = new CurrencyWindow(isRank: true);
            window.Closed += Window_Closed;
            window.Show();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }
    }
}
