using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public abstract class PermissionsCommandBase : CommandBase
    {
        public static IEnumerable<string> PermissionsAllowedValues { get { return EnumHelper.GetEnumNames(UserViewModel.SelectableUserRoles()); } }

        [DataMember]
        public UserRole Permissions { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public UserCurrencyRequirementViewModel CurrencyRequirement { get; set; }

        [DataMember]
        public UserCurrencyRequirementViewModel RankRequirement { get; set; }

        [JsonIgnore]
        protected DateTimeOffset lastRun = DateTimeOffset.MinValue;

        public PermissionsCommandBase() { }

        public PermissionsCommandBase(string name, CommandTypeEnum type, string command, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement, UserCurrencyRequirementViewModel rankRequirement)
            : this(name, type, new List<string>() { command }, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement)
        { }

        public PermissionsCommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement, UserCurrencyRequirementViewModel rankRequirement)
            : base(name, type, commands)
        {
            this.Permissions = lowestAllowedRole;
            this.Cooldown = cooldown;
            this.CurrencyRequirement = currencyRequirement;
            this.RankRequirement = rankRequirement;
        }

        [JsonIgnore]
        public string PermissionsString { get { return EnumHelper.GetEnumName(this.Permissions); } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            await this.CheckPermissions(user);

            await this.CheckRankRequirement(user);

            await this.CheckCurrencyRequirement(user);

            this.lastRun = DateTimeOffset.Now;

            await base.PerformInternal(user, arguments);
        }

        public async Task<bool> CheckPermissions(UserViewModel user)
        {
            if (!user.Roles.Any(r => r >= this.Permissions))
            {
                if (ChannelSession.Chat != null)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "You do not permission to run this command");
                }
                return false;
            }
            return true;
        }

        public async Task<bool> CheckRankRequirement(UserViewModel user)
        {
            if (this.RankRequirement != null && this.RankRequirement.GetCurrency() != null)
            {
                if (!this.RankRequirement.DoesMeetRankRequirement(user.Data))
                {
                    await this.RankRequirement.SendRankNotMetWhisper(user);
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> CheckCurrencyRequirement(UserViewModel user)
        {
            if (this.CurrencyRequirement != null && this.CurrencyRequirement.GetCurrency() != null)
            {
                if (!this.CurrencyRequirement.TrySubtractAmount(user.Data, this.CurrencyRequirement.RequiredAmount))
                {
                    await this.CurrencyRequirement.SendCurrencyNotMetWhisper(user);
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> CheckLastRun(UserViewModel user)
        {
            if (this.lastRun.AddSeconds(this.Cooldown) > DateTimeOffset.Now)
            {
                if (ChannelSession.Chat != null)
                {
                    TimeSpan timeLeft = this.lastRun.AddSeconds(this.Cooldown) - DateTimeOffset.Now;
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("This command is currently on cooldown, please wait another {0} second(s).", Math.Max((int)timeLeft.TotalSeconds, 1)));
                }
                return false;
            }
            return true;
        }
    }
}
