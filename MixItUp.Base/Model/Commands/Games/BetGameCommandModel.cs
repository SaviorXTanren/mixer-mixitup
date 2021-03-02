using MixItUp.Base.Model.User;
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
    public class BetGameCommandModel : GameCommandModelBase
    {
        public const string GameBetOptionsSpecialIdentifier = "gamebetoptions";
        public const string GameBetWinningOptionSpecialIdentifier = "gamebetwinningoption";

        [DataMember]
        public UserRoleEnum StarterRole { get; set; }
        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public List<GameOutcomeModel> BetOptions { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public CustomCommandModel BetsClosedCommand { get; set; }

        [DataMember]
        public CustomCommandModel GameCompleteCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private bool betsClosed = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private Dictionary<UserViewModel, CommandParametersModel> runUsers = new Dictionary<UserViewModel, CommandParametersModel>();
        [JsonIgnore]
        private Dictionary<UserViewModel, int> runUserSelections = new Dictionary<UserViewModel, int>();

        public BetGameCommandModel(string name, HashSet<string> triggers, UserRoleEnum starterRole, int minimumParticipants, int timeLimit, IEnumerable<GameOutcomeModel> betOptions,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel betsClosedCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.Bet)
        {
            this.StarterRole = starterRole;
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.BetOptions = new List<GameOutcomeModel>(betOptions);
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.BetsClosedCommand = betsClosedCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal BetGameCommandModel(Base.Commands.BetGameCommand command)
            : base(command, GameCommandTypeEnum.Bet)
        {
            this.StarterRole = command.GameStarterRequirement.MixerRole;
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.BetOptions = new List<GameOutcomeModel>(command.BetOptions.Select(bo => new GameOutcomeModel(bo)));
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.UserJoinCommand = new CustomCommandModel(command.UserJoinCommand) { IsEmbedded = true };
            this.NotEnoughPlayersCommand = new CustomCommandModel(command.NotEnoughPlayersCommand) { IsEmbedded = true };
            this.BetsClosedCommand = new CustomCommandModel(command.BetsClosedCommand) { IsEmbedded = true };
            this.GameCompleteCommand = new CustomCommandModel(command.GameCompleteCommand) { IsEmbedded = true };
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private BetGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.BetsClosedCommand);
            commands.Add(this.GameCompleteCommand);
            commands.AddRange(this.BetOptions.Select(bo => bo.Command));
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.gameActive)
            {
                if (this.betsClosed)
                {
                    if (parameters.User.HasPermissionsTo(this.StarterRole))
                    {
                        // At least to arguments
                        //      1st must be "answer"
                        //      2nd must be a number from 1 to option count
                        if (parameters.Arguments.Count == 2 && string.Equals(parameters.Arguments[0], MixItUp.Base.Resources.Answer, StringComparison.CurrentCultureIgnoreCase) && int.TryParse(parameters.Arguments[1], out int answer) && answer > 0 && answer <= this.BetOptions.Count)
                        {
                            this.gameActive = false;
                            this.betsClosed = false;
                            GameOutcomeModel winningOutcome = this.BetOptions[answer - 1];

                            this.runParameters.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;
                            await this.GameCompleteCommand.Perform(this.runParameters);

                            foreach (CommandParametersModel winner in this.runUserSelections.Where(kvp => kvp.Value == answer).Select(kvp => this.runUsers[kvp.Key]))
                            {
                                winner.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;
                                await this.PerformOutcome(winner, winningOutcome);
                            }

                            await this.PerformCooldown(this.runParameters);
                            this.ClearData();
                        }
                        else
                        {
                            string trigger = this.GetFullTriggers().FirstOrDefault() ?? "!bet";
                            await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCommandBetAnswerExample, trigger));
                        }
                    }
                    else
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterRole));
                    }
                }
                else
                {
                    if (parameters.Arguments.Count > 0 && int.TryParse(parameters.Arguments[0], out int choice) && choice > 0 && choice <= this.BetOptions.Count)
                    {
                        return await base.ValidateRequirements(parameters);
                    }
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCommandBetInvalidSelection, parameters.User.Username));
                }
            }
            else
            {
                if (parameters.User.HasPermissionsTo(this.StarterRole))
                {
                    this.gameActive = true;
                    this.runParameters = parameters;

                    int i = 1;
                    List<string> betOptions = new List<string>();
                    foreach (GameOutcomeModel betOption in this.BetOptions)
                    {
                        betOptions.Add($"{i}) {betOption.Name}");
                        i++;
                    }
                    this.runParameters.SpecialIdentifiers[BetGameCommandModel.GameBetOptionsSpecialIdentifier] = string.Join(", ", betOptions);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                        if (this.runUsers.Count < this.MinimumParticipants)
                        {
                            await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                            foreach (var kvp in this.runUsers.ToList())
                            {
                                await this.Requirements.Refund(kvp.Value);
                            }
                            await this.PerformCooldown(this.runParameters);
                            this.ClearData();
                            return;
                        }

                        this.betsClosed = true;
                        await this.BetsClosedCommand.Perform(this.runParameters);
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.StartedCommand.Perform(parameters);
                    return false;
                }
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterRole));
            }
            return false;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int.TryParse(parameters.Arguments[0], out int choice);
            this.runUsers[parameters.User] = parameters;
            this.runUserSelections[parameters.User] = choice;

            await this.UserJoinCommand.Perform(parameters);
            await this.PerformCooldown(parameters);
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.betsClosed = false;
            this.runParameters = null;
            this.runUsers.Clear();
            this.runUserSelections.Clear();
        }
    }
}