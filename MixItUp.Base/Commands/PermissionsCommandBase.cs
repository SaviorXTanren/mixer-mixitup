using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
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
        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public RequirementViewModel Requirements { get; set; }

        [DataMember]
        [Obsolete]
        internal UserRole Permissions { get; set; }

        [DataMember]
        [Obsolete]
        internal CurrencyRequirementViewModel CurrencyRequirement { get; set; }

        [DataMember]
        [Obsolete]
        internal CurrencyRequirementViewModel RankRequirement { get; set; }

        [JsonIgnore]
        protected DateTimeOffset lastRun = DateTimeOffset.MinValue;

        public PermissionsCommandBase()
        {
            this.Requirements = new RequirementViewModel();
        }

        public PermissionsCommandBase(string name, CommandTypeEnum type, string command, int cooldown, RequirementViewModel requirements)
            : this(name, type, new List<string>() { command }, cooldown, requirements)
        { }

        public PermissionsCommandBase(string name, CommandTypeEnum type, IEnumerable<string> commands, int cooldown, RequirementViewModel requirements)
            : base(name, type, commands)
        {
            this.Cooldown = cooldown;
            this.Requirements = requirements;
        }

        [JsonIgnore]
        public string UserRoleRequirementString { get { return EnumHelper.GetEnumName(this.Requirements.UserRole); } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user) || !await this.CheckUserRoleRequirement(user) || !await this.CheckRankRequirement(user) || !await this.CheckCurrencyRequirement(user))
            {
                return;
            }

            this.lastRun = DateTimeOffset.Now;

            await base.PerformInternal(user, arguments);
        }

        public async Task<bool> CheckUserRoleRequirement(UserViewModel user)
        {
            if (!user.Roles.Any(r => r >= this.Requirements.UserRole))
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
            if (this.Requirements.Rank != null && this.Requirements.Rank.GetCurrency() != null)
            {
                if (!this.Requirements.Rank.DoesMeetRankRequirement(user.Data))
                {
                    await this.Requirements.Rank.SendRankNotMetWhisper(user);
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> CheckCurrencyRequirement(UserViewModel user)
        {
            if (this.Requirements.Currency != null && this.Requirements.Currency.GetCurrency() != null)
            {
                if (!this.Requirements.Currency.TrySubtractAmount(user.Data, this.Requirements.Currency.RequiredAmount))
                {
                    await this.Requirements.Currency.SendCurrencyNotMetWhisper(user);
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
