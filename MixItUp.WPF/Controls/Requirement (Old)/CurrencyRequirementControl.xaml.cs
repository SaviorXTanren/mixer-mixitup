using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for CurrencyRequirementControl.xaml
    /// </summary>
    public partial class CurrencyRequirementControl : LoadingControlBase
    {
        public CurrencyRequirementControl()
        {
            InitializeComponent();
        }

        public CurrencyModel GetCurrencyType() { return (CurrencyModel)this.CurrencyTypeComboBox.SelectedItem; }

        public int GetCurrencyAmount()
        {
            int currencyCost = -1;
            if (!string.IsNullOrEmpty(this.CurrencyCostTextBox.Text))
            {
                int.TryParse(this.CurrencyCostTextBox.Text, out currencyCost);
            }
            return currencyCost;
        }

        public CurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetCurrencyType() != null && this.GetCurrencyAmount() >= 0)
            {
                return new CurrencyRequirementViewModel(this.GetCurrencyType(), this.GetCurrencyAmount());
            }
            return null;
        }

        public void SetCurrencyRequirement(CurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currency.ContainsKey(currencyRequirement.CurrencyID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.CurrencyTypeComboBox.ItemsSource = ChannelSession.Settings.Currency.Values;
                this.CurrencyTypeComboBox.SelectedItem = ChannelSession.Settings.Currency[currencyRequirement.CurrencyID];

                this.CurrencyCostTextBox.IsEnabled = true;
                if (currencyRequirement.RequiredAmount >= 0)
                {
                    this.CurrencyCostTextBox.Text = currencyRequirement.RequiredAmount.ToString();
                }
            }
        }

        public async Task<bool> Validate()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault())
            {
                if (this.GetCurrencyRequirement() == null)
                {
                    await DialogHelper.ShowMessage("A Currency must be specified when a Currency requirement is set");
                    return false;
                }

                if (this.GetCurrencyAmount() <= 0)
                {
                    await DialogHelper.ShowMessage("A valid Currency Amount must be specified when a Currency requirement is set");
                    return false;
                }
            }
            return true;
        }

        protected override Task OnLoaded()
        {
            CurrencyRequirementViewModel requirement = this.GetCurrencyRequirement();

            if (ChannelSession.Settings != null)
            {
                IEnumerable<CurrencyModel> currencies = ChannelSession.Settings.Currency.Values;
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
