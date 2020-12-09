using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class TreasureDefenseGameCommandModel : GameCommandModelBase
    {
        public const string GameWinLoseTypeSpecialIdentifier = "gameusertype";

        private enum WinLosePlayerType
        {
            King,
            Knight,
            Thief
        }

        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public int KingTimeLimit { get; set; } = 60;
        [DataMember]
        public int ThiefPlayerPercentage { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public GameOutcomeModel UserSuccessOutcome { get; set; }
        [DataMember]
        public GameOutcomeModel UserFailOutcome { get; set; }

        [DataMember]
        public CustomCommandModel KnightUserCommand { get; set; }
        [DataMember]
        public CustomCommandModel ThiefUserCommand { get; set; }
        [DataMember]
        public CustomCommandModel KingUserCommand { get; set; }

        [DataMember]
        public CustomCommandModel KnightSelectedCommand { get; set; }
        [DataMember]
        public CustomCommandModel ThiefSelectedCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private int runBetAmount;
        [JsonIgnore]
        private Dictionary<UserViewModel, CommandParametersModel> runUsers = new Dictionary<UserViewModel, CommandParametersModel>();
        [JsonIgnore]
        private Dictionary<UserViewModel, WinLosePlayerType> runUserTypes = new Dictionary<UserViewModel, WinLosePlayerType>();

        public TreasureDefenseGameCommandModel(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, int kingTimeLimit, int thiefPlayerPercentage, CustomCommandModel startedCommand,
            CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel knightUserCommand, CustomCommandModel thiefUserCommand, CustomCommandModel kingUserCommand,
            CustomCommandModel knightSelectedCommand, CustomCommandModel thiefSelectedCommand)
            : base(name, triggers, GameCommandTypeEnum.TreasureDefense)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.KingTimeLimit = kingTimeLimit;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.ThiefPlayerPercentage = thiefPlayerPercentage;
            this.KnightUserCommand = knightUserCommand;
            this.ThiefUserCommand = thiefUserCommand;
            this.KingUserCommand = kingUserCommand;
            this.KnightSelectedCommand = knightSelectedCommand;
            this.ThiefSelectedCommand = thiefSelectedCommand;
        }

        private TreasureDefenseGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.UserSuccessOutcome.Command);
            commands.Add(this.UserFailOutcome.Command);
            commands.Add(this.KnightUserCommand);
            commands.Add(this.ThiefUserCommand);
            commands.Add(this.KingUserCommand);
            commands.Add(this.KnightSelectedCommand);
            commands.Add(this.ThiefSelectedCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await Task.Delay(this.TimeLimit * 1000);

                    if (this.runUsers.Count < this.MinimumParticipants)
                    {
                        await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                        foreach (var kvp in this.runUsers.ToList())
                        {
                            await this.Requirements.Refund(kvp.Value);
                        }
                        await this.CooldownRequirement.Perform(this.runParameters);
                        this.ClearData();
                        return;
                    }

                    IEnumerable<CommandParametersModel> shuffledParticipants = this.runUsers.Values.Shuffle();
                    this.runUserTypes[shuffledParticipants.ElementAt(0).User] = WinLosePlayerType.King;

                    double thiefDecimalPercentage = ((double)this.ThiefPlayerPercentage) / 100.0;
                    int totalThieves = (int)Math.Ceiling(thiefDecimalPercentage * (double)shuffledParticipants.Count());
                    foreach (CommandParametersModel participant in shuffledParticipants.Skip(1).Take(totalThieves))
                    {
                        this.runUserTypes[participant.User] = WinLosePlayerType.Thief;
                    }

                    foreach (CommandParametersModel participant in shuffledParticipants.Skip(1 + totalThieves))
                    {
                        this.runUserTypes[participant.User] = WinLosePlayerType.Knight;
                    }

                    foreach (CommandParametersModel participant in shuffledParticipants)
                    {
                        if (this.runUserTypes[participant.User] == WinLosePlayerType.Thief)
                        {
                            await this.ThiefUserCommand.Perform(participant);
                        }
                        else if (this.runUserTypes[participant.User] == WinLosePlayerType.Thief)
                        {
                            await this.ThiefUserCommand.Perform(participant);
                        }
                    }
                    await this.KingUserCommand.Perform(shuffledParticipants.ElementAt(0));

                    await Task.Delay(this.KingTimeLimit * 1000);

                    if (this.gameActive && this.runParameters != null)
                    {
                        await this.CooldownRequirement.Perform(this.runParameters);
                        this.ClearData();
                    }
                    this.gameActive = false;
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.gameActive = true;
                await this.StartedCommand.Perform(this.runParameters);
                await this.UserJoinCommand.Perform(this.runParameters);
                this.ResetCooldown();
                return;
            }
            else if (this.runParameters != null)
            {
                if (this.runUserTypes.Count > 0 && this.runUserTypes[parameters.User] == WinLosePlayerType.King)
                {
                    if (this.runUserTypes.ContainsKey(parameters.TargetUser))
                    {
                        this.gameActive = false;

                        WinLosePlayerType selectedType = (this.runUserTypes[parameters.TargetUser] == WinLosePlayerType.Knight) ? WinLosePlayerType.Knight : WinLosePlayerType.Thief;
                        var winners = (selectedType == WinLosePlayerType.Knight) ? this.runUserTypes.Where(kvp => kvp.Value != WinLosePlayerType.Thief) : this.runUserTypes.Where(kvp => kvp.Value == WinLosePlayerType.Thief);
                        IEnumerable<CommandParametersModel> winnerParameters = winners.Select(kvp => this.runUsers[kvp.Key]);

                        int payout = this.runBetAmount * this.runUsers.Count;
                        int individualPayout = payout / winnerParameters.Count();
                        this.runParameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = individualPayout.ToString();
                        foreach (CommandParametersModel winner in winnerParameters)
                        {
                            this.PerformPayout(winner, individualPayout);
                        }
                        this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameWinnersSpecialIdentifier] = string.Join(", ", winnerParameters.Select(u => "@" + u.User.Username));

                        if (selectedType == WinLosePlayerType.Knight)
                        {
                            await this.KnightSelectedCommand.Perform(parameters);
                        }
                        else
                        {
                            await this.ThiefSelectedCommand.Perform(parameters);
                        }
                        await this.CooldownRequirement.Perform(this.runParameters);
                        this.ClearData();
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser);
                    }
                }
                else if (!this.runUsers.ContainsKey(parameters.User))
                {
                    this.runUsers[parameters.User] = parameters;
                    await this.UserJoinCommand.Perform(this.runParameters);
                    this.ResetCooldown();
                    return;
                }
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runUsers.Clear();
            this.runUserTypes.Clear();
        }
    }
}