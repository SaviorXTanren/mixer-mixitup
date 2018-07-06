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
            return Convert.ToInt32(Convert.ToDouble(betAmount) * this.GetPayoutAmount(user.PrimaryRole));
        }

        private double GetPayoutAmount(MixerRoleEnum role)
        {
            if (this.RolePayouts.Count > 0)
            {
                if (this.RolePayouts.ContainsKey(role))
                {
                    return this.RolePayouts[role];
                }

                foreach (MixerRoleEnum checkRole in this.RolePayouts.Select(kvp => kvp.Key).OrderByDescending(k => k))
                {
                    if (role >= checkRole)
                    {
                        return this.RolePayouts[checkRole];
                    }
                }

                return this.RolePayouts.LastOrDefault().Value;
            }
            return this.Payout;
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

        protected virtual string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return arguments.FirstOrDefault();
        }

        protected virtual string GetBetAmountUserArgument(IEnumerable<string> arguments)
        {
            if (arguments.Count() == 2)
            {
                return arguments.Skip(1).FirstOrDefault();
            }
            return arguments.FirstOrDefault();
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
                if (cumulativeOutcomeProbability < randomNumber && randomNumber <= (cumulativeOutcomeProbability + outcome.GetRoleProbability(user.PrimaryRole)))
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
            await this.PerformCommand(outcome.Command, user, arguments, betAmount, payout);
        }

        protected virtual async Task PerformCommand(CommandBase command, UserViewModel user, IEnumerable<string> arguments, int betAmount, int payout)
        {
            if (command != null)
            {
                command.AddSpecialIdentifier(GameCommandBase.GameBetSpecialIdentifier, betAmount.ToString());
                command.AddSpecialIdentifier(GameCommandBase.GamePayoutSpecialIdentifier, payout.ToString());
                command.AddSpecialIdentifier(GameCommandBase.GameWinnersSpecialIdentifier, "@" + user.UserName);
                await command.Perform(user, arguments);
            }
        }

        protected int GenerateRandomNumber(int maxValue)
        {
            this.randomSeed -= 123;
            Random random = new Random(this.randomSeed);
            return random.Next(maxValue);
        }

        protected int GenerateProbability() { return this.GenerateRandomNumber(100) + 1; }
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
                int betAmount = await this.GetBetAmount(user, this.GetBetAmountArgument(arguments));
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
                            if (randomNumber <= this.SuccessfulOutcome.GetRoleProbability(user.PrimaryRole))
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

        protected virtual async Task<UserViewModel> GetArgumentsTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
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
            return await base.PerformUsernameUsageChecks(user, arguments);
        }

        protected override string GetBetAmountArgument(IEnumerable<string> arguments)
        {
            return base.GetBetAmountUserArgument(arguments);
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

        public DuelGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, GameOutcome successfulOutcome, GameOutcome failedOutcome, CustomCommand startedCommand, int timeLimit)
            : base(name, commands, requirements, successfulOutcome, failedOutcome)
        {
            this.StartedCommand = startedCommand;
            this.TimeLimit = timeLimit;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments, CancellationToken token)
        {
            bool hasTarget = false;
            bool isTarget = false;

            await this.targetUserSemaphore.WaitAsync();

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

            this.targetUserSemaphore.Release();

            if (hasTarget)
            {
                if (isTarget)
                {
                    UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                    if (currency == null)
                    {
                        return;
                    }

                    int randomNumber = this.GenerateProbability();
                    if (randomNumber <= this.SuccessfulOutcome.GetRoleProbability(user.PrimaryRole))
                    {
                        this.currentStarterUser.Data.AddCurrencyAmount(currency, this.currentBetAmount * 2);
                        user.Data.SubtractCurrencyAmount(currency, this.currentBetAmount);
                        await this.PerformCommand(this.SuccessfulOutcome.Command, this.currentStarterUser, new List<string>() { user.UserName }, currentBetAmount, currentBetAmount);
                    }
                    else
                    {
                        user.Data.AddCurrencyAmount(currency, this.currentBetAmount);
                        await this.PerformCommand(this.FailedOutcome.Command, this.currentStarterUser, new List<string>() { user.UserName }, currentBetAmount, currentBetAmount);
                    }
                    this.Requirements.UpdateCooldown(user);

                    this.currentStarterUser = null;
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
                    if (await this.PerformCurrencyChecks(user, this.currentBetAmount))
                    {
                        UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                        if (currency == null)
                        {
                            return;
                        }

                        this.currentTargetUser = await this.GetTargetUser(user, arguments, currency, this.currentBetAmount);
                        if (currentTargetUser != null)
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
                                        await this.targetUserSemaphore.WaitAsync();

                                        if (this.currentTargetUser != null)
                                        {
                                            await ChannelSession.Chat.SendMessage(string.Format("@{0} did not respond in time...", this.currentTargetUser.UserName));
                                            this.currentStarterUser = null;
                                            this.currentTargetUser = null;
                                            this.Requirements.UpdateCooldown(user);
                                        }
                                    }
                                }
                                catch (Exception) { }
                                finally { this.targetUserSemaphore.Release(); }
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
            return base.GetBetAmountUserArgument(arguments);
        }

        protected override Task<UserViewModel> GetTargetUser(UserViewModel user, IEnumerable<string> arguments, UserCurrencyViewModel currency, int betAmount)
        {
            return base.GetArgumentsTargetUser(user, arguments, currency, betAmount);
        }
    }

    [DataContract]
    public class HeistGameCommand : GameCommandBase
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
        public CustomCommand AllSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand TopThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand MiddleThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand LowThirdsSucceedCommand { get; set; }
        [DataMember]
        public CustomCommand NoneSucceedCommand { get; set; }

        [JsonIgnore]
        private UserViewModel starterUser;
        [JsonIgnore]
        private Task timeLimitTask;
        [JsonIgnore]
        private Dictionary<UserViewModel, int> enteredUsers = new Dictionary<UserViewModel, int>();

        public HeistGameCommand() { }

        public HeistGameCommand(string name, IEnumerable<string> commands, RequirementViewModel requirements, int minimumParticipants, int timeLimit, CustomCommand startedCommand,
            CustomCommand userJoinCommand, GameOutcome userSuccessOutcome, GameOutcome userFailOutcome, CustomCommand allSucceedCommand, CustomCommand topThirdsSucceedCommand,
            CustomCommand middleThirdsSucceedCommand, CustomCommand lowThirdsSucceedCommand, CustomCommand noneSucceedCommand)
            : base(name, commands, requirements)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.UserSuccessOutcome = userSuccessOutcome;
            this.UserFailOutcome = userFailOutcome;
            this.AllSucceedCommand = allSucceedCommand;
            this.TopThirdsSucceedCommand = topThirdsSucceedCommand;
            this.MiddleThirdsSucceedCommand = middleThirdsSucceedCommand;
            this.LowThirdsSucceedCommand = lowThirdsSucceedCommand;
            this.NoneSucceedCommand = noneSucceedCommand;
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
                        if (this.timeLimitTask == null)
                        {
                            this.enteredUsers.Clear();
                            this.enteredUsers[user] = betAmount;
                            this.starterUser = user;

                            this.timeLimitTask = Task.Run(async () =>
                            {
                                await Task.Delay(this.TimeLimit * 1000);

                                this.Requirements.UpdateCooldown(user);
                                this.timeLimitTask = null;

                                UserCurrencyViewModel currency = this.Requirements.Currency.GetCurrency();
                                if (currency == null)
                                {
                                    return;
                                }

                                if (this.enteredUsers.Count < this.MinimumParticipants)
                                {
                                    foreach (var enteredUser in this.enteredUsers)
                                    {
                                        enteredUser.Key.Data.AddCurrencyAmount(currency, enteredUser.Value);
                                    }
                                    await ChannelSession.Chat.SendMessage(string.Format("@{0} couldn't get enough users to join in...", this.starterUser.UserName));
                                    return;
                                }

                                List<UserViewModel> successUsers = new List<UserViewModel>();
                                foreach (var enteredUser in this.enteredUsers)
                                {
                                    int randomNumber = this.GenerateProbability();
                                    if (randomNumber <= this.UserSuccessOutcome.GetRoleProbability(user.PrimaryRole))
                                    {
                                        successUsers.Add(user);
                                        await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserSuccessOutcome, enteredUser.Value);
                                    }
                                    else
                                    {
                                        await this.PerformOutcome(enteredUser.Key, new List<string>(), this.UserFailOutcome, enteredUser.Value);
                                    }
                                }

                                double successRate = Convert.ToDouble(successUsers.Count) / Convert.ToDouble(this.enteredUsers.Count);
                                if (successRate == 1.0)
                                {
                                    await this.PerformCommand(this.AllSucceedCommand, user, arguments, betAmount, 0);
                                }
                                else if (successRate > (2.0 / 3.0))
                                {
                                    await this.PerformCommand(this.TopThirdsSucceedCommand, user, arguments, betAmount, 0);
                                }
                                else if (successRate > (1.0 / 3.0))
                                {
                                    await this.PerformCommand(this.MiddleThirdsSucceedCommand, user, arguments, betAmount, 0);
                                }
                                else if (successRate > 0)
                                {
                                    await this.PerformCommand(this.LowThirdsSucceedCommand, user, arguments, betAmount, 0);
                                }
                                else
                                {
                                    await this.PerformCommand(this.NoneSucceedCommand, user, arguments, betAmount, 0);
                                }
                            });

                            await this.PerformCommand(this.StartedCommand, user, arguments, betAmount, 0);
                        }
                        else
                        {
                            this.enteredUsers[user] = betAmount;
                            await this.PerformCommand(this.UserJoinCommand, user, arguments, betAmount, 0);
                        }
                    }
                }
            }
        }
    }
}
