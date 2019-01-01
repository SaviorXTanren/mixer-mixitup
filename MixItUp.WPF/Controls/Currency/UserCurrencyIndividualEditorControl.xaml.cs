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

        private UserInventoryItemViewModel inventoryItem;
        private UserInventoryDataViewModel inventoryData;

        public UserCurrencyIndividualEditorControl(UserCurrencyDataViewModel currencyData)
            : this()
        {
            this.currencyData = currencyData;
        }

        public UserCurrencyIndividualEditorControl(UserInventoryItemViewModel inventoryItem, UserInventoryDataViewModel inventoryData)
            : this()
        {
            this.inventoryItem = inventoryItem;
            this.inventoryData = inventoryData;
        }

        private UserCurrencyIndividualEditorControl()
        {
            InitializeComponent();

            this.Loaded += UserCurrencyIndividualEditorControl_Loaded;
        }

        private void UserCurrencyIndividualEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.currencyData != null)
            {
                this.NameTextBlock.Text = this.currencyData.Currency.Name;
                this.AmountTextBox.Text = this.currencyData.Amount.ToString();
            }
            else if (this.inventoryItem != null && this.inventoryData != null)
            {
                this.NameTextBlock.Text = this.inventoryItem.Name;
                if (this.inventoryData.Amounts.ContainsKey(this.inventoryItem.Name))
                {
                    this.AmountTextBox.Text = this.inventoryData.Amounts[this.inventoryItem.Name].ToString();
                }
                else
                {
                    this.AmountTextBox.Text = "0";
                }
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AmountTextBox.Text) && int.TryParse(this.AmountTextBox.Text, out int amount) && amount >= 0)
            {
                if (this.currencyData != null)
                {
                    this.currencyData.Amount = amount;
                }
                else if (this.inventoryItem != null && this.inventoryData != null)
                {
                    this.inventoryData.Amounts[this.inventoryItem.Name] = amount;
                }
            }
        }
    }
}
