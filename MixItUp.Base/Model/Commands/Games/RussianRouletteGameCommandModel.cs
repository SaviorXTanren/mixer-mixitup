using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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
        private Dictionary<UserViewModel, CommandParametersModel> runUsers = new Dictionary<UserViewModel, CommandParametersModel>();

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

        internal RussianRouletteGameCommandModel(Base.Commands.RussianRouletteGameCommand command)
            : base(command, GameCommandTypeEnum.RussianRoulette)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.MaxWinners = command.MinimumParticipants;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.UserJoinCommand = new CustomCommandModel(command.UserJoinCommand) { IsEmbedded = true };
            this.NotEnoughPlayersCommand = new CustomCommandModel(command.NotEnoughPlayersCommand) { IsEmbedded = true };
            this.UserSuccessCommand = new CustomCommandModel(command.UserSuccessOutcome.Command) { IsEmbedded = true };
            this.UserFailureCommand = new CustomCommandModel(command.UserFailOutcome.Command) { IsEmbedded = true };
            this.GameCompleteCommand = new CustomCommandModel(command.GameCompleteCommand) { IsEmbedded = true };
        }

        private RussianRouletteGameCommandModel() { }

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

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
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

                    List<CommandParametersModel> participants = new List<CommandParametersModel>(this.runUsers.Values);
                    int payout = this.runBetAmount * participants.Count;
                    int individualPayout = payout / this.MaxWinners;

                    List<CommandParametersModel> winners = new List<CommandParametersModel>();
                    for (int i = 0; i < this.MaxWinners; i++)
                    {
                        CommandParametersModel winner = participants.Random();
                        winners.Add(winner);
                        participants.Remove(winner);
                    }

                    foreach (CommandParametersModel winner in winners)
                    {
                        this.PerformPrimarySetPayout(winner.User, individualPayout);
                        winner.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = individualPayout.ToString();
                        await this.UserSuccessCommand.Perform(winner);
                    }

                    foreach (CommandParametersModel loser in participants)
                    {
                        await this.UserFailureCommand.Perform(loser);
                    }

                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameWinnersSpecialIdentifier] = string.Join(", ", winners.Select(u => "@" + u.User.Username));
                    await this.GameCompleteCommand.Perform(this.runParameters);

                    await this.CooldownRequirement.Perform(this.runParameters);
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                await this.StartedCommand.Perform(this.runParameters);
                await this.UserJoinCommand.Perform(this.runParameters);
                this.ResetCooldown();
                return;
            }
            else if (this.runParameters != null && !this.runUsers.ContainsKey(parameters.User))
            {
                this.runUsers[parameters.User] = parameters;
                await this.UserJoinCommand.Perform(this.runParameters);
                this.ResetCooldown();
                return;
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runUsers.Clear();
        }
    }
}