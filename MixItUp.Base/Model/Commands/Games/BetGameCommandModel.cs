using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services.Twitch;
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
        public const string GameBetOptionSpecialIdentifier = "gamebetoption";
        public const string GameBetOptionsSpecialIdentifier = "gamebetoptions";
        public const string GameBetWinningOptionSpecialIdentifier = "gamebetwinningoption";

        [DataMember]
        public UserRoleEnum StarterUserRole { get; set; }
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
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, int> runUserSelections = new Dictionary<UserV2ViewModel, int>();

        public BetGameCommandModel(string name, HashSet<string> triggers, UserRoleEnum starterRole, int minimumParticipants, int timeLimit, IEnumerable<GameOutcomeModel> betOptions,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel betsClosedCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.Bet)
        {
            this.StarterUserRole = starterRole;
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.BetOptions = new List<GameOutcomeModel>(betOptions);
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.BetsClosedCommand = betsClosedCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        [Obsolete]
        public BetGameCommandModel() : base() { }

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

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            this.SetPrimaryCurrencyRequirementArgumentIndex(argumentIndex: 1);

            if (this.gameActive)
            {
                if (this.betsClosed)
                {
                    if (parameters.User.MeetsRole(this.StarterUserRole))
                    {
                        // At least to arguments
                        //      1st must be "answer"
                        //      2nd must be a number from 1 to option count
                        if (parameters.Arguments.Count == 2 && string.Equals(parameters.Arguments[0], MixItUp.Base.Resources.Answer, StringComparison.CurrentCultureIgnoreCase) && int.TryParse(parameters.Arguments[1], out int answer) && answer > 0 && answer <= this.BetOptions.Count)
                        {
                            this.gameActive = false;
                            this.betsClosed = false;
                            GameOutcomeModel winningOutcome = this.BetOptions[answer - 1];

                            List<CommandParametersModel> winners = new List<CommandParametersModel>(this.runUserSelections.Where(kvp => kvp.Value == answer).Select(kvp => this.runUsers[kvp.Key]));

                            this.SetGameWinners(this.runParameters, winners);
                            this.runParameters.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;
                            await this.RunSubCommand(this.GameCompleteCommand, this.runParameters);

                            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forBet: true);
                            if (widgets.Count() > 0)
                            {
                                foreach (OverlayPollV3Model widget in widgets)
                                {
                                    await widget.End(answer.ToString());
                                }
                            }

                            foreach (CommandParametersModel winner in winners)
                            {
                                winner.SpecialIdentifiers[BetGameCommandModel.GameBetWinningOptionSpecialIdentifier] = winningOutcome.Name;
                                await this.RunOutcome(winner, winningOutcome);
                            }

                            await this.PerformCooldown(this.runParameters);
                            this.ClearData();

                            return new Result(success: false);
                        }
                        else
                        {
                            string trigger = this.GetFullTriggers().FirstOrDefault() ?? "!bet";
                            return new Result(string.Format(MixItUp.Base.Resources.GameCommandBetAnswerExample, trigger));
                        }
                    }
                    else
                    {
                        return new Result(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterUserRole));
                    }
                }
                else if (parameters.Arguments.Count == 0 || !int.TryParse(parameters.Arguments[0], out int choice) || choice <= 0 || choice > this.BetOptions.Count)
                {
                    return new Result(string.Format(MixItUp.Base.Resources.GameCommandBetInvalidSelection, parameters.User.Username));
                }
            }
            else
            {
                if (parameters.User.MeetsRole(this.StarterUserRole))
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
                            await this.RunSubCommand(this.NotEnoughPlayersCommand, this.runParameters);
                            foreach (var kvp in this.runUsers.ToList())
                            {
                                await this.Requirements.Refund(kvp.Value);
                            }
                            await this.PerformCooldown(this.runParameters);
                            this.ClearData();
                            return;
                        }

                        this.betsClosed = true;
                        await this.RunSubCommand(this.BetsClosedCommand, this.runParameters);
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.RunSubCommand(this.StartedCommand, parameters);

                    IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forBet: true);
                    if (widgets.Count() > 0)
                    {
                        foreach (OverlayPollV3Model widget in widgets)
                        {
                            await widget.NewBetCommand(this.Name, this.TimeLimit, this.BetOptions);
                        }
                    }

                    return new Result(success: false);
                }
                return new Result(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterUserRole));
            }
            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            int.TryParse(parameters.Arguments[0], out int choice);
            this.runUsers[parameters.User] = parameters;
            this.runUserSelections[parameters.User] = choice;

            parameters.SpecialIdentifiers[BetGameCommandModel.GameBetOptionSpecialIdentifier] = this.BetOptions[choice - 1].Name;

            await this.RunSubCommand(this.UserJoinCommand, parameters);
            await this.PerformCooldown(parameters);

            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forBet: true);
            if (widgets.Count() > 0)
            {
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.UpdateBetCommand(choice);
                }
            }
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