using MixItUp.Base.Services;
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

        [Obsolete]
        public LockBoxGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessfulCommand);
            commands.Add(this.FailureCommand);
            commands.Add(this.StatusCommand);
            commands.Add(this.InspectionCommand);
            return commands;
        }

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            this.SetPrimaryCurrencyRequirementArgumentIndex(argumentIndex: 1);

            if (this.CurrentCombination <= 0)
            {
                this.ClearData();
            }

            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                this.AddSpecialIdentifiersToParameters(parameters);
                await this.RunSubCommand(this.StatusCommand, parameters);

                return new Result(success: false);
            }
            else if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.InspectionArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                if (this.ValidatePrimaryCurrencyAmount(parameters, this.InspectionCost))
                {
                    this.PerformPrimarySetPayout(parameters.User, -this.InspectionCost);
                    this.TotalAmount += this.InspectionCost;

                    string currentCombinationString = this.CurrentCombination.ToString();
                    int index = this.GenerateRandomNumber(currentCombinationString.Length);
                    this.AddSpecialIdentifiersToParameters(parameters);
                    parameters.SpecialIdentifiers[LockBoxGameCommandModel.GameHitmanInspectionSpecialIdentifier] = currentCombinationString.ElementAt(index).ToString();
                    await this.RunSubCommand(this.InspectionCommand, parameters);

                    ChannelSession.Settings.Commands.ManualValueChanged(this.ID);

                    return new Result(success: false);
                }
                else
                {
                    return new Result(string.Format(MixItUp.Base.Resources.CurrencyRequirementDoNotHaveAmount, this.InspectionCost, this.GetPrimaryCurrencyRequirement().Currency.Name));
                }
            }

            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && parameters.Arguments[0].Length == this.CombinationLength)
            {
                if (int.TryParse(parameters.Arguments[0], out int guess) && guess > 0)
                {
                    this.TotalAmount += this.GetPrimaryBetAmount(parameters);
                    this.AddSpecialIdentifiersToParameters(parameters);
                    if (guess == this.CurrentCombination)
                    {
                        parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = this.TotalAmount.ToString();
                        this.SetGameWinners(parameters, new List<CommandParametersModel>() { parameters });

                        this.PerformPrimarySetPayout(parameters.User, this.TotalAmount);
                        this.ClearData();

                        await this.RunSubCommand(this.SuccessfulCommand, parameters);
                    }
                    else
                    {
                        parameters.SpecialIdentifiers[LockBoxGameCommandModel.GameHitmanHintSpecialIdentifier] =
                            (guess < this.CurrentCombination) ? MixItUp.Base.Resources.GameCommandLockBoxLow : MixItUp.Base.Resources.GameCommandLockBoxHigh;
                        await this.RunSubCommand(this.FailureCommand, parameters);
                    }
                    await this.PerformCooldown(parameters);

                    ChannelSession.Settings.Commands.ManualValueChanged(this.ID);

                    return;
                }
                else
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandLockBoxNotNumber, parameters);
                }
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(string.Format(MixItUp.Base.Resources.GameCommandLockBoxIncorrectLength, this.CombinationLength), parameters);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.CurrentCombination = 0;

            string digits = string.Empty;
            for (int i = 0; i < this.CombinationLength; i++)
            {
                digits += this.GenerateRandomNumber(10).ToString();
            }
            this.CurrentCombination = int.Parse(digits);

            this.TotalAmount = this.InitialAmount;
        }

        private void AddSpecialIdentifiersToParameters(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
        }
    }
}