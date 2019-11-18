using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public class RoleRequirementViewModel
    {
        public static IEnumerable<MixerRoleEnum> BasicUserRoleAllowedValues { get { return UserViewModel.SelectableBasicUserRoles(); } }

        public static IEnumerable<MixerRoleEnum> AdvancedUserRoleAllowedValues { get { return UserViewModel.SelectableAdvancedUserRoles(); } }

        [JsonProperty]
        public MixerRoleEnum MixerRole { get; set; }

        [JsonProperty]
        public string CustomRole { get; set; }

        public RoleRequirementViewModel()
        {
            this.MixerRole = MixerRoleEnum.User;
        }

        public RoleRequirementViewModel(MixerRoleEnum mixerRole)
        {
            this.MixerRole = mixerRole;
        }

        public void SetRoleBasedOnString(MixerRoleEnum role, string custom)
        {
            this.MixerRole = role;
            this.CustomRole = custom;
        }

        public bool DoesMeetRequirement(UserViewModel user) { return user.HasPermissionsTo(this.MixerRole); }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Services.Chat != null)
            {
                string role = EnumLocalizationHelper.GetLocalizedName(this.MixerRole);
                await ChannelSession.Services.Chat.Whisper(user.UserName, string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, role));
            }
        }
    }
}
