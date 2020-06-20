using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
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

        private InventoryModel inventory;
        private InventoryItemModel item;

        public UserCurrencyIndividualEditorControl(UserDataModel user, CurrencyModel currency)
            : this(user)
        {
            this.currency = currency;
        }

        public UserCurrencyIndividualEditorControl(UserDataModel user, InventoryModel inventory, InventoryItemModel item)
            : this(user)
        {
            this.inventory = inventory;
            this.item = item;
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
            else if (this.inventory != null && this.item != null)
            {
                this.NameTextBlock.Text = this.item.Name;
                this.AmountTextBox.Text = this.inventory.GetAmount(this.user, this.item).ToString();
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
                else if (this.inventory != null && this.item != null)
                {
                    this.inventory.SetAmount(this.user, this.item, amount);
                }
            }
        }
    }
}
