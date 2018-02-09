using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Controls.Currency;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyUserEditorWindow.xaml
    /// </summary>
    public partial class CurrencyUserEditorWindow : Window
    {
        private UserCurrencyViewModel currency;

        private List<UserCurrencyDataViewModel> allUserCurrencyData = new List<UserCurrencyDataViewModel>();

        private ObservableCollection<UserCurrencyIndividualEditorControl> userCurrencyControls = new ObservableCollection<UserCurrencyIndividualEditorControl>();

        public CurrencyUserEditorWindow(UserCurrencyViewModel currency)
        {
            this.currency = currency;

            InitializeComponent();

            this.Loaded += CurrencyUserEditorWindow_Loaded;
        }

        private void CurrencyUserEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var kvp in ChannelSession.Settings.UserData)
            {
                this.allUserCurrencyData.Add(kvp.Value.GetCurrency(this.currency));
            }

            this.UserCurrencyDataListView.ItemsSource = this.userCurrencyControls;

            this.RefreshData();
        }

        private void RefreshData()
        {
            string filter = this.UsernameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.LimitingResultsMessage.Visibility = Visibility.Collapsed;
            this.userCurrencyControls.Clear();
            foreach (UserCurrencyDataViewModel userCurrencyData in this.allUserCurrencyData)
            {
                if (string.IsNullOrEmpty(filter) || userCurrencyData.User.UserName.ToLower().Contains(filter))
                {
                    this.userCurrencyControls.Add(new UserCurrencyIndividualEditorControl(userCurrencyData));
                }

                if (this.userCurrencyControls.Count >= 200)
                {
                    this.LimitingResultsMessage.Visibility = Visibility.Visible;
                    break;
                }
            }
        }

        private void UsernameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshData();
        }
    }
}
