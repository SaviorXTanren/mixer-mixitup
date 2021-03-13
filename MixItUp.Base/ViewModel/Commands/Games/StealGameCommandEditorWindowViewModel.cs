using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class StealGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public bool UserSelectionTargeted
        {
            get { return this.userSelectionTargeted; }
            set
            {
                this.userSelectionTargeted = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool userSelectionTargeted;

        public bool UserSelectionRandom
        {
            get { return this.userSelectionRandom; }
            set
            {
                this.userSelectionRandom = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool userSelectionRandom;

        public GameOutcomeViewModel SuccessfulOutcome
        {
            get { return this.successfulOutcome; }
            set
            {
                this.successfulOutcome = value;
                this.NotifyPropertyChanged();
            }
        }
        private GameOutcomeViewModel successfulOutcome;

        public CustomCommandModel FailedCommand
        {
            get { return this.failedCommand; }
            set
            {
                this.failedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel failedCommand;

        public StealGameCommandEditorWindowViewModel(StealGameCommandModel command)
            : base(command)
        {
            this.UserSelectionTargeted = command.PlayerSelectionType.HasFlag(GamePlayerSelectionType.Targeted);
            this.UserSelectionRandom = command.PlayerSelectionType.HasFlag(GamePlayerSelectionType.Random);
            this.SuccessfulOutcome = new GameOutcomeViewModel(command.SuccessfulOutcome);
            this.FailedCommand = command.FailedCommand;
        }

        public StealGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Steal;
            this.Triggers = MixItUp.Base.Resources.Steal.Replace(" ", string.Empty).ToLower();

            this.UserSelectionTargeted = true;
            this.UserSelectionRandom = true;
            this.SuccessfulOutcome = new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 0, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandStealWinExample, this.PrimaryCurrencyName)));
            this.FailedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandStealLoseExample);
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new StealGameCommandModel(this.Name, this.GetChatTriggers(), this.GetSelectionType(), this.SuccessfulOutcome.GetModel(), this.FailedCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            StealGameCommandModel gCommand = (StealGameCommandModel)command;
            gCommand.PlayerSelectionType = this.GetSelectionType();
            gCommand.SuccessfulOutcome = this.SuccessfulOutcome.GetModel();
            gCommand.FailedCommand = this.FailedCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (!this.UserSelectionTargeted && !this.UserSelectionRandom)
            {
                return new Result(MixItUp.Base.Resources.GameCommandOneUserSelectionTypeMustBeSelected);
            }

            foreach (RoleProbabilityPayoutViewModel rpp in this.SuccessfulOutcome.RoleProbabilityPayouts)
            {
                if (rpp.Probability <= 0 || rpp.Probability > 100)
                {
                    return new Result(MixItUp.Base.Resources.GameCommandProbabilityMustBeBetween1And100);
                }
            }

            return new Result();
        }

        private GamePlayerSelectionType GetSelectionType()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            GamePlayerSelectionType selectionType = GamePlayerSelectionType.None;
#pragma warning restore CS0612 // Type or member is obsolete
            if (this.UserSelectionTargeted) { selectionType |= GamePlayerSelectionType.Targeted; }
            if (this.UserSelectionRandom) { selectionType |= GamePlayerSelectionType.Random; }
            return selectionType;
        }
    }
}
