using MixItUp.Base.Model.User;
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
        public UserRoleEnum StartRoleRequirement { get; set; }
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

        public BetGameCommandModel(string name, HashSet<string> triggers, UserRoleEnum startRoleRequirement, int minimumParticipants, int timeLimit, IEnumerable<GameOutcomeModel> betOptions,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel betsClosedCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.Bet)
        {
            this.StartRoleRequirement = startRoleRequirement;
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.BetOptions = new List<GameOutcomeModel>(betOptions);
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.BetsClosedCommand = betsClosedCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

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
                if (this.betsClosed && parameters.Arguments.Count() == 2 && string.Equals(parameters.Arguments[0], MixItUp.Base.Resources.Answer, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (parameters.User.HasPermissionsTo(this.StartRoleRequirement))
                    {
                        if (int.TryParse(parameters.Arguments[1], out int answer) && answer > 0 && answer <= this.BetOptions.Count)
                        {
                            this.gameActive = false;
                            this.betsClosed = false;

                            await this.GameCompleteCommand.Perform(this.runParameters);

                            GameOutcomeModel winningOutcome = this.BetOptions[answer - 1];
                            this.runParameters.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;

                            foreach (CommandParametersModel winner in this.runUserSelections.Where(kvp => kvp.Value == answer).Select(kvp => this.runUsers[kvp.Key]))
                            {
                                winner.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;
                                await this.PerformOutcome(winner, winningOutcome, this.GetBetAmount(parameters));
                            }

                            this.ClearData();
                            await this.CooldownRequirement.Perform(this.runParameters);
                        }
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StartRoleRequirement));
                    }
                }
                else
                {
                    if (int.TryParse(parameters.Arguments[0], out int choice) && choice > 0 && choice <= this.BetOptions.Count)
                    {
                        return await base.ValidateRequirements(parameters);
                    }
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandBetInvalidSelection);
                }
            }
            else
            {
                if (parameters.User.HasPermissionsTo(this.StartRoleRequirement))
                {
                    this.gameActive = true;
                    this.runParameters = parameters;

                    int i = 1;
                    List<string> betOptions = new List<string>();
                    foreach (GameOutcomeModel betOption in this.BetOptions)
                    {
                        betOptions.Add($"{i}) {betOption.Name}");
                    }
                    this.runParameters.SpecialIdentifiers[BetGameCommandModel.GameBetOptionsSpecialIdentifier] = string.Join(", ", betOptions);

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

                        this.betsClosed = true;
                        await this.BetsClosedCommand.Perform(this.runParameters);
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.StartedCommand.Perform(parameters);
                    return false;
                }
                await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StartRoleRequirement));
            }
            return false;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int.TryParse(parameters.Arguments[0], out int choice);
            this.runUsers[parameters.User] = parameters;
            this.runUserSelections[parameters.User] = choice;

            await this.UserJoinCommand.Perform(this.runParameters);
            this.ResetCooldown();
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