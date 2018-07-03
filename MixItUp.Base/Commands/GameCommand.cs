using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    [DataContract]
    public class GameOutcome
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public double Payout { get; set; }

        [DataMember]
        public Dictionary<MixerRoleEnum, int> RoleProbabilities { get; set; }

        [DataMember]
        public CustomCommand Command { get; set; }

        public GameOutcome() { }

        public GameOutcome(string name, double payout, Dictionary<MixerRoleEnum, int> roleProbabilities, CustomCommand command)
        {
            this.Name = name;
            this.Payout = payout;
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public int GetRoleProbability(MixerRoleEnum role)
        {
            if (this.RoleProbabilities.ContainsKey(role))
            {
                return this.RoleProbabilities[role];
            }

            foreach (MixerRoleEnum checkRole in this.RoleProbabilities.Select(kvp => kvp.Key).OrderByDescending(k => k))
            {
                if (role >= checkRole)
                {
                    return this.RoleProbabilities[checkRole];
                }
            }

            return this.RoleProbabilities.LastOrDefault().Value;
        }

        public int GetPayout(UserViewModel user, int betAmount)
        {
            return Convert.ToInt32(Convert.ToDouble(betAmount) * this.Payout);
        }
    }

    [DataContract]
    public abstract class GameCommandBase : PermissionsCommandBase
    {
        public const string GameBetSpecialIdentifier = "gamebet";

        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameWinnersSpecialIdentifier = "gamewinners";

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonIgnore]
        private int randomSeed = (int)DateTime.Now.Ticks;

        public GameCommandBase() { }

        public GameCommandBase(string name, IEnumerable<string> commands, RequirementViewModel requirements) : base(name, CommandTypeEnum.Game, commands, requirements) { }

        public override bool ContainsCommand(string command)
        {
            return this.Commands.Select(c => "!" + c).Contains(command);
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return GameCommandBase.gameCommandPerformSemaphore; } }

        protected virtual async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0}", this.Commands.First()));
                    return false;
                }
            }
            else if (arguments.Count() != 1)
            {
                string betAmountUsageText = this.Requirements.Currency.RequiredAmount.ToString();
                if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    betAmountUsageText += "-" + this.Requirements.Currency.MaximumAmount.ToString();
                }
                else if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                {
                    betAmountUsageText += "+";
                }
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} {1}", this.Commands.First(), betAmountUsageText));
                return false;
            }
            return true;
        }

        protected async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            int betAmount = 0;

            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return -1;
            }

            if (this.Requirements.Currency.RequirementType != CurrencyRequirementTypeEnum.NoCurrencyCost)
            {
                if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                {
                    betAmount = this.Requirements.Currency.RequiredAmount;
                }
                else
                {
                    if (!int.TryParse(betAmountText, out betAmount) || betAmount <= 0)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You must specify a valid amount");
                        return -1;
                    }

                    if (!this.Requirements.DoesMeetCurrencyRequirement(betAmount))
                    {
                        if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must specify an amount between {0} - {1} {2}",
                                this.Requirements.Currency.RequiredAmount, this.Requirements.Currency.MaximumAmount, currency.Name));
                        }
                        else if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must specify an amount of at least {0} {1}",
                                this.Requirements.Currency.RequiredAmount, currency.Name));
                        }
                        return -1;
                    }
                }
            }
            return betAmount;
        }

        protected async Task<bool> PerformCurrencyChecks(UserViewModel user, int betAmount)
        {
            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            if (!(await this.CheckCooldownRequirement(user) && await this.CheckUserRoleRequirement(user) && await this.CheckRankRequirement(user)))
            {
                return false;
            }

            if (user.Data.GetCurrencyAmount(currency) < betAmount)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have {0} {1}", betAmount, currency.Name));
                return false;
            }
            user.Data.SubtractCurrencyAmount(currency, betAmount);

            return true;
        }

        protected GameOutcome SelectRandomOutcome(UserViewModel user, IEnumerable<GameOutcome> outcomes)
        {
            int randomNumber = this.GenerateRandomNumber();

            int cumulativeOutcomeProbability = 0;
            foreach (GameOutcome outcome in outcomes)
            {
                if (cumulativeOutcomeProbability <= randomNumber && randomNumber < (cumulativeOutcomeProbability + outcome.GetRoleProbability(user.PrimaryRole)))
                {
                    return outcome;
                }
                cumulativeOutcomeProbability += outcome.GetRoleProbability(user.PrimaryRole);
            }
            return outcomes.Last();
        }

        protected virtual async Task PerformOutcome(UserViewModel user, IEnumerable<string> arguments, GameOutcome outcome, int betAmount)
        {
            int payout = outcome.GetPayout(user, betAmount);
            user.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), payout);

            if (outcome.Command != null)
            {
                outcome.Command.AddSpecialIdentifier(GameCommandBase.GameBetSpecialIdentifier, betAmount.ToString());
                outcome.Command.AddSpecialIdentifier(GameCommandBase.GamePayoutSpecialIdentifier, payout.ToString());
                outcome.Command.AddSpecialIdentifier(GameCommandBase.GameWinnersSpecialIdentifier, "@" + user.UserName);
                await outcome.Command.Perform(user, arguments);
            }
        }

        protected int GenerateRandomNumber()
        {
            this.randomSeed -= 123;
            Random random = new Random(this.randomSeed);
            return random.Next(100);
        }
    }

    [DataContract]
    public abstract class BasicOutcomeGameCommand : GameCommandBase
    {
        [DataMember]
        public List<GameOutcome> Outcomes { get; set; }

        public BasicOutcomeGameCommand()
        {
            this.Outcomes = new List<GameOutcome>();
        }

        public BasicOutcomeGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes)
            : base(name, commands, requirements)
        {
            this.Outcomes = new List<GameOutcome>(outcomes);
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, arguments.FirstOrDefault());
                if (betAmount >= 0)
                {
                    if (await this.PerformCurrencyChecks(user, betAmount))
                    {
                        await this.PerformOutcome(user, arguments, this.SelectRandomOutcome(user, this.Outcomes), betAmount);
                        this.Requirements.UpdateCooldown(user);
                    }
                }
            }
        }
    }

    [DataContract]
    public class SpinGameCommand : BasicOutcomeGameCommand
    {
        public SpinGameCommand() { }

        public SpinGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes) : base(name, commands, requirements, outcomes) { }
    }

    [DataContract]
    public class VendingMachineGameCommand : BasicOutcomeGameCommand
    {
        public VendingMachineGameCommand() { }

        public VendingMachineGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes) : base(name, commands, requirements, outcomes) { }
    }

    [DataContract]
    public class TwoPlayerGameCommand : GameCommandBase
    {
        [DataMember]
        public int StarterPlayerSuccessProbability { get; set; }

        [DataMember]
        public bool RequiresOtherPlayerAgreement { get; set; }
        [DataMember]
        public int OtherPlayerResponseTime { get; set; }

        private SemaphoreSlim otherPlayerSemaphore = new SemaphoreSlim(1);
        private UserViewModel otherPlayer;
        private int betAmount = 0;

        public TwoPlayerGameCommand()
        {

        }

        public TwoPlayerGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements)
            : base(name, commands, requirements)
        {

        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, CancellationToken token)
        {
            if (this.otherPlayer != null)
            {
                bool matchesPlayer = false;
                await this.otherPlayerSemaphore.WaitAsync();

                if (this.otherPlayer.Equals(user))
                {
                    matchesPlayer = true;
                    this.otherPlayer = null;
                }

                this.otherPlayerSemaphore.Release();

                if (matchesPlayer)
                {
                    await this.PerformTwoPlayerCompetition(user, arguments, betAmount);
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "There is already an existing game underway, please wait until it is finished");
                }
            }
            else if (await this.PerformUsageChecks(user, arguments))
            {
                this.otherPlayer = await ChannelSession.ActiveUsers.GetUserByUsername(arguments.First());
                if (this.otherPlayer == null)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("The user {0} either does not exist or is not currently in the channel", arguments.First()));
                }

                this.betAmount = await this.GetBetAmount(user, arguments.Skip(1).First());
                if (this.betAmount >= 0)
                {
                    UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                    if (this.otherPlayer.Data.GetCurrencyAmount(currency) < betAmount)
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, string.Format("{0} does not have {1} {2}", this.otherPlayer.UserName, betAmount, currency.Name));
                        return;
                    }

                    if (this.RequiresOtherPlayerAgreement)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            await Task.Delay(this.OtherPlayerResponseTime * 1000);

                            await this.otherPlayerSemaphore.WaitAsync();

                            if (this.otherPlayer != null)
                            {
                                await ChannelSession.Chat.SendMessage(string.Format("{0} did not accept in time...", this.otherPlayer.UserName));
                                this.otherPlayer = null;
                            }

                            this.otherPlayerSemaphore.Release();
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    else
                    {
                        await this.PerformTwoPlayerCompetition(user, arguments, betAmount);
                    }
                }
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if ((this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                && arguments.Count() != 1)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <USERNAME>", this.Commands.First()));
                return false;
            }
            else if (arguments.Count() != 2)
            {
                string betAmountUsageText = this.Requirements.Currency.RequiredAmount.ToString();
                if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumAndMaximum)
                {
                    betAmountUsageText += "-" + this.Requirements.Currency.MaximumAmount.ToString();
                }
                else if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.MinimumOnly)
                {
                    betAmountUsageText += "+";
                }
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <USERNAME> {1}", this.Commands.First(), betAmountUsageText));
                return false;
            }
            return true;
        }

        private async Task PerformTwoPlayerCompetition(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (await this.PerformCurrencyChecks(user, this.betAmount))
            {
                //await this.PerformOutcome(user, arguments, this.SelectRandomOutcome(user, this.Outcomes), betAmount);
                this.Requirements.UpdateCooldown(user);
            }
        }
    }

    [DataContract]
    public class GroupGameCommand
    {
        public const string GameTotalBetsSpecialIdentifier = "gametotalbets";
    }

    [DataContract]
    public class LongRunningGameCommand
    {

    }
}
