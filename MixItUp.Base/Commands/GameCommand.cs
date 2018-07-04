using MixItUp.Base.Util;
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
    #region Base Game Classes

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
            int randomNumber = this.GenerateProbability();

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

        protected int GenerateRandomNumber(int maxValue)
        {
            this.randomSeed -= 123;
            Random random = new Random(this.randomSeed);
            return random.Next(maxValue);
        }

        protected int GenerateProbability() { return this.GenerateRandomNumber(100); }
    }

    [DataContract]
    public abstract class SinglePlayerOutcomeGameCommand : GameCommandBase
    {
        [DataMember]
        public List<GameOutcome> Outcomes { get; set; }

        public SinglePlayerOutcomeGameCommand()
        {
            this.Outcomes = new List<GameOutcome>();
        }

        public SinglePlayerOutcomeGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes)
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
    public class TwoPlayerGameCommandBase : GameCommandBase
    {
        [DataMember]
        public GameOutcome SuccessfulOutcome { get; set; }

        [DataMember]
        public GameOutcome FailedOutcome { get; set; }

        public TwoPlayerGameCommandBase() { }

        public TwoPlayerGameCommandBase(string name, IEnumerable<string> commands, RequirementViewModel requirements, GameOutcome successfulOutcome, GameOutcome failedOutcome)
            : base(name, commands, requirements)
        {
            this.SuccessfulOutcome = successfulOutcome;
            this.FailedOutcome = failedOutcome;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformCurrencyChecks(user, betAmount))
                    {
                        UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                        if (currency == null)
                        {
                            return;
                        }

                        UserViewModel targetUser = await this.GetTargetUser(user, arguments, currency, betAmount);
                        if (targetUser != null)
                        {
                            int randomNumber = this.GenerateProbability();
                            if (randomNumber < this.SuccessfulOutcome.GetRoleProbability(user.PrimaryRole))
                            {
                                user.Data.AddCurrencyAmount(currency, betAmount);
                                targetUser.Data.SubtractCurrencyAmount(currency, betAmount);
                                await this.PerformOutcome(user, arguments, this.SuccessfulOutcome, betAmount, targetUser);
                            }
                            else
                            {
                                await this.PerformOutcome(user, arguments, this.FailedOutcome, betAmount, targetUser);
                            }
                            this.Requirements.UpdateCooldown(user);
                        }
                    }
                }
            }
        }

        protected virtual string GetBetAmountArgument(IEnumerable<string> arguments) { return arguments.FirstOrDefault(); }

        protected virtual async Task<UserViewModel> GetTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (UserViewModel activeUser in await ChannelSession.ActiveUsers.GetAllWorkableUsers())
            {
                if (!user.Equals(activeUser) && activeUser.Data.GetCurrencyAmount(currency) >= betAmount)
                {
                    users.Add(activeUser);
                }
            }

            if (users.Count == 0)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("There are no active users with {0} {1}", betAmount, currency.Name));
                user.Data.AddCurrencyAmount(currency, betAmount);
                return null;
            }

            int userIndex = this.GenerateRandomNumber(users.Count);
            return users[userIndex];
        }

        protected async Task PerformOutcome(UserViewModel user, IEnumerable<string> arguments, GameOutcome outcome, int betAmount, UserViewModel targetUser)
        {
            await base.PerformOutcome(user, new List<string>() { targetUser.UserName }, outcome, betAmount);
        }
    }

    #endregion Base Game Classes

    [DataContract]
    public class SpinGameCommand : SinglePlayerOutcomeGameCommand
    {
        public SpinGameCommand() { }

        public SpinGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes) : base(name, commands, requirements, outcomes) { }
    }

    [DataContract]
    public class VendingMachineGameCommand : SinglePlayerOutcomeGameCommand
    {
        public VendingMachineGameCommand() { }

        public VendingMachineGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<GameOutcome> outcomes) : base(name, commands, requirements, outcomes) { }
    }

    [DataContract]
    public class StealGameCommand : TwoPlayerGameCommandBase
    {
        public StealGameCommand() { }

        public StealGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, GameOutcome successfulOutcome, GameOutcome failedOutcome)
            : base(name, commands, requirements, successfulOutcome, failedOutcome)
        { }
    }

    [DataContract]
    public class PickpocketGameCommand : TwoPlayerGameCommandBase
    {
        public PickpocketGameCommand() { }

        public PickpocketGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, GameOutcome successfulOutcome, GameOutcome failedOutcome)
            : base(name, commands, requirements, successfulOutcome, failedOutcome)
        { }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments.Count() != 1)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <USERNAME>", this.Commands.First()));
                    return false;
                }
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

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                return arguments.Skip(1).FirstOrDefault();
            }
            return arguments.FirstOrDefault();
        }

        protected override async Task<UserViewModel> GetTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            string username = arguments.FirstOrDefault().Replace("@", "");
            UserViewModel targetUser = await ChannelSession.ActiveUsers.GetUserByUsername(username);

            if (targetUser == null || user.Equals(targetUser))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "The User specified is either not valid or not currently in the channel");
                user.Data.AddCurrencyAmount(currency, betAmount);
                return null;
            }

            if (targetUser.Data.GetCurrencyAmount(currency) < betAmount)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("@{0} does not have {1} {2}", targetUser.UserName, betAmount, currency.Name));
                user.Data.AddCurrencyAmount(currency, betAmount);
                return null;
            }

            return targetUser;
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
