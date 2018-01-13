using Mixer.Base.Util;
using MixItUp.Base.Util;
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
    public enum CurrencyRequirementTypeEnum
    {
        [Name("No Currency Cost")]
        NoCurrencyCost,
        [Name("Minimum Only")]
        MinimumOnly,
        [Name("Minimum & Maximum")]
        MinimumAndMaximum,
        [Name("Required Amount")]
        RequiredAmount
    }

    #region Game Outcome Classes

    [DataContract]
    public class GameOutcome
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public CustomCommand ResultCommand { get; set; }

        public GameOutcome() { }

        public GameOutcome(string name)
        {
            this.Name = name;
        }

        public GameOutcome(string name, CustomCommand resultCommand)
            : this(name)
        {
            this.ResultCommand = resultCommand;
        }
    }

    [DataContract]
    public class GameOutcomeGroup
    {
        [DataMember]
        public UserRole Role { get; set; }

        [DataMember]
        public UserCurrencyRequirementViewModel RankRequirement { get; set; }

        [DataMember]
        public List<GameOutcomeProbability> Probabilities { get; set; }

        public GameOutcomeGroup() { this.Probabilities = new List<GameOutcomeProbability>(); }

        public GameOutcomeGroup(UserRole role) : this() { this.Role = role; }

        public GameOutcomeGroup(UserCurrencyRequirementViewModel rankRequirement) : this() { this.RankRequirement = rankRequirement; }

        public int GetEvaluatedTestPayout() { return this.Probabilities.Sum(p => p.GetEvaluatedTestPayout()); }

        public bool IsObtainableByUser(UserViewModel user)
        {
            if (this.RankRequirement != null && this.RankRequirement.GetCurrency() != null)
            {
                return (user.Data.GetCurrencyAmount(this.RankRequirement.GetCurrency()) >= this.RankRequirement.RequiredRank.MinimumPoints);
            }
            else
            {
                return (user.Roles.Any(r => r >= this.Role));
            }
        }
    }

    [DataContract]
    public class GameOutcomeProbability
    {
        [DataMember]
        public double Probability { get; set; }

        [DataMember]
        public double Payout { get; set; }

        [DataMember]
        public string OutcomeName { get; set; }

        public GameOutcomeProbability() { }

        public GameOutcomeProbability(int probability, int payout)
        {
            this.Probability = probability;
            this.Payout = payout;
        }

        public GameOutcomeProbability(int probability, int payout, string outcomeName)
            : this(probability, payout)
        {
            this.OutcomeName = outcomeName;
        }

        public int GetEvaluatedTestPayout() { return this.EvaluatePayout(100) * (int)this.Probability; }

        public int EvaluatePayout(double bet) { return (this.Payout == 0 || this.Probability == 0) ? 0 : (int)(bet + (bet * (this.Payout / 100.0))); }
    }

    #endregion Game Outcome Classes

    #region Game Type Classes

    [DataContract]
    public class UserCharityGameCommand : GameCommandBase
    {
        [DataMember]
        public bool GiveToRandomUser { get; set; }

        [DataMember]
        public CustomCommand UserParticipatedCommand { get; set; }

        [JsonIgnore]
        private int lastBet = 0;

        public UserCharityGameCommand() { }

        public UserCharityGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            CurrencyRequirementTypeEnum currencyRequirementType, bool giveToRandomUser)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, currencyRequirementType)
        {
            this.GiveToRandomUser = giveToRandomUser;
        }

        public override int TotalBets { get { return this.lastBet; } }

        public override int TotalUsers { get { return 1; } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            List<UserViewModel> users = ChannelSession.Chat.ChatUsers.Values.ToList();
            users.Remove(user);
            if (ChannelSession.BotUser != null)
            {
                users.RemoveAll(u => u.ID.Equals(ChannelSession.BotUser.id));
            }

            if (users.Count == 0)
            {
                await this.WhisperUser(user, "There are not enough other users in chat to use this command");
                return;
            }

            UserViewModel receiver = null;
            if (this.GiveToRandomUser)
            {
                Random random = new Random();
                int randomNumber = random.Next(users.Count);
                receiver = users[randomNumber];
            }
            else if (arguments != null && arguments.Count() == 2)
            {
                string username = arguments.First();
                username = username.Replace("@", "");
                receiver = users.FirstOrDefault(u => u.UserName.Equals(username));
            }
            else
            {
                await this.WhisperUser(user, string.Format("USAGE: !{0} <USERNAME> <BET>", this.Commands.FirstOrDefault()));
                return;
            }

            if (receiver == null)
            {
                await this.WhisperUser(user, "We could not find someone with that username in chat");
                return;
            }

            if (await this.PerformUserJoinChecks(user, arguments))
            {
                this.gameStarterUser = user;
                this.SetLastRun();

                this.lastBet = this.GetBetAmount(arguments);

                receiver.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), this.TotalBets);

                await this.RunCommand(this.UserParticipatedCommand, receiver, this.TotalBets);
            }
        }

        protected override int GetUserBet(UserViewModel user) { return this.lastBet; }

        protected override int GetBetAmount(IEnumerable<string> arguments)
        {
            int amount = -1;

            if (!this.GiveToRandomUser && arguments != null && arguments.Count() == 2)
            {
                int.TryParse(arguments.ElementAt(1), out amount);
            }

            if (amount < 0)
            {
                amount = base.GetBetAmount(arguments);
            }

            return amount;
        }
    }

    [DataContract]
    public class OnlyOneWinnerGameCommand : GameCommandBase
    {
        [DataMember]
        public int GameLength { get; set; }
        [DataMember]
        public int MinimumParticipants { get; set; }

        [DataMember]
        public CustomCommand GameStartedCommand { get; set; }
        [DataMember]
        public CustomCommand GameEndedCommand { get; set; }
        [DataMember]
        public CustomCommand NotEnoughUsersCommand { get; set; }
        [DataMember]
        public CustomCommand UserJoinedCommand { get; set; }

        [JsonIgnore]
        protected Dictionary<UserViewModel, int> UserBets = new Dictionary<UserViewModel, int>();

        [JsonIgnore]
        private Task currentGameInstance = null;

        public OnlyOneWinnerGameCommand() { }

        public OnlyOneWinnerGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            int gameLength, int minimumParticipants)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, CurrencyRequirementTypeEnum.RequiredAmount)
        {
            this.GameLength = gameLength;
            this.MinimumParticipants = minimumParticipants;
        }

        public override int TotalBets { get { return this.UserBets.Sum(kvp => kvp.Value); } }

        public override int TotalUsers { get { return this.UserBets.Count; } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            if (!this.UserBets.ContainsKey(user))
            {
                if (await this.PerformUserJoinChecks(user, arguments))
                {
                    this.UserBets[user] = this.GetBetAmount(arguments);
                    if (this.UserBets.Count == 1)
                    {
                        await this.StartGame(user);
                    }
                    await this.RunCommand(this.UserJoinedCommand, user);
                }
            }
            else
            {
                await this.WhisperUser(user, "You have already entered the game");
                return;
            }
        }

        private async Task StartGame(UserViewModel user)
        {
            this.gameStarterUser = user;
            this.currentGameInstance = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(this.GameLength * 1000);

                    this.SetLastRun();

                    if (this.UserBets.Count >= this.MinimumParticipants)
                    {
                        Random random = new Random();
                        int randomNumber = random.Next(this.UserBets.Count);
                        UserViewModel winner = this.UserBets.ElementAt(randomNumber).Key;

                        winner.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), this.TotalBets);

                        await this.RunCommand(this.GameEndedCommand, winner, this.TotalBets);
                    }
                    else
                    {
                        await this.RunCommand(this.NotEnoughUsersCommand, user);
                    }

                    this.UserBets.Clear();
                }
                catch (Exception ex) { Logger.Log(ex); }
                finally { this.currentGameInstance = null; }
            });

            await this.RunCommand(this.GameStartedCommand, user);
        }

        protected override int GetUserBet(UserViewModel user) { return this.UserBets.ContainsKey(user) ? this.UserBets[user] : 0; }
    }

    [DataContract]
    public class IndividualProbabilityGameCommand : OutcomeGameCommandBase
    {
        [DataMember]
        public int GameLength { get; set; }
        [DataMember]
        public int MinimumParticipants { get; set; }

        [DataMember]
        public CustomCommand GameStartedCommand { get; set; }
        [DataMember]
        public CustomCommand GameEndedCommand { get; set; }
        [DataMember]
        public CustomCommand NotEnoughUsersCommand { get; set; }
        [DataMember]
        public CustomCommand UserJoinedCommand { get; set; }

        [JsonIgnore]
        protected Dictionary<UserViewModel, int> UserBets = new Dictionary<UserViewModel, int>();

        [JsonIgnore]
        private Task currentGameInstance = null;

        public IndividualProbabilityGameCommand() { }

        public IndividualProbabilityGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            CurrencyRequirementTypeEnum currencyRequirementType, IEnumerable<GameOutcome> outcomes, IEnumerable<GameOutcomeGroup> groups, CustomCommand loseLeftoverCommand, int gameLength,
            int minimumParticipants)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, currencyRequirementType, outcomes, groups, loseLeftoverCommand)
        {
            this.GameLength = gameLength;
            this.MinimumParticipants = minimumParticipants;
        }

        public override int TotalBets { get { return this.UserBets.Sum(kvp => kvp.Value); } }

        public override int TotalUsers { get { return this.UserBets.Count; } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            if (!this.UserBets.ContainsKey(user))
            {
                if (await this.PerformUserJoinChecks(user, arguments))
                {
                    this.UserBets[user] = this.GetBetAmount(arguments);
                    if (this.UserBets.Count == 1)
                    {
                        await this.StartGame(user);
                    }
                    await this.RunCommand(this.UserJoinedCommand, user);
                }
            }
            else
            {
                await this.WhisperUser(user, "You have already entered the game");
                return;
            }
        }

        private async Task StartGame(UserViewModel user)
        {
            this.gameStarterUser = user;
            this.currentGameInstance = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(this.GameLength * 1000);

                    this.SetLastRun();

                    if (this.UserBets.Count >= this.MinimumParticipants)
                    {
                        foreach (var kvp in this.UserBets)
                        {
                            await this.RunRandomProbabilityForUser(this, kvp.Key, kvp.Value, this.CurrencyRequirement.GetCurrency());
                        }

                        await this.RunCommand(this.GameEndedCommand, user, this.TotalBets);
                    }
                    else
                    {
                        await this.RunCommand(this.NotEnoughUsersCommand, user);
                    }

                    this.UserBets.Clear();
                }
                catch (Exception ex) { Logger.Log(ex); }
                finally { this.currentGameInstance = null; }
            });

            await this.RunCommand(this.GameStartedCommand, user);
        }

        protected override int GetUserBet(UserViewModel user) { return this.UserBets.ContainsKey(user) ? this.UserBets[user] : 0; }
    }

    [DataContract]
    public class SinglePlayerGameCommand : OutcomeGameCommandBase
    {
        [JsonIgnore]
        private int lastBet = 0;

        public SinglePlayerGameCommand() { }

        public SinglePlayerGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            CurrencyRequirementTypeEnum currencyRequirementType, IEnumerable<GameOutcome> outcomes, IEnumerable<GameOutcomeGroup> groups, CustomCommand loseLeftoverCommand)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, currencyRequirementType, outcomes, groups, loseLeftoverCommand)
        { }

        public override int TotalBets { get { return this.lastBet; } }

        public override int TotalUsers { get { return 1; } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            if (await this.PerformUserJoinChecks(user, arguments))
            {
                this.gameStarterUser = user;

                this.SetLastRun();

                int betAmount = this.GetBetAmount(arguments);

                await this.RunRandomProbabilityForUser(this, user, betAmount, this.CurrencyRequirement.GetCurrency());
            }
        }

        protected override int GetUserBet(UserViewModel user) { return this.lastBet; }
    }

    #endregion Game Type Classes

    #region Base Game Classes

    [DataContract]
    public abstract class OutcomeGameCommandBase : GameCommandBase
    {
        [DataMember]
        public List<GameOutcome> Outcomes { get; set; }

        [DataMember]
        public List<GameOutcomeGroup> Groups { get; set; }

        [DataMember]
        public CustomCommand LoseLeftoverCommand { get; set; }

        [JsonIgnore]
        private int randomSeed = (int)DateTime.Now.Ticks;

        public OutcomeGameCommandBase()
        {
            this.Outcomes = new List<GameOutcome>();
            this.Groups = new List<GameOutcomeGroup>();
        }

        public OutcomeGameCommandBase(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            CurrencyRequirementTypeEnum currencyRequirementType, IEnumerable<GameOutcome> outcomes, IEnumerable<GameOutcomeGroup> groups, CustomCommand loseLeftoverCommand)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, currencyRequirementType)
        {
            this.Outcomes = outcomes.ToList();
            this.Groups = groups.ToList();
            this.LoseLeftoverCommand = loseLeftoverCommand;
        }

        public async Task RunRandomProbabilityForUser(GameCommandBase command, UserViewModel user, int bet, UserCurrencyViewModel currency)
        {
            GameOutcomeGroup bestGroup = this.Groups.First();
            foreach (GameOutcomeGroup group in this.Groups)
            {
                if (group.IsObtainableByUser(user) && group.GetEvaluatedTestPayout() > bestGroup.GetEvaluatedTestPayout())
                {
                    bestGroup = group;
                }
            }

            GameOutcomeProbability selectedProbability = this.SelectRandomProbability(bestGroup.Probabilities);
            int payout = selectedProbability.EvaluatePayout(bet);
            user.Data.AddCurrencyAmount(currency, payout);

            GameOutcome outcome = this.Outcomes.FirstOrDefault(o => o.Name.Equals(selectedProbability.OutcomeName));
            if (outcome != null)
            {
                await command.RunCommand(outcome.ResultCommand, user, payout);
            }
            else
            {
                await command.RunCommand(this.LoseLeftoverCommand, user, payout);
            }
        }

        public GameOutcomeProbability SelectRandomProbability(IEnumerable<GameOutcomeProbability> outcomeProbabilities)
        {
            this.randomSeed -= 123;
            Random random = new Random(this.randomSeed);
            int randomNumber = random.Next(100);

            int minProbability = 0;
            int maxProbability = 0;
            foreach (GameOutcomeProbability outcomeProbability in outcomeProbabilities)
            {
                maxProbability += (int)outcomeProbability.Probability;
                if (minProbability <= randomNumber && randomNumber < maxProbability)
                {
                    return outcomeProbability;
                }
                minProbability = maxProbability;
            }
            return new GameOutcomeProbability(0, 0, null);
        }
    }

    [DataContract]
    public abstract class GameCommandBase : PermissionsCommandBase
    {
        public const string GameStarterUserNameSpecialIdentifier = "gamestarterusername";
        public const string GameBetSpecialIdentifier = "usergamebet";
        public const string GameTotalBetsSpecialIdentifier = "gametotalbets";
        public const string GameTotalUsersSpecialIdentifier = "gametotalusers";
        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameCurrencyNameSpecialIdentifier = "gamecurrencyname";

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public CurrencyRequirementTypeEnum CurrencyRequirementType { get; set; }

        public GameCommandBase() { }

        public GameCommandBase(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            CurrencyRequirementTypeEnum currencyRequirementType)
            : base(name, CommandTypeEnum.Game, commands, lowestAllowedRole, cooldown, currencyRequirement, null)
        {
            this.CurrencyRequirementType = currencyRequirementType;
        }

        [JsonIgnore]
        public abstract int TotalBets { get; }

        [JsonIgnore]
        public abstract int TotalUsers { get; }

        [JsonIgnore]
        protected UserViewModel gameStarterUser;

        [JsonIgnore]
        protected override SemaphoreSlim AsyncSemaphore { get { return GameCommandBase.gameCommandPerformSemaphore; } }

        protected abstract int GetUserBet(UserViewModel user);

        public async Task RunCommand(CustomCommand command, UserViewModel user, int payout = 0)
        {
            if (command != null)
            {
                this.AddSpecialIdentifiersToCommand(command, user, payout);
                await command.Perform(user);
            }
        }

        protected virtual void AddSpecialIdentifiersToCommand(CustomCommand command, UserViewModel user = null, int payout = 0)
        {
            command.AddSpecialIdentifier(GameCommandBase.GameTotalBetsSpecialIdentifier, this.TotalBets.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GameTotalUsersSpecialIdentifier, this.TotalUsers.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GamePayoutSpecialIdentifier, payout.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GameCurrencyNameSpecialIdentifier, this.CurrencyRequirement.CurrencyName);
            if (user != null)
            {
                command.AddSpecialIdentifier(GameCommandBase.GameBetSpecialIdentifier, this.GetUserBet(user).ToString());
            }
            if (this.gameStarterUser != null)
            {
                command.AddSpecialIdentifier(GameCommandBase.GameStarterUserNameSpecialIdentifier, this.gameStarterUser.UserName);
            }
        }

        protected virtual async Task<bool> PerformUserJoinChecks(UserViewModel user, IEnumerable<string> arguments)
        {
            if (!await this.CheckPermissions(user))
            {
                return false;
            }

            if (!this.CurrencyRequirement.GetCurrency().Enabled)
            {
                return false;
            }

            int betAmount = this.GetBetAmount(arguments);
            if (betAmount < 0)
            {
                if (this.CurrencyRequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.CurrencyRequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
                {
                    await this.WhisperUser(user, string.Format("USAGE: !{0}", this.Commands.FirstOrDefault()));
                    return false;
                }
                else
                {
                    await this.WhisperUser(user, string.Format("USAGE: !{0} <BET>", this.Commands.FirstOrDefault()));
                    return false;
                }
            }

            if (!this.CurrencyRequirement.DoesMeetCurrencyRequirement(betAmount))
            {
                string requiredBet = "You must enter a valid bet";
                if (this.CurrencyRequirement.MaximumAmount > 0)
                {
                    requiredBet += string.Format(" between {0} - {1} {2}", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.MaximumAmount, this.CurrencyRequirement.CurrencyName);
                }
                else
                {
                    requiredBet += string.Format(" of {0} or more {1}", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.CurrencyName);
                }
                await this.WhisperUser(user, requiredBet);
                return false;
            }

            if (!this.CurrencyRequirement.TrySubtractAmount(user.Data, betAmount))
            {
                await this.WhisperUser(user, string.Format("You do not have the minimum {0} {1} to participate", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.CurrencyName));
                return false;
            }
            return true;
        }

        protected virtual int GetBetAmount(IEnumerable<string> arguments)
        {
            int amount = -1;
            if (this.CurrencyRequirementType == CurrencyRequirementTypeEnum.NoCurrencyCost || this.CurrencyRequirementType == CurrencyRequirementTypeEnum.RequiredAmount)
            {
                if (arguments == null || arguments.Count() == 0)
                {
                    amount = this.CurrencyRequirement.RequiredAmount;
                }
            }
            else if (arguments != null && arguments.Count() == 1)
            {
                int.TryParse(arguments.FirstOrDefault(), out amount);
            }
            return amount;
        }

        protected void SetLastRun() { this.lastRun = DateTimeOffset.Now; }

        protected async Task WhisperUser(UserViewModel user, string message)
        {
            if (ChannelSession.Chat != null && !string.IsNullOrEmpty(message))
            {
                await ChannelSession.Chat.Whisper(user.UserName, message);
            }
        }
    }

    #endregion Base Game Classes
}
