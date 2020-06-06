using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencyUserEditorControl.xaml
    /// </summary>
    public partial class UserCurrencyIndividualEditorControl : UserControl
    {
        private UserDataModel user;

        private CurrencyModel currency;

        private UserInventoryItemModel inventoryItem;
        private UserInventoryDataViewModel inventoryData;

        public UserCurrencyIndividualEditorControl(UserDataModel user, CurrencyModel currency)
            : this(user)
        {
            this.currency = currency;
        }

        public UserCurrencyIndividualEditorControl(UserDataModel user, UserInventoryItemModel inventoryItem, UserInventoryDataViewModel inventoryData)
            : this(user)
        {
            this.inventoryItem = inventoryItem;
            this.inventoryData = inventoryData;
        }

        private UserCurrencyIndividualEditorControl(UserDataModel user)
        {
            this.user = user;

            InitializeComponent();

            this.Loaded += UserCurrencyIndividualEditorControl_Loaded;
        }

        private void UserCurrencyIndividualEditorControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.currency != null)
            {
                this.NameTextBlock.Text = this.currency.Name;
                this.AmountTextBox.Text = this.currency.GetAmount(this.user).ToString();
            }
            else if (this.inventoryItem != null && this.inventoryData != null)
            {
                this.NameTextBlock.Text = this.inventoryItem.Name;
                this.AmountTextBox.Text = this.inventoryData.GetAmount(this.inventoryItem).ToString();
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.AmountTextBox.Text) && int.TryParse(this.AmountTextBox.Text, out int amount) && amount >= 0)
            {
                if (this.currency != null)
                {
                    this.currency.SetAmount(this.user, amount);
                }
                else if (this.inventoryItem != null && this.inventoryData != null)
                {
                    this.inventoryData.SetAmount(this.inventoryItem, amount);
                }
            }
        }
    }
}
