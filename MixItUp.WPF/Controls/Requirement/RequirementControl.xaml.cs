using MixItUp.Base.ViewModel.Requirement;
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

        public void HideCurrencyRequirement()
        {
            this.CurrencyPopup.Visibility = Visibility.Collapsed;
        }

        public async Task<bool> Validate()
        {
            if (this.CurrencyPopup.Visibility == Visibility.Visible)
            {
                return await this.CurrencyRequirement.Validate() && await this.RankRequirement.Validate();
            }
            else
            {
                return await this.RankRequirement.Validate();
            }
        }

        public RequirementViewModel GetRequirements()
        {
            RequirementViewModel requirement = new RequirementViewModel();
            requirement.UserRole = this.UserRoleRequirement.GetUserRoleRequirement();
            if (this.CurrencyPopup.Visibility == Visibility.Visible)
            {
                requirement.Currency = this.CurrencyRequirement.GetCurrencyRequirement();
            }
            requirement.Rank = this.RankRequirement.GetCurrencyRequirement();
            return requirement;
        }

        public void SetRequirements(RequirementViewModel requirement)
        {
            this.UserRoleRequirement.SetUserRoleRequirement(requirement.UserRole);
            if (this.CurrencyPopup.Visibility == Visibility.Visible)
            {
                this.CurrencyRequirement.SetCurrencyRequirement(requirement.Currency);
            }
            this.RankRequirement.SetCurrencyRequirement(requirement.Rank);
        }
    }
}
