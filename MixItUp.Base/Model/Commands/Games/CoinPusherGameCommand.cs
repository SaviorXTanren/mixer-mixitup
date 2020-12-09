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
        public string StatusArgument { get; set; }
        [DataMember]
        public CustomCommandModel StatusCommand { get; set; }

        [DataMember]
        public int MinimumAmountForPayout { get; set; }
        [DataMember]
        public int PayoutProbability { get; set; }

        [DataMember]
        public double PayoutPercentageMinimum { get; set; }
        [DataMember]
        public double PayoutPercentageMaximum { get; set; }

        [DataMember]
        public CustomCommandModel NoPayoutCommand { get; set; }
        [DataMember]
        public CustomCommandModel PayoutCommand { get; set; }

        [DataMember]
        public int TotalAmount { get; set; }

        public CoinPusherGameCommandModel(string name, HashSet<string> triggers, string statusArgument, CustomCommandModel statusCommand,
            int minimumAmountForPayout, int payoutProbability, double payoutPercentageMinimum, double payoutPercentageMaximum, CustomCommandModel noPayoutCommand, CustomCommandModel payoutCommand)
            : base(name, triggers, GameCommandTypeEnum.CoinPusher)
        {
            this.StatusArgument = statusArgument;
            this.StatusCommand = statusCommand;
            this.MinimumAmountForPayout = minimumAmountForPayout;
            this.PayoutProbability = payoutProbability;
            this.PayoutPercentageMinimum = payoutPercentageMinimum;
            this.PayoutPercentageMaximum = payoutPercentageMaximum;
            this.NoPayoutCommand = noPayoutCommand;
            this.PayoutCommand = payoutCommand;
        }

        private CoinPusherGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StatusCommand);
            commands.Add(this.NoPayoutCommand);
            commands.Add(this.PayoutCommand);
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
            int betAmount = this.GetBetAmount(parameters);
            this.TotalAmount += betAmount;

            if (this.TotalAmount >= this.MinimumAmountForPayout && this.GenerateProbability() <= this.PayoutProbability)
            {
                int payout = this.GenerateRandomNumber(this.TotalAmount, this.PayoutPercentageMinimum, this.PayoutPercentageMaximum);
                this.PerformPayout(parameters, payout);
                this.TotalAmount -= payout;

                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();
                await this.PayoutCommand.Perform(parameters);
            }
            else
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                await this.NoPayoutCommand.Perform(parameters);
            }
        }
    }
}