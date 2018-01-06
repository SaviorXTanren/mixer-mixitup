using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Commands
{
    public enum GameResultType
    {
        [Name("Individual User Probability")]
        IndividualUserProbability,
        [Name("All Entered Users Probability")]
        AllEnteredUserProbability,
        [Name("All Chat Users Probability")]
        AllChatUserProbability,
        [Name("Select Random Enter User")]
        SelectRandomEnteredUser,
        [Name("Select Random User In Chat")]
        SelectRandomChatUser
    }

    [DataContract]
    public class GameResultProbability
    {
        [DataMember]
        public double Probability { get; set; }

        [DataMember]
        public string PayoutEquation { get; set; }

        [DataMember]
        public CustomCommand ResultCommand { get; set; }

        public GameResultProbability(double probability, string payoutEquation, CustomCommand command)
        {
            this.Probability = probability;
            this.PayoutEquation = payoutEquation;
            this.ResultCommand = command;
        }

        public async Task<int> EvaluatePayout(GameCommand command, UserViewModel user = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.PayoutEquation))
                {
                    SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(this.PayoutEquation);

                    await siString.ReplaceCommonSpecialModifiers(user);
                    siString.ReplaceSpecialIdentifier(GameCommand.AllGameBetsSpecialIdentifier, command.TotalBets.ToString());
                    if (user != null && command.EnteredUsers.ContainsKey(user))
                    {
                        siString.ReplaceSpecialIdentifier(GameCommand.GameBetSpecialIdentifier, command.EnteredUsers[user].ToString());
                    }

                    return Convert.ToInt32(new DataTable().Compute(siString.ToString(), null));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return 0;
        }

        public async Task RunCommand(GameCommand command, UserViewModel user = null, int payout = 0)
        {
            if (this.ResultCommand != null)
            {
                command.AddSpecialIdentifiersToCommand(this.ResultCommand, user, payout);
                await this.ResultCommand.Perform(user);
            }
        }
    }

    [DataContract]
    public class GameCommand : PermissionsCommandBase
    {
        public const string GameBetSpecialIdentifier = "gamebet";
        public const string AllGameBetsSpecialIdentifier = "allgamebets";
        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameWinnerSpecialIdentifier = "gamewinner";

        public static GameResultProbability FailureResultProbability = new GameResultProbability(100.0, null, null);

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public int GameLength { get; set; }

        [DataMember]
        public int MinimumParticipants { get; set; }

        [DataMember]
        public GameResultType ResultType { get; set; }
        [DataMember]
        public List<GameResultProbability> ResultProbabilities { get; set; }

        [DataMember]
        public CustomCommand GameStartedCommand { get; set; }
        [DataMember]
        public CustomCommand GameEndedCommand { get; set; }
        [DataMember]
        public CustomCommand NotEnoughUsersCommand { get; set; }
        [DataMember]
        public CustomCommand UserJoinedCommand { get; set; }

        [JsonIgnore]
        public UserViewModel GameStarterUser { get; private set; }
        [JsonIgnore]
        public DateTimeOffset GameEndTime { get; private set; }
        [JsonIgnore]
        public Dictionary<UserViewModel, int> EnteredUsers { get; private set; }

        [JsonIgnore]
        private Task gameInstance;

        public GameCommand()
        {
            this.EnteredUsers = new Dictionary<UserViewModel, int>();
            this.ResultProbabilities = new List<GameResultProbability>();
        }

        public GameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            UserCurrencyRequirementViewModel rankRequirement, IEnumerable<GameResultProbability> resultProbabilities)
            : this(name, commands, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement, resultProbabilities, 0, 0, GameResultType.IndividualUserProbability)
        { }

        public GameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            UserCurrencyRequirementViewModel rankRequirement, IEnumerable<GameResultProbability> resultProbabilities, int gameLength, int minParticipants, GameResultType resultType)
            : base(name, CommandTypeEnum.Game, commands, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement)
        {
            this.EnteredUsers = new Dictionary<UserViewModel, int>();
            this.GameLength = gameLength;
            this.MinimumParticipants = minParticipants;
            this.ResultType = resultType;
            this.ResultProbabilities = resultProbabilities.ToList();
        }

        [JsonIgnore]
        public bool IsMultiplayer { get { return this.GameLength > 0; } }

        [JsonIgnore]
        public int TotalBets { get { return this.EnteredUsers.Values.ToList().Sum(); } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckLastRun(user))
            {
                return;
            }

            if (!this.EnteredUsers.ContainsKey(user))
            {
                await this.UserJoined(user, arguments);
            }
            else
            {
                await this.WhisperUser(user, "You have already entered the game");
            }
        }

        public async Task UserJoined(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (!await this.CheckRankRequirement(user))
            {
                return;
            }

            int amount;
            if (arguments != null && arguments.Count() > 0 && int.TryParse(arguments.First(), out amount))
            {
                if (this.CurrencyRequirement.GetCurrency().Enabled && amount >= this.CurrencyRequirement.RequiredAmount &&
                    (this.CurrencyRequirement.MaximumAmount == 0 || amount <= this.CurrencyRequirement.MaximumAmount))
                {
                    user.Data.SubtractCurrencyAmount(this.CurrencyRequirement.GetCurrency(), amount);
                    this.EnteredUsers.Add(user, amount);
                    await this.RunCommand(this.UserJoinedCommand, user);

                    if (this.GameEndTime == DateTimeOffset.MinValue)
                    {
                        await this.StartGame(user);
                    }
                }
                else
                {
                    string requiredBet = "You must enter a valid bet";
                    if (this.CurrencyRequirement.MaximumAmount > 0)
                    {
                        requiredBet += string.Format(" between {0} - {1} {2}", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.MaximumAmount, this.CurrencyRequirement.CurrencyName);
                    }
                    else
                    {
                        requiredBet += string.Format(" greater than {0} {1}", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.CurrencyName);
                    }
                    await this.WhisperUser(user, requiredBet);
                }
            }
            else
            {
                await this.WhisperUser(user, string.Format("USAGE: !{0} <BET>", this.Commands.First()));
            }
        }

        public async Task StartGame(UserViewModel user)
        {
            this.GameStarterUser = user;
            this.GameEndTime = DateTimeOffset.Now.AddMinutes(this.GameLength);
            await this.RunCommand(this.GameStartedCommand, this.GameStarterUser);

            if (this.IsMultiplayer)
            {
                if (this.gameInstance == null)
                {
                    this.gameInstance = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(this.GameLength * 1000);

                            await this.EndGame();
                        }
                        catch (Exception ex) { Logger.Log(ex); }
                        finally { this.gameInstance = null; }
                    });
                }
            }
            else
            {
                await this.EndGame();
            }
        }

        public async Task EndGame()
        {
            this.GameEndTime = DateTimeOffset.MinValue;
            this.lastRun = DateTimeOffset.Now;

            if (this.EnteredUsers.Count > 0 && this.EnteredUsers.Count >= this.MinimumParticipants)
            {
                if (this.ResultType == GameResultType.IndividualUserProbability)
                {
                    foreach (var kvp in this.EnteredUsers)
                    {
                        GameResultProbability resultProbability = this.SelectRandomProbability();
                        int payout = await resultProbability.EvaluatePayout(this, kvp.Key);
                        kvp.Key.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), payout);
                        await resultProbability.RunCommand(this, kvp.Key, payout);
                    }

                    await this.RunCommand(this.GameEndedCommand, this.GameStarterUser);
                }
                else if (this.ResultType == GameResultType.AllEnteredUserProbability || this.ResultType == GameResultType.AllChatUserProbability)
                {
                    IEnumerable<UserViewModel> users = new List<UserViewModel>();
                    if (this.ResultType == GameResultType.AllEnteredUserProbability)
                    {
                        users = this.EnteredUsers.Keys.ToList();
                    }
                    else if (this.ResultType == GameResultType.AllChatUserProbability)
                    {
                        users = ChannelSession.Chat.ChatUsers.Values.ToList();
                    }

                    GameResultProbability resultProbability = this.SelectRandomProbability();
                    foreach (UserViewModel user in users)
                    {
                        int payout = await resultProbability.EvaluatePayout(this, user);
                        user.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), payout);
                        await resultProbability.RunCommand(this, user, payout);
                    }

                    await this.RunCommand(this.GameEndedCommand, this.GameStarterUser);
                }
                else if (this.ResultType == GameResultType.SelectRandomEnteredUser || this.ResultType == GameResultType.SelectRandomChatUser)
                {
                    IEnumerable<UserViewModel> users = new List<UserViewModel>();
                    if (this.ResultType == GameResultType.SelectRandomEnteredUser)
                    {
                        users = this.EnteredUsers.Keys.ToList();
                    }
                    else if (this.ResultType == GameResultType.SelectRandomChatUser)
                    {
                        users = ChannelSession.Chat.ChatUsers.Values.ToList();
                    }

                    Random random = new Random();
                    int index = random.Next(users.Count());
                    UserViewModel winnerUser = users.ElementAt(index);

                    GameResultProbability resultProbability = this.ResultProbabilities.FirstOrDefault();
                    if (resultProbability != null)
                    {
                        int payout = await resultProbability.EvaluatePayout(this, winnerUser);
                        winnerUser.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), payout);
                        await resultProbability.RunCommand(this, winnerUser, payout);
                    }

                    await this.RunCommand(this.GameEndedCommand, winnerUser);
                }
            }
            else
            {
                await this.RunCommand(this.NotEnoughUsersCommand, this.GameStarterUser);
            }

            this.EnteredUsers.Clear();
        }

        public void AddSpecialIdentifiersToCommand(CustomCommand command, UserViewModel user = null, int payout = 0)
        {
            command.AddSpecialIdentifier(GameCommand.AllGameBetsSpecialIdentifier, this.TotalBets.ToString());
            command.AddSpecialIdentifier(GameCommand.GamePayoutSpecialIdentifier, payout.ToString());
            if (user != null)
            {
                command.AddSpecialIdentifier(GameCommand.GameWinnerSpecialIdentifier, user.UserName);
                if (this.EnteredUsers.ContainsKey(user))
                {
                    command.AddSpecialIdentifier(GameCommand.GameBetSpecialIdentifier, this.EnteredUsers[user].ToString());
                }
            }
        }

        protected override SemaphoreSlim AsyncSemaphore { get { return GameCommand.gameCommandPerformSemaphore; } }

        private GameResultProbability SelectRandomProbability()
        {
            Random random = new Random();
            double selectedNumber = (random.NextDouble() * 100.0);

            double total = 0.0;
            foreach (GameResultProbability resultProbability in this.ResultProbabilities)
            {
                total += resultProbability.Probability;
                if (selectedNumber <= total)
                {
                    return resultProbability;
                }
            }
            return GameCommand.FailureResultProbability;
        }

        private async Task RunCommand(CustomCommand command, UserViewModel user = null, int payout = 0)
        {
            if (command != null)
            {
                this.AddSpecialIdentifiersToCommand(command, user, payout);
                await command.Perform(user);
            }
        }

        private async Task WhisperUser(UserViewModel user, string message)
        {
            if (ChannelSession.Chat != null && !string.IsNullOrEmpty(message))
            {
                await ChannelSession.Chat.Whisper(user.UserName, message);
            }
        }
    }
}
