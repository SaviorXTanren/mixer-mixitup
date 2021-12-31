using MixItUp.Base.Util;
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

        [Obsolete]
        public CoinPusherGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessCommand);
            commands.Add(this.FailureCommand);
            commands.Add(this.StatusCommand);
            return commands;
        }

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                await this.RunSubCommand(this.StatusCommand, parameters);
                return new Result(success: false);
            }
            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
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
                this.SetGameWinners(parameters, new List<CommandParametersModel>() { parameters });
                await this.RunSubCommand(this.SuccessCommand, parameters);
            }
            else
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                await this.RunSubCommand(this.FailureCommand, parameters);
            }
            await this.PerformCooldown(parameters);

            ChannelSession.Settings.Commands.ManualValueChanged(this.ID);
        }
    }
}