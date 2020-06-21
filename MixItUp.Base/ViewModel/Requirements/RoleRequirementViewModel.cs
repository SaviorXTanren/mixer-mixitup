using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;

namespace MixItUp.Base.ViewModel.Requirements
{
    public class RoleRequirementViewModel : RequirementViewModelBase
    {
        public static IEnumerable<UserRoleEnum> SelectableUserRoles()
        {
            List<UserRoleEnum> roles = new List<UserRoleEnum>(EnumHelper.GetEnumList<UserRoleEnum>());
            roles.Remove(UserRoleEnum.GlobalMod);
            roles.Remove(UserRoleEnum.Banned);
            roles.Remove(UserRoleEnum.Custom);
            return roles;
        }

        private static PatreonBenefit NonePatreonBenefit = new PatreonBenefit() { ID = string.Empty, Title = "None" };

        public IEnumerable<UserRoleEnum> Roles { get { return RoleRequirementViewModel.SelectableUserRoles(); } }

        public UserRoleEnum SelectedRole
        {
            get { return this.selectedRole; }
            set
            {
                this.selectedRole = value;
                this.NotifyPropertyChanged();
            }
        }
        private UserRoleEnum selectedRole = UserRoleEnum.User;

        public bool IsPatreonConnected { get { return ChannelSession.Services.Patreon.IsConnected; } }

        public IEnumerable<PatreonBenefit> PatreonBenefits
        {
            get
            {
                List<PatreonBenefit> benefits = new List<PatreonBenefit>();
                benefits.Add(RoleRequirementViewModel.NonePatreonBenefit);
                if (this.IsPatreonConnected)
                {
                    benefits.AddRange(ChannelSession.Services.Patreon.Campaign.Benefits.Values.OrderBy(b => b.Title));
                }
                return benefits;
            }
        }

        public PatreonBenefit SelectedPatreonBenefit
        {
            get { return this.selectedPatreonBenefit; }
            set
            {
                this.selectedPatreonBenefit = value;
                this.NotifyPropertyChanged();
            }
        }
        private PatreonBenefit selectedPatreonBenefit = RoleRequirementViewModel.NonePatreonBenefit;

        public RoleRequirementViewModel() { }

        public RoleRequirementViewModel(RoleRequirementModel requirement)
        {
            this.SelectedRole = requirement.Role;

            if (this.IsPatreonConnected && !string.IsNullOrEmpty(requirement.PatreonBenefitID))
            {
                this.SelectedPatreonBenefit = this.PatreonBenefits.FirstOrDefault(b => b.ID.Equals(requirement.PatreonBenefitID));
                if (this.SelectedPatreonBenefit == null)
                {
                    this.SelectedPatreonBenefit = RoleRequirementViewModel.NonePatreonBenefit;
                }
            }
        }

        public override RequirementModelBase GetRequirement()
        {
            return new RoleRequirementModel(this.SelectedRole, this.selectedPatreonBenefit?.ID);
        }
    }
}
