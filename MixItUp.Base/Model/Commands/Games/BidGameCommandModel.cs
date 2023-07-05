using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
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
    public class BidGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public UserRoleEnum StarterUserRole { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }

        [DataMember]
        public CustomCommandModel NewTopBidderCommand { get; set; }

        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }
        [DataMember]
        public CustomCommandModel GameCompleteCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private CommandParametersModel lastBidParameters;
        [JsonIgnore]
        private int lastBidAmount;

        public BidGameCommandModel(string name, HashSet<string> triggers, UserRoleEnum starterRole, int timeLimit, CustomCommandModel startedCommand, CustomCommandModel newTopBidderCommand,
            CustomCommandModel notEnoughPlayersCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.Bid)
        {
            this.StarterUserRole = starterRole;
            this.TimeLimit = timeLimit;
            this.StartedCommand = startedCommand;
            this.NewTopBidderCommand = newTopBidderCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        [Obsolete]
        public BidGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.NewTopBidderCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            if (!this.gameActive)
            {
                if (parameters.User.MeetsRole(this.StarterUserRole))
                {
                    this.gameActive = true;
                    this.lastBidAmount = this.GetPrimaryCurrencyRequirement()?.GetAmount(parameters) ?? 0;

                    this.runParameters = parameters;
                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameBetSpecialIdentifier] = this.lastBidAmount.ToString();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                        if (this.lastBidParameters != null)
                        {
                            this.SetGameWinners(this.lastBidParameters, new List<CommandParametersModel>() { this.lastBidParameters });
                            await this.RunSubCommand(this.GameCompleteCommand, this.lastBidParameters);
                        }
                        else
                        {
                            await this.RunSubCommand(this.NotEnoughPlayersCommand, this.runParameters);
                        }

                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.RunSubCommand(this.StartedCommand, this.runParameters);
                    return new Result(success: false);
                }
                return new Result(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterUserRole));
            }
            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            int betAmount = this.GetPrimaryBetAmount(parameters);
            if (betAmount > this.lastBidAmount)
            {
                if (this.lastBidParameters != null)
                {
                    this.PerformPrimarySetPayout(this.lastBidParameters.User, this.lastBidAmount);
                }

                this.lastBidParameters = parameters;
                this.lastBidAmount = this.GetPrimaryBetAmount(parameters);

                await this.RunSubCommand(this.NewTopBidderCommand, parameters);
            }
            else
            {
                CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
                if (currencyRequirement != null)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.lastBidAmount, currencyRequirement.Currency.Name), parameters);
                }
                await this.Requirements.Refund(parameters);
            }
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.runParameters = null;
            this.lastBidParameters = null;
            this.lastBidAmount = 0;
        }
    }
}