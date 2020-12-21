using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
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
        public int SubscriberTier { get; set; } = 1;

        [DataMember]
        public string PatreonBenefitID { get; set; }

        public RoleRequirementModel() { }

        internal RoleRequirementModel(MixItUp.Base.ViewModel.Requirement.RoleRequirementViewModel requirement)
            : this()
        {
            this.Role = requirement.MixerRole;
            this.SubscriberTier = requirement.SubscriberTier;
        }

        public RoleRequirementModel(UserRoleEnum role, int subscriberTier = 1, string patreonBenefitID = null)
        {
            this.Role = role;
            this.SubscriberTier = subscriberTier;
            this.PatreonBenefitID = patreonBenefitID;
        }

        public override async Task<bool> Validate(CommandParametersModel parameters)
        {
            if (!parameters.User.HasPermissionsTo(this.Role))
            {
                if (!string.IsNullOrEmpty(this.PatreonBenefitID) && ChannelSession.Services.Patreon.IsConnected)
                {
                    PatreonBenefit benefit = ChannelSession.Services.Patreon.Campaign.GetBenefit(this.PatreonBenefitID);
                    if (benefit != null)
                    {
                        PatreonTier tier = parameters.User.PatreonTier;
                        if (tier != null && tier.BenefitIDs.Contains(benefit.ID))
                        {
                            return true;
                        }
                    }
                }
                await this.SendErrorMessage();
                return false;
            }

            if (this.Role == UserRoleEnum.Subscriber && !parameters.User.ExceedsPermissions(this.Role))
            {
                if (parameters.User.SubscribeTier < this.SubscriberTier)
                {
                    await this.SendErrorMessage();
                    return false;
                }
            }
            return true;
        }

        private async Task SendErrorMessage()
        {
            string role = EnumLocalizationHelper.GetLocalizedName(this.Role);
            if (this.Role == UserRoleEnum.Subscriber)
            {
                string tierText = string.Empty;
                switch (this.SubscriberTier)
                {
                    case 1: tierText = MixItUp.Base.Resources.Tier1; break;
                    case 2: tierText = MixItUp.Base.Resources.Tier2; break;
                    case 3: tierText = MixItUp.Base.Resources.Tier3; break;
                }
                role = tierText + " " + role;
            }
            await this.SendErrorChatMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, role));
        }
    }
}
