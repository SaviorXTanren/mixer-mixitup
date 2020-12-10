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

        public GameOutcomeViewModel FailedOutcome
        {
            get { return this.failedOutcome; }
            set
            {
                this.failedOutcome = value;
                this.NotifyPropertyChanged();
            }
        }
        private GameOutcomeViewModel failedOutcome;

        public StealGameCommandEditorWindowViewModel(StealGameCommandModel command)
            : base(command)
        {
            this.UserSelectionTargeted = command.SelectionType.HasFlag(StealGamePlayerSelectionType.Targeted);
            this.UserSelectionRandom = command.SelectionType.HasFlag(StealGamePlayerSelectionType.Random);

            this.SuccessfulOutcome = new GameOutcomeViewModel(command.SuccessfulOutcome);
            this.FailedOutcome = new GameOutcomeViewModel(command.FailedOutcome);
        }

        public StealGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.UserSelectionTargeted = true;
            this.UserSelectionRandom = true;

            this.SuccessfulOutcome = new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 0, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandStealWinExample, currency.Name)));
            this.FailedOutcome = new GameOutcomeViewModel(MixItUp.Base.Resources.Lose, 0, 0, this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandStealLoseExample));
        }

        public override Task<CommandModelBase> GetCommand()
        {
#pragma warning disable CS0612 // Type or member is obsolete
            StealGamePlayerSelectionType selectionType = StealGamePlayerSelectionType.None;
#pragma warning restore CS0612 // Type or member is obsolete
            if (this.UserSelectionTargeted) { selectionType |= StealGamePlayerSelectionType.Targeted; }
            if (this.UserSelectionRandom) { selectionType |= StealGamePlayerSelectionType.Random; }

            return Task.FromResult<CommandModelBase>(new StealGameCommandModel(this.Name, this.GetChatTriggers(), selectionType, this.SuccessfulOutcome.GetModel(), this.FailedOutcome.GetModel()));
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
                return new Result(MixItUp.Base.Resources.GameCommandStealOneUserSelectionTypeMustBeSelected);
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
    }
}
