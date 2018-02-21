using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Requirement
{
    [DataContract]
    public class RequirementViewModel
    {
        public static IEnumerable<string> UserRoleAllowedValues { get { return EnumHelper.GetEnumNames(UserViewModel.SelectableUserRoles()); } }

        [JsonProperty]
        public UserRole UserRole { get; set; }

        [JsonProperty]
        public CooldownRequirementViewModel Cooldown { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Currency { get; set; }

        [JsonProperty]
        public CurrencyRequirementViewModel Rank { get; set; }

        public RequirementViewModel()
        {
            this.UserRole = UserRole.User;
            this.Cooldown = new CooldownRequirementViewModel();
        }

        public RequirementViewModel(UserRole userRole, int cooldown)
            : this()
        {
            this.UserRole = userRole;
            this.Cooldown.Amount = cooldown;
        }

        public RequirementViewModel(UserRole userRole, CooldownRequirementViewModel cooldown = null, CurrencyRequirementViewModel currency = null, CurrencyRequirementViewModel rank = null)
        {
            this.UserRole = userRole;
            this.Cooldown = (cooldown != null) ? cooldown : new CooldownRequirementViewModel();
            this.Currency = currency;
            this.Rank = rank;
        }

        public async Task<bool> DoesMeetUserRoleRequirement(UserViewModel user)
        {
            if (this.UserRole == UserRole.Follower)
            {
                if (!user.IsFollower)
                {
                    await user.SetDetails(checkForFollow: true);
                }
                return user.IsFollower;
            }
            else if (this.UserRole == UserRole.Subscriber)
            {
                if (!user.IsSubscriber)
                {
                    await user.SetSubscribeDate();
                }
                return user.IsSubscriber;
            }
            else
            {
                return user.PrimaryRole >= this.UserRole;
            }
        }

        public bool DoesMeetCooldownRequirement(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                return this.Cooldown.DoesMeetCooldownRequirement(user);
            }
            return true;
        }

        public bool DoesMeetCurrencyRequirement(UserViewModel user)
        {
            if (this.Currency != null)
            {
                return this.Currency.DoesMeetCurrencyRequirement(user.Data);
            }
            return true;
        }

        public bool DoesMeetCurrencyRequirement(int amount)
        {
            if (this.Currency != null)
            {
                return this.Currency.DoesMeetCurrencyRequirement(amount);
            }
            return true;
        }

        public bool DoesMeetRankRequirement(UserViewModel user)
        {
            if (this.Rank != null)
            {
                return this.Rank.DoesMeetRankRequirement(user.Data);
            }
            return true;
        }

        public async Task SendUserRoleNotMetWhisper(UserViewModel user)
        {
            if (ChannelSession.Chat != null)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to do this", EnumHelper.GetEnumName(this.UserRole)));
            }
        }

        public void UpdateCooldown(UserViewModel user)
        {
            if (this.Cooldown != null)
            {
                this.Cooldown.UpdateCooldown(user);
            }
        }

        public bool TrySubtractCurrencyAmount(UserViewModel user)
        {
            if (this.Currency != null)
            {
                return ChannelSession.Settings.GameQueueRequirements.Currency.TrySubtractAmount(user.Data);
            }
            return true;
        }

        public bool TrySubtractCurrencyAmount(UserViewModel user, int amount)
        {
            if (this.Currency != null)
            {
                return ChannelSession.Settings.GameQueueRequirements.Currency.TrySubtractAmount(user.Data, amount);
            }
            return true;
        }
    }
}
