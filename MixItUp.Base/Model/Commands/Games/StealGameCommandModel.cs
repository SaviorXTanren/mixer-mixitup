using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class StealGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public GamePlayerSelectionType SelectionType { get; set; }

        [DataMember]
        public GameOutcomeModel SuccessfulOutcome { get; set; }

        [DataMember]
        public CustomCommandModel FailedCommand { get; set; }

        public StealGameCommandModel(string name, HashSet<string> triggers, GamePlayerSelectionType selectionType, GameOutcomeModel successfulOutcome, CustomCommandModel failedCommand)
            : base(name, triggers, GameCommandTypeEnum.Steal)
        {
            this.SelectionType = selectionType;
            this.SuccessfulOutcome = successfulOutcome;
            this.FailedCommand = failedCommand;
        }

        internal StealGameCommandModel(Base.Commands.StealGameCommand command)
            : base(command, GameCommandTypeEnum.Steal)
        {
            this.SelectionType = GamePlayerSelectionType.Random;
            this.SuccessfulOutcome = new GameOutcomeModel(command.SuccessfulOutcome);
            this.FailedCommand = new CustomCommandModel(command.FailedOutcome.Command) { IsEmbedded = true };
        }

        internal StealGameCommandModel(Base.Commands.PickpocketGameCommand command)
            : base(command, GameCommandTypeEnum.Steal)
        {
            this.SelectionType = GamePlayerSelectionType.Targeted;
            this.SuccessfulOutcome = new GameOutcomeModel(command.SuccessfulOutcome);
            this.FailedCommand = new CustomCommandModel(command.FailedOutcome.Command) { IsEmbedded = true };
        }

        private StealGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessfulOutcome.Command);
            commands.Add(this.FailedCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int betAmount = this.GetBetAmount(parameters);
            if (betAmount > 0)
            {
                await this.SetSelectedUser(this.SelectionType, parameters);
                if (parameters.TargetUser != null)
                {
                    if (this.CurrencyRequirement.Currency.HasAmount(parameters.TargetUser.Data, betAmount))
                    {
                        parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = betAmount.ToString();
                        if (this.GenerateProbability() <= this.SuccessfulOutcome.GetRoleProbabilityPayout(parameters.User).Probability)
                        {
                            this.CurrencyRequirement.Currency.AddAmount(parameters.User.Data, betAmount);
                            this.CurrencyRequirement.Currency.SubtractAmount(parameters.TargetUser.Data, betAmount);
                            await this.SuccessfulOutcome.Command.Perform(parameters);
                        }
                        else
                        {
                            await this.FailedCommand.Perform(parameters);
                        }
                        return;
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandTargetUserInvalidAmount);
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser);
                }
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandInvalidBetAmount);
            }
            await this.Requirements.Refund(parameters);
        }
    }
}