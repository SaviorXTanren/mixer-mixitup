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
        public static IEnumerable<UserRoleEnum> BasicUserRoleAllowedValues { get { return UserViewModel.SelectableBasicUserRoles(); } }

        public static IEnumerable<UserRoleEnum> AdvancedUserRoleAllowedValues { get { return UserViewModel.SelectableAdvancedUserRoles(); } }

        [JsonProperty]
        public UserRoleEnum MixerRole { get; set; }

        [JsonProperty]
        public string CustomRole { get; set; }

        public RoleRequirementViewModel()
        {
            this.MixerRole = UserRoleEnum.User;
        }

        public RoleRequirementViewModel(UserRoleEnum mixerRole)
        {
            this.MixerRole = mixerRole;
        }

        public void SetRoleBasedOnString(UserRoleEnum role, string custom)
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
                await ChannelSession.Services.Chat.Whisper(user, string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, role));
            }
        }
    }
}
