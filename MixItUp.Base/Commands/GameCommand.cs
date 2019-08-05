using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
        public Dictionary<MixerRoleEnum, double> RolePayouts { get; set; }

        [DataMember]
        public Dictionary<MixerRoleEnum, int> RoleProbabilities { get; set; }

        [DataMember]
        public CustomCommand Command { get; set; }

        public GameOutcome() { }

        public GameOutcome(string name, double payout, Dictionary<MixerRoleEnum, int> roleProbabilities, CustomCommand command)
        {
            this.Name = name;
            this.Payout = payout;
            this.RolePayouts = new Dictionary<MixerRoleEnum, double>();
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public GameOutcome(string name, Dictionary<MixerRoleEnum, double> rolePayouts, Dictionary<MixerRoleEnum, int> roleProbabilities, CustomCommand command)
        {
            this.Name = name;
            this.Payout = 0;
            this.RolePayouts = rolePayouts;
            this.RoleProbabilities = roleProbabilities;
            this.Command = command;
        }

        public int GetRoleProbability(UserViewModel user)
        {
            var roleProbabilities = this.RoleProbabilities.Select(kvp => kvp.Key).OrderByDescending(k => k);
            if (roleProbabilities.Any(r => user.HasPermissionsTo(r)))
            {
                return this.RoleProbabilities[roleProbabilities.FirstOrDefault(r => user.HasPermissionsTo(r))];
            }
            return this.RoleProbabilities[roleProbabilities.LastOrDefault()];
        }

        public int GetPayout(UserViewModel user, int betAmount)
        {
            return Convert.ToInt32(Convert.ToDouble(betAmount) * this.GetPayoutAmount(user));
        }

        private double GetPayoutAmount(UserViewModel user)
        {
            if (this.RolePayouts.Count > 0)
            {
                var rolePayouts = this.RolePayouts.Select(kvp => kvp.Key).OrderByDescending(k => k);
                if (rolePayouts.Any(r => user.HasPermissionsTo(r)))
                {
                    return this.RolePayouts[rolePayouts.FirstOrDefault(r => user.HasPermissionsTo(r))];
                }
                return this.RolePayouts[rolePayouts.LastOrDefault()];
            }
            return this.Payout;
        }
    }

    [DataContract]
    public abstract class GameCommandBase : PermissionsCommandBase
    {
        public const string GameBetSpecialIdentifier = "gamebet";

        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameAllPayoutSpecialIdentifier = "gameallpayout";

        public const string GameWinnersSpecialIdentifier = "gamewinners";

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        [JsonIgnore]
        protected HashSet<UserViewModel> winners = new HashSet<UserViewModel>();
        [JsonIgnore]
        protected int totalPayout = 0;

        public GameCommandBase() { }

        public GameCommandBase(string name, IEnumerable<string> commands, RequirementViewModel requirements) : base(name, CommandTypeEnum.Game, commands, requirements) { }

        [JsonIgnore]
        public override IEnumerable<string> CommandTriggers { get { return this.Commands.Select(c => "!" + c); } }

        public abstract IEnumerable<CommandBase> GetAllInnerCommands();

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

        protected virtual async Task<bool> PerformUsernameUsageChecks(UserViewModel user, IEnumerable<string> arguments)
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

        protected override Task<bool> PerformPreChecks(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            return Task.FromResult(true);
        }

        protected virtual string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return arguments.FirstOrDefault();
        }

        protected virtual string GetBetAmountSecondArgument(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                return arguments.Skip(1).FirstOrDefault();
            }
            return arguments.FirstOrDefault();
        }

        protected virtual async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
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

        protected virtual async Task<UserViewModel> GetArgumentsTargetUser(UserViewModel user, IEnumerable<string> arguments)
        {
            UserViewModel targetUser = null;
            if (arguments != null && arguments.Count() > 0)
            {
                string username = arguments.FirstOrDefault().Replace("@", "");
                targetUser = await ChannelSession.ActiveUsers.GetUserByUsername(username);

                if (targetUser == null || user.Equals(targetUser))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "The User specified is either not valid or not currently in the channel");
                    return null;
                }
            }
            return targetUser;
        }

        protected async Task<bool> PerformRequirementChecks(UserViewModel user)
        {
            if (await this.CheckCooldownRequirement(user) && await this.PerformNonCooldownRequirementChecks(user))
            {
                return true;
            }
            return false;
        }

        protected async Task<bool> PerformNonCooldownRequirementChecks(UserViewModel user)
        {
            if (await this.CheckUserRoleRequirement(user) && await this.CheckRankRequirement(user) && await this.CheckInventoryRequirement(user) && await this.CheckSettingsRequirement(user))
            {
                return true;
            }
            return false;
        }

        protected virtual async Task<bool> PerformCurrencyChecks(UserViewModel user, int betAmount)
        {
            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return false;
            }

            if (!user.Data.HasCurrencyAmount(currency, betAmount))
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have {0} {1}", betAmount, currency.Name));
                return false;
            }
            user.Data.SubtractCurrencyAmount(currency, betAmount);

            this.Requirements.TrySubtractInventoryAmount(user);

            return true;
        }

        protected GameOutcome SelectRandomOutcome(UserViewModel user, IEnumerable<GameOutcome> outcomes)
        {
            int randomNumber = this.GenerateProbability();

            int cumulativeOutcomeProbability = 0;
            foreach (GameOutcome outcome in outcomes)
            {
                if (cumulativeOutcomeProbability < randomNumber && randomNumber <= (cumulativeOutcomeProbability + outcome.GetRoleProbability(user)))
                {
                    return outcome;
                }
                cumulativeOutcomeProbability += outcome.GetRoleProbability(user);
            }
            return outcomes.Last();
        }

        protected virtual async Task PerformOutcome(UserViewModel user, IEnumerable<string> arguments, GameOutcome outcome, int betAmount)
        {
            int payout = outcome.GetPayout(user, betAmount);
            user.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), payout);
            this.totalPayout += payout;
            await this.PerformCommand(outcome.Command, user, arguments, betAmount, payout);
        }

        protected virtual async Task PerformCommand(CommandBase command, UserViewModel user, IEnumerable<string> arguments, int betAmount, int payout)
        {
            if (command != null)
            {
                Dictionary<string, string> specialIdentifiers = new Dictionary<string, string>();
                this.AddAdditionalSpecialIdentifiers(user, arguments, specialIdentifiers);
                specialIdentifiers[GameCommandBase.GameBetSpecialIdentifier] = betAmount.ToString();
                specialIdentifiers[GameCommandBase.GamePayoutSpecialIdentifier] = payout.ToString();
                specialIdentifiers[GameCommandBase.GameAllPayoutSpecialIdentifier] = this.totalPayout.ToString();
                if (this.winners.Count > 0)
                {
                    specialIdentifiers[GameCommandBase.GameWinnersSpecialIdentifier] = string.Join(", ", this.winners.Select(w => "@" + w.UserName));
                }
                else if (user != null)
                {
                    specialIdentifiers[GameCommandBase.GameWinnersSpecialIdentifier] = "@" + user.UserName;
                }
                else
                {
                    specialIdentifiers[GameCommandBase.GameWinnersSpecialIdentifier] = "None";
                }
                await command.Perform(user, arguments, specialIdentifiers);
            }
        }

        protected virtual void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers) { }

        protected int GenerateProbability() { return RandomHelper.GenerateProbability(); }

        protected int GenerateRandomNumber(int maxValue) { return RandomHelper.GenerateRandomNumber(maxValue); }

        protected int GenerateRandomNumber(int minValue, int maxValue) { return RandomHelper.GenerateRandomNumber(minValue, maxValue); }

        protected virtual void ResetData(UserViewModel user)
        {
            this.winners.Clear();
            this.totalPayout = 0;
            this.Requirements.UpdateCooldown(user);
        }
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

        public override IEnumerable<CommandBase> GetAllInnerCommands() { return this.Outcomes.Select(o => o.Command); }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, betAmount))
                    {
                        await this.PerformOutcome(user, arguments, this.SelectRandomOutcome(user, this.Outcomes), betAmount);
                        this.ResetData(user);
                    }
                }
            }
        }
    }

    [DataContract]
    public abstract class TwoPlayerGameCommandBase : GameCommandBase
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

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        { 
            List<CommandBase> commands = new List<CommandBase>();
            if (this.SuccessfulOutcome != null && this.SuccessfulOutcome.Command != null)
            {
                commands.Add(this.SuccessfulOutcome.Command);
            }
            if (this.FailedOutcome != null && this.FailedOutcome.Command != null)
            {
                commands.Add(this.FailedOutcome.Command);
            }
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, betAmount))
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
                            if (randomNumber <= this.SuccessfulOutcome.GetRoleProbability(user))
                            {
                                user.Data.AddCurrencyAmount(currency, betAmount);
                                targetUser.Data.SubtractCurrencyAmount(currency, betAmount);
                                await this.PerformOutcome(user, arguments, this.SuccessfulOutcome, betAmount, targetUser);
                            }
                            else
                            {
                                await this.PerformOutcome(user, arguments, this.FailedOutcome, betAmount, targetUser);
                            }
                            this.ResetData(user);
                        }
                    }
                }
            }
        }

        protected virtual async Task<UserViewModel> GetTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            List<UserViewModel> users = new List<UserViewModel>();
            foreach (UserViewModel activeUser in await ChannelSession.ActiveUsers.GetAllWorkableUsers())
            {
                if (!user.Equals(activeUser) && !activeUser.Data.IsCurrencyRankExempt && activeUser.Data.HasCurrencyAmount(currency, betAmount))
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

        protected virtual async Task<UserViewModel> GetArgumentsTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            UserViewModel targetUser = await this.GetArgumentsTargetUser(user, arguments);
            if (targetUser != null)
            {
                if (!targetUser.Data.HasCurrencyAmount(currency, betAmount))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("@{0} does not have {1} {2}", targetUser.UserName, betAmount, currency.Name));
                    return null;
                }
            }
            return targetUser;
        }

        protected async Task PerformOutcome(UserViewModel user, IEnumerable<string> arguments, GameOutcome outcome, int betAmount, UserViewModel targetUser)
        {
            await base.PerformOutcome(user, new List<string>() { targetUser.UserName }, outcome, betAmount);
        }
    }

    [DataContract]
    public abstract class GroupGameCommand : GameCommandBase
    {
        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand UserJoinCommand { get; set; }

        [DataMember]
        public GameOutcome UserSuccessOutcome { get; set; }
        [DataMember]
        public GameOutcome UserFailOutcome { get; set; }

        [DataMember]
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        [JsonIgnore]
        protected UserViewModel starterUser;
        [JsonIgnore]
        protected Task timeLimitTask;
        [JsonIgnore]
        protected Dictionary<UserViewModel, int> enteredUsers = new Dictionary<UserViewModel, int>();

        public GroupGameCommand() { }

        public GroupGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, CustomCommand startedCommand,
            CustomCommand userJoinCommand, GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.UserSuccessOutcome = userSuccessOutcome;
            this.UserFailOutcome = userFailOutcome;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>() { this.StartedCommand, this.UserJoinCommand };
            if (this.UserSuccessOutcome != null && this.UserSuccessOutcome.Command != null)
            {
                commands.Add(this.UserSuccessOutcome.Command);
            }
            if (this.UserFailOutcome != null && this.UserFailOutcome.Command != null)
            {
                commands.Add(this.UserFailOutcome.Command);
            }
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, betAmount) && await this.CanUserEnter(user, arguments, betAmount))
                    {
                        if (this.timeLimitTask == null)
                        {
                            this.totalPayout = 0;

                            this.enteredUsers.Clear();
                            this.enteredUsers[user] = betAmount;
                            this.starterUser = user;

                            await this.GameStarted(user, arguments, betAmount);

                            this.timeLimitTask = Task.Run(async () =>
                            {
                                await Task.Delay(this.TimeLimit * 1000);

                                if (this.enteredUsers.Count < this.MinimumParticipants)
                                {
                                    await this.NotEnoughUsers();

                                    this.Requirements.UpdateCooldown(user);
                                }
                                else
                                {
                                    await this.TimeComplete();

                                    this.Requirements.UpdateCooldown(user);

                                    UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                                    if (currency == null)
                                    {
                                        this.ResetData(user);
                                        return;
                                    }

                                    await this.SelectWinners();

                                    await this.GameCompleted();
                                }

                                this.ResetData(user);
                            });

                            await this.PerformCommand(this.StartedCommand, user, arguments, betAmount, 0);
                        }
                        else
                        {
                            await this.UserJoined(user, arguments, betAmount);
                            await this.PerformCommand(this.UserJoinCommand, user, arguments, betAmount, 0);
                        }
                    }
                }
            }
        }

        protected virtual async Task<bool> CanUserEnter(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (this.enteredUsers.ContainsKey(user))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You've already joined the game");
                return false;
            }
            return true;
        }

        protected virtual Task UserJoined(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.enteredUsers[user] = betAmount;
            return Task.FromResult(0);
        }

        protected virtual Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount) { return Task.FromResult(0); }

        protected virtual async Task NotEnoughUsers()
        {
            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            foreach (var enteredUser in this.enteredUsers)
            {
                enteredUser.Key.Data.AddCurrencyAmount(currency, enteredUser.Value);
            }
            
            if (this.NotEnoughPlayersCommand != null)
            {
                await this.NotEnoughPlayersCommand.Perform(this.starterUser);
            }
        }

        protected virtual Task TimeComplete() { return Task.FromResult(0); }

        protected virtual Task SelectWinners() { return Task.FromResult(0); }

        protected virtual Task GameCompleted() { return Task.FromResult(0); }

        protected override void ResetData(UserViewModel user)
        {
            this.starterUser = null;
            this.timeLimitTask = null;
            this.enteredUsers.Clear();
            base.ResetData(user);
        }
    }

    [DataContract]
    public abstract class LongRunningGameCommand : GameCommandBase
    {
        private const string GameTotalAmountSpecialIdentifier = "gametotalamount";

        [DataMember]
        public int TotalAmount { get; set; }

        [DataMember]
        public string StatusArgument { get; set; }

        public LongRunningGameCommand() { }

        public LongRunningGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, string statusArgument)
            : base(name, commands, requirements)
        {
            this.StatusArgument = statusArgument;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands() { return new List<CommandBase>(); }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (arguments.Count() == 1 && arguments.ElementAt(0).Equals(this.StatusArgument))
            {
                if (await this.PerformNonCooldownRequirementChecks(user))
                {
                    await this.ReportStatus(user, arguments);
                }
            }
            else if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, betAmount))
                    {
                        await this.AddBetAmount(user, arguments, betAmount);

                        if (await this.ShouldPerformPayout(user, arguments, betAmount))
                        {
                            await this.PerformPayout(user, arguments, betAmount);
                        }

                        this.Requirements.UpdateCooldown(user);
                    }
                }
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} -or- !{0} {1}", this.Commands.First(), this.StatusArgument));
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
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} {1} -or- !{0} {2}", this.Commands.First(), betAmountUsageText, this.StatusArgument));
                return false;
            }
            return true;
        }

        protected virtual Task AddBetAmount(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.TotalAmount += betAmount;
            return Task.FromResult(0);
        }

        protected virtual Task ReportStatus(UserViewModel user, IEnumerable<string> arguments) { return Task.FromResult(0); }

        protected abstract Task<bool> ShouldPerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount);

        protected abstract Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount);

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            specialIdentifiers[GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
        }
    }

    #endregion Base Game Classes

    [DataContract]
    public class HotPotatoGameCommand : GameCommandBase
    {
        [DataMember]
        [Obsolete]
        public int TimeLimit { get; set; }

        [DataMember]
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand TossPotatoCommand { get; set; }

        [DataMember]
        public CustomCommand PotatoExplodeCommand { get; set; }

        [JsonIgnore]
        private Task timeLimitTask;
        [JsonIgnore]
        private UserViewModel currentUser;
        [JsonIgnore]
        private UserViewModel targetUser;

        public HotPotatoGameCommand() { }

        public HotPotatoGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int lowerLimit, int upperLimit, bool allowUserTargeting,
            CustomCommand startedCommand, CustomCommand tossPotatoCommand, CustomCommand potatoExplodeCommand)
            : base(name, commands, requirements)
        {
            this.LowerLimit = lowerLimit;
            this.UpperLimit = upperLimit;
            this.AllowUserTargeting = allowUserTargeting;
            this.StartedCommand = startedCommand;
            this.TossPotatoCommand = tossPotatoCommand;
            this.PotatoExplodeCommand = potatoExplodeCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands() { return new List<CommandBase>() { this.StartedCommand, this.TossPotatoCommand, this.PotatoExplodeCommand }; }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (this.TimeLimit > 0)
            {
                this.LowerLimit = this.TimeLimit;
                this.UpperLimit = this.TimeLimit;
                this.TimeLimit = 0;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            if (await this.PerformUsageChecks(user, arguments) && await this.PerformRequirementChecks(user))
            {
                UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                if (currency == null)
                {
                    return;
                }

                if (this.timeLimitTask != null)
                {
                    if (this.targetUser.Equals(user))
                    {
                        UserViewModel newTarget = await this.GetUserToPassTo(user, arguments);
                        if (newTarget != null)
                        {
                            this.currentUser = this.targetUser;
                            this.targetUser = newTarget;
                            await this.PerformCommand(this.TossPotatoCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "Could not find a user to pass to");
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You do not have the ability to pass right now");
                    }
                }
                else
                {
                    this.currentUser = this.targetUser = user;
                    this.targetUser = await this.GetUserToPassTo(user, arguments);
                    if (this.targetUser != null)
                    {
                        await this.PerformCommand(this.StartedCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);

                        this.timeLimitTask = Task.Run(async () =>
                        {
                            await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerLimit, this.UpperLimit));

                            this.Requirements.UpdateCooldown(user);

                            await this.PerformCommand(this.PotatoExplodeCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);

                            this.ResetData(user);
                        });
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Could not find a user to pass to");
                    }
                }
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.AllowUserTargeting)
            {
                if (arguments.Count() != 0 && arguments.Count() != 1)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} -OR- !{0} <TARGET USER>", this.Commands.First()));
                    return false;
                }

                if (arguments.Count() == 1)
                {
                    UserViewModel targetUser = await this.GetArgumentsTargetUser(user, arguments);
                    if (targetUser == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0}", this.Commands.First()));
                    return false;
                }
            }
            return true;
        }

        protected override void ResetData(UserViewModel user)
        {
            this.timeLimitTask = null;
            base.ResetData(user);
        }

        private async Task<UserViewModel> GetUserToPassTo(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.AllowUserTargeting && arguments.Count() == 1)
            {
                return await this.GetArgumentsTargetUser(user, arguments);
            }
            else
            {
                foreach (UserViewModel u in (await ChannelSession.ActiveUsers.GetAllWorkableUsers()).Shuffle())
                {
                    if (!u.Data.IsCurrencyRankExempt && (this.targetUser == null || !u.Equals(this.targetUser)))
                    {
                        return u;
                    }
                }
            }
            return null;
        }
    }

    [DataContract]
    public class BeachBallGameCommand : GameCommandBase
    {
        [DataMember]
        [Obsolete]
        public int HitTimeLimit { get; set; }

        [DataMember]
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

        [DataMember]
        public CustomCommand StartedCommand { get; set; }

        [DataMember]
        public CustomCommand BallHitCommand { get; set; }

        [DataMember]
        public CustomCommand BallMissedCommand { get; set; }

        [JsonIgnore]
        private Task timeLimitTask;
        [JsonIgnore]
        private CancellationTokenSource timeLimitCancellationTokenSource;
        [JsonIgnore]
        private UserViewModel currentUser;
        [JsonIgnore]
        private UserViewModel targetUser;

        public BeachBallGameCommand() { }

        public BeachBallGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int lowerLimit, int upperLimit, bool allowUserTargeting,
            CustomCommand startedCommand, CustomCommand ballHitCommand, CustomCommand ballMissedCommand)
            : base(name, commands, requirements)
        {
            this.LowerLimit = lowerLimit;
            this.UpperLimit = upperLimit;
            this.AllowUserTargeting = allowUserTargeting;
            this.StartedCommand = startedCommand;
            this.BallHitCommand = ballHitCommand;
            this.BallMissedCommand = ballMissedCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands() { return new List<CommandBase>() { this.StartedCommand, this.BallHitCommand, this.BallMissedCommand }; }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
#pragma warning disable CS0612 // Type or member is obsolete
            if (this.HitTimeLimit > 0)
            {
                this.LowerLimit = this.HitTimeLimit;
                this.UpperLimit = this.HitTimeLimit;
                this.HitTimeLimit = 0;
            }
#pragma warning restore CS0612 // Type or member is obsolete

            if (await this.PerformUsageChecks(user, arguments) && await this.PerformRequirementChecks(user))
            {
                UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                if (currency == null)
                {
                    return;
                }

                if (this.timeLimitTask != null)
                {
                    if (this.targetUser.Equals(user))
                    {
                        UserViewModel newTarget = await this.GetUserToPassTo(user, arguments);
                        if (newTarget != null)
                        {
                            this.currentUser = this.targetUser;
                            this.targetUser = newTarget;

                            await this.PerformCommand(this.BallHitCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);

                            this.PerformPass();
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "Could not find a user to pass to");
                        }
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "You do not have the ability to pass right now");
                    }
                }
                else
                {
                    this.currentUser = this.targetUser = user;
                    this.targetUser = await this.GetUserToPassTo(user, arguments);
                    if (this.targetUser != null)
                    {
                        await this.PerformCommand(this.StartedCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);

                        this.PerformPass();
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Could not find a user to pass to");
                    }
                }
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.AllowUserTargeting)
            {
                if (arguments.Count() != 0 && arguments.Count() != 1)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} -OR- !{0} <TARGET USER>", this.Commands.First()));
                    return false;
                }

                if (arguments.Count() == 1)
                {
                    UserViewModel targetUser = await this.GetArgumentsTargetUser(user, arguments);
                    if (targetUser == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0}", this.Commands.First()));
                    return false;
                }
            }
            return true;
        }

        protected override void ResetData(UserViewModel user)
        {
            this.timeLimitTask = null;
            base.ResetData(user);
        }

        private async Task<UserViewModel> GetUserToPassTo(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.AllowUserTargeting && arguments.Count() == 1)
            {
                return await this.GetArgumentsTargetUser(user, arguments);
            }
            else
            {
                foreach (UserViewModel u in (await ChannelSession.ActiveUsers.GetAllWorkableUsers()).Shuffle())
                {
                    if (!u.Data.IsCurrencyRankExempt && (this.targetUser == null || !u.Equals(this.targetUser)))
                    {
                        return u;
                    }
                }
            }
            return null;
        }

        private void PerformPass()
        {
            if (this.timeLimitCancellationTokenSource != null)
            {
                this.timeLimitCancellationTokenSource.Cancel();
            }

            this.timeLimitCancellationTokenSource = new CancellationTokenSource();
            this.timeLimitTask = Task.Run(async () =>
            {
                try
                {
                    CancellationToken token = this.timeLimitCancellationTokenSource.Token;

                    await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerLimit, this.UpperLimit));

                    if (!token.IsCancellationRequested)
                    {
                        await this.PerformCommand(this.BallMissedCommand, this.currentUser, new List<string>() { this.targetUser.UserName }, 0, 0);
                        this.Requirements.UpdateCooldown(this.currentUser);

                        this.ResetData(this.currentUser);
                    }
                }
                catch (Exception) { }
            }, this.timeLimitCancellationTokenSource.Token);
        }
    }

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

    #region SlotsGameOutcome

    public class SlotsGameOutcome : GameOutcome
    {
        [DataMember]
        public string Symbol1 { get; set; }
        [DataMember]
        public string Symbol2 { get; set; }
        [DataMember]
        public string Symbol3 { get; set; }

        [DataMember]
        public bool AnyOrder { get; set; }

        public SlotsGameOutcome() { }

        public SlotsGameOutcome(string name, string symbol1, string symbol2, string symbol3, Dictionary<MixerRoleEnum, double> rolePayouts, CustomCommand command, bool anyOrder)
            : base(name, rolePayouts, new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } }, command)
        {
            this.Symbol1 = symbol1;
            this.Symbol2 = symbol2;
            this.Symbol3 = symbol3;
            this.AnyOrder = anyOrder;
        }
    }

    #endregion SlotsGameOutcome

    [DataContract]
    public class SlotMachineGameCommand : SinglePlayerOutcomeGameCommand
    {
        public const string GameSlotsOutcomeSpecialIdentifier = "gameslotsoutcome";

        [DataMember]
        public List<string> AllSymbols { get; set; }

        [DataMember]
        public CustomCommand FailureOutcomeCommand { get; set; }

        private string lastOutcome = null;

        public SlotMachineGameCommand() { }

        public SlotMachineGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, IEnumerable<SlotsGameOutcome> outcomes, IEnumerable<string> allSymbols,
            CustomCommand failureOutcomeCommand)
            : base(name, commands, requirements, outcomes)
        {
            this.AllSymbols = new List<string>(allSymbols);
            this.FailureOutcomeCommand = failureOutcomeCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.FailureOutcomeCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (await this.PerformUsageChecks(user, arguments))
            {
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (betAmount >= 0)
                {
                    if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, betAmount))
                    {
                        string symbol1 = this.AllSymbols[this.GenerateRandomNumber(this.AllSymbols.Count)];
                        string symbol2 = this.AllSymbols[this.GenerateRandomNumber(this.AllSymbols.Count)];
                        string symbol3 = this.AllSymbols[this.GenerateRandomNumber(this.AllSymbols.Count)];
                        this.lastOutcome = symbol1 + " " + symbol2 + " " + symbol3;

                        SlotsGameOutcome winningOutcome = null;
                        foreach (GameOutcome outcome in this.Outcomes)
                        {
                            SlotsGameOutcome slotsOutcome = (SlotsGameOutcome)outcome;
                            if (slotsOutcome.Symbol1.Equals(symbol1) && slotsOutcome.Symbol2.Equals(symbol2) && slotsOutcome.Symbol3.Equals(symbol3))
                            {
                                winningOutcome = slotsOutcome;
                                break;
                            }
                            else if (slotsOutcome.AnyOrder)
                            {
                                if ((slotsOutcome.Symbol1.Equals(symbol2) && slotsOutcome.Symbol2.Equals(symbol1) && slotsOutcome.Symbol3.Equals(symbol3)) ||
                                    (slotsOutcome.Symbol1.Equals(symbol3) && slotsOutcome.Symbol2.Equals(symbol1) && slotsOutcome.Symbol3.Equals(symbol2)) ||
                                    (slotsOutcome.Symbol1.Equals(symbol1) && slotsOutcome.Symbol2.Equals(symbol3) && slotsOutcome.Symbol3.Equals(symbol2)) ||
                                    (slotsOutcome.Symbol1.Equals(symbol2) && slotsOutcome.Symbol2.Equals(symbol3) && slotsOutcome.Symbol3.Equals(symbol1)) ||
                                    (slotsOutcome.Symbol1.Equals(symbol3) && slotsOutcome.Symbol2.Equals(symbol2) && slotsOutcome.Symbol3.Equals(symbol1)))
                                {
                                    winningOutcome = slotsOutcome;
                                    break;
                                }
                            }
                        }

                        if (winningOutcome != null)
                        {
                            await this.PerformOutcome(user, arguments, winningOutcome, betAmount);
                        }
                        else
                        {
                            await this.PerformCommand(this.FailureOutcomeCommand, user, arguments, betAmount, 0);
                        }
                        this.ResetData(user);
                    }
                }
            }
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(this.lastOutcome))
            {
                specialIdentifiers[GameSlotsOutcomeSpecialIdentifier] = this.lastOutcome;
            }
        }

        protected override void ResetData(UserViewModel user)
        {
            this.lastOutcome = null;
            base.ResetData(user);
        }
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
            return await base.PerformUsernameUsageChecks(user, arguments);
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountSecondArgument(arguments);
        }

        protected override Task<UserViewModel> GetTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            return base.GetArgumentsTargetUser(user, arguments, currency, betAmount);
        }
    }

    [DataContract]
    public class DuelGameCommand : TwoPlayerGameCommandBase
    {
        [DataMember]
        public CustomCommand StartedCommand { get; set; }
        [DataMember]
        public CustomCommand NotAcceptedCommand { get; set; }

        [DataMember]
        public int TimeLimit { get; set; }

        [JsonIgnore]
        private SemaphoreSlim targetUserSemaphore = new SemaphoreSlim(1);
        [JsonIgnore]
        private UserViewModel currentStarterUser;
        [JsonIgnore]
        private UserViewModel currentTargetUser;
        [JsonIgnore]
        private int currentBetAmount = 0;

        [JsonIgnore]
        private Task timeLimitTask;
        [JsonIgnore]
        private CancellationTokenSource taskCancellationSource;

        public DuelGameCommand() { }

        public DuelGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, GameOutcome successfulOutcome, GameOutcome failedOutcome,
            CustomCommand startedCommand, int timeLimit, CustomCommand notAcceptedCommand)
            : base(name, commands, requirements, successfulOutcome, failedOutcome)
        {
            this.StartedCommand = startedCommand;
            this.TimeLimit = timeLimit;
            this.NotAcceptedCommand = notAcceptedCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.StartedCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            bool hasTarget = false;
            bool isTarget = false;

            await this.targetUserSemaphore.WaitAndRelease(() =>
            {
                hasTarget = (this.currentTargetUser != null);
                if (hasTarget)
                {
                    isTarget = (user.Equals(this.currentTargetUser));
                    if (isTarget)
                    {
                        this.currentTargetUser = null;
                        this.taskCancellationSource.Cancel();
                    }
                }
                return Task.FromResult(0);
            });

            if (hasTarget)
            {
                if (isTarget)
                {
                    UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                    if (currency == null)
                    {
                        return;
                    }

                    this.Requirements.TrySubtractInventoryAmount(user);

                    int randomNumber = this.GenerateProbability();
                    if (randomNumber <= this.SuccessfulOutcome.GetRoleProbability(user))
                    {
                        this.winners.Add(this.currentStarterUser);
                        this.currentStarterUser.Data.AddCurrencyAmount(currency, this.currentBetAmount * 2);
                        user.Data.SubtractCurrencyAmount(currency, this.currentBetAmount);
                        await this.PerformCommand(this.SuccessfulOutcome.Command, this.currentStarterUser, new List<string>() { user.UserName }, currentBetAmount, currentBetAmount);
                    }
                    else
                    {
                        this.winners.Add(user);
                        user.Data.AddCurrencyAmount(currency, this.currentBetAmount);
                        await this.PerformCommand(this.FailedOutcome.Command, this.currentStarterUser, new List<string>() { user.UserName }, currentBetAmount, currentBetAmount);
                    }
                    this.ResetData(user);
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "This game is already underway, please wait until it is finished");
                }
            }
            else if (await this.PerformUsageChecks(user, arguments))
            {
                this.currentBetAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
                if (this.currentBetAmount >= 0)
                {
                    UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                    if (currency == null)
                    {
                        return;
                    }

                    this.currentTargetUser = await this.GetArgumentsTargetUser(user, arguments, currency, this.currentBetAmount);
                    if (currentTargetUser != null)
                    {
                        if (await this.PerformRequirementChecks(user) && await this.PerformCurrencyChecks(user, this.currentBetAmount))
                        {
                            this.currentStarterUser = user;

                            this.taskCancellationSource = new CancellationTokenSource();
                            this.timeLimitTask = Task.Run(async () =>
                            {
                                try
                                {
                                    CancellationTokenSource currentCancellationSource = this.taskCancellationSource;

                                    await Task.Delay(this.TimeLimit * 1000);

                                    if (!currentCancellationSource.Token.IsCancellationRequested)
                                    {
                                        await this.targetUserSemaphore.WaitAndRelease(async () =>
                                        {
                                            if (this.currentTargetUser != null)
                                            {
                                                this.currentStarterUser.Data.AddCurrencyAmount(currency, this.currentBetAmount);
                                                if (this.Requirements.Inventory != null)
                                                {
                                                    this.currentStarterUser.Data.AddInventoryAmount(this.Requirements.Inventory.GetInventory(), this.Requirements.Inventory.ItemName, this.Requirements.Inventory.Amount);
                                                }

                                                if (this.NotAcceptedCommand != null)
                                                {
                                                    await this.NotAcceptedCommand.Perform(this.currentStarterUser, arguments: new List<string>() { this.currentTargetUser.UserName });
                                                }

                                                this.ResetData(user);
                                            }
                                        });
                                    }
                                }
                                catch (Exception) { }
                            }, this.taskCancellationSource.Token);

                            await this.PerformCommand(this.StartedCommand, user, new List<string>() { currentTargetUser.UserName }, currentBetAmount, 0);
                        }
                    }
                }
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            return await base.PerformUsernameUsageChecks(user, arguments);
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountSecondArgument(arguments);
        }

        protected override async Task<UserViewModel> GetArgumentsTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            UserViewModel targetUser = await base.GetArgumentsTargetUser(user, arguments, currency, betAmount);
            if (targetUser != null && this.Requirements.Inventory != null && !this.Requirements.Inventory.DoesMeetRequirement(targetUser.Data))
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("@{0} does not have {1} {2}", targetUser.UserName, this.Requirements.Inventory.Amount, this.Requirements.Inventory.ItemName));
                return null;
            }
            return targetUser;
        }

        protected override void ResetData(UserViewModel user)
        {
            this.currentStarterUser = null;
            this.currentTargetUser = null;
            base.ResetData(user);
        }
    }

    [DataContract]
    public class HeistGameCommand : GroupGameCommand
    {
        [DataMember]
        public CustomCommand AllSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand TopThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand MiddleThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand LowThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand NoneSucceedCommand { get; set; }

        public HeistGameCommand() { }

        public HeistGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, CustomCommand startedCommand,
            CustomCommand userJoinCommand, GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand allSucceedCommand, CustomCommand topThirdsSucceedCommand,
            CustomCommand middleThirdsSucceedCommand, CustomCommand lowThirdsSucceedCommand, CustomCommand noneSucceedCommand, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, userSuccessOutcome, userFailOutcome, notEnoughPlayersCommand)
        {
            this.AllSucceedCommand = allSucceedCommand;
            this.TopThirdsSucceedCommand = topThirdsSucceedCommand;
            this.MiddleThirdsSucceedCommand = middleThirdsSucceedCommand;
            this.LowThirdsSucceedCommand = lowThirdsSucceedCommand;
            this.NoneSucceedCommand = noneSucceedCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.AllSucceedCommand);
            commands.Add(this.TopThirdsSucceedCommand);
            commands.Add(this.MiddleThirdsSucceedCommand);
            commands.Add(this.LowThirdsSucceedCommand);
            commands.Add(this.NoneSucceedCommand);
            return commands;
        }

        protected override async Task SelectWinners()
        {
            foreach (var enteredUser in this.enteredUsers)
            {
                int randomNumber = this.GenerateProbability();
                if (randomNumber <= this.UserSuccessOutcome.GetRoleProbability(enteredUser.Key))
                {
                    this.winners.Add(enteredUser.Key);
                    await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserSuccessOutcome, enteredUser.Value);
                }
                else
                {
                    await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserFailOutcome, enteredUser.Value);
                }
            }
        }

        protected override async Task GameCompleted()
        {
            double successRate = Convert.ToDouble(this.winners.Count) / Convert.ToDouble(this.enteredUsers.Count);
            if (successRate == 1.0)
            {
                await this.PerformCommand(this.AllSucceedCommand, this.starterUser, new List<string>(), 0, 0);
            }
            else if (successRate > (2.0 / 3.0))
            {
                await this.PerformCommand(this.TopThirdsSucceedCommand, this.starterUser, new List<string>(), 0, 0);
            }
            else if (successRate > (1.0 / 3.0))
            {
                await this.PerformCommand(this.MiddleThirdsSucceedCommand, this.starterUser, new List<string>(), 0, 0);
            }
            else if (successRate > 0)
            {
                await this.PerformCommand(this.LowThirdsSucceedCommand, this.starterUser, new List<string>(), 0, 0);
            }
            else
            {
                await this.PerformCommand(this.NoneSucceedCommand, this.starterUser, new List<string>(), 0, 0);
            }
        }
    }

    [DataContract]
    public class RussianRouletteGameCommand : GroupGameCommand
    {
        [DataMember]
        public int MaxWinners { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        [JsonIgnore]
        private int betAmount = 0;

        public RussianRouletteGameCommand() { }

        public RussianRouletteGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, CustomCommand startedCommand,
            CustomCommand userJoinCommand, GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, int maxWinners, CustomCommand gameCompleteCommand, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, userSuccessOutcome, userFailOutcome, notEnoughPlayersCommand)
        {
            this.MaxWinners = maxWinners;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask != null)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("The game is already underway, type !{0} in chat to join!", this.Commands.First()));
                    return false;
                }
                return true;
            }
            return await base.PerformUsageChecks(user, arguments);
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            if (this.timeLimitTask != null)
            {
                return this.betAmount;
            }
            return await base.GetBetAmount(user, betAmountText);
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.betAmount = betAmount;
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task SelectWinners()
        {
            for (int i = 0; i < this.MaxWinners; i++)
            {
                int randomNumber = this.GenerateRandomNumber(this.enteredUsers.Count);
                UserViewModel winner = this.enteredUsers.ElementAt(randomNumber).Key;
                this.winners.Add(winner);
            }

            this.totalPayout = this.enteredUsers.Values.Sum();
            int individualPayout = this.totalPayout / this.winners.Count;

            foreach (var kvp in this.enteredUsers)
            {
                if (this.winners.Contains(kvp.Key))
                {
                    kvp.Key.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), individualPayout);
                    await this.PerformCommand(this.UserSuccessOutcome.Command, kvp.Key, new List<string>(), kvp.Value, individualPayout);
                }
                else
                {
                    await this.PerformOutcome(kvp.Key, new List<string>(), this.UserFailOutcome, kvp.Value);
                }
            }
        }

        protected override async Task GameCompleted()
        {
            UserViewModel winner = this.winners.FirstOrDefault();
            if (winner != null)
            {
                await this.PerformCommand(this.GameCompleteCommand, winner, new List<string>(), this.enteredUsers[winner], this.totalPayout);
            }
        }
    }

    [DataContract]
    public class TreasureDefenseGameCommand : GroupGameCommand
    {
        public const string GameWinLoseTypeSpecialIdentifier = "gameusertype";

        private enum WinLosePlayerType
        {
            King,
            Knight,
            Thief
        }

        [DataMember]
        public int ThiefPlayerPercentage { get; set; }

        [DataMember]
        public CustomCommand KnightUserCommand { get; set; }
        [DataMember]
        public CustomCommand ThiefUserCommand { get; set; }
        [DataMember]
        public CustomCommand KingUserCommand { get; set; }

        [DataMember]
        public CustomCommand KnightSelectedCommand { get; set; }
        [DataMember]
        public CustomCommand ThiefSelectedCommand { get; set; }

        [JsonIgnore]
        private int betAmount = 0;

        [JsonIgnore]
        private Dictionary<UserViewModel, WinLosePlayerType> playerTypes = new Dictionary<UserViewModel, WinLosePlayerType>();

        [JsonIgnore]
        private bool timeComplete;
        [JsonIgnore]
        private UserViewModel playerSelected = null;
        [JsonIgnore]
        protected int individualPayout = 0;

        public TreasureDefenseGameCommand() { }

        public TreasureDefenseGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, int losePlayerPercentage,
            CustomCommand startedCommand, CustomCommand userJoinCommand, CustomCommand knightUserCommand, CustomCommand thiefUserCommand, CustomCommand kingUserCommand,
            CustomCommand knightSelectedCommand, CustomCommand thiefSelectedCommand, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, null, null, notEnoughPlayersCommand)
        {
            this.ThiefPlayerPercentage = losePlayerPercentage;
            this.KnightUserCommand = knightUserCommand;
            this.ThiefUserCommand = thiefUserCommand;
            this.KingUserCommand = kingUserCommand;
            this.KnightSelectedCommand = knightSelectedCommand;
            this.ThiefSelectedCommand = thiefSelectedCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.KnightUserCommand);
            commands.Add(this.ThiefUserCommand);
            commands.Add(this.KingUserCommand);
            commands.Add(this.KnightSelectedCommand);
            commands.Add(this.ThiefSelectedCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.timeComplete)
            {
                if (this.playerTypes.ContainsKey(user) && this.playerTypes[user] == WinLosePlayerType.King)
                {
                    UserViewModel targetUser = await this.GetArgumentsTargetUser(user, arguments);
                    if (targetUser != null)
                    {
                        if (this.playerTypes.ContainsKey(targetUser) && this.playerTypes[targetUser] != WinLosePlayerType.King)
                        {
                            this.playerSelected = targetUser;
                        }
                        else
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "You must select a valid player who is participating in the game");
                            return;
                        }
                    }
                }
                else
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "Only the King can choose a defender");
                    return;
                }
            }
            else
            {
                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask != null)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("The game is already underway, type !{0} in chat to join!", this.Commands.First()));
                    return false;
                }
                return true;
            }
            else if (this.timeComplete)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("The game is already locked and no more players may join"));
                return false;
            }
            return await base.PerformUsageChecks(user, arguments);
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            if (this.timeLimitTask != null)
            {
                return this.betAmount;
            }
            return await base.GetBetAmount(user, betAmountText);
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.betAmount = betAmount;
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task TimeComplete()
        {
            this.timeComplete = true;

            this.playerSelected = null;

            foreach (UserViewModel user in this.enteredUsers.Keys.ToList())
            {
                this.playerTypes[user] = WinLosePlayerType.Knight;
            }

            int totalLosers = (int)Math.Ceiling((double)this.ThiefPlayerPercentage * (double)this.playerTypes.Count);
            IEnumerable<UserViewModel> shuffledPlayers = this.playerTypes.Keys.ToList().Shuffle();
            this.playerTypes[shuffledPlayers.First()] = WinLosePlayerType.King;
            foreach (UserViewModel user in shuffledPlayers.Skip(2).Take(totalLosers))
            {
                this.playerTypes[user] = WinLosePlayerType.Thief;
            }

            foreach (var kvp in this.playerTypes.ToList())
            {
                if (kvp.Value == WinLosePlayerType.Knight)
                {
                    await this.PerformCommand(this.KnightUserCommand, kvp.Key, null, 0, 0);
                }
                else if (kvp.Value == WinLosePlayerType.Thief)
                {
                    await this.PerformCommand(this.ThiefUserCommand, kvp.Key, null, 0, 0);
                }
            }

            await this.PerformCommand(this.KingUserCommand, shuffledPlayers.First(), null, 0, 0);

            do
            {
                await Task.Delay(1000);
            } while (this.playerSelected == null);
        }

        protected override Task SelectWinners()
        {
            if (this.playerTypes[this.playerSelected] == WinLosePlayerType.Knight)
            {
                foreach (var kvp in this.playerTypes.Where(kvp => kvp.Value != WinLosePlayerType.Thief))
                {
                    this.winners.Add(kvp.Key);
                }
            }
            else
            {
                foreach (var kvp in this.playerTypes.Where(kvp => kvp.Value == WinLosePlayerType.Thief))
                {
                    this.winners.Add(kvp.Key);
                }
            }

            this.totalPayout = this.enteredUsers.Values.Sum();
            this.individualPayout = this.totalPayout / this.winners.Count;

            foreach (var kvp in this.enteredUsers)
            {
                if (this.winners.Contains(kvp.Key))
                {
                    kvp.Key.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), this.individualPayout);
                }
            }

            return Task.FromResult(0);
        }

        protected override async Task GameCompleted()
        {
            UserViewModel king = this.playerTypes.FirstOrDefault(kvp => kvp.Value == WinLosePlayerType.King).Key;
            if (this.playerTypes[this.playerSelected] == WinLosePlayerType.Knight)
            {
                await this.PerformCommand(this.KnightSelectedCommand, king, new List<string>() { this.playerSelected.UserName }, this.betAmount, this.individualPayout);
            }
            else
            {
                await this.PerformCommand(this.ThiefSelectedCommand, king, new List<string>() { this.playerSelected.UserName }, this.betAmount, this.individualPayout);
            }
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.playerTypes.ContainsKey(user))
            {
                specialIdentifiers[GameWinLoseTypeSpecialIdentifier] = this.playerTypes[user].ToString();
            }
            base.AddAdditionalSpecialIdentifiers(user, arguments, specialIdentifiers);
        }

        protected override void ResetData(UserViewModel user)
        {
            this.betAmount = 0;
            this.playerTypes.Clear();
            this.timeComplete = false;
            this.playerSelected = null;
            base.ResetData(user);
        }
    }

    [DataContract]
    public class BidGameCommand : GroupGameCommand
    {
        [DataMember]
        public RoleRequirementViewModel GameStarterRequirement { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        [JsonIgnore]
        public UserViewModel highestUser = null;
        [JsonIgnore]
        public int highestBid = 0;

        public BidGameCommand() { }

        public BidGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, RoleRequirementViewModel gameStarterRequirement,
            CustomCommand startedCommand, CustomCommand userJoinCommand, CustomCommand gameCompleteCommand, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, null, null, notEnoughPlayersCommand)
        {
            this.GameStarterRequirement = gameStarterRequirement;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask == null)
            {
                if (!this.GameStarterRequirement.DoesMeetRequirement(user))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to start this game", this.GameStarterRequirement.RoleNameString));
                    return false;
                }
            }
            return await base.PerformUsageChecks(user, arguments);
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountSecondArgument(arguments);
        }

        protected override Task<bool> CanUserEnter(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            return Task.FromResult(true);
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            int betAmount = await base.GetBetAmount(user, betAmountText);
            if (betAmount >= 0 && betAmount <= this.highestBid)
            {
                await ChannelSession.Chat.Whisper(user.UserName, "You must specify an amount higher than the current highest: " + this.highestBid);
                return -1;
            }
            return betAmount;
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.highestUser = user;
            this.highestBid = betAmount;

            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task UserJoined(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (this.highestUser != null)
            {
                this.highestUser.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), this.highestBid);
            }

            await base.UserJoined(user, arguments, betAmount);

            this.highestUser = user;
            this.highestBid = betAmount;
        }

        protected override async Task NotEnoughUsers()
        {
            this.highestUser.Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), this.highestBid);
            await ChannelSession.Chat.SendMessage(string.Format("@{0} couldn't get enough users to join in...", this.starterUser.UserName));
        }

        protected override Task SelectWinners()
        {
            this.totalPayout = this.highestBid;
            this.winners.Add(this.highestUser);
            return Task.FromResult(0);
        }

        protected override async Task GameCompleted()
        {
            await this.PerformCommand(this.GameCompleteCommand, this.highestUser, new List<string>(), this.highestBid, this.totalPayout);
        }

        protected override void ResetData(UserViewModel user)
        {
            this.highestUser = null;
            this.highestBid = 0;
            base.ResetData(user);
        }
    }

    [DataContract]
    public class BetGameCommand : GroupGameCommand
    {
        public const string GameBetOptionsSpecialIdentifier = "gamebetoptions";
        public const string GameBetWinningOptionSpecialIdentifier = "gamebetwinningoption";

        [DataMember]
        public RoleRequirementViewModel GameStarterRequirement { get; set; }

        [DataMember]
        public List<GameOutcome> BetOptions { get; set; }

        [DataMember]
        public CustomCommand BetsClosedCommand { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        [JsonIgnore]
        private Dictionary<UserViewModel, int> userOptionSelection = new Dictionary<UserViewModel, int>();
        [JsonIgnore]
        private bool timeComplete = false;
        [JsonIgnore]
        private int winningOption = 0;

        public BetGameCommand() { }

        public BetGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, RoleRequirementViewModel gameStarterRequirement,
            IEnumerable<GameOutcome> options, CustomCommand startedCommand, CustomCommand userJoinCommand, CustomCommand betsClosedCommand, GameOutcome userFailureOutcome, CustomCommand gameCompleteCommand,
            CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, null, userFailureOutcome, notEnoughPlayersCommand)
        {
            this.BetOptions = new List<GameOutcome>(options);
            this.GameStarterRequirement = gameStarterRequirement;
            this.BetsClosedCommand = betsClosedCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.BetsClosedCommand);
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.timeComplete)
            {
                if (arguments.Count() == 2 && arguments.ElementAt(0).Equals("answer") && int.TryParse(arguments.ElementAt(1), out int option) && option > 0 && option <= this.BetOptions.Count)
                {
                    if (!this.GameStarterRequirement.DoesMeetRequirement(user))
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to pick the answer", this.GameStarterRequirement.RoleNameString));
                        return;
                    }
                    this.winningOption = option;
                    return;
                }
                await ChannelSession.Chat.Whisper(user.UserName, "All betting is currently closed");
            }
            else
            {
                if (this.timeLimitTask == null && !this.GameStarterRequirement.DoesMeetRequirement(user))
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("You must be a {0} to start this game", this.GameStarterRequirement.RoleNameString));
                    return;
                }

                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask == null && !this.timeComplete)
            {
                return true;
            }
            if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments.Count() != 1)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <OPTION #>", this.Commands.First()));
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
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <OPTION #> {1}", this.Commands.First(), betAmountUsageText));
                return false;
            }
            return true;
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            if (this.timeLimitTask == null && !this.timeComplete)
            {
                return 0;
            }
            return await base.GetBetAmount(user, betAmountText);
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountSecondArgument(arguments);
        }

        protected override async Task<bool> CanUserEnter(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (this.timeLimitTask == null && !this.timeComplete)
            {
                return true;
            }

            if (!int.TryParse(arguments.First(), out int option) || option <= 0 || option > this.BetOptions.Count)
            {
                await ChannelSession.Chat.Whisper(user.UserName, "The option number you selected is not a valid number");
                return false;
            }

            return true;
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.userOptionSelection.Clear();
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task UserJoined(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.userOptionSelection[user] = int.Parse(arguments.First());
            await base.UserJoined(user, arguments, betAmount);
        }

        protected override async Task NotEnoughUsers()
        {
            await ChannelSession.Chat.SendMessage(string.Format("@{0} couldn't get enough users to join in...", this.starterUser.UserName));
        }

        protected override async Task TimeComplete()
        {
            this.timeComplete = true;

            await this.PerformCommand(this.BetsClosedCommand, null, null, 0, 0);

            do
            {
                await Task.Delay(1000);
            } while (this.winningOption <= 0);
        }

        protected override async Task SelectWinners()
        {
            GameOutcome winningOutcome = this.BetOptions[this.winningOption - 1];
            foreach (var kvp in this.userOptionSelection)
            {
                if (kvp.Value == this.winningOption)
                {
                    this.winners.Add(kvp.Key);
                    await this.PerformOutcome(kvp.Key, new List<string>(), winningOutcome, this.enteredUsers[kvp.Key]);
                }
                else
                {
                    await this.PerformOutcome(kvp.Key, new List<string>(), this.UserFailOutcome, this.enteredUsers[kvp.Key]);
                }
            }
        }

        protected override async Task GameCompleted()
        {
            await this.PerformCommand(this.GameCompleteCommand, null, new List<string>(), 0, 0);
        }

        protected override void ResetData(UserViewModel user)
        {
            this.timeComplete = false;
            this.winningOption = 0;
            base.ResetData(user);
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            List<string> optionStrings = new List<string>();
            for (int i = 0; i < this.BetOptions.Count; i++)
            {
                optionStrings.Add(string.Format("{0}) {1}", (i + 1), this.BetOptions[i].Name));
            }

            specialIdentifiers[GameBetOptionsSpecialIdentifier] = string.Join(" ", optionStrings);
            if (this.winningOption > 0)
            {
                specialIdentifiers[GameBetWinningOptionSpecialIdentifier] = this.BetOptions[this.winningOption - 1].Name;
            }
        }
    }

    [DataContract]
    public class RouletteGameCommand : GroupGameCommand
    {
        public const string GameBetTypeSpecialIdentifier = "gamebettype";

        public const string GameValidBetTypesSpecialIdentifier = "gamevalidbettypes";

        public const string GameWinningBetTypeSpecialIdentifier = "gamewinningbettype";

        [DataMember]
        public bool IsNumberRange { get; set; }
        [DataMember]
        public HashSet<string> ValidBetTypes { get; set; }

        [DataMember]
        public CustomCommand GameCompleteCommand { get; set; }

        [JsonIgnore]
        private Dictionary<UserViewModel, string> userBetTypes = new Dictionary<UserViewModel, string>();
        [JsonIgnore]
        private string winningBetType = string.Empty;

        public RouletteGameCommand() { }

        public RouletteGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, bool isNumberRange, HashSet<string> validBetTypes,
            CustomCommand startedCommand, CustomCommand userJoinCommand, GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand gameCompleteCommand, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, userSuccessOutcome, userFailOutcome, notEnoughPlayersCommand)
        {
            this.IsNumberRange = isNumberRange;
            this.ValidBetTypes = validBetTypes;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.Requirements.Currency.RequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments.Count() != 1)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <NUMBER>", this.Commands.First()));
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
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <NUMBER> {1}", this.Commands.First(), betAmountUsageText));
                return false;
            }
            return true;
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountSecondArgument(arguments);
        }

        protected override async Task<bool> CanUserEnter(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            string betType = arguments.ElementAt(0).ToLower();
            if (!this.ValidBetTypes.Contains(betType))
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("Valid Bet Types: {0}", this.GetValidBetTypeString()));
                return false;
            }
            return await base.CanUserEnter(user, arguments, betAmount);
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.userBetTypes[user] = arguments.ElementAt(0).ToLower();
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task UserJoined(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.userBetTypes[user] = arguments.ElementAt(0).ToLower();
            await base.UserJoined(user, arguments, betAmount);
        }

        protected override async Task SelectWinners()
        {
            int randomNumber = this.GenerateRandomNumber(this.ValidBetTypes.Count);
            this.winningBetType = this.ValidBetTypes.ElementAt(randomNumber);

            foreach (var enteredUser in this.enteredUsers)
            {
                if (this.userBetTypes.ContainsKey(enteredUser.Key) && this.userBetTypes[enteredUser.Key].Equals(this.winningBetType))
                {
                    this.winners.Add(enteredUser.Key);
                    await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserSuccessOutcome, enteredUser.Value);
                }
                else
                {
                    await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserFailOutcome, enteredUser.Value);
                }
            }
        }

        protected override async Task GameCompleted()
        {
            await this.PerformCommand(this.GameCompleteCommand, null, new List<string>(), 0, this.totalPayout);
        }

        protected override void ResetData(UserViewModel user)
        {
            this.userBetTypes.Clear();
            base.ResetData(user);
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (user != null)
            {
                specialIdentifiers[GameBetTypeSpecialIdentifier] = this.userBetTypes[user];
            }
            specialIdentifiers[GameValidBetTypesSpecialIdentifier] = this.GetValidBetTypeString();
            if (!string.IsNullOrEmpty(this.winningBetType))
            {
                specialIdentifiers[GameWinningBetTypeSpecialIdentifier] = winningBetType;
            }
        }

        private string GetValidBetTypeString()
        {
            if (this.IsNumberRange)
            {
                IEnumerable<int> numbers = this.ValidBetTypes.Select(s => int.Parse(s));
                return numbers.Min() + "-" + numbers.Max();
            }
            else
            {
                return string.Join(", ", this.ValidBetTypes);
            }
        }
    }

    [DataContract]
    public class HitmanGameCommand : GroupGameCommand
    {
        public const string GameHitmanNameSpecialIdentifier = "gamehitmanname";

        public static readonly HashSet<string> DefaultWords = new HashSet<string>() { "ABLE", "ACCEPTABLE", "ACCORDING", "ACCURATE", "ACTION", "ACTIVE", "ACTUAL", "ADDITIONAL", "ADMINISTRATIVE", "ADULT", "AFRAID", "AFTER", "AFTERNOON", "AGENT", "AGGRESSIVE", "AGO", "AIRLINE", "ALIVE", "ALL", "ALONE", "ALTERNATIVE", "AMAZING", "ANGRY", "ANIMAL", "ANNUAL", "ANOTHER", "ANXIOUS", "ANY", "APART", "APPROPRIATE", "ASLEEP", "AUTOMATIC", "AVAILABLE", "AWARE", "AWAY", "BACKGROUND", "BASIC", "BEAUTIFUL", "BEGINNING", "BEST", "BETTER", "BIG", "BITTER", "BORING", "BORN", "BOTH", "BRAVE", "BRIEF", "BRIGHT", "BRILLIANT", "BROAD", "BROWN", "BUDGET", "BUSINESS", "BUSY", "CALM", "CAPABLE", "CAPITAL", "CAR", "CAREFUL", "CERTAIN", "CHANCE", "CHARACTER", "CHEAP", "CHEMICAL", "CHICKEN", "CHOICE", "CIVIL", "CLASSIC", "CLEAN", "CLEAR", "CLOSE", "COLD", "COMFORTABLE", "COMMERCIAL", "COMMON", "COMPETITIVE", "COMPLETE", "COMPLEX", "COMPREHENSIVE", "CONFIDENT", "CONNECT", "CONSCIOUS", "CONSISTENT", "CONSTANT", "CONTENT", "COOL", "CORNER", "CORRECT", "CRAZY", "CREATIVE", "CRITICAL", "CULTURAL", "CURIOUS", "CURRENT", "CUTE", "DANGEROUS", "DARK", "DAUGHTER", "DAY", "DEAD", "DEAR", "DECENT", "DEEP", "DEPENDENT", "DESIGNER", "DESPERATE", "DIFFERENT", "DIFFICULT", "DIRECT", "DIRTY", "DISTINCT", "DOUBLE", "DOWNTOWN", "DRAMATIC", "DRESS", "DRUNK", "DRY", "DUE", "EACH", "EAST", "EASTERN", "EASY", "ECONOMY", "EDUCATIONAL", "EFFECTIVE", "EFFICIENT", "EITHER", "ELECTRICAL", "ELECTRONIC", "EMBARRASSED", "EMERGENCY", "EMOTIONAL", "EMPTY", "ENOUGH", "ENTIRE", "ENVIRONMENTAL", "EQUAL", "EQUIVALENT", "EVEN", "EVENING", "EVERY", "EXACT", "EXCELLENT", "EXCITING", "EXISTING", "EXPENSIVE", "EXPERT", "EXPRESS", "EXTENSION", "EXTERNAL", "EXTRA", "EXTREME", "FAIR", "FAMILIAR", "FAMOUS", "FAR", "FAST", "FAT", "FEDERAL", "FEELING", "FEMALE", "FEW", "FINAL", "FINANCIAL", "FINE", "FIRM", "FIRST", "FIT", "FLAT", "FOREIGN", "FORMAL", "FORMER", "FORWARD", "FREE", "FREQUENT", "FRESH", "FRIENDLY", "FRONT", "FULL", "FUN", "FUNNY", "FUTURE", "GAME", "GENERAL", "GLAD", "GLASS", "GLOBAL", "GOLD", "GOOD", "GRAND", "GREAT", "GREEN", "GROSS", "GUILTY", "HAPPY", "HARD", "HEAD", "HEALTHY", "HEAVY", "HELPFUL", "HIGH", "HIS", "HISTORICAL", "HOLIDAY", "HOME", "HONEST", "HORROR", "HOT", "HOUR", "HOUSE", "HUGE", "HUMAN", "HUNGRY", "IDEAL", "ILL", "ILLEGAL", "IMMEDIATE", "IMPORTANT", "IMPOSSIBLE", "IMPRESSIVE", "INCIDENT", "INDEPENDENT", "INDIVIDUAL", "INEVITABLE", "INFORMAL", "INITIAL", "INNER", "INSIDE", "INTELLIGENT", "INTERESTING", "INTERNAL", "INTERNATIONAL", "JOINT", "JUNIOR", "JUST", "KEY", "KIND", "KITCHEN", "KNOWN", "LARGE", "LAST", "LATE", "LATTER", "LEADING", "LEAST", "LEATHER", "LEFT", "LEGAL", "LESS", "LEVEL", "LIFE", "LITTLE", "LIVE", "LIVING", "LOCAL", "LOGICAL", "LONELY", "LONG", "LOOSE", "LOST", "LOUD", "LOW", "LOWER", "LUCKY", "MAD", "MAIN", "MAJOR", "MALE", "MANY", "MASSIVE", "MASTER", "MATERIAL", "MAXIMUM", "MEAN", "MEDICAL", "MEDIUM", "MENTAL", "MIDDLE", "MINIMUM", "MINOR", "MINUTE", "MISSION", "MOBILE", "MONEY", "MORE", "MOST", "MOTHER", "MOTOR", "MOUNTAIN", "MUCH", "NARROW", "NASTY", "NATIONAL", "NATIVE", "NATURAL", "NEARBY", "NEAT", "NECESSARY", "NEGATIVE", "NEITHER", "NERVOUS", "NEW", "NEXT", "NICE", "NO", "NORMAL", "NORTH", "NOVEL", "NUMEROUS", "OBJECTIVE", "OBVIOUS", "ODD", "OFFICIAL", "OK", "OLD", "ONE", "ONLY", "OPEN", "OPENING", "OPPOSITE", "ORDINARY", "ORIGINAL", "OTHER", "OTHERWISE", "OUTSIDE", "OVER", "OVERALL", "OWN", "PARKING", "PARTICULAR", "PARTY", "PAST", "PATIENT", "PERFECT", "PERIOD", "PERSONAL", "PHYSICAL", "PLANE", "PLASTIC", "PLEASANT", "PLENTY", "PLUS", "POLITICAL", "POOR", "POPULAR", "POSITIVE", "POSSIBLE", "POTENTIAL", "POWERFUL", "PRACTICAL", "PREGNANT", "PRESENT", "PRETEND", "PRETTY", "PREVIOUS", "PRIMARY", "PRIOR", "PRIVATE", "PRIZE", "PROFESSIONAL", "PROOF", "PROPER", "PROUD", "PSYCHOLOGICAL", "PUBLIC", "PURE", "PURPLE", "QUICK", "QUIET", "RARE", "RAW", "READY", "REAL", "REALISTIC", "REASONABLE", "RECENT", "RED", "REGULAR", "RELATIVE", "RELEVANT", "REMARKABLE", "REMOTE", "REPRESENTATIVE", "RESIDENT", "RESPONSIBLE", "RICH", "RIGHT", "ROUGH", "ROUND", "ROUTINE", "ROYAL", "SAD", "SAFE", "SALT", "SAME", "SAVINGS", "SCARED", "SEA", "SECRET", "SECURE", "SELECT", "SENIOR", "SENSITIVE", "SEPARATE", "SERIOUS", "SEVERAL", "SEVERE", "SEXUAL", "SHARP", "SHORT", "SHOT", "SICK", "SIGNAL", "SIGNIFICANT", "SILLY", "SILVER", "SIMILAR", "SIMPLE", "SINGLE", "SLIGHT", "SLOW", "SMALL", "SMART", "SMOOTH", "SOFT", "SOLID", "SOME", "SORRY", "SOUTH", "SOUTHERN", "SPARE", "SPECIAL", "SPECIALIST", "SPECIFIC", "SPIRITUAL", "SQUARE", "STANDARD", "STATUS", "STILL", "STOCK", "STRAIGHT", "STRANGE", "STREET", "STRICT", "STRONG", "STUPID", "SUBJECT", "SUBSTANTIAL", "SUCCESSFUL", "SUCH", "SUDDEN", "SUFFICIENT", "SUITABLE", "SUPER", "SURE", "SUSPICIOUS", "SWEET", "SWIMMING", "TALL", "TECHNICAL", "TEMPORARY", "TERRIBLE", "THAT", "THEN", "THESE", "THICK", "THIN", "THINK", "THIS", "TIGHT", "TIME", "TINY", "TOP", "TOTAL", "TOUGH", "TRADITIONAL", "TRAINING", "TRICK", "TYPICAL", "UGLY", "UNABLE", "UNFAIR", "UNHAPPY", "UNIQUE", "UNITED", "UNLIKELY", "UNUSUAL", "UPPER", "UPSET", "UPSTAIRS", "USED", "USEFUL", "USUAL", "VALUABLE", "VARIOUS", "VAST", "VEGETABLE", "VISIBLE", "VISUAL", "WARM", "WASTE", "WEAK", "WEEKLY", "WEIRD", "WEST", "WESTERN", "WHAT", "WHICH", "WHITE", "WHOLE", "WIDE", "WILD", "WILLING", "WINE", "WINTER", "WISE", "WONDERFUL", "WOODEN", "WORK", "WORKING", "WORTH", "WRONG", "YELLOW", "YOUNG" };

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommand HitmanApproachingCommand { get; set; }
        [DataMember]
        public CustomCommand HitmanAppearsCommand { get; set; }

        [DataMember]
        public int HitmanTimeLimit { get; set; }

        [JsonIgnore]
        private int betAmount = 0;
        [JsonIgnore]
        private string hitmanName = null;

        public HitmanGameCommand() { }

        public HitmanGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, string customWordsFilePath,
            int hitmanTimeLimit, CustomCommand startedCommand, CustomCommand userJoinCommand, CustomCommand hitmanApproachingCommand, CustomCommand hitmanAppearsCommand,
            GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, userSuccessOutcome, userFailOutcome, notEnoughPlayersCommand)
        {
            this.CustomWordsFilePath = customWordsFilePath;
            this.HitmanApproachingCommand = hitmanApproachingCommand;
            this.HitmanAppearsCommand = hitmanAppearsCommand;
            this.HitmanTimeLimit = hitmanTimeLimit;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.HitmanApproachingCommand);
            commands.Add(this.HitmanAppearsCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask != null)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("The game is already underway, type !{0} in chat to join!", this.Commands.First()));
                    return false;
                }
                return true;
            }
            return await base.PerformUsageChecks(user, arguments);
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            if (this.timeLimitTask != null)
            {
                return this.betAmount;
            }
            return await base.GetBetAmount(user, betAmountText);
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.betAmount = betAmount;
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task SelectWinners()
        {
            await this.PerformCommand(this.HitmanApproachingCommand, this.starterUser, new List<string>(), this.betAmount, 0);

            HashSet<string> wordsToUse = HitmanGameCommand.DefaultWords;
            if (!string.IsNullOrEmpty(this.CustomWordsFilePath) && ChannelSession.Services.FileService.FileExists(this.CustomWordsFilePath))
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(this.CustomWordsFilePath);
                if (!string.IsNullOrEmpty(fileData))
                {
                    wordsToUse = new HashSet<string>();
                    foreach (string split in fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordsToUse.Add(split);
                    }
                }
            }

            await Task.Delay(5000);

            int randomNumber = this.GenerateRandomNumber(wordsToUse.Count);
            this.hitmanName = wordsToUse.ElementAt(randomNumber);

            ChannelSession.Chat.OnMessageOccurred += Chat_OnMessageOccurred;

            await this.PerformCommand(this.HitmanAppearsCommand, this.starterUser, new List<string>(), this.betAmount, 0);

            for (int i = 0; i < this.HitmanTimeLimit * 2; i++)
            {
                await Task.Delay(500);
                if (this.winners.Count > 0)
                {
                    break;
                }
            }

            ChannelSession.Chat.OnMessageOccurred -= Chat_OnMessageOccurred;

            if (this.winners.Count > 0)
            {
                this.totalPayout = this.enteredUsers.Values.Sum();
                this.winners.First().Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), this.totalPayout);
                await this.PerformCommand(this.UserSuccessOutcome.Command, this.winners.First(), new List<string>(), this.betAmount, this.totalPayout);
            }
            else
            {
                await this.PerformCommand(this.UserFailOutcome.Command, await ChannelSession.GetCurrentUser(), new List<string>(), this.betAmount, this.totalPayout);
            }
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(this.hitmanName))
            {
                specialIdentifiers[GameHitmanNameSpecialIdentifier] = this.hitmanName;
            }
        }

        private void Chat_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            if (!string.IsNullOrEmpty(this.hitmanName) && this.winners.Count == 0 && this.enteredUsers.ContainsKey(message.User))
            {
                if (!string.IsNullOrEmpty(message.Message) && message.Message.Equals(this.hitmanName, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.winners.Add(message.User);
                }
            }
        }
    }

    [DataContract]
    public class WordScrambleGameCommand : GroupGameCommand
    {
        public const string GameWordScrambleWordSpecialIdentifier = "gamewordscrambleword";
        public const string GameWordScrambleAnswerSpecialIdentifier = "gamewordscrambleanswer";

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommand WordScramblePrepareCommand { get; set; }
        [DataMember]
        public CustomCommand WordScrambleBeginCommand { get; set; }

        [DataMember]
        public int WordScrambleTimeLimit { get; set; }

        [JsonIgnore]
        private int betAmount = 0;
        [JsonIgnore]
        private string selectedWord = null;
        [JsonIgnore]
        private string selectedWordScrambled = null;

        public WordScrambleGameCommand() { }

        public WordScrambleGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, string customWordsFilePath,
            int wordScrambleTimeLimit, CustomCommand startedCommand, CustomCommand userJoinCommand, CustomCommand wordScramblePrepareCommand, CustomCommand wordScrambleBeginCommand,
            GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand notEnoughPlayersCommand)
            : base(name, commands, requirements, minimumParticipants, timeLimit, startedCommand, userJoinCommand, userSuccessOutcome, userFailOutcome, notEnoughPlayersCommand)
        {
            this.CustomWordsFilePath = customWordsFilePath;
            this.WordScramblePrepareCommand = wordScramblePrepareCommand;
            this.WordScrambleBeginCommand = wordScrambleBeginCommand;
            this.WordScrambleTimeLimit = wordScrambleTimeLimit;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.WordScramblePrepareCommand);
            commands.Add(this.WordScrambleBeginCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.timeLimitTask != null)
            {
                if (arguments.Count() != 0)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("The game is already underway, type !{0} in chat to join!", this.Commands.First()));
                    return false;
                }
                return true;
            }
            return await base.PerformUsageChecks(user, arguments);
        }

        protected override async Task<int> GetBetAmount(UserViewModel user, string betAmountText)
        {
            if (this.timeLimitTask != null)
            {
                return this.betAmount;
            }
            return await base.GetBetAmount(user, betAmountText);
        }

        protected override async Task GameStarted(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.betAmount = betAmount;
            await base.GameStarted(user, arguments, betAmount);
        }

        protected override async Task SelectWinners()
        {
            await this.PerformCommand(this.WordScramblePrepareCommand, this.starterUser, new List<string>(), this.betAmount, 0);

            HashSet<string> wordsToUse = new HashSet<string>(HitmanGameCommand.DefaultWords.Where(s => s.Length > 4));
            if (!string.IsNullOrEmpty(this.CustomWordsFilePath) && ChannelSession.Services.FileService.FileExists(this.CustomWordsFilePath))
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(this.CustomWordsFilePath);
                if (!string.IsNullOrEmpty(fileData))
                {
                    wordsToUse = new HashSet<string>();
                    foreach (string split in fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordsToUse.Add(split);
                    }
                }
            }

            await Task.Delay(5000);

            int randomNumber = this.GenerateRandomNumber(wordsToUse.Count);
            this.selectedWord = wordsToUse.ElementAt(randomNumber);
            this.selectedWordScrambled = this.selectedWord.Shuffle();

            ChannelSession.Chat.OnMessageOccurred += Chat_OnMessageOccurred;

            await this.PerformCommand(this.WordScrambleBeginCommand, this.starterUser, new List<string>(), this.betAmount, 0);

            for (int i = 0; i < this.WordScrambleTimeLimit * 2; i++)
            {
                await Task.Delay(500);
                if (this.winners.Count > 0)
                {
                    break;
                }
            }

            ChannelSession.Chat.OnMessageOccurred -= Chat_OnMessageOccurred;

            if (this.winners.Count > 0)
            {
                this.totalPayout = this.enteredUsers.Values.Sum();
                this.winners.First().Data.AddCurrencyAmount(this.Requirements.Currency.GetCurrency(), this.totalPayout);
                await this.PerformCommand(this.UserSuccessOutcome.Command, this.winners.First(), new List<string>(), this.betAmount, this.totalPayout);
            }
            else
            {
                await this.PerformCommand(this.UserFailOutcome.Command, await ChannelSession.GetCurrentUser(), new List<string>(), this.betAmount, this.totalPayout);
            }
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(this.selectedWord))
            {
                specialIdentifiers[GameWordScrambleWordSpecialIdentifier] = this.selectedWordScrambled;
                specialIdentifiers[GameWordScrambleAnswerSpecialIdentifier] = this.selectedWord;
            }
        }

        private void Chat_OnMessageOccurred(object sender, ChatMessageViewModel message)
        {
            if (!string.IsNullOrEmpty(this.selectedWord) && this.winners.Count == 0 && this.enteredUsers.ContainsKey(message.User))
            {
                if (!string.IsNullOrEmpty(message.Message) && message.Message.Equals(this.selectedWord, StringComparison.CurrentCultureIgnoreCase))
                {
                    this.winners.Add(message.User);
                }
            }
        }
    }

    [DataContract]
    public class CoinPusherGameCommand : LongRunningGameCommand
    {
        [DataMember]
        public int MinimumAmountForPayout { get; set; }
        [DataMember]
        public int PayoutProbability { get; set; }

        [DataMember]
        public double PayoutPercentageMinimum { get; set; }
        [DataMember]
        public double PayoutPercentageMaximum { get; set; }

        [DataMember]
        public CustomCommand StatusCommand { get; set; }
        [DataMember]
        public CustomCommand NoPayoutCommand { get; set; }
        [DataMember]
        public CustomCommand PayoutCommand { get; set; }

        public CoinPusherGameCommand() { }

        public CoinPusherGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, string statusArgument, int minimumAmountForPayout, int payoutProbability,
            double payoutPercentageMinimum, double payoutPercentageMaximum, CustomCommand statusCommand, CustomCommand noPayoutCommand, CustomCommand payoutCommand)
            : base(name, commands, requirements, statusArgument)
        {
            this.MinimumAmountForPayout = minimumAmountForPayout;
            this.PayoutProbability = payoutProbability;
            this.PayoutPercentageMinimum = payoutPercentageMinimum;
            this.PayoutPercentageMaximum = payoutPercentageMaximum;
            this.StatusCommand = statusCommand;
            this.NoPayoutCommand = noPayoutCommand;
            this.PayoutCommand = payoutCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.StatusCommand);
            commands.Add(this.NoPayoutCommand);
            commands.Add(this.PayoutCommand);
            return commands;
        }

        protected override async Task ReportStatus(UserViewModel user, IEnumerable<string> arguments)
        {
            await this.PerformCommand(this.StatusCommand, user, arguments, 0, 0);
        }

        protected override async Task<bool> ShouldPerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (this.TotalAmount >= this.MinimumAmountForPayout)
            {
                int randomNumber = this.GenerateProbability();
                if (randomNumber <= this.PayoutProbability)
                {
                    return true;
                }
            }

            await this.PerformCommand(this.NoPayoutCommand, user, arguments, betAmount, 0);
            return false;
        }

        protected override async Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            double amount = Convert.ToDouble(this.TotalAmount);
            int minimum = Convert.ToInt32(amount * this.PayoutPercentageMinimum);
            int maximum = Convert.ToInt32(amount * this.PayoutPercentageMaximum);

            this.totalPayout = this.GenerateRandomNumber(minimum, maximum + 1);

            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return;
            }
            user.Data.AddCurrencyAmount(currency, this.totalPayout);
            this.TotalAmount -= this.totalPayout;

            await this.PerformCommand(this.PayoutCommand, user, arguments, betAmount, this.totalPayout);
        }
    }

    [DataContract]
    public class VolcanoGameCommand : LongRunningGameCommand
    {
        [DataMember]
        public CustomCommand Stage1DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage1StatusCommand { get; set; }

        [DataMember]
        public int Stage2MinimumAmount { get; set; }
        [DataMember]
        public CustomCommand Stage2DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage2StatusCommand { get; set; }

        [DataMember]
        public int Stage3MinimumAmount { get; set; }
        [DataMember]
        public CustomCommand Stage3DepositCommand { get; set; }
        [DataMember]
        public CustomCommand Stage3StatusCommand { get; set; }

        [DataMember]
        public int PayoutProbability { get; set; }
        [DataMember]
        public double PayoutPercentageMinimum { get; set; }
        [DataMember]
        public double PayoutPercentageMaximum { get; set; }
        [DataMember]
        public CustomCommand PayoutCommand { get; set; }

        [DataMember]
        public string CollectArgument { get; set; }
        [DataMember]
        public int CollectTimeLimit { get; set; }
        [DataMember]
        public double CollectPayoutPercentageMinimum { get; set; }
        [DataMember]
        public double CollectPayoutPercentageMaximum { get; set; }
        [DataMember]
        public CustomCommand CollectCommand { get; set; }

        [JsonIgnore]
        private bool collectActive = false;
        [JsonIgnore]
        private HashSet<UserViewModel> collectUsers = new HashSet<UserViewModel>();

        public VolcanoGameCommand() { }

        public VolcanoGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, string statusArgument, CustomCommand stage1DepositCommand, CustomCommand stage1StatusCommand,
            int stage2MinimumAmount, CustomCommand stage2DepositCommand, CustomCommand stage2StatusCommand, int stage3MinimumAmount, CustomCommand stage3DepositCommand, CustomCommand stage3StatusCommand,
            int payoutProbability, double payoutPercentageMinimum, double payoutPercentageMaximum, CustomCommand payoutCommand, string collectArgument, int collectTimeLimit,
            double collectPayoutPercentageMinimum, double collectPayoutPercentageMaximum, CustomCommand collectCommand)
            : base(name, commands, requirements, statusArgument)
        {
            this.Stage1DepositCommand = stage1DepositCommand;
            this.Stage1StatusCommand = stage1StatusCommand;
            this.Stage2MinimumAmount = stage2MinimumAmount;
            this.Stage2DepositCommand = stage2DepositCommand;
            this.Stage2StatusCommand = stage2StatusCommand;
            this.Stage3MinimumAmount = stage3MinimumAmount;
            this.Stage3DepositCommand = stage3DepositCommand;
            this.Stage3StatusCommand = stage3StatusCommand;
            this.PayoutProbability = payoutProbability;
            this.PayoutPercentageMinimum = payoutPercentageMinimum;
            this.PayoutPercentageMaximum = payoutPercentageMaximum;
            this.PayoutCommand = payoutCommand;
            this.CollectArgument = collectArgument;
            this.CollectTimeLimit = collectTimeLimit;
            this.CollectPayoutPercentageMinimum = collectPayoutPercentageMinimum;
            this.CollectPayoutPercentageMaximum = collectPayoutPercentageMaximum;
            this.CollectCommand = collectCommand;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.Stage1DepositCommand);
            commands.Add(this.Stage1StatusCommand);
            commands.Add(this.Stage2DepositCommand);
            commands.Add(this.Stage2StatusCommand);
            commands.Add(this.Stage3DepositCommand);
            commands.Add(this.Stage3StatusCommand);
            commands.Add(this.PayoutCommand);
            commands.Add(this.CollectCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (this.collectActive)
            {
                if (await this.PerformNonCooldownRequirementChecks(user))
                {
                    if (arguments.Count() == 1 && arguments.ElementAt(0).Equals(this.CollectArgument))
                    {
                        if (this.collectUsers.Contains(user))
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, "You've already collected your share");
                            return;
                        }

                        this.collectUsers.Add(user);
                        await this.PerformPayout(user, arguments, 0, this.CollectPayoutPercentageMinimum, this.CollectPayoutPercentageMaximum, this.CollectCommand);
                    }
                    else
                    {
                        await ChannelSession.Chat.Whisper(user.UserName, "Collecting is currently underway, please wait until it has completed");
                    }
                }
            }
            else
            {
                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }

        protected override async Task ReportStatus(UserViewModel user, IEnumerable<string> arguments)
        {
            if (this.TotalAmount >= this.Stage3MinimumAmount)
            {
                await this.PerformCommand(this.Stage3StatusCommand, user, arguments, 0, 0);
            }
            else if (this.TotalAmount >= this.Stage2MinimumAmount)
            {
                await this.PerformCommand(this.Stage2StatusCommand, user, arguments, 0, 0);
            }
            else
            {
                await this.PerformCommand(this.Stage1StatusCommand, user, arguments, 0, 0);
            }
        }

        protected override async Task<bool> ShouldPerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (this.TotalAmount >= this.Stage3MinimumAmount)
            {
                int randomNumber = this.GenerateProbability();
                if (randomNumber <= this.PayoutProbability)
                {
                    return true;
                }
            }

            if (this.TotalAmount >= this.Stage3MinimumAmount)
            {
                await this.PerformCommand(this.Stage3DepositCommand, user, arguments, betAmount, 0);
            }
            else if (this.TotalAmount >= this.Stage2MinimumAmount)
            {
                await this.PerformCommand(this.Stage2DepositCommand, user, arguments, betAmount, 0);
            }
            else
            {
                await this.PerformCommand(this.Stage1DepositCommand, user, arguments, betAmount, 0);
            }
            return false;
        }

        protected override async Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.totalPayout = 0;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                this.collectActive = true;

                await Task.Delay(this.CollectTimeLimit * 1000);

                this.collectActive = false;

                this.TotalAmount = 0;
                this.collectUsers.Clear();
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await this.PerformPayout(user, arguments, betAmount, this.PayoutPercentageMinimum, this.PayoutPercentageMaximum, this.PayoutCommand);
        }

        private async Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount, double minimum, double maximum, CustomCommand command)
        {
            double amount = Convert.ToDouble(this.TotalAmount);
            int minAmount = Convert.ToInt32(amount * minimum);
            int maxAmount = Convert.ToInt32(amount * maximum);

            int payout = this.GenerateRandomNumber(minAmount, maxAmount + 1);
            this.totalPayout += payout;

            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return;
            }
            user.Data.AddCurrencyAmount(currency, payout);

            await this.PerformCommand(command, user, arguments, betAmount, payout);
        }
    }

    [DataContract]
    public class LockBoxGameCommand : LongRunningGameCommand
    {
        public const string GameHitmanHintSpecialIdentifier = "gamelockboxhint";
        public const string GameHitmanInspectionSpecialIdentifier = "gamelockboxinspection";

        [DataMember]
        public CustomCommand StatusCommand { get; set; }

        [DataMember]
        public int CombinationLength { get; set; }

        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public CustomCommand SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommand FailedGuessCommand { get; set; }

        [DataMember]
        public string InspectionArgument { get; set; }
        [DataMember]
        public int InspectionCost { get; set; }
        [DataMember]
        public CustomCommand InspectionCommand { get; set; }

        [DataMember]
        public string CurrentCombination { get; set; }

        private string lastHint = null;
        private string lastInspection = null;

        public LockBoxGameCommand() { }

        public LockBoxGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, string statusArgument, CustomCommand statusCommand, int combinationLength,
            int initialAmount, CustomCommand successfulGuessCommand, CustomCommand failedGuessCommand, string inspectionArgument, int inspectionCost, CustomCommand inspectionCommand)
            : base(name, commands, requirements, statusArgument)
        {
            this.StatusCommand = statusCommand;
            this.CombinationLength = combinationLength;
            this.InitialAmount = initialAmount;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
            this.InspectionArgument = inspectionArgument;
            this.InspectionCost = inspectionCost;
            this.InspectionCommand = inspectionCommand;

            this.ResetCombination();
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.StatusCommand);
            commands.Add(this.SuccessfulGuessCommand);
            commands.Add(this.FailedGuessCommand);
            commands.Add(this.InspectionCommand);
            return commands;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers, CancellationToken token)
        {
            if (arguments.Count() == 1 && arguments.ElementAt(0).Equals(this.InspectionArgument))
            {
                if (await this.PerformNonCooldownRequirementChecks(user))
                {
                    if (this.Requirements.Currency != null && this.Requirements.Currency.GetCurrency() != null)
                    {
                        UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                        if (!user.Data.HasCurrencyAmount(currency, this.InspectionCost))
                        {
                            await ChannelSession.Chat.Whisper(user.UserName, string.Format("You do not have the required {0} {1} to do this", this.InspectionCost, currency.Name));
                            return;
                        }

                        await this.ReportInspection(user, arguments);
                    }
                }
            }
            else
            {
                await base.PerformInternal(user, arguments, extraSpecialIdentifiers, token);
            }
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (arguments.Count() != 1)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <GUESS> -or- !{0} {1} -or- !{0} {2}", this.Commands.First(), this.StatusArgument, this.InspectionArgument));
                return false;
            }

            if (!int.TryParse(arguments.ElementAt(0), out int guess) || guess < 0)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("The guess must a valid combination of the digits 0 - 9", this.CombinationLength));
                return false;
            }

            if (arguments.ElementAt(0).Length != this.CombinationLength)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("The guess must be exactly {0} numbers", this.CombinationLength));
                return false;
            }

            return true;
        }

        protected override async Task ReportStatus(UserViewModel user, IEnumerable<string> arguments)
        {
            await this.PerformCommand(this.StatusCommand, user, arguments, 0, 0);
        }

        protected override async Task<bool> ShouldPerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            if (arguments.Count() == 1 && this.CurrentCombination.Equals(arguments.ElementAt(0)))
            {
                return true;
            }

            int guess = int.Parse(arguments.ElementAt(0));
            int combo = int.Parse(this.CurrentCombination);

            if (guess < combo)
            {
                this.lastHint = "low";
            }
            else if (guess > combo)
            {
                this.lastHint = "high";
            }

            await this.PerformCommand(this.FailedGuessCommand, user, arguments, betAmount, 0);

            return false;
        }

        protected override async Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.totalPayout = this.TotalAmount;

            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return;
            }
            user.Data.AddCurrencyAmount(currency, this.totalPayout);

            await this.PerformCommand(this.SuccessfulGuessCommand, user, arguments, betAmount, this.totalPayout);

            this.ResetData(user);
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(this.lastHint))
            {
                specialIdentifiers[GameHitmanHintSpecialIdentifier] = this.lastHint;
            }

            if (!string.IsNullOrEmpty(this.lastInspection))
            {
                specialIdentifiers[GameHitmanInspectionSpecialIdentifier] = this.lastInspection;
            }

            base.AddAdditionalSpecialIdentifiers(user, arguments, specialIdentifiers);
        }

        protected override void ResetData(UserViewModel user)
        {
            this.ResetCombination();
            base.ResetData(user);
        }

        private async Task ReportInspection(UserViewModel user, IEnumerable<string> arguments)
        {
            this.TotalAmount += this.InspectionCost;
            int index = this.GenerateRandomNumber(this.CurrentCombination.Length);
            this.lastInspection = this.CurrentCombination.ElementAt(index).ToString();
            await this.PerformCommand(this.InspectionCommand, user, arguments, 0, 0);
        }

        private void ResetCombination()
        {
            this.TotalAmount = this.InitialAmount;

            this.CurrentCombination = string.Empty;
            for (int i = 0; i < this.CombinationLength; i++)
            {
                this.CurrentCombination += this.GenerateRandomNumber(10).ToString();
            }
        }
    }

    [DataContract]
    public class HangmanGameCommand : LongRunningGameCommand
    {
        public const string GameHangmanCurrentSpecialIdentifier = "gamehangmancurrent";
        public const string GameHangmanFailedGuessesSpecialIdentifier = "gamehangmanfailedguesses";
        public const string GameHangmanAnswerSpecialIdentifier = "gamehangmananswer";

        [DataMember]
        public CustomCommand StatusCommand { get; set; }

        [DataMember]
        public int MaxFailures { get; set; }
        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public CustomCommand SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommand FailedGuessCommand { get; set; }

        [DataMember]
        public CustomCommand GameWonCommand { get; set; }
        [DataMember]
        public CustomCommand GameLostCommand { get; set; }

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public string CurrentWord { get; set; }
        [DataMember]
        public HashSet<char> SuccessfulGuesses { get; set; } = new HashSet<char>();
        [DataMember]
        public HashSet<char> FailedGuesses { get; set; } = new HashSet<char>();

        public HangmanGameCommand() { }

        public HangmanGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, string statusArgument, CustomCommand statusCommand, int maxFailures,
            int initialAmount, CustomCommand successfulGuessCommand, CustomCommand failedGuessCommand, CustomCommand gameWonCommand, CustomCommand gameLostCommand, string customWordsFilePath)
            : base(name, commands, requirements, statusArgument)
        {
            this.StatusCommand = statusCommand;
            this.MaxFailures = maxFailures;
            this.InitialAmount = initialAmount;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
            this.GameWonCommand = gameWonCommand;
            this.GameLostCommand = gameLostCommand;
            this.CustomWordsFilePath = customWordsFilePath;
        }

        public override IEnumerable<CommandBase> GetAllInnerCommands()
        {
            List<CommandBase> commands = new List<CommandBase>(base.GetAllInnerCommands());
            commands.Add(this.StatusCommand);
            commands.Add(this.SuccessfulGuessCommand);
            commands.Add(this.FailedGuessCommand);
            commands.Add(this.GameWonCommand);
            commands.Add(this.GameLostCommand);
            return commands;
        }

        protected override async Task<bool> PerformUsageChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (arguments.Count() != 1)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("USAGE: !{0} <GUESS> -or- !{0} {1}", this.Commands.First(), this.StatusArgument));
                return false;
            }

            if (arguments.ElementAt(0).Length != 1 || arguments.ElementAt(0).Any(c => !char.IsLetter(c)))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "The guess must be a single letter A-Z");
                return false;
            }

            char letter = arguments.ElementAt(0).ToUpper().First();
            if (this.SuccessfulGuesses.Contains(letter) || this.FailedGuesses.Contains(letter))
            {
                await ChannelSession.Chat.Whisper(user.UserName, "This letter has already been guessed");
                return false;
            }

            return true;
        }

        protected override async Task ReportStatus(UserViewModel user, IEnumerable<string> arguments)
        {
            if (string.IsNullOrEmpty(this.CurrentWord) || this.FailedGuesses.Count >= this.MaxFailures)
            {
                await this.ResetWord();
            }

            await this.PerformCommand(this.StatusCommand, user, arguments, 0, 0);
        }

        protected override async Task<bool> ShouldPerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            char letter = arguments.ElementAt(0).ToUpper().First();

            if (string.IsNullOrEmpty(this.CurrentWord) || this.FailedGuesses.Count >= this.MaxFailures)
            {
                await this.ResetWord();
            }

            if (this.CurrentWord.Contains(letter))
            {
                this.SuccessfulGuesses.Add(letter);
                await this.PerformCommand(this.SuccessfulGuessCommand, user, arguments, betAmount, this.TotalAmount);

                if (this.CurrentWord.All(c => this.SuccessfulGuesses.Contains(c)))
                {
                    return true;
                }
            }
            else
            {
                this.FailedGuesses.Add(letter);
                if (this.FailedGuesses.Count >= this.MaxFailures)
                {
                    await this.PerformCommand(this.GameLostCommand, user, arguments, betAmount, this.TotalAmount);
                    await this.ResetWord();
                }
                else
                {
                    await this.PerformCommand(this.FailedGuessCommand, user, arguments, betAmount, this.TotalAmount);
                }
            }
            return false;
        }

        protected override async Task PerformPayout(UserViewModel user, IEnumerable<string> arguments, int betAmount)
        {
            this.totalPayout = this.TotalAmount;

            UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
            if (currency == null)
            {
                return;
            }
            user.Data.AddCurrencyAmount(currency, this.totalPayout);

            await this.PerformCommand(this.GameWonCommand, user, arguments, betAmount, this.TotalAmount);

            await this.ResetWord();
            this.ResetData(user);
        }

        protected override void AddAdditionalSpecialIdentifiers(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (!string.IsNullOrEmpty(this.CurrentWord))
            {
                specialIdentifiers[GameHangmanAnswerSpecialIdentifier] = this.CurrentWord;
            }
            specialIdentifiers[GameHangmanFailedGuessesSpecialIdentifier] = string.Join(", ", this.FailedGuesses);

            List<string> currentLetters = new List<string>();
            foreach (char c in this.CurrentWord)
            {
                if (this.SuccessfulGuesses.Contains(c))
                {
                    currentLetters.Add(c.ToString());
                }
                else
                {
                    currentLetters.Add("_");
                }
            }
            specialIdentifiers[GameHangmanCurrentSpecialIdentifier] = string.Join(" ", currentLetters);

            base.AddAdditionalSpecialIdentifiers(user, arguments, specialIdentifiers);
        }

        private async Task ResetWord()
        {
            HashSet<string> wordsToUse = new HashSet<string>(HitmanGameCommand.DefaultWords.Where(s => s.Length > 4));
            if (!string.IsNullOrEmpty(this.CustomWordsFilePath) && ChannelSession.Services.FileService.FileExists(this.CustomWordsFilePath))
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(this.CustomWordsFilePath);
                if (!string.IsNullOrEmpty(fileData))
                {
                    wordsToUse = new HashSet<string>();
                    foreach (string split in fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordsToUse.Add(split);
                    }
                }
            }

            int randomNumber = this.GenerateRandomNumber(wordsToUse.Count);
            this.CurrentWord = wordsToUse.ElementAt(randomNumber).ToUpper();
            this.SuccessfulGuesses.Clear();
            this.FailedGuesses.Clear();
            this.TotalAmount = this.InitialAmount;

            this.ResetData(await ChannelSession.GetCurrentUser());
        }
    }
}
