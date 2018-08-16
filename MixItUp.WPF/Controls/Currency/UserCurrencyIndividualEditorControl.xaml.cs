using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyUserEditorControl.xaml
    /// </summary>
    public partial class UserCurrencyIndividualEditorControl : UserControl
    {
        private UserCurrencyDataViewModel currencyData;

        public UserCurrencyIndividualEditorControl(UserCurrencyDataViewModel currencyData)
        {
            this.currencyData = currencyData;

            InitializeComponent();

            this.Loaded += UserCurrencyIndividualEditorControl_Loaded;
        }

        private void UserCurrencyIndividualEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            this.NameTextBlock.Text = this.currencyData.Currency.Name;
            this.AmountTextBox.Text = this.currencyData.Amount.ToString();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AmountTextBox.Text) && int.TryParse(this.AmountTextBox.Text, out int amount) && amount >= 0)
            {
                this.currencyData.Amount = amount;
            }
        }
    }
}
