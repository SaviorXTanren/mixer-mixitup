using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
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

        public async void DeleteCurrency(UserCurrencyViewModel currency)
        {
            await this.Window.RunAsyncOperation(async () =>
            {
                if (await MessageBoxHelper.ShowConfirmationDialog("Are you sure you wish to delete this?"))
                {
                    ChannelSession.Settings.Currencies.Remove(currency.ID);
                    currency.Reset();
                    this.RefreshList();
                }
            });
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

        private void Window_Closed(object sender, System.EventArgs e)
        {
            this.RefreshList();
        }

        private void AddNewCurrencyRankButton_Click(object sender, RoutedEventArgs e)
        {
            CurrencyWindow window = new CurrencyWindow();
            window.Closed += Window_Closed;
            window.Show();
        }
    }
}
