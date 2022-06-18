using MixItUp.Base.Services;
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
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, WinLosePlayerType> runUserTypes = new Dictionary<UserV2ViewModel, WinLosePlayerType>();

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

        [Obsolete]
        public TreasureDefenseGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.KnightUserCommand);
            commands.Add(this.ThiefUserCommand);
            commands.Add(this.KingUserCommand);
            commands.Add(this.KnightSelectedCommand);
            commands.Add(this.ThiefSelectedCommand);
            return commands;
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;
                this.GetPrimaryCurrencyRequirement()?.SetTemporaryAmount(this.runBetAmount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                    if (this.runUsers.Count < this.MinimumParticipants)
                    {
                        await this.RunSubCommand(this.NotEnoughPlayersCommand, this.runParameters);
                        foreach (var kvp in this.runUsers.ToList())
                        {
                            await this.Requirements.Refund(kvp.Value);
                        }
                        await this.PerformCooldown(this.runParameters);
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
                            await this.RunSubCommand(this.ThiefUserCommand, participant);
                        }
                        else if (this.runUserTypes[participant.User] == WinLosePlayerType.Knight)
                        {
                            await this.RunSubCommand(this.KnightUserCommand, participant);
                        }
                    }
                    await this.RunSubCommand(this.KingUserCommand, shuffledParticipants.ElementAt(0));

                    await DelayNoThrow(this.KingTimeLimit * 1000, cancellationToken);

                    if (this.gameActive && this.runParameters != null)
                    {
                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                    }
                    this.gameActive = false;
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.gameActive = true;
                await this.RunSubCommand(this.StartedCommand, this.runParameters);
                await this.RunSubCommand(this.UserJoinCommand, this.runParameters);
                return;
            }
            else if (this.runParameters != null)
            {
                if (this.runUserTypes.Count > 0 && this.runUserTypes[parameters.User] == WinLosePlayerType.King)
                {
                    await parameters.SetTargetUser();
                    if (parameters.TargetUser != null && this.runUserTypes.ContainsKey(parameters.TargetUser))
                    {
                        this.gameActive = false;

                        WinLosePlayerType selectedType = (this.runUserTypes[parameters.TargetUser] == WinLosePlayerType.Knight) ? WinLosePlayerType.Knight : WinLosePlayerType.Thief;
                        var winners = (selectedType == WinLosePlayerType.Knight) ? this.runUserTypes.Where(kvp => kvp.Value != WinLosePlayerType.Thief) : this.runUserTypes.Where(kvp => kvp.Value == WinLosePlayerType.Thief);
                        IEnumerable<CommandParametersModel> winnerParameters = winners.Select(kvp => this.runUsers[kvp.Key]);

                        int payout = this.runBetAmount * this.runUsers.Count;
                        int individualPayout = payout / winnerParameters.Count();
                        foreach (CommandParametersModel winner in winnerParameters)
                        {
                            this.PerformPrimarySetPayout(winner.User, individualPayout);
                        }

                        parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = individualPayout.ToString();
                        parameters.SpecialIdentifiers[GameCommandModelBase.GameAllPayoutSpecialIdentifier] = payout.ToString();
                        this.SetGameWinners(this.runParameters, winnerParameters);
                        this.SetGameWinners(runParameters, winnerParameters);

                        if (selectedType == WinLosePlayerType.Knight)
                        {
                            await this.RunSubCommand(this.KnightSelectedCommand, parameters);
                        }
                        else
                        {
                            await this.RunSubCommand(this.ThiefSelectedCommand, parameters);
                        }
                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser, parameters);
                    }
                }
                else if (!this.runUsers.ContainsKey(parameters.User))
                {
                    this.runUsers[parameters.User] = parameters;
                    await this.RunSubCommand(this.UserJoinCommand, parameters);
                    return;
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway, parameters);
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
            this.GetPrimaryCurrencyRequirement()?.ResetTemporaryAmount();
        }
    }
}