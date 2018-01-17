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
            if (!string.IsNullOrEmpty(this.CurrencyCostTextBox.Text) && !int.TryParse(this.CurrencyCostTextBox.Text, out currencyCost))
            {
                currencyCost = -1;
            }
            return currencyCost;
        }

        public UserCurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetCurrencyType() != null && this.GetCurrencyAmount() >= 0)
            {
                return new UserCurrencyRequirementViewModel(this.GetCurrencyType(), this.GetCurrencyAmount());
            }
            return null;
        }

        public void SetCurrencyRequirement(UserCurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currencies.ContainsKey(currencyRequirement.CurrencyID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values;
                this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyID];

                this.CurrencyCostTextBox.IsEnabled = true;
                this.CurrencyCostTextBox.Text = currencyRequirement.RequiredAmount.ToString();
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
        }
    }
}
