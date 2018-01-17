using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Currency
{
    /// <summary>
    /// Interaction logic for RankSelectorControl.xaml
    /// </summary>
    public partial class RankSelectorControl : LoadingControlBase
    {
        public RankSelectorControl()
        {
            InitializeComponent();

            this.RankMustEqualComboBox.ItemsSource = new List<string>() { ">=", "=" };
            this.RankMustEqualComboBox.SelectedIndex = 0;
        }

        public UserCurrencyViewModel GetRankType() { return (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem; }

        public bool GetRankMustEqual() { return this.RankMustEqualComboBox.SelectedIndex == 1; }

        public UserRankViewModel GetRankMinimum() { return (UserRankViewModel)this.RankMinimumComboBox.SelectedItem; }

        public UserCurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetRankType() != null && this.GetRankMinimum() != null)
            {
                return new UserCurrencyRequirementViewModel(this.GetRankType(), this.GetRankMinimum(), this.GetRankMustEqual());
            }
            return null;
        }

        public void SetCurrencyRequirement(UserCurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currencies.ContainsKey(currencyRequirement.CurrencyID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.RankTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
                this.RankTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyID];

                this.RankMustEqualComboBox.IsEnabled = true;
                this.RankMustEqualComboBox.SelectedIndex = (currencyRequirement.MustEqual) ? 1 : 0;

                this.RankMinimumComboBox.IsEnabled = true;
                this.RankMinimumComboBox.ItemsSource = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyID].Ranks;
                this.RankMinimumComboBox.SelectedItem = currencyRequirement.RequiredRank;
            }
        }

        public async Task<bool> Validate()
        {
            if (this.GetRankType() != null && this.GetRankMinimum() == null)
            {
                await MessageBoxHelper.ShowMessageDialog("A rank minimum must be specified with a rank");
                return false;
            }
            return true;
        }

        protected override Task OnLoaded()
        {
            UserCurrencyRequirementViewModel requirement = this.GetCurrencyRequirement();

            if (ChannelSession.Settings != null)
            {
                IEnumerable<UserCurrencyViewModel> ranks = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
                this.IsEnabled = (ranks.Count() > 0);
                this.RankTypeComboBox.ItemsSource = ranks;
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
            this.RankDataGrid.IsEnabled = this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault();
        }

        private void RankTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserCurrencyViewModel rankType = (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem;

            this.RankMustEqualComboBox.IsEnabled = true;

            this.RankMinimumComboBox.IsEnabled = true;
            this.RankMinimumComboBox.ItemsSource = rankType.Ranks;
        }
    }
}
