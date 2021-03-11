using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Games
{
    public class CoinPusherGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int MinimumAmountForPayout
        {
            get { return this.minimumAmountForPayout; }
            set
            {
                this.minimumAmountForPayout = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimumAmountForPayout;

        public int ProbabilityPercentage
        {
            get { return this.probabilityPercentage; }
            set
            {
                this.probabilityPercentage = value;
                this.NotifyPropertyChanged();
            }
        }
        private int probabilityPercentage;

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

        public CustomCommandModel SuccessCommand
        {
            get { return this.successCommand; }
            set
            {
                this.successCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel successCommand;

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

        public CoinPusherGameCommandEditorWindowViewModel(CoinPusherGameCommandModel command)
            : base(command)
        {
            this.MinimumAmountForPayout = command.MinimumAmountForPayout;
            this.ProbabilityPercentage = command.ProbabilityPercentage;
            this.PayoutMinimumPercentage = command.PayoutMinimumPercentage;
            this.PayoutMaximumPercentage = command.PayoutMaximumPercentage;
            this.SuccessCommand = command.SuccessCommand;
            this.FailureCommand = command.FailureCommand;
            this.StatusArgument = command.StatusArgument;
            this.StatusCommand = command.StatusCommand;
        }

        public CoinPusherGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.CoinPusher;
            this.Triggers = MixItUp.Base.Resources.CoinPusher.Replace(" ", string.Empty).ToLower();

            this.MinimumAmountForPayout = 1000;
            this.ProbabilityPercentage = 40;
            this.PayoutMinimumPercentage = 25;
            this.PayoutMaximumPercentage = 75;
            this.SuccessCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandCoinPusherWinExample, this.PrimaryCurrencyName));
            this.FailureCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandCoinPusherLoseExample, this.PrimaryCurrencyName));
            this.StatusArgument = MixItUp.Base.Resources.GameCommandStatusArgumentExample;
            this.StatusCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandCoinPusherStatusExample, this.PrimaryCurrencyName));
        }

        public override bool RequirePrimaryCurrency { get { return true; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new CoinPusherGameCommandModel(this.Name, this.GetChatTriggers(), this.MinimumAmountForPayout, this.ProbabilityPercentage, this.PayoutMinimumPercentage, this.PayoutMaximumPercentage,
                this.SuccessCommand, this.FailureCommand, this.StatusArgument, this.StatusCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            CoinPusherGameCommandModel gCommand = (CoinPusherGameCommandModel)command;
            gCommand.MinimumAmountForPayout = this.MinimumAmountForPayout;
            gCommand.ProbabilityPercentage = this.ProbabilityPercentage;
            gCommand.PayoutMinimumPercentage = this.PayoutMinimumPercentage;
            gCommand.PayoutMaximumPercentage = this.PayoutMaximumPercentage;
            gCommand.SuccessCommand = this.SuccessCommand;
            gCommand.FailureCommand = this.FailureCommand;
            gCommand.StatusArgument = this.StatusArgument;
            gCommand.StatusCommand = this.StatusCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.MinimumAmountForPayout < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandCoinPusherMinimumPayoutMustBePostive);
            }

            if (this.ProbabilityPercentage <= 0 || this.ProbabilityPercentage > 100)
            {
                return new Result(MixItUp.Base.Resources.GameCommandProbabilityMustBeBetween1And100);
            }

            if (this.PayoutMinimumPercentage <= 0 || this.PayoutMaximumPercentage <= 0 || this.PayoutMaximumPercentage < this.PayoutMinimumPercentage)
            {
                return new Result(MixItUp.Base.Resources.GameCommandCoinPusherPayoutPercentageInvalid);
            }

            return new Result();
        }
    }
}
