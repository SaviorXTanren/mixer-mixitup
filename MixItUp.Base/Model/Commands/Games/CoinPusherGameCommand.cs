using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class CoinPusherGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public int MinimumAmountForPayout { get; set; }
        [DataMember]
        public int ProbabilityPercentage { get; set; }
        [DataMember]
        public double PayoutMinimumPercentage { get; set; }
        [DataMember]
        public double PayoutMaximumPercentage { get; set; }

        [DataMember]
        public CustomCommandModel SuccessCommand { get; set; }
        [DataMember]
        public CustomCommandModel FailureCommand { get; set; }

        [DataMember]
        public string StatusArgument { get; set; }
        [DataMember]
        public CustomCommandModel StatusCommand { get; set; }

        [DataMember]
        public int TotalAmount { get; set; }

        public CoinPusherGameCommandModel(string name, HashSet<string> triggers, int minimumAmountForPayout, int probabilityPercentage, double payoutMinimumPercentage, double payoutMaximumPercentage,
            CustomCommandModel successCommand, CustomCommandModel failureCommand, string statusArgument, CustomCommandModel statusCommand)
            : base(name, triggers, GameCommandTypeEnum.CoinPusher)
        {
            this.MinimumAmountForPayout = minimumAmountForPayout;
            this.ProbabilityPercentage = probabilityPercentage;
            this.PayoutMinimumPercentage = payoutMinimumPercentage;
            this.PayoutMaximumPercentage = payoutMaximumPercentage;
            this.SuccessCommand = successCommand;
            this.FailureCommand = failureCommand;
            this.StatusArgument = statusArgument;
            this.StatusCommand = statusCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal CoinPusherGameCommandModel(Base.Commands.CoinPusherGameCommand command)
            : base(command, GameCommandTypeEnum.CoinPusher)
        {
            this.MinimumAmountForPayout = command.MinimumAmountForPayout;
            this.ProbabilityPercentage = command.PayoutProbability;
            this.PayoutMinimumPercentage = command.PayoutPercentageMinimum;
            this.PayoutMaximumPercentage = command.PayoutPercentageMaximum;
            this.SuccessCommand = new CustomCommandModel(command.PayoutCommand) { IsEmbedded = true };
            this.FailureCommand = new CustomCommandModel(command.NoPayoutCommand) { IsEmbedded = true };
            this.StatusArgument = command.StatusArgument;
            this.StatusCommand = new CustomCommandModel(command.StatusArgument) { IsEmbedded = true };
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private CoinPusherGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessCommand);
            commands.Add(this.FailureCommand);
            commands.Add(this.StatusCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                await this.StatusCommand.Perform(parameters);
                return false;
            }
            else
            {
                return await base.ValidateRequirements(parameters);
            }
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int betAmount = this.GetPrimaryBetAmount(parameters);
            this.TotalAmount += betAmount;

            if (this.TotalAmount >= this.MinimumAmountForPayout && this.GenerateProbability() <= this.ProbabilityPercentage)
            {
                int payout = this.GenerateRandomNumber(this.TotalAmount, this.PayoutMinimumPercentage / 100.0d, this.PayoutMaximumPercentage / 100.0d);
                this.PerformPrimarySetPayout(parameters.User, payout);
                this.TotalAmount -= payout;

                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();
                await this.SuccessCommand.Perform(parameters);
            }
            else
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                await this.FailureCommand.Perform(parameters);
            }
            await this.PerformCooldown(parameters);

            ChannelSession.Settings.Commands.ManualValueChanged(this.ID);
        }
    }
}