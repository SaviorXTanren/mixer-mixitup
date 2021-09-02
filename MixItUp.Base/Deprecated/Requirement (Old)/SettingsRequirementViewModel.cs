using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.ViewModel.User;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [Obsolete]
    [DataContract]
    public class SettingsRequirementViewModel
    {
        [DataMember]
        public bool DeleteChatCommandWhenRun { get; set; }
        [DataMember]
        public bool DontDeleteChatCommandWhenRun { get; set; }

        [DataMember]
        public string PatreonBenefitIDRequirement { get; set; }

        [DataMember]
        public bool ShowOnChatMenu { get; set; }

        public SettingsRequirementViewModel() { }

        public bool DoesMeetRequirement(UserV2ViewModel user)
        {
            if (!string.IsNullOrEmpty(this.PatreonBenefitIDRequirement) && !user.HasPermissionsTo(UserRoleEnum.Mod))
            {
                PatreonBenefit benefit = ServiceManager.Get<PatreonService>().Campaign.GetBenefit(this.PatreonBenefitIDRequirement);
                if (benefit != null)
                {
                    PatreonTier tier = user.PatreonTier;
                    return tier != null && tier.BenefitIDs.Contains(benefit.ID);
                }
            }
            return true;
        }
    }
}
