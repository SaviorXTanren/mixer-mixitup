using MixItUp.Base;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for RankRequirementControl.xaml
    /// </summary>
    public partial class RankRequirementControl : LoadingControlBase
    {
        public RankRequirementControl()
        {
            InitializeComponent();

            this.RankMustEqualComboBox.ItemsSource = new List<string>() { ">=", "=" };
            this.RankMustEqualComboBox.SelectedIndex = 0;
        }

        public CurrencyModel GetRankType() { return (CurrencyModel)this.RankTypeComboBox.SelectedItem; }

        public bool GetRankMustEqual() { return this.RankMustEqualComboBox.SelectedIndex == 1; }

        public RankModel GetRankMinimum() { return (RankModel)this.RankMinimumComboBox.SelectedItem; }

        public CurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault() && this.GetRankType() != null && this.GetRankMinimum() != null)
            {
                return new CurrencyRequirementViewModel(this.GetRankType(), this.GetRankMinimum(), this.GetRankMustEqual());
            }
            return null;
        }

        public void SetCurrencyRequirement(CurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currency.ContainsKey(currencyRequirement.CurrencyID))
            {
                this.EnableDisableToggleSwitch.IsChecked = true;

                this.RankTypeComboBox.ItemsSource = ChannelSession.Settings.Currency.Values.Where(c => c.IsRank);
                this.RankTypeComboBox.SelectedItem = ChannelSession.Settings.Currency[currencyRequirement.CurrencyID];

                this.RankMustEqualComboBox.IsEnabled = true;
                this.RankMustEqualComboBox.SelectedIndex = (currencyRequirement.MustEqual) ? 1 : 0;

                this.RankMinimumComboBox.IsEnabled = true;
                this.RankMinimumComboBox.ItemsSource = ChannelSession.Settings.Currency[currencyRequirement.CurrencyID].Ranks;
                this.RankMinimumComboBox.SelectedItem = currencyRequirement.RequiredRank;
            }
        }

        public async Task<bool> Validate()
        {
            if (this.EnableDisableToggleSwitch.IsChecked.GetValueOrDefault())
            {
                if (this.GetRankType() == null)
                {
                    await DialogHelper.ShowMessage("A Rank must be specified when a Rank requirement is set");
                    return false;
                }

                if (this.GetRankMinimum() == null)
                {
                    await DialogHelper.ShowMessage("A Minimum Rank must be specified when a Rank Requirement is set");
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
                IEnumerable<CurrencyModel> ranks = ChannelSession.Settings.Currency.Values.Where(c => c.IsRank);
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
            CurrencyModel rankType = (CurrencyModel)this.RankTypeComboBox.SelectedItem;

            this.RankMustEqualComboBox.IsEnabled = true;

            this.RankMinimumComboBox.IsEnabled = true;
            this.RankMinimumComboBox.ItemsSource = rankType.Ranks;
        }
    }
}
