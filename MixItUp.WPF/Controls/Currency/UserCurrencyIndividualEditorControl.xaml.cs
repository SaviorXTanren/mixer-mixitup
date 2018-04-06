using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyUserEditorControl.xaml
    /// </summary>
    public partial class UserCurrencyIndividualEditorControl : UserControl
    {
        private UserDataViewModel userData;
        private UserCurrencyViewModel currency;

        public UserCurrencyIndividualEditorControl(UserDataViewModel userData, UserCurrencyViewModel currency)
        {
            this.userData = userData;
            this.currency = currency;

            InitializeComponent();

            this.Loaded += UserCurrencyIndividualEditorControl_Loaded;
        }

        private void UserCurrencyIndividualEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.UsernameTextBlock.Text = this.userData.UserName;
            this.AmountTextBox.Text = this.userData.GetCurrencyAmount(currency).ToString();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AmountTextBox.Text) && int.TryParse(this.AmountTextBox.Text, out int amount) && amount > 0)
            {
                this.userData.SetCurrencyAmount(currency, amount);
            }
        }
    }
}
