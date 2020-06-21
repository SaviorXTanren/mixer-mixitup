using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    public class RoleRequirementViewModel
    {
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
