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
        public int CombinationLength { get; set; }
        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public CustomCommandModel SuccessfulCommand { get; set; }
        [DataMember]
        public CustomCommandModel FailureCommand { get; set; }

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
        public int CurrentCombination { get; set; }
        [DataMember]
        public int TotalAmount { get; set; }

        public LockBoxGameCommandModel(string name, HashSet<string> triggers, int combinationLength, int initialAmount, CustomCommandModel successfulCommand, CustomCommandModel failureCommand,
            string statusArgument, CustomCommandModel statusCommand, string inspectionArgument, int inspectionCost, CustomCommandModel inspectionCommand)
            : base(name, triggers, GameCommandTypeEnum.LockBox)
        {
            this.CombinationLength = combinationLength;
            this.InitialAmount = initialAmount;
            this.SuccessfulCommand = successfulCommand;
            this.FailureCommand = failureCommand;
            this.StatusArgument = statusArgument;
            this.StatusCommand = statusCommand;
            this.InspectionArgument = inspectionArgument;
            this.InspectionCost = inspectionCost;
            this.InspectionCommand = inspectionCommand;
        }

        private LockBoxGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessfulCommand);
            commands.Add(this.FailureCommand);
            commands.Add(this.StatusCommand);
            commands.Add(this.InspectionCommand);
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
                        this.PerformPayout(parameters, this.TotalAmount);
                        this.ClearData();
                        await this.SuccessfulCommand.Perform(parameters);
                    }
                    else
                    {
                        parameters.SpecialIdentifiers[LockBoxGameCommandModel.GameHitmanHintSpecialIdentifier] =
                            (guess < this.CurrentCombination) ? MixItUp.Base.Resources.GameCommandLockBoxLow : MixItUp.Base.Resources.GameCommandLockBoxHigh;
                        await this.FailureCommand.Perform(parameters);
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