using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Requirements
{
    [DataContract]
    public class RoleRequirementModel : RequirementModelBase
    {
        [DataMember]
        public UserRoleEnum Role { get; set; }

        [DataMember]
        public string PatreonBenefitID { get; set; }

        public RoleRequirementModel() { }

        public RoleRequirementModel(UserRoleEnum role, string patreonBenefitID = null)
        {
            this.Role = role;
            this.PatreonBenefitID = patreonBenefitID;
        }

        public override async Task<bool> Validate(UserViewModel user)
        {
            if (!user.HasPermissionsTo(this.Role))
            {
                if (!string.IsNullOrEmpty(this.PatreonBenefitID) && ChannelSession.Services.Patreon.IsConnected)
                {
                    PatreonBenefit benefit = ChannelSession.Services.Patreon.Campaign.GetBenefit(this.PatreonBenefitID);
                    if (benefit != null)
                    {
                        PatreonTier tier = user.PatreonTier;
                        if (tier != null && tier.BenefitIDs.Contains(benefit.ID))
                        {
                            return true;
                        }
                    }
                }
                await this.SendChatWhisper(user, string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, EnumLocalizationHelper.GetLocalizedName(this.Role)));
                return false;
            }
            return true;
        }
    }
}
