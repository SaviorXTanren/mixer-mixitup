using MixItUp.Base;
using MixItUp.Base.ViewModel.User;
using MixItUp.WPF.Util;
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
        }

        public UserCurrencyViewModel GetRankType() { return (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem; }

        public UserRankViewModel GetRankMinimum() { return (UserRankViewModel)this.RankMinimumComboBox.SelectedItem; }

        public UserCurrencyRequirementViewModel GetCurrencyRequirement()
        {
            if (this.GetRankType() != null && this.GetRankMinimum() != null)
            {
                return new UserCurrencyRequirementViewModel(this.GetRankType(), this.GetRankMinimum());
            }
            return null;
        }

        public void SetCurrencyRequirement(UserCurrencyRequirementViewModel currencyRequirement)
        {
            if (currencyRequirement != null && ChannelSession.Settings.Currencies.ContainsKey(currencyRequirement.CurrencyName))
            {
                this.RankTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
                this.RankTypeComboBox.SelectedItem = ChannelSession.Settings.Currencies[currencyRequirement.CurrencyName];
                this.RankTypeComboBox.SelectedItem = currencyRequirement.RequiredRank;
            }
        }

        public void SetRankTypeAndMinimum(UserCurrencyViewModel rank, UserRankViewModel minimum)
        {
            this.RankTypeComboBox.SelectedItem = rank;
            this.RankMinimumComboBox.SelectedItem = minimum;
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
            if (ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank).Count() > 0)
            {
                this.IsEnabled = true;

                UserCurrencyViewModel rankType = (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem;
                UserRankViewModel rankMinimum = (UserRankViewModel)this.RankMinimumComboBox.SelectedItem;

                this.RankTypeComboBox.ItemsSource = ChannelSession.Settings.Currencies.Values.Where(c => c.IsRank);
                this.RankMinimumComboBox.IsEnabled = false;

                this.RankTypeComboBox.SelectedItem = rankType;
                this.RankMinimumComboBox.SelectedItem = rankMinimum;
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

        private void RankTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UserCurrencyViewModel rankType = (UserCurrencyViewModel)this.RankTypeComboBox.SelectedItem;
            this.RankMinimumComboBox.IsEnabled = true;
            this.RankMinimumComboBox.ItemsSource = rankType.Ranks;
        }
    }
}
