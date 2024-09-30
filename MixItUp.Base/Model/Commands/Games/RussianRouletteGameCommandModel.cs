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
    public class RussianRouletteGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public int MaxWinners { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public CustomCommandModel UserSuccessCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserFailureCommand { get; set; }
        [DataMember]
        public CustomCommandModel GameCompleteCommand { get; set; }

        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private int runBetAmount;
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();

        public RussianRouletteGameCommandModel(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, int maxWinners, CustomCommandModel startedCommand,
            CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel userSuccessCommand, CustomCommandModel userFailureCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.RussianRoulette)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.MaxWinners = maxWinners;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.UserSuccessCommand = userSuccessCommand;
            this.UserFailureCommand = userFailureCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        [Obsolete]
        public RussianRouletteGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.UserSuccessCommand);
            commands.Add(this.UserFailureCommand);
            commands.Add(this.GameCompleteCommand);
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

                    List<CommandParametersModel> participants = new List<CommandParametersModel>(this.runUsers.Values);
                    int payout = this.runBetAmount * participants.Count;

                    List<CommandParametersModel> winners = new List<CommandParametersModel>();
                    for (int i = 0; i < this.MaxWinners; i++)
                    {
                        CommandParametersModel winner = participants.Random();
                        if (winner == null)
                        {
                            // No more players to pick
                            break;
                        }

                        winners.Add(winner);
                        participants.Remove(winner);
                    }

                    foreach (CommandParametersModel loser in participants)
                    {
                        await this.RunSubCommand(this.UserFailureCommand, loser);
                    }

                    int individualPayout = payout / winners.Count;

                    foreach (CommandParametersModel winner in winners)
                    {
                        this.PerformPrimarySetPayout(winner.User, individualPayout);
                        winner.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = individualPayout.ToString();
                        await this.RunSubCommand(this.UserSuccessCommand, winner);
                    }

                    this.SetGameWinners(this.runParameters, winners);
                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = individualPayout.ToString();
                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameAllPayoutSpecialIdentifier] = payout.ToString();
                    await this.RunSubCommand(this.GameCompleteCommand, this.runParameters);

                    await this.PerformCooldown(this.runParameters);
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                await this.RunSubCommand(this.StartedCommand, this.runParameters);
                await this.RunSubCommand(this.UserJoinCommand, this.runParameters);
                return;
            }
            else if (this.runParameters != null && !this.runUsers.ContainsKey(parameters.User))
            {
                this.runUsers[parameters.User] = parameters;
                await this.RunSubCommand(this.UserJoinCommand, parameters);
                return;
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway, parameters);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runUsers.Clear();
            this.GetPrimaryCurrencyRequirement()?.ResetTemporaryAmount();
        }
    }
}