using Mixer.Base.Util;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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
        public UserRole MixerRole { get; set; }

        [JsonProperty]
        public string CustomRole { get; set; }

        public RoleRequirementViewModel()
        {
            this.MixerRole = UserRole.User;
        }

        public RoleRequirementViewModel(UserRole mixerRole)
        {
            this.MixerRole = mixerRole;
        }

        public RoleRequirementViewModel(string customRole)
            : this(UserRole.Custom)
        {
            this.CustomRole = customRole;
        }

        public string RoleNameString
        {
            get
            {
                if (this.MixerRole == UserRole.Custom && !string.IsNullOrEmpty(this.CustomRole))
                {
                    return this.CustomRole;
                }
                return EnumHelper.GetEnumName(this.MixerRole);
            }
        }

        public async Task<bool> DoesMeetUserRoleRequirement(UserViewModel user)
        {
            if (this.MixerRole == UserRole.Follower)
            {
                if (!user.IsFollower)
                {
                    await user.SetDetails();
                }
                return user.IsFollower;
            }
            else if (this.MixerRole == UserRole.Subscriber)
            {
                if (!user.IsSubscriber)
                {
                    await user.SetSubscribeDate();
                }
                return user.IsSubscriber;
            }
            else if (this.MixerRole == UserRole.Custom && !string.IsNullOrEmpty(this.CustomRole))
            {
                if (this.CustomRole.StartsWith(GameWispTier.MIURolePrefix))
                {
                    return this.DoesUserMeetGameWispRequirement(user);
                }
                return false;
            }
            else
            {
                return user.PrimaryRole >= this.MixerRole;
            }
        }

        public async Task SendUserRoleNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to do this", (this.MixerRole != UserRole.Custom) ?
                    EnumHelper.GetEnumName(this.MixerRole) : this.CustomRole));
            }
        }

        private bool DoesUserMeetGameWispRequirement(UserViewModel user)
        {
            if (ChannelSession.Services.GameWisp != null && user.GameWispTier != null)
            {
                GameWispTier requirementTier = ChannelSession.Services.GameWisp.ChannelInfo.GetActiveTiers().FirstOrDefault(t => t.MIURoleName.Equals(this.CustomRole));
                return (requirementTier != null && user.GameWispTier.Level >= requirementTier.Level);
            }
            return false;
        }
    }
}
