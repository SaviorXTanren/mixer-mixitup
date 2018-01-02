using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Threading.Tasks;
using MixItUp.Base;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for CurrencySelectorControl.xaml
    /// </summary>
    public partial class CurrencySelectorControl : LoadingControlBase
    {
        public CurrencySelectorControl()
        {
            InitializeComponent();
        }

        public UserCurrencyViewModel GetCurrencyType() { return (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem; }

        public int GetCurrencyAmount()
        {
            int currencyCost = 0;
            int.TryParse(this.CurrencyCostTextBox.Text, out currencyCost);
            return currencyCost;
        }

        public UserCurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.GetCurrencyType() != null && this.GetCurrencyAmount() >= 0)
            {
                return new UserCurrencyRequirementViewModel(this.GetCurrencyType(), this.GetCurrencyAmount());
            }
            return null;
        }

        public void SetCurrencyRequirement(UserCurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currencies.ContainsKey(currencyRequirement.CurrencyName))
            {
                this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
                this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyName];
                this.CurrencyCostTextBox.Text = currencyRequirement.RequiredAmount.ToString();
            }
        }

        public Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        protected override Task OnLoaded()
        {
            if (ChannelSession.Settings.Currencies.Count > 0)
            {
                this.IsEnabled = true;

                UserCurrencyViewModel currencyType = (UserCurrencyViewModel)this.CurrencyTypeComboBox.SelectedItem;
                int currencyAmount = 0;
                int.TryParse(this.CurrencyCostTextBox.Text, out currencyAmount);

                this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
                this.CurrencyTypeComboBox.SelectedItem = currencyType;
                this.CurrencyCostTextBox.Text = currencyAmount.ToString();
            }
            else
            {
                this.IsEnabled = false;
            }
            return Task.FromResult(0);
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.OnLoaded();
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CurrencyCostTextBox.IsEnabled = (this.CurrencyTypeComboBox.SelectedIndex >= 0);
        }

        private void CurrencyCostTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int currencyCost = 0;
            if (!int.TryParse(this.CurrencyCostTextBox.Text, out currencyCost) || currencyCost < 0)
            {
                this.CurrencyCostTextBox.Text = "0";
            }
        }
    }
}
