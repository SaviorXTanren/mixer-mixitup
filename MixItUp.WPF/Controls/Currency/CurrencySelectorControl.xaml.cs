using MixItUp.Base.ViewModel.User;
using System.Windows.Controls;
using System.Threading.Tasks;
using MixItUp.Base;
using System.Collections.Generic;
using System.Linq;
using MaterialDesignThemes.Wpf;

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

        public int GetCurrencyMaximumAmount()
        {
            int currencyCost = 0;
            int.TryParse(this.CurrencyMaximumAmountTextBox.Text, out currencyCost);
            return currencyCost;
        }

        public void ShowMaximumAmountOption()
        {
            this.CurrencyMaximumAmountTextBox.Visibility = System.Windows.Visibility.Visible;
            HintAssist.SetHint(this.CurrencyCostTextBox, "Minimum Amount");
        }

        public UserCurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetCurrencyType() != null && this.GetCurrencyAmount() >= 0)
            {
                return new UserCurrencyRequirementViewModel(this.GetCurrencyType(), this.GetCurrencyAmount(), this.GetCurrencyMaximumAmount());
            }
            return null;
        }

        public void SetCurrencyRequirement(UserCurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currencies.ContainsKey(currencyRequirement.CurrencyName))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
                this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyName];

                this.CurrencyCostTextBox.IsEnabled = true;
                this.CurrencyCostTextBox.Text = currencyRequirement.RequiredAmount.ToString();

                this.CurrencyMaximumAmountTextBox.IsEnabled = true;
                this.CurrencyMaximumAmountTextBox.Text = (currencyRequirement.MaximumAmount > 0) ? currencyRequirement.MaximumAmount.ToString() : string.Empty;
                if (currencyRequirement.MaximumAmount > 0)
                {
                    this.ShowMaximumAmountOption();
                }
            }
        }

        public Task<bool> Validate()
        {
            return Task.FromResult(true);
        }

        protected override Task OnLoaded()
        {
            UserCurrencyRequirementViewModel requirement = this.GetCurrencyRequirement();

            if (ChannelSession.Settings != null)
            {
                IEnumerable<UserCurrencyViewModel> currencies = ChannelSession.Settings.Currencies.Values;
                this.IsEnabled = (currencies.Count() > 0);
                this.CurrencyTypeComboBox.ItemsSource = currencies;
            }

            this.SetCurrencyRequirement(requirement);

            return Task.FromResult(0);
        }

        protected override async Task OnVisibilityChanged()
        {
            await this.OnLoaded();
        }

        private void EnableDisableToggleSwitch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            this.CurrencyDataGrid.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
        }

        private void CurrencyTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.CurrencyCostTextBox.IsEnabled = (this.CurrencyTypeComboBox.SelectedIndex >= 0);
            this.CurrencyMaximumAmountTextBox.IsEnabled = (this.CurrencyTypeComboBox.SelectedIndex >= 0);
        }

        private void CurrencyCostTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int currencyCost = 0;
            if (!int.TryParse(this.CurrencyCostTextBox.Text, out currencyCost) || currencyCost < 0)
            {
                this.CurrencyCostTextBox.Text = "0";
            }
        }

        private void CurrencyMaximumAmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int currencyCost = 0;
            if (!int.TryParse(this.CurrencyMaximumAmountTextBox.Text, out currencyCost) || currencyCost < 0)
            {
                this.CurrencyMaximumAmountTextBox.Text = "0";
            }
        }
    }
}
