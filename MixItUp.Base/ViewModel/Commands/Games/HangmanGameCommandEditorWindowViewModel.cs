using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Games
{
    public class HangmanGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int MaxFailures
        {
            get { return this.maxFailures; }
            set
            {
                this.maxFailures = value;
                this.NotifyPropertyChanged();
            }
        }
        private int maxFailures;

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

        public bool AllowWordGuess
        {
            get { return this.allowWordGuess; }
            set
            {
                this.allowWordGuess = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool allowWordGuess;

        public string CustomWordsFilePath
        {
            get { return this.customWordsFilePath; }
            set
            {
                this.customWordsFilePath = value;
                this.NotifyPropertyChanged();
            }
        }
        private string customWordsFilePath;

        public CustomCommandModel SuccessfulGuessCommand
        {
            get { return this.successfulGuessCommand; }
            set
            {
                this.successfulGuessCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel successfulGuessCommand;

        public CustomCommandModel FailedGuessCommand
        {
            get { return this.failedGuessCommand; }
            set
            {
                this.failedGuessCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel failedGuessCommand;

        public CustomCommandModel GameWonCommand
        {
            get { return this.gameWonCommand; }
            set
            {
                this.gameWonCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel gameWonCommand;

        public CustomCommandModel GameLostCommand
        {
            get { return this.gameLostCommand; }
            set
            {
                this.gameLostCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel gameLostCommand;

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

        public ICommand BrowseCustomWordsFilePathCommand { get; set; }

        public HangmanGameCommandEditorWindowViewModel(HangmanGameCommandModel command)
            : base(command)
        {
            this.MaxFailures = command.MaxFailures;
            this.InitialAmount = command.InitialAmount;
            this.AllowWordGuess = command.AllowWordGuess;
            this.CustomWordsFilePath = command.CustomWordsFilePath;
            this.SuccessfulGuessCommand = command.SuccessfulGuessCommand;
            this.FailedGuessCommand = command.FailedGuessCommand;
            this.GameWonCommand = command.GameWonCommand;
            this.GameLostCommand = command.GameLostCommand;
            this.StatusArgument = command.StatusArgument;
            this.StatusCommand = command.StatusCommand;

            this.SetUICommands();
        }

        public HangmanGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Hangman;
            this.Triggers = MixItUp.Base.Resources.Hangman.Replace(" ", string.Empty).ToLower();

            this.MaxFailures = 5;
            this.InitialAmount = 100;
            this.AllowWordGuess = false;
            this.SuccessfulGuessCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandHangmanSuccessfulGuessExample);
            this.FailedGuessCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandHangmanFailedGuessExample);
            this.GameWonCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandHangmanGameWonExample, this.PrimaryCurrencyName));
            this.GameLostCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandHangmanGameLostExample, this.PrimaryCurrencyName));
            this.StatusArgument = MixItUp.Base.Resources.GameCommandStatusArgumentExample;
            this.StatusCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandHangmanStatusExample, this.PrimaryCurrencyName));

            this.SetUICommands();
        }

        public override bool RequirePrimaryCurrency { get { return true; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new HangmanGameCommandModel(this.Name, this.GetChatTriggers(), this.MaxFailures, this.InitialAmount, this.AllowWordGuess, this.CustomWordsFilePath,
                this.SuccessfulGuessCommand, this.FailedGuessCommand, this.GameWonCommand, this.GameLostCommand, this.StatusArgument, this.StatusCommand));
        }


        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            HangmanGameCommandModel gCommand = (HangmanGameCommandModel)command;
            gCommand.MaxFailures = this.MaxFailures;
            gCommand.InitialAmount = this.InitialAmount;
            gCommand.AllowWordGuess = this.AllowWordGuess;
            gCommand.CustomWordsFilePath = this.CustomWordsFilePath;
            gCommand.SuccessfulGuessCommand = this.SuccessfulGuessCommand;
            gCommand.FailedGuessCommand = this.FailedGuessCommand;
            gCommand.GameWonCommand = this.GameWonCommand;
            gCommand.GameLostCommand = this.GameLostCommand;
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

            if (this.MaxFailures < 1)
            {
                return new Result(MixItUp.Base.Resources.GameCommandHangmanMaxFailuresMustBePositive);
            }

            if (this.InitialAmount < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandInitialAmountMustBePositive);
            }

            return new Result();
        }

        private void SetUICommands()
        {
            this.BrowseCustomWordsFilePathCommand = this.CreateCommand(() =>
            {
                string filePath = ServiceManager.Get<IFileService>().ShowOpenFileDialog(ServiceManager.Get<IFileService>().TextFileFilter());
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.CustomWordsFilePath = filePath;
                }
            });
        }
    }
}