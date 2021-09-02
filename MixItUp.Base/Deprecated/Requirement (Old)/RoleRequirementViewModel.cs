using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [Obsolete]
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

        public bool DoesMeetRequirement(UserV2ViewModel user)
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
    }
}
