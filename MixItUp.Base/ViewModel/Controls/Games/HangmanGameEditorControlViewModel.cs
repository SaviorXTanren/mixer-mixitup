using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class HangmanGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string MaxFailuresString
        {
            get { return this.MaxFailures.ToString(); }
            set
            {
                this.MaxFailures = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MaxFailures { get; set; } = 5;

        public string InitialAmountString
        {
            get { return this.InitialAmount.ToString(); }
            set
            {
                this.InitialAmount = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int InitialAmount { get; set; } = 500;

        public string StatusArgument { get; set; } = "status";

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

        public CustomCommand FailedGuessCommand { get; set; }
        public CustomCommand SuccessfulGuessCommand { get; set; }

        public CustomCommand StatusCommand { get; set; }

        public CustomCommand GameWonCommand { get; set; }
        public CustomCommand GameLostCommand { get; set; }

        public ICommand BrowseCustomWordsFilePathCommand { get; set; }

        private HangmanGameCommand existingCommand;

        public HangmanGameEditorControlViewModel(UserCurrencyModel currency)
            : this()
        {
            this.FailedGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into the pot and try the letter $arg1text...but it was wrong! $gamehangmancurrent");
            this.SuccessfulGuessCommand = this.CreateBasicChatCommand("@$username drops their coins into the pot and try the letter $arg1text...and gets it right! $gamehangmancurrent");
            this.StatusCommand = this.CreateBasicChatCommand("So far, you've gotten $gamehangmancurrent correct & guessed $gamehangmanfailedguesses wrong. Looking into the pot, you see $gametotalamount " + currency.Name + " inside it.");
            this.GameWonCommand = this.CreateBasicChatCommand("@$username got the last letter for \"$gamehangmananswer\", winning the whole pot of $gamepayout " + currency.Name + "!");
            this.GameLostCommand = this.CreateBasicChatCommand("The game is lost with too many failed guesses; the correct answer was \"$gamehangmananswer\". There goes $gametotalamount " + currency.Name + "!");
        }

        public HangmanGameEditorControlViewModel(HangmanGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.MaxFailures = this.existingCommand.MaxFailures;
            this.InitialAmount = this.existingCommand.InitialAmount;
            this.StatusArgument = this.existingCommand.StatusArgument;
            this.CustomWordsFilePath = this.existingCommand.CustomWordsFilePath;

            this.FailedGuessCommand = this.existingCommand.FailedGuessCommand;
            this.SuccessfulGuessCommand = this.existingCommand.SuccessfulGuessCommand;
            this.StatusCommand = this.existingCommand.StatusCommand;
            this.GameWonCommand = this.existingCommand.GameWonCommand;
            this.GameLostCommand = this.existingCommand.GameLostCommand;
        }

        private HangmanGameEditorControlViewModel()
        {
            this.BrowseCustomWordsFilePathCommand = this.CreateCommand((parameter) =>
            {
                string filePath = ChannelSession.Services.FileService.ShowOpenFileDialog("Text Files (*.txt)|*.txt|All files (*.*)|*.*");
                if (!string.IsNullOrEmpty(filePath))
                {
                    this.CustomWordsFilePath = filePath;
                }
                return Task.FromResult(0);
            });
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            GameCommandBase newCommand = new HangmanGameCommand(name, triggers, requirements, this.StatusArgument, this.StatusCommand, this.MaxFailures,
                this.InitialAmount, this.SuccessfulGuessCommand, this.FailedGuessCommand, this.GameWonCommand, this.GameLostCommand, this.CustomWordsFilePath);
            this.SaveGameCommand(newCommand, this.existingCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.MaxFailures <= 0)
            {
                await DialogHelper.ShowMessage("The Max Failures is not a valid number greater than 0");
                return false;
            }

            if (this.InitialAmount <= 0)
            {
                await DialogHelper.ShowMessage("The Initial Amount is not a valid number greater than 0");
                return false;
            }

            if (string.IsNullOrEmpty(this.StatusArgument) && !this.StatusArgument.Any(c => char.IsLetterOrDigit(c)))
            {
                await DialogHelper.ShowMessage("The Status Argument must have a valid value");
                return false;
            }

            if (!string.IsNullOrEmpty(this.CustomWordsFilePath) && !ChannelSession.Services.FileService.FileExists(this.CustomWordsFilePath))
            {
                await DialogHelper.ShowMessage("The Custom Words file you specified does not exist");
                return false;
            }

            return true;
        }
    }
}
