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
            this.CurrencyRankRequirement.HideCurrencyRequirement();
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
            if (this.CurrencyRankRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                return await this.CooldownRequirement.Validate() && await this.CurrencyRankRequirement.CurrencyRequirement.Validate() && await this.CurrencyRankRequirement.RankRequirement.Validate() && await this.ThresholdRequirement.Validate() && await this.SettingsRequirement.Validate();
            }
            else
            {
                return await this.CooldownRequirement.Validate() && await this.CurrencyRankRequirement.RankRequirement.Validate() && await this.ThresholdRequirement.Validate() && await this.SettingsRequirement.Validate();
            }
        }

        public RequirementViewModel GetRequirements()
        {
            RequirementViewModel requirement = new RequirementViewModel();
            requirement.Role = this.RoleRequirement.GetRoleRequirement();
            requirement.Cooldown = this.CooldownRequirement.GetCooldownRequirement();
            if (this.CurrencyRankRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                requirement.Currency = this.CurrencyRankRequirement.CurrencyRequirement.GetCurrencyRequirement();
            }
            requirement.Rank = this.CurrencyRankRequirement.RankRequirement.GetCurrencyRequirement();
            requirement.Threshold = this.ThresholdRequirement.GetThresholdRequirement();
            requirement.Settings = this.SettingsRequirement.GetSettingsRequirement();
            return requirement;
        }

        public void SetRequirements(RequirementViewModel requirement)
        {
            this.RoleRequirement.SetRoleRequirement(requirement.Role);
            this.CooldownRequirement.SetCooldownRequirement(requirement.Cooldown);
            if (this.CurrencyRankRequirement.CurrencyRequirement.Visibility == Visibility.Visible)
            {
                this.CurrencyRankRequirement.CurrencyRequirement.SetCurrencyRequirement(requirement.Currency);
            }
            this.CurrencyRankRequirement.RankRequirement.SetCurrencyRequirement(requirement.Rank);
            this.ThresholdRequirement.SetThresholdRequirement(requirement.Threshold);
            this.SettingsRequirement.SetSettingsRequirement(requirement.Settings);
        }

        private void UsageRequirementsHelpButton_Click(object sender, RoutedEventArgs e) { Process.Start("https://github.com/SaviorXTanren/mixer-mixitup/wiki/Usage-Requirements"); }
    }
}
