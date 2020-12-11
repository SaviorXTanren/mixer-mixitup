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
        public GameOutcomeModel FailedOutcome { get; set; }

        public StealGameCommandModel(string name, HashSet<string> triggers, GamePlayerSelectionType selectionType, GameOutcomeModel successfulOutcome, GameOutcomeModel failedOutcome)
            : base(name, triggers, GameCommandTypeEnum.Steal)
        {
            this.SelectionType = selectionType;
            this.SuccessfulOutcome = successfulOutcome;
            this.FailedOutcome = failedOutcome;
        }

        private StealGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.SuccessfulOutcome.Command);
            commands.Add(this.FailedOutcome.Command);
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
                    if (this.GameCurrencyRequirement.Currency.HasAmount(parameters.TargetUser.Data, betAmount))
                    {
                        if (this.GenerateProbability() <= this.SuccessfulOutcome.GetRoleProbabilityPayout(parameters.User).Probability)
                        {
                            this.GameCurrencyRequirement.Currency.AddAmount(parameters.User.Data, betAmount);
                            this.GameCurrencyRequirement.Currency.SubtractAmount(parameters.TargetUser.Data, betAmount);
                            await this.PerformOutcome(parameters, this.SuccessfulOutcome, betAmount);
                        }
                        else
                        {
                            await this.PerformOutcome(parameters, this.FailedOutcome, betAmount);
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