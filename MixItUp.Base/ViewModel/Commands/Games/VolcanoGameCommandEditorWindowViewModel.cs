using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class VolcanoGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
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

        public CustomCommandModel Stage1DepositCommand
        {
            get { return this.stage1DepositCommand; }
            set
            {
                this.stage1DepositCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage1DepositCommand;

        public CustomCommandModel Stage1StatusCommand
        {
            get { return this.stage1StatusCommand; }
            set
            {
                this.stage1StatusCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage1StatusCommand;

        public int Stage2MinimumAmount
        {
            get { return this.stage2MinimumAmount; }
            set
            {
                this.stage2MinimumAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int stage2MinimumAmount;

        public CustomCommandModel Stage2DepositCommand
        {
            get { return this.stage2DepositCommand; }
            set
            {
                this.stage2DepositCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage2DepositCommand;

        public CustomCommandModel Stage2StatusCommand
        {
            get { return this.stage2StatusCommand; }
            set
            {
                this.stage2StatusCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage2StatusCommand;

        public int Stage3MinimumAmount
        {
            get { return this.stage3MinimumAmount; }
            set
            {
                this.stage3MinimumAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int stage3MinimumAmount;

        public CustomCommandModel Stage3DepositCommand
        {
            get { return this.stage3DepositCommand; }
            set
            {
                this.stage3DepositCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage3DepositCommand;

        public CustomCommandModel Stage3StatusCommand
        {
            get { return this.stage3StatusCommand; }
            set
            {
                this.stage3StatusCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel stage3StatusCommand;

        public int PayoutProbability
        {
            get { return this.payoutProbability; }
            set
            {
                this.payoutProbability = value;
                this.NotifyPropertyChanged();
            }
        }
        private int payoutProbability;

        public double PayoutMinimumPercentage
        {
            get { return this.payoutMinimumPercentage; }
            set
            {
                this.payoutMinimumPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private double payoutMinimumPercentage;

        public double PayoutMaximumPercentage
        {
            get { return this.payoutMaximumPercentage; }
            set
            {
                this.payoutMaximumPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private double payoutMaximumPercentage;

        public CustomCommandModel PayoutCommand
        {
            get { return this.payoutCommand; }
            set
            {
                this.payoutCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel payoutCommand;

        public string CollectArgument
        {
            get { return this.collectArgument; }
            set
            {
                this.collectArgument = value;
                this.NotifyPropertyChanged();
            }
        }
        private string collectArgument;

        public int CollectTimeLimit
        {
            get { return this.collectTimeLimit; }
            set
            {
                this.collectTimeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int collectTimeLimit;

        public double CollectMinimumPercentage
        {
            get { return this.collectMinimumPercentage; }
            set
            {
                this.collectMinimumPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private double collectMinimumPercentage;

        public double CollectMaximumPercentage
        {
            get { return this.collectMaximumPercentage; }
            set
            {
                this.collectMaximumPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private double collectMaximumPercentage;

        public CustomCommandModel CollectCommand
        {
            get { return this.collectCommand; }
            set
            {
                this.collectCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel collectCommand;

        public VolcanoGameCommandEditorWindowViewModel(VolcanoGameCommandModel command)
            : base(command)
        {
            this.StatusArgument = command.StatusArgument;
            this.Stage1DepositCommand = command.Stage1DepositCommand;
            this.Stage1StatusCommand = command.Stage1StatusCommand;
            this.Stage2MinimumAmount = command.Stage2MinimumAmount;
            this.Stage2DepositCommand = command.Stage2DepositCommand;
            this.Stage2StatusCommand = command.Stage2StatusCommand;
            this.Stage3MinimumAmount = command.Stage3MinimumAmount;
            this.Stage3DepositCommand = command.Stage3DepositCommand;
            this.Stage3StatusCommand = command.Stage3StatusCommand;
            this.PayoutProbability = command.PayoutProbability;
            this.PayoutMinimumPercentage = command.PayoutMinimumPercentage;
            this.PayoutMaximumPercentage = command.PayoutMaximumPercentage;
            this.PayoutCommand = command.PayoutCommand;
            this.CollectArgument = command.CollectArgument;
            this.CollectTimeLimit = command.CollectTimeLimit;
            this.CollectMinimumPercentage = command.CollectMinimumPercentage;
            this.CollectMaximumPercentage = command.CollectMaximumPercentage;
            this.CollectCommand = command.CollectCommand;
        }

        public VolcanoGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Volcano;
            this.Triggers = MixItUp.Base.Resources.Volcano.Replace(" ", string.Empty).ToLower();

            this.StatusArgument = MixItUp.Base.Resources.GameCommandStatusArgumentExample;
            this.Stage1DepositCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage1DepositExample);
            this.Stage1StatusCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage1StatusExample);
            this.Stage2MinimumAmount = 1000;
            this.Stage2DepositCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage2DepositExample);
            this.Stage2StatusCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage2StatusExample);
            this.Stage3MinimumAmount = 2000;
            this.Stage3DepositCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage3DepositExample);
            this.Stage3StatusCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandVolcanoStage3StatusExample);
            this.PayoutProbability = 25;
            this.PayoutMinimumPercentage = 50;
            this.PayoutMaximumPercentage = 75;
            this.PayoutCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandVolcanoPayoutExample, this.PrimaryCurrencyName));
            this.CollectArgument = MixItUp.Base.Resources.GameCommandVolcanoCollectArgumentExample;
            this.CollectTimeLimit = 60;
            this.CollectMinimumPercentage = 25;
            this.CollectMaximumPercentage = 50;
            this.CollectCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandVolcanoCollectExample, this.PrimaryCurrencyName));
        }

        public override bool RequirePrimaryCurrency { get { return true; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new VolcanoGameCommandModel(this.Name, this.GetChatTriggers(), this.StatusArgument, this.Stage1DepositCommand, this.Stage1StatusCommand, this.Stage2MinimumAmount, this.Stage2DepositCommand,
                this.Stage2StatusCommand, this.Stage3MinimumAmount, this.Stage3DepositCommand, this.Stage3StatusCommand, this.PayoutProbability, this.PayoutMinimumPercentage, this.PayoutMaximumPercentage, this.PayoutCommand,
                this.CollectArgument, this.CollectTimeLimit, this.CollectMinimumPercentage, this.CollectMaximumPercentage, this.CollectCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            VolcanoGameCommandModel gCommand = (VolcanoGameCommandModel)command;
            gCommand.StatusArgument = this.StatusArgument;
            gCommand.Stage1DepositCommand = this.Stage1DepositCommand;
            gCommand.Stage1StatusCommand = this.Stage1StatusCommand;
            gCommand.Stage2MinimumAmount = this.Stage2MinimumAmount;
            gCommand.Stage2DepositCommand = this.Stage2DepositCommand;
            gCommand.Stage2StatusCommand = this.Stage2StatusCommand;
            gCommand.Stage3MinimumAmount = this.Stage3MinimumAmount;
            gCommand.Stage3DepositCommand = this.Stage3DepositCommand;
            gCommand.Stage3StatusCommand = this.Stage3StatusCommand;
            gCommand.PayoutProbability = this.PayoutProbability;
            gCommand.PayoutMinimumPercentage = this.PayoutMinimumPercentage;
            gCommand.PayoutMaximumPercentage = this.PayoutMaximumPercentage;
            gCommand.PayoutCommand = this.PayoutCommand;
            gCommand.CollectArgument = this.CollectArgument;
            gCommand.CollectTimeLimit = this.CollectTimeLimit;
            gCommand.CollectMinimumPercentage = this.CollectMinimumPercentage;
            gCommand.CollectMaximumPercentage = this.CollectMaximumPercentage;
            gCommand.CollectCommand = this.CollectCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.Stage2MinimumAmount < 0 || Stage3MinimumAmount < 0 || this.Stage3MinimumAmount < this.Stage2MinimumAmount)
            {
                return new Result(MixItUp.Base.Resources.GameCommandVolcanoStageMinimumAmountsMustBePositiveNumbers);
            }

            if (this.PayoutProbability <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandProbabilityMustBeBetween1And100);
            }

            if (this.PayoutMinimumPercentage < 0 || this.PayoutMaximumPercentage < 0 || this.PayoutMaximumPercentage < this.PayoutMinimumPercentage)
            {
                return new Result(MixItUp.Base.Resources.GameCommandVolcanoPayoutPercentageMustBePositive);
            }

            if (this.CollectTimeLimit <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (this.CollectMinimumPercentage < 0 || this.CollectMaximumPercentage < 0 || this.CollectMaximumPercentage < this.CollectMinimumPercentage)
            {
                return new Result(MixItUp.Base.Resources.GameCommandVolcanoCollectPercentageMustBePositive);
            }

            return new Result();
        }
    }
}