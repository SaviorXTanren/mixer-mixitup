using Mixer.Base.Util;
using MixItUp.Base.Services;
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
        public static IEnumerable<string> BasicUserRoleAllowedValues { get { return UserViewModel.SelectableBasicUserRoles().Select(r => EnumHelper.GetEnumName(r)); } }

        public static IEnumerable<string> AdvancedUserRoleAllowedValues { get { return UserViewModel.SelectableAdvancedUserRoles(); } }

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

        public RoleRequirementViewModel(string customRole)
        {
            this.SetRoleBasedOnString(customRole);
        }

        [JsonIgnore]
        public string RoleNameString
        {
            get
            {
                if (this.MixerRole == MixerRoleEnum.Custom && !string.IsNullOrEmpty(this.CustomRole))
                {
                    return this.CustomRole;
                }
                return EnumHelper.GetEnumName(this.MixerRole);
            }
        }

        public void SetRoleBasedOnString(string role)
        {
            MixerRoleEnum mixerRole = EnumHelper.GetEnumValueFromString<MixerRoleEnum>(role);
            if (mixerRole > MixerRoleEnum.Banned)
            {
                this.MixerRole = mixerRole;
            }
            else
            {
                this.MixerRole = MixerRoleEnum.Custom;
                this.CustomRole = role;
            }
        }

        public bool DoesMeetRequirement(UserViewModel user) { return user.HasPermissionsTo(this.MixerRole); }

        public async Task SendNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to do this", (this.MixerRole != MixerRoleEnum.Custom) ?
                    EnumHelper.GetEnumName(this.MixerRole) : this.CustomRole));
            }
        }
    }
}
