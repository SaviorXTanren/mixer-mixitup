using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class LockBoxGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int CombinationLength
        {
            get { return this.combinationLength; }
            set
            {
                this.combinationLength = value;
                this.NotifyPropertyChanged();
            }
        }
        private int combinationLength;

        public int InitialAmount
        {
            get { return this.initialAmount; }
            set
            {
                this.initialAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int initialAmount;

        public CustomCommandModel SuccessfulCommand
        {
            get { return this.successfulCommand; }
            set
            {
                this.successfulCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel successfulCommand;

        public CustomCommandModel FailureCommand
        {
            get { return this.failureCommand; }
            set
            {
                this.failureCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel failureCommand;

        public string StatusArgument
        {
            get { return this.statusArgument; }
            set
            {
                this.statusArgument = value;
                this.NotifyPropertyChanged();
            }
        }
        private string statusArgument;

        public CustomCommandModel StatusCommand
        {
            get { return this.statusCommand; }
            set
            {
                this.statusCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel statusCommand;

        public string InspectionArgument
        {
            get { return this.inspectionArgument; }
            set
            {
                this.inspectionArgument = value;
                this.NotifyPropertyChanged();
            }
        }
        private string inspectionArgument;

        public int InspectionCost
        {
            get { return this.inspectionCost; }
            set
            {
                this.inspectionCost = value;
                this.NotifyPropertyChanged();
            }
        }
        private int inspectionCost;

        public CustomCommandModel InspectionCommand
        {
            get { return this.inspectionCommand; }
            set
            {
                this.inspectionCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel inspectionCommand;

        public LockBoxGameCommandEditorWindowViewModel(LockBoxGameCommandModel command)
            : base(command)
        {
            this.CombinationLength = command.CombinationLength;
            this.InitialAmount = command.InitialAmount;
            this.SuccessfulCommand = command.SuccessfulCommand;
            this.FailureCommand = command.FailureCommand;
            this.StatusArgument = command.StatusArgument;
            this.StatusCommand = command.StatusCommand;
            this.InspectionArgument = command.InspectionArgument;
            this.InspectionCost = command.InspectionCost;
            this.InspectionCommand = command.InspectionCommand;
        }

        public LockBoxGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.LockBox;
            this.Triggers = MixItUp.Base.Resources.LockBox.Replace(" ", string.Empty).ToLower();

            this.CombinationLength = 3;
            this.InitialAmount = 100;
            this.SuccessfulCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandLockBoxSuccessfulExample, this.PrimaryCurrencyName));
            this.FailureCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandLockBoxFailureExample);
            this.StatusArgument = MixItUp.Base.Resources.GameCommandStatusArgumentExample;
            this.StatusCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandLockBoxStatusExample, this.PrimaryCurrencyName));
            this.InspectionArgument = MixItUp.Base.Resources.GameCommandLockBoxInspectionArgmentExample;
            this.InspectionCost = 10;
            this.InspectionCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandLockBoxInspectionExample);
        }

        public override bool RequirePrimaryCurrency { get { return true; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new LockBoxGameCommandModel(this.Name, this.GetChatTriggers(), this.CombinationLength, this.InitialAmount, this.SuccessfulCommand, this.FailureCommand, this.StatusArgument, this.StatusCommand,
                this.InspectionArgument, this.InspectionCost, this.InspectionCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            LockBoxGameCommandModel gCommand = (LockBoxGameCommandModel)command;
            gCommand.CombinationLength = this.CombinationLength;
            gCommand.InitialAmount = this.InitialAmount;
            gCommand.SuccessfulCommand = this.SuccessfulCommand;
            gCommand.FailureCommand = this.FailureCommand;
            gCommand.StatusArgument = this.StatusArgument;
            gCommand.StatusCommand = this.StatusCommand;
            gCommand.InspectionArgument = this.InspectionArgument;
            gCommand.InspectionCost = this.InspectionCost;
            gCommand.InspectionCommand = this.InspectionCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.CombinationLength <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandLockBoxCombinationLengthMustBeGreaterThan0);
            }

            if (this.InitialAmount < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandInitialAmountMustBePositive);
            }

            if (this.InspectionCost < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandLockBoxInspetionCostMustBePositive);
            }

            return new Result();
        }
    }
}