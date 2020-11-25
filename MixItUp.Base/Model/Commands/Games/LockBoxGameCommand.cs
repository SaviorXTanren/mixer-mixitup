using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class LockBoxGameCommandModel : GameCommandModelBase
    {
        public const string GameHitmanHintSpecialIdentifier = "gamelockboxhint";
        public const string GameHitmanInspectionSpecialIdentifier = "gamelockboxinspection";

        [DataMember]
        public string StatusArgument { get; set; }
        [DataMember]
        public CustomCommandModel StatusCommand { get; set; }

        [DataMember]
        public string InspectionArgument { get; set; }
        [DataMember]
        public int InspectionCost { get; set; }
        [DataMember]
        public CustomCommandModel InspectionCommand { get; set; }

        [DataMember]
        public int InitialAmount { get; set; }
        [DataMember]
        public int CombinationLength { get; set; }

        [DataMember]
        public CustomCommandModel SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommandModel FailedGuessCommand { get; set; }

        [DataMember]
        public int CurrentCombination { get; set; }
        [DataMember]
        public int TotalAmount { get; set; }

        public LockBoxGameCommandModel(string name, HashSet<string> triggers, string statusArgument, CustomCommandModel statusCommand,
            string inspectionArgument, int inspectionCost, CustomCommandModel inspectionCommand,
            int combinationLength, int initialAmount, CustomCommandModel successfulGuessCommand, CustomCommandModel failedGuessCommand)
            : base(name, triggers)
        {
            this.StatusCommand = statusCommand;
            this.CombinationLength = combinationLength;
            this.InspectionArgument = inspectionArgument;
            this.InspectionCost = inspectionCost;
            this.InspectionCommand = inspectionCommand;
            this.InitialAmount = initialAmount;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
        }

        private LockBoxGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StatusCommand);
            commands.Add(this.InspectionCommand);
            commands.Add(this.SuccessfulGuessCommand);
            commands.Add(this.FailedGuessCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.CurrentCombination <= 0)
            {
                this.ClearData();
            }

            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                this.AddSpecialIdentifiersToParameters(parameters);
                await this.StatusCommand.Perform(parameters);

                return false;
            }
            else if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.InspectionArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                if (this.GameCurrencyRequirement.Currency.HasAmount(parameters.User.Data, this.InspectionCost))
                {
                    this.GameCurrencyRequirement.Currency.SubtractAmount(parameters.User.Data, this.InspectionCost);
                    this.TotalAmount += this.InspectionCost;

                    string currentCombinationString = this.CurrentCombination.ToString();
                    int index = this.GenerateRandomNumber(currentCombinationString.Length);
                    this.AddSpecialIdentifiersToParameters(parameters);
                    parameters.SpecialIdentifiers[LockBoxGameCommandModel.GameHitmanInspectionSpecialIdentifier] = currentCombinationString.ElementAt(index).ToString();
                    await this.InspectionCommand.Perform(parameters);
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, this.InspectionCost, this.GameCurrencyRequirement.Currency.Name));
                }
                return false;
            }
            else
            {
                return await base.ValidateRequirements(parameters);
            }
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && parameters.Arguments[0].Length != this.CombinationLength)
            {
                if (int.TryParse(parameters.Arguments[0], out int guess) && guess > 0)
                {
                    this.TotalAmount += this.GetBetAmount(parameters);
                    this.AddSpecialIdentifiersToParameters(parameters);
                    if (guess == this.CurrentCombination)
                    {
                        this.GameCurrencyRequirement.Currency.AddAmount(parameters.User.Data, this.TotalAmount);
                        this.ClearData();
                        await this.SuccessfulGuessCommand.Perform(parameters);
                    }
                    else
                    {
                        parameters.SpecialIdentifiers[LockBoxGameCommandModel.GameHitmanHintSpecialIdentifier] =
                            (guess < this.CurrentCombination) ? MixItUp.Base.Resources.GameCommandLockBoxLow : MixItUp.Base.Resources.GameCommandLockBoxHigh;
                        await this.FailedGuessCommand.Perform(parameters);
                    }
                    return;
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandLockBoxNotNumber);
                }
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandLockBoxIncorrectLength);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.CurrentCombination = 0;
            for (int i = 0; i < this.CombinationLength; i++)
            {
                this.CurrentCombination += this.GenerateRandomNumber(10) * i;
            }
            this.TotalAmount = this.InitialAmount;
        }

        private void AddSpecialIdentifiersToParameters(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
        }
    }
}