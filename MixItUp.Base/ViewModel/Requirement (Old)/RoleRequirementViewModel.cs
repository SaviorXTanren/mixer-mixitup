using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public class RoleRequirementViewModel
    {
        public static List<string> SubTierNames = new List<string>() { MixItUp.Base.Resources.Tier1, MixItUp.Base.Resources.Tier2, MixItUp.Base.Resources.Tier3 };

        [JsonProperty]
        public UserRoleEnum MixerRole { get; set; }

        [JsonProperty]
        public string CustomRole { get; set; }

        [JsonProperty]
        public int SubscriberTier { get; set; } = 1;

        public RoleRequirementViewModel()
        {
            this.MixerRole = UserRoleEnum.User;
        }

        public RoleRequirementViewModel(UserRoleEnum mixerRole, int subscriberTier = 1)
        {
            this.MixerRole = mixerRole;
            this.SubscriberTier = subscriberTier;
        }

        public bool DoesMeetRequirement(UserViewModel user)
        {
            if (user.HasPermissionsTo(this.MixerRole))
            {
                if (this.MixerRole == UserRoleEnum.Subscriber && !user.ExceedsPermissions(this.MixerRole))
                {
                    return user.SubscribeTier >= this.SubscriberTier;
                }
                return true;
            }
            return false;
        }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Services.Chat != null)
            {
                string role = EnumLocalizationHelper.GetLocalizedName(this.MixerRole);
                if (this.MixerRole == UserRoleEnum.Subscriber && this.SubscriberTier > 0)
                {
                    role += " - " + RoleRequirementViewModel.SubTierNames[this.SubscriberTier - 1];
                }
                await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, role));
            }
        }
    }
}
