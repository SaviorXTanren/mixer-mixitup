using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Windows.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyUserEditorWindow.xaml
    /// </summary>
    public partial class CurrencyUserEditorWindow : Window
    {
        private UserCurrencyViewModel currency;

        private List<UserCurrencyDataViewModel> allUserCurrencyData = new List<UserCurrencyDataViewModel>();
        private ObservableCollection<UserCurrencyDataViewModel> userCurrencyData = new ObservableCollection<UserCurrencyDataViewModel>();

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

            this.UserCurrencyDataDataGrid.ItemsSource = this.userCurrencyData;

            this.RefreshData();
        }

        private void RefreshData()
        {
            string filter = this.UsernameFilterTextBox.Text;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
            }

            this.userCurrencyData.Clear();
            foreach (UserCurrencyDataViewModel userCurrencyData in this.allUserCurrencyData)
            {
                if (string.IsNullOrEmpty(filter) || userCurrencyData.User.UserName.ToLower().Contains(filter))
                {
                    this.userCurrencyData.Add(userCurrencyData);
                }
            }
        }

        private void UsernameFilterTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            this.RefreshData();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            UserCurrencyDataViewModel currencyData = (UserCurrencyDataViewModel)textBox.DataContext;
            if (!string.IsNullOrEmpty(textBox.Text) && int.TryParse(textBox.Text, out int amount) && amount >= 0)
            {
                currencyData.Amount = amount;
            }
        }
    }
}
