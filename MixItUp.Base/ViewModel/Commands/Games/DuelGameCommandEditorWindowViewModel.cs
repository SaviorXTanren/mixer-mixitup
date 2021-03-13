using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class DuelGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int TimeLimit
        {
            get { return this.timeLimit; }
            set
            {
                this.timeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int timeLimit;

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

        public CustomCommandModel StartedCommand
        {
            get { return this.startedCommand; }
            set
            {
                this.startedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel startedCommand;

        public CustomCommandModel NotAcceptedCommand
        {
            get { return this.notAcceptedCommand; }
            set
            {
                this.notAcceptedCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel notAcceptedCommand;

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

        public DuelGameCommandEditorWindowViewModel(DuelGameCommandModel command)
            : base(command)
        {
            this.TimeLimit = command.TimeLimit;
            this.UserSelectionTargeted = command.PlayerSelectionType.HasFlag(GamePlayerSelectionType.Targeted);
            this.UserSelectionRandom = command.PlayerSelectionType.HasFlag(GamePlayerSelectionType.Random);
            this.StartedCommand = command.StartedCommand;
            this.NotAcceptedCommand = command.NotAcceptedCommand;
            this.SuccessfulOutcome = new GameOutcomeViewModel(command.SuccessfulOutcome);
            this.FailedCommand = command.FailedCommand;
        }

        public DuelGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Duel;
            this.Triggers = MixItUp.Base.Resources.Duel.Replace(" ", string.Empty).ToLower();

            this.TimeLimit = 60;
            this.UserSelectionTargeted = true;
            this.UserSelectionRandom = true;
            this.StartedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandDuelStartedExample, this.PrimaryCurrencyName));
            this.NotAcceptedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandDuelNotAcceptedExample);
            this.SuccessfulOutcome = new GameOutcomeViewModel(MixItUp.Base.Resources.Win, 50, 0, this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandDuelSuccessExample, this.PrimaryCurrencyName)));
            this.FailedCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandDuelFailureExample, this.PrimaryCurrencyName));
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new DuelGameCommandModel(this.Name, this.GetChatTriggers(), this.TimeLimit, this.GetSelectionType(), this.StartedCommand, this.NotAcceptedCommand, this.SuccessfulOutcome.GetModel(), this.FailedCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            DuelGameCommandModel gCommand = (DuelGameCommandModel)command;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.PlayerSelectionType = this.GetSelectionType();
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.NotAcceptedCommand = this.NotAcceptedCommand;
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

            if (this.TimeLimit < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
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