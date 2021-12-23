using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Games
{
    public class WordScrambleGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int MinimumParticipants
        {
            get { return this.minimumParticipants; }
            set
            {
                this.minimumParticipants = value;
                this.NotifyPropertyChanged();
            }
        }
        private int minimumParticipants;

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

        public int WordScrambleTimeLimit
        {
            get { return this.wordScrambleTimeLimit; }
            set
            {
                this.wordScrambleTimeLimit = value;
                this.NotifyPropertyChanged();
            }
        }
        private int wordScrambleTimeLimit;

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

        public CustomCommandModel UserJoinCommand
        {
            get { return this.userJoinCommand; }
            set
            {
                this.userJoinCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userJoinCommand;

        public CustomCommandModel NotEnoughPlayersCommand
        {
            get { return this.notEnoughPlayersCommand; }
            set
            {
                this.notEnoughPlayersCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel notEnoughPlayersCommand;

        public CustomCommandModel WordScramblePrepareCommand
        {
            get { return this.wordScramblePrepareCommand; }
            set
            {
                this.wordScramblePrepareCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel wordScramblePrepareCommand;

        public CustomCommandModel WordScrambleBeginCommand
        {
            get { return this.wordScrambleBeginCommand; }
            set
            {
                this.wordScrambleBeginCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel wordScrambleBeginCommand;

        public CustomCommandModel UserSuccessCommand
        {
            get { return this.userSuccessCommand; }
            set
            {
                this.userSuccessCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userSuccessCommand;

        public CustomCommandModel UserFailureCommand
        {
            get { return this.userFailureCommand; }
            set
            {
                this.userFailureCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel userFailureCommand;

        public ICommand BrowseCustomWordsFilePathCommand { get; set; }

        public WordScrambleGameCommandEditorWindowViewModel(WordScrambleGameCommandModel command)
            : base(command)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.WordScrambleTimeLimit = command.WordScrambleTimeLimit;
            this.CustomWordsFilePath = command.CustomWordsFilePath;
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.NotEnoughPlayersCommand = command.NotEnoughPlayersCommand;
            this.WordScramblePrepareCommand = command.WordScramblePrepareCommand;
            this.WordScrambleBeginCommand = command.WordScrambleBeginCommand;
            this.UserSuccessCommand = command.UserSuccessCommand;
            this.UserFailureCommand = command.UserFailureCommand;

            this.SetUICommands();
        }

        public WordScrambleGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.WordScramble;
            this.Triggers = MixItUp.Base.Resources.GameCommandWordScrambleDefaultChatTrigger;

            this.MinimumParticipants = 2;
            this.TimeLimit = 60;
            this.WordScrambleTimeLimit = 300;
            this.StartedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandWordScrambleStartedExample);
            this.UserJoinCommand = this.CreateBasicCommand();
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandNotEnoughPlayersExample);
            this.WordScramblePrepareCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandWordScrambleWordScramblePrepareExample);
            this.WordScrambleBeginCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandWordScrambleWordScrambleBeginExample);
            this.UserSuccessCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandWordScrambleUserSuccessExample, this.PrimaryCurrencyName));
            this.UserFailureCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandWordScrambleUserFailureExample);

            this.SetUICommands();
        }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new WordScrambleGameCommandModel(this.Name, this.GetChatTriggers(), this.MinimumParticipants, this.TimeLimit, this.WordScrambleTimeLimit, this.CustomWordsFilePath,
                this.StartedCommand, this.UserJoinCommand, this.NotEnoughPlayersCommand, this.WordScramblePrepareCommand, this.WordScrambleBeginCommand, this.UserSuccessCommand, this.UserFailureCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            WordScrambleGameCommandModel gCommand = (WordScrambleGameCommandModel)command;
            gCommand.MinimumParticipants = this.MinimumParticipants;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.WordScrambleTimeLimit = this.WordScrambleTimeLimit;
            gCommand.CustomWordsFilePath = this.CustomWordsFilePath;
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.NotEnoughPlayersCommand = this.NotEnoughPlayersCommand;
            gCommand.WordScramblePrepareCommand = this.WordScramblePrepareCommand;
            gCommand.WordScrambleBeginCommand = this.WordScrambleBeginCommand;
            gCommand.UserSuccessCommand = this.UserSuccessCommand;
            gCommand.UserFailureCommand = this.UserFailureCommand;
        }

        public override async Task<Result> Validate()
        {
            Result result = await base.Validate();
            if (!result.Success)
            {
                return result;
            }

            if (this.MinimumParticipants < 1)
            {
                return new Result(MixItUp.Base.Resources.GameCommandMinimumParticipantsMustBeGreaterThan0);
            }

            if (this.TimeLimit <= 0 || this.WordScrambleTimeLimit <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
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