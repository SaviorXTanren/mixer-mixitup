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

        public async Task<int> EvaluatePayout(GameCommandBase command, UserViewModel user = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.PayoutEquation))
                {
                    SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(this.PayoutEquation);

                    await siString.ReplaceCommonSpecialModifiers(user);
                    siString.ReplaceSpecialIdentifier(GameCommandBase.GameTotalBetsSpecialIdentifier, command.TotalBets.ToString());
                    if (user != null && command.EnteredUsers.ContainsKey(user))
                    {
                        siString.ReplaceSpecialIdentifier(GameCommandBase.GameBetSpecialIdentifier, command.EnteredUsers[user].ToString());
                    }

                    return Convert.ToInt32(new DataTable().Compute(siString.ToString(), null));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return 0;
        }

        public async Task RunCommand(GameCommandBase command, UserViewModel user = null, int payout = 0)
        {
            if (this.ResultCommand != null)
            {
                command.AddSpecialIdentifiersToCommand(this.ResultCommand, user, payout);
                await this.ResultCommand.Perform(user);
            }
        }
    }

    [DataContract]
    public class SinglePlayerGameCommand : GameCommandBase
    {
        public SinglePlayerGameCommand() { }

        public SinglePlayerGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            UserCurrencyRequirementViewModel rankRequirement, IEnumerable<GameResultProbability> resultProbabilities)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement, resultProbabilities)
        { }

        protected override async Task UserJoined(UserViewModel user, IEnumerable<string> arguments = null)
        {
            this.GameStarterUser = user;
            this.lastRun = DateTimeOffset.Now;
            await this.RunRandomProbabilityOnUser(user);
            this.EnteredUsers.Clear();
        }
    }

    [DataContract]
    public class MultiPlayerGameCommand : GameCommandBase
    {
        [DataMember]
        public int GameLength { get; set; }

        [DataMember]
        public int MinimumParticipants { get; set; }

        [DataMember]
        public GameResultType ResultType { get; set; }

        [DataMember]
        public CustomCommand GameStartedCommand { get; set; }
        [DataMember]
        public CustomCommand GameEndedCommand { get; set; }
        [DataMember]
        public CustomCommand NotEnoughUsersCommand { get; set; }
        [DataMember]
        public CustomCommand UserJoinedCommand { get; set; }

        [JsonIgnore]
        public DateTimeOffset GameEndTime { get; private set; }

        [JsonIgnore]
        private Task gameInstance;

        public MultiPlayerGameCommand() { }

        public MultiPlayerGameCommand(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            UserCurrencyRequirementViewModel rankRequirement, IEnumerable<GameResultProbability> resultProbabilities, int gameLength, int minParticipants, GameResultType resultType)
            : base(name, commands, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement, resultProbabilities)
        {
            this.GameLength = gameLength;
            this.MinimumParticipants = minParticipants;
            this.ResultType = resultType;
        }

        protected override async Task UserJoined(UserViewModel user, IEnumerable<string> arguments = null)
        {
            await this.RunCommand(this.UserJoinedCommand, user);

            if (this.GameEndTime == DateTimeOffset.MinValue)
            {
                await this.StartGame(user);
            }
        }

        private async Task StartGame(UserViewModel user)
        {
            this.GameEndTime = DateTimeOffset.Now.AddMinutes(this.GameLength);
            this.GameStarterUser = user;

            await this.RunCommand(this.GameStartedCommand, this.GameStarterUser);

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

        private async Task EndGame()
        {
            this.GameEndTime = DateTimeOffset.MinValue;
            this.lastRun = DateTimeOffset.Now;

            if (this.EnteredUsers.Count >= this.MinimumParticipants)
            {
                if (this.ResultType == GameResultType.IndividualUserProbability)
                {
                    foreach (var kvp in this.EnteredUsers)
                    {
                        await this.RunRandomProbabilityOnUser(kvp.Key);
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

                    GameResultProbability resultProbability = this.SelectRandomProbability();
                    int payout = await resultProbability.EvaluatePayout(this, winnerUser);
                    winnerUser.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), payout);
                    await resultProbability.RunCommand(this, winnerUser, payout);

                    await this.RunCommand(this.GameEndedCommand, winnerUser, payout);
                }
            }
            else
            {
                await this.RunCommand(this.NotEnoughUsersCommand, this.GameStarterUser);
            }

            this.EnteredUsers.Clear();
        }
    }

    [DataContract]
    public abstract class GameCommandBase : PermissionsCommandBase
    {
        public const string GameBetSpecialIdentifier = "gamebet";
        public const string GameTotalBetsSpecialIdentifier = "gametotalbets";
        public const string GameTotalUsersSpecialIdentifier = "gametotalusers";
        public const string GamePayoutSpecialIdentifier = "gamepayout";
        public const string GameCurrencyNameSpecialIdentifier = "gamecurrencyname";
        public const string GameStarterNameSpecialIdentifier = "gamestarterusername";

        private static GameResultProbability FailureResultProbability = new GameResultProbability(100.0, null, null);

        private static SemaphoreSlim gameCommandPerformSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public UserViewModel GameStarterUser { get; protected set; }

        [DataMember]
        public List<GameResultProbability> ResultProbabilities { get; set; }

        [JsonIgnore]
        public Dictionary<UserViewModel, int> EnteredUsers { get; protected set; }

        public GameCommandBase()
        {
            this.ResultProbabilities = new List<GameResultProbability>();
            this.EnteredUsers = new Dictionary<UserViewModel, int>();
        }

        public GameCommandBase(string name, IEnumerable<string> commands, UserRole lowestAllowedRole, int cooldown, UserCurrencyRequirementViewModel currencyRequirement,
            UserCurrencyRequirementViewModel rankRequirement, IEnumerable<GameResultProbability> resultProbabilities)
            : base(name, CommandTypeEnum.Game, commands, lowestAllowedRole, cooldown, currencyRequirement, rankRequirement)
        {
            this.EnteredUsers = new Dictionary<UserViewModel, int>();
            this.ResultProbabilities = resultProbabilities.ToList();
        }

        [JsonIgnore]
        public int TotalBets { get { return this.EnteredUsers.Values.ToList().Sum(); } }

        public override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments = null)
        {
            if (arguments != null)
            {
                arguments = new List<string>();
            }

            if (!await this.CheckLastRun(user))
            {
                return;
            }

            if (!this.EnteredUsers.ContainsKey(user))
            {
                if (!await this.CheckPermissions(user))
                {
                    return;
                }

                if (!await this.CheckRankRequirement(user))
                {
                    return;
                }

                if (!this.CurrencyRequirement.GetCurrency().Enabled)
                {
                    return;
                }

                int amount = 0;
                if (this.CurrencyRequirement.IsSameAmountSpecific && arguments.Count() > 0)
                {
                    await this.WhisperUser(user, string.Format("USAGE: !{0}", this.Commands.First()));
                    return;
                }
                else if (arguments.Count() > 1 || !int.TryParse(arguments.First(), out amount) || amount > 0)
                {
                    await this.WhisperUser(user, string.Format("USAGE: !{0} <BET>", this.Commands.First()));
                    return;
                }

                if (!this.CurrencyRequirement.DoesMeetCurrencyRequirement(amount))
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
                    return;
                }

                if (!this.CurrencyRequirement.TrySubtractAmount(user.Data, amount))
                {
                    await this.WhisperUser(user, string.Format("You do not have the minimum {0} {1} to participate", this.CurrencyRequirement.RequiredAmount, this.CurrencyRequirement.CurrencyName));
                    return;
                }

                this.EnteredUsers.Add(user, amount);

                await this.UserJoined(user, arguments);
            }
            else
            {
                await this.WhisperUser(user, "You have already entered the game");
            }
        }

        public void AddSpecialIdentifiersToCommand(CustomCommand command, UserViewModel user = null, int payout = 0)
        {
            command.AddSpecialIdentifier(GameCommandBase.GameTotalBetsSpecialIdentifier, this.TotalBets.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GameTotalUsersSpecialIdentifier, this.EnteredUsers.Count.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GamePayoutSpecialIdentifier, payout.ToString());
            command.AddSpecialIdentifier(GameCommandBase.GameCurrencyNameSpecialIdentifier, this.CurrencyRequirement.CurrencyName);
            if (this.GameStarterUser != null)
            {
                command.AddSpecialIdentifier(GameCommandBase.GameStarterNameSpecialIdentifier, this.GameStarterUser.UserName);
            }
            if (user != null)
            {
                if (this.EnteredUsers.ContainsKey(user))
                {
                    command.AddSpecialIdentifier(GameCommandBase.GameBetSpecialIdentifier, this.EnteredUsers[user].ToString());
                }
            }
        }

        protected abstract Task UserJoined(UserViewModel user, IEnumerable<string> arguments = null);

        protected override SemaphoreSlim AsyncSemaphore { get { return GameCommandBase.gameCommandPerformSemaphore; } }

        protected GameResultProbability SelectRandomProbability()
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
            return GameCommandBase.FailureResultProbability;
        }

        protected async Task RunRandomProbabilityOnUser(UserViewModel user)
        {
            GameResultProbability resultProbability = this.SelectRandomProbability();
            int payout = await resultProbability.EvaluatePayout(this, user);
            user.Data.AddCurrencyAmount(this.CurrencyRequirement.GetCurrency(), payout);
            await resultProbability.RunCommand(this, user, payout);
        }

        protected async Task RunCommand(CustomCommand command, UserViewModel user = null, int payout = 0)
        {
            if (command != null)
            {
                this.AddSpecialIdentifiersToCommand(command, user, payout);
                await command.Perform(user);
            }
        }

        protected async Task WhisperUser(UserViewModel user, string message)
        {
            if (ChannelSession.Chat != null && !string.IsNullOrEmpty(message))
            {
                await ChannelSession.Chat.Whisper(user.UserName, message);
            }
        }
    }
}
