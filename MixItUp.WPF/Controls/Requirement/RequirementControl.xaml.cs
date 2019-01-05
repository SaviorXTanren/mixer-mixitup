using MixItUp.Base.ViewModel.Requirement;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MixItUp.WPF.Controls.Requirement
{
    /// <summary>
    /// Interaction logic for RequirementControl.xaml
    /// </summary>
    public partial class RequirementControl : UserControl
    {
        public RequirementControl()
        {
            InitializeComponent();
        }

        public void HideCooldownRequirement()
        {
            this.CooldownPopup.Visibility = Visibility.Collapsed;
        }

        public void HideCurrencyRequirement()
        {
            this.CurrencyRankInventoryRequirement.HideCurrencyRequirement();
        }

        public void HideThresholdRequirement()
        {
            this.ThresholdPopup.Visibility = Visibility.Collapsed;
        }

        public void HideSettingsRequirement()
        {
            this.SettingsPopup.Visibility = Visibility.Collapsed;
        }

        public async Task<bool> Validate()
        {
            if (this.CurrencyRankInventoryRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                return await this.CooldownRequirement.Validate() && await this.CurrencyRankInventoryRequirement.CurrencyRequirement.Validate() &&
                    await this.CurrencyRankInventoryRequirement.RankRequirement.Validate() && await this.CurrencyRankInventoryRequirement.InventoryRequirement.Validate() &&
                    await this.ThresholdRequirement.Validate() && await this.SettingsRequirement.Validate();
            }
            else
            {
                return await this.CooldownRequirement.Validate() && await this.CurrencyRankInventoryRequirement.RankRequirement.Validate() &&
                    await this.CurrencyRankInventoryRequirement.InventoryRequirement.Validate() && await this.ThresholdRequirement.Validate() && await this.SettingsRequirement.Validate();
            }
        }

        public RequirementViewModel GetRequirements()
        {
            RequirementViewModel requirement = new RequirementViewModel();
            requirement.Role = this.RoleRequirement.GetRoleRequirement();
            requirement.Cooldown = this.CooldownRequirement.GetCooldownRequirement();
            if (this.CurrencyRankInventoryRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                requirement.Currency = this.CurrencyRankInventoryRequirement.CurrencyRequirement.GetCurrencyRequirement();
            }
            requirement.Rank = this.CurrencyRankInventoryRequirement.RankRequirement.GetCurrencyRequirement();
            requirement.Inventory = this.CurrencyRankInventoryRequirement.InventoryRequirement.GetInventoryRequirement();
            requirement.Threshold = this.ThresholdRequirement.GetThresholdRequirement();
            requirement.Settings = this.SettingsRequirement.GetSettingsRequirement();
            return requirement;
        }

        public void SetRequirements(RequirementViewModel requirement)
        {
            this.RoleRequirement.SetRoleRequirement(requirement.Role);
            this.CooldownRequirement.SetCooldownRequirement(requirement.Cooldown);
            if (this.CurrencyRankInventoryRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                this.CurrencyRankInventoryRequirement.CurrencyRequirement.SetCurrencyRequirement(requirement.Currency);
            }
            this.CurrencyRankInventoryRequirement.RankRequirement.SetCurrencyRequirement(requirement.Rank);
            this.CurrencyRankInventoryRequirement.InventoryRequirement.SetInventoryRequirement(requirement.Inventory);
            this.ThresholdRequirement.SetThresholdRequirement(requirement.Threshold);
            this.SettingsRequirement.SetSettingsRequirement(requirement.Settings);
        }

        private void UsageRequirementsHelpButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Usage-Requirements"); }
    }
}
