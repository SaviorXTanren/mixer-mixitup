using Mixer.Base.Util;
using MixItUp.Base.Actions;
using MixItUp.Base.ViewModel.Import;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class ChatCommand : CommandBase
    {
        private static SemaphoreSlim chatCommandPerformSemaphore = new SemaphoreSlim(1);

        public static IEnumerable<string> PermissionsAllowedValues { get { return EnumHelper.GetEnumNames(UserViewModel.SelectableUserRoles()); } }

        [DataMember]
        public UserRole Permissions { get; set; }

        [DataMember]
        public int Cooldown { get; set; }

        [DataMember]
        public UserCurrencyRequirementViewModel CurrencyCost { get; set; }

        [JsonIgnore]
        private DateTimeOffset lastRun = DateTimeOffset.MinValue;

        public ChatCommand() { }

        public ChatCommand(string name, string command, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyCost)
            : this(name, new List<string>() { command }, lowestAllowedRole, cooldown, currencyCost)
        { }

        public ChatCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyCost)
            : base(name, CommandTypeEnum.Chat, commands)
        {
            this.Permissions = lowestAllowedRole;
            this.Cooldown = cooldown;
            this.CurrencyCost = currencyCost;
        }

        public ChatCommand(ScorpBotCommand command)
            : this(command.Command, command.Command, command.Permission, command.Cooldown, null)
        {
            this.Actions.Add(new ChatAction(command.Text, isWhisper: false, sendAsStreamer: false));
        }

        [JsonIgnore]
        public string PermissionsString { get { return EnumHelper.GetEnumName(this.Permissions); } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (this.lastRun.AddSeconds(this.Cooldown) > DateTimeOffset.Now)
            {
                if (ChannelSession.Chat != null)
                {
                    TimeSpan timeLeft = this.lastRun.AddSeconds(this.Cooldown) - DateTimeOffset.Now;
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("This command is currently on cooldown, please wait another {0} second(s).", (int)timeLeft.TotalSeconds));
                }
                return;
            }

            if (this.CurrencyCost != null && this.CurrencyCost.GetCurrency() != null && this.CurrencyCost.GetCurrency().Enabled)
            {
                UserCurrencyDataViewModel currencyData = user.Data.GetCurrency(this.CurrencyCost.GetCurrency());
                if (currencyData.Amount < this.CurrencyCost.RequiredAmount)
                {
                    if (ChannelSession.Chat != null)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to run this command.",
                            this.CurrencyCost.RequiredAmount, currencyData.Currency.Name));
                    }
                    return;
                }
                currencyData.Amount -= this.CurrencyCost.RequiredAmount;
            }

            this.lastRun = DateTimeOffset.Now;
            await base.PerformInternal(user, arguments);
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return ChatCommand.chatCommandPerformSemaphore; } }
    }
}
