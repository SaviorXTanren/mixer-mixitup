using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class SettingsRequirementViewModel
    {
        [DataMember]
        public bool DeleteChatCommandWhenRun { get; set; }
        [DataMember]
        public bool DontDeleteChatCommandWhenRun { get; set; }

        [DataMember]
        public string PatreonBenefitIDRequirement { get; set; }

        public SettingsRequirementViewModel() { }

        public bool DoesMeetRequirement(UserViewModel user)
        {
            if (!string.IsNullOrEmpty(this.PatreonBenefitIDRequirement) && !user.HasPermissionsTo(MixerRoleEnum.Mod))
            {
                PatreonBenefit benefit = ChannelSession.Services.Patreon.Campaign.GetBenefit(this.PatreonBenefitIDRequirement);
                if (benefit != null)
                {
                    PatreonTier tier = user.PatreonTier;
                    return tier != null && tier.BenefitIDs.Contains(benefit.ID);
                }
            }
            return true;
        }

        public async Task SendSettingsNotMetWhisper(UserViewModel user)
        {
            if (!string.IsNullOrEmpty(this.PatreonBenefitIDRequirement))
            {
                PatreonBenefit benefit = ChannelSession.Services.Patreon.Campaign.GetBenefit(this.PatreonBenefitIDRequirement);
                if (benefit != null)
                {
                    if (ChannelSession.Services.Chat != null)
                    {
                        await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format("You must have the {0} Patreon Benefit to do this", benefit.Title));
                    }
                }
            }
        }
    }
}
