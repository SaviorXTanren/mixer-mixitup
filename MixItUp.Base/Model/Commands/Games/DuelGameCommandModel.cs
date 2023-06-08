using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class DuelGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public GamePlayerSelectionType PlayerSelectionType { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }

        [DataMember]
        public CustomCommandModel NotAcceptedCommand { get; set; }
        [DataMember]
        public GameOutcomeModel SuccessfulOutcome { get; set; }
        [DataMember]
        public CustomCommandModel FailedCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private CommandParametersModel targetParameters;
        [JsonIgnore]
        private CancellationTokenSource runCancellationTokenSource;

        public DuelGameCommandModel(string name, HashSet<string> triggers, int timeLimit, GamePlayerSelectionType playerSelectionType, CustomCommandModel startedCommand, CustomCommandModel notAcceptedCommand,
            GameOutcomeModel successfulOutcome, CustomCommandModel failedCommand)
            : base(name, triggers, GameCommandTypeEnum.Duel)
        {
            this.TimeLimit = timeLimit;
            this.PlayerSelectionType = playerSelectionType;
            this.StartedCommand = startedCommand;
            this.NotAcceptedCommand = notAcceptedCommand;
            this.SuccessfulOutcome = successfulOutcome;
            this.FailedCommand = failedCommand;
        }

        [Obsolete]
        public DuelGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.NotAcceptedCommand);
            commands.Add(this.SuccessfulOutcome.Command);
            commands.Add(this.FailedCommand);
            return commands;
        }

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            this.SetPrimaryCurrencyRequirementArgumentIndex(argumentIndex: 1);

            if (this.runCancellationTokenSource != null && this.runParameters != null)
            {
                if (parameters.User == this.runParameters.TargetUser)
                {
                    this.targetParameters = parameters;
                    this.targetParameters.SetArguments(this.runParameters.Arguments);

                    this.gameActive = false;

                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = this.GetPrimaryBetAmount(this.runParameters).ToString();

                    if (this.GenerateProbability() <= this.SuccessfulOutcome.GetRoleProbabilityPayout(this.runParameters.User).Probability)
                    {
                        this.SetGameWinners(this.runParameters, new List<CommandParametersModel>() { this.runParameters });

                        this.PerformPrimaryMultiplierPayout(this.runParameters, 2);
                        this.PerformPrimaryMultiplierPayout(this.targetParameters, -1);
                        await this.RunSubCommand(this.SuccessfulOutcome.Command, this.runParameters);
                    }
                    else
                    {
                        this.SetGameWinners(this.runParameters, new List<CommandParametersModel>() { this.targetParameters });

                        this.PerformPrimaryMultiplierPayout(this.targetParameters, 1);
                        await this.RunSubCommand(this.FailedCommand, this.runParameters);
                    }

                    await this.PerformCooldown(this.runParameters);
                    this.ClearData();

                    return new Result(success: false);
                }
                else
                {
                    return new Result(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
                }
            }

            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            await this.SetSelectedUser(this.PlayerSelectionType, parameters);
            if (parameters.TargetUser != null && !parameters.IsTargetUserSelf)
            {
                if (await this.ValidateTargetUserPrimaryBetAmount(parameters))
                {
                    this.runParameters = parameters;

                    this.runCancellationTokenSource = new CancellationTokenSource();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                        if (this.gameActive && cancellationToken != null && !cancellationToken.IsCancellationRequested)
                        {
                            this.gameActive = false;
                            await this.RunSubCommand(this.NotAcceptedCommand, parameters);
                            await this.Requirements.Refund(parameters);
                            await this.PerformCooldown(parameters);
                        }
                        this.ClearData();
                    }, this.runCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    this.gameActive = true;
                    await this.RunSubCommand(this.StartedCommand, parameters);
                    return;
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser, parameters);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.runParameters = null;
            this.targetParameters = null;
            try
            {
                if (this.runCancellationTokenSource != null)
                {
                    this.runCancellationTokenSource.Cancel();
                }
            }
            catch (Exception) { }
            this.runCancellationTokenSource = null;
        }
    }
}