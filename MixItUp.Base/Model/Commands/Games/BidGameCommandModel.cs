using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class BidGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public UserRoleEnum StarterRole { get; set; }
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
            this.StarterRole = starterRole;
            this.TimeLimit = timeLimit;
            this.StartedCommand = startedCommand;
            this.NewTopBidderCommand = newTopBidderCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal BidGameCommandModel(Base.Commands.BidGameCommand command)
            : base(command, GameCommandTypeEnum.Bid)
        {
            this.StarterRole = command.GameStarterRequirement.MixerRole;
            this.TimeLimit = command.TimeLimit;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.NewTopBidderCommand = new CustomCommandModel(command.UserJoinCommand) { IsEmbedded = true };
            this.NotEnoughPlayersCommand = new CustomCommandModel(command.NotEnoughPlayersCommand) { IsEmbedded = true };
            this.GameCompleteCommand = new CustomCommandModel(command.GameCompleteCommand) { IsEmbedded = true };

            if (this.Requirements.Currency.Count() == 0)
            {
                this.Requirements.Requirements.Add(new CurrencyRequirementModel(ChannelSession.Settings.Currency.Values.First(c => !c.IsRank), CurrencyRequirementTypeEnum.MinimumOnly, 0, 0));
            }
            this.Requirements.Currency.First().RequirementType = CurrencyRequirementTypeEnum.MinimumOnly;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private BidGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.NewTopBidderCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.gameActive)
            {
                return await base.ValidateRequirements(parameters);
            }
            else
            {
                if (parameters.User.HasPermissionsTo(this.StarterRole))
                {
                    this.gameActive = true;
                    this.lastBidAmount = this.GetPrimaryCurrencyRequirement().GetAmount(parameters);

                    this.runParameters = parameters;
                    this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameBetSpecialIdentifier] = this.lastBidAmount.ToString();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await Task.Delay(this.TimeLimit * 1000);

                        if (this.lastBidParameters != null)
                        {
                            await this.GameCompleteCommand.Perform(this.lastBidParameters);
                        }
                        else
                        {
                            await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                        }

                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.StartedCommand.Perform(this.runParameters);
                    return false;
                }
                await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StarterRole));
            }
            return false;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int betAmount = this.GetPrimaryBetAmount(parameters);
            if (betAmount > this.lastBidAmount)
            {
                if (this.lastBidParameters != null)
                {
                    this.PerformPrimarySetPayout(this.lastBidParameters.User, this.lastBidAmount);
                }

                this.lastBidParameters = parameters;
                this.lastBidAmount = this.GetPrimaryBetAmount(parameters);

                await this.NewTopBidderCommand.Perform(parameters);
            }
            else
            {
                CurrencyRequirementModel currencyRequirement = this.GetPrimaryCurrencyRequirement();
                if (currencyRequirement != null)
                {
                    await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.lastBidAmount, currencyRequirement.Currency.Name));
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