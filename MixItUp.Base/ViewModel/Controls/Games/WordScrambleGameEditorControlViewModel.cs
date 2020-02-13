using MixItUp.Base.Commands;
using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class WordScrambleGameEditorControlViewModel : GameEditorControlViewModelBase
    {
        public string MinimumParticipantsString
        {
            get { return this.MinimumParticipants.ToString(); }
            set
            {
                this.MinimumParticipants = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int MinimumParticipants { get; set; } = 2;

        public string TimeLimitString
        {
            get { return this.TimeLimit.ToString(); }
            set
            {
                this.TimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int TimeLimit { get; set; } = 30;

        public string WordScrambleTimeLimitString
        {
            get { return this.WordScrambleTimeLimit.ToString(); }
            set
            {
                this.WordScrambleTimeLimit = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int WordScrambleTimeLimit { get; set; } = 60;

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

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }
        public CustomCommand NotEnoughPlayersCommand { get; set; }

        public CustomCommand WordScramblePrepareCommand { get; set; }
        public CustomCommand WordScrambleBeginCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }
        public CustomCommand UserFailCommand { get; set; }

        public ICommand BrowseCustomWordsFilePathCommand { get; set; }

        public WordScrambleGameCommand existingCommand;

        public WordScrambleGameEditorControlViewModel(UserCurrencyModel currency)
            : this()
        {
            this.StartedCommand = this.CreateBasicChatCommand("@$username has started a game of word scramble! Type !scramble in chat to play!");

            this.UserJoinCommand = this.CreateBasicChatCommand("You assemble with everyone else arond the table...", whisper: true);
            this.NotEnoughPlayersCommand = this.CreateBasicChatCommand("@$username couldn't get enough users to join in...");

            this.WordScramblePrepareCommand = this.CreateBasicChatCommand("Everyone is gathered patiently with their pencils in hand. Get ready...");
            this.WordScrambleBeginCommand = this.CreateBasicChatCommand("The scrambled word is \"$gamewordscrambleword\"; solve it and be the first to type it in chat!");

            this.UserSuccessCommand = this.CreateBasicChatCommand("$gamewinners correctly guessed the word \"$gamewordscrambleanswer\" and walked away with a bounty of $gamepayout " + currency.Name + "!");
            this.UserFailCommand = this.CreateBasicChatCommand("No one was able the guess the word \"$gamewordscrambleanswer\"! Better luck next time...");
        }

        public WordScrambleGameEditorControlViewModel(WordScrambleGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.MinimumParticipants = this.existingCommand.MinimumParticipants;
            this.TimeLimit = this.existingCommand.TimeLimit;
            this.WordScrambleTimeLimit = this.existingCommand.WordScrambleTimeLimit;
            this.CustomWordsFilePath = this.existingCommand.CustomWordsFilePath;

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;
            this.NotEnoughPlayersCommand = this.existingCommand.NotEnoughPlayersCommand;

            this.WordScramblePrepareCommand = this.existingCommand.WordScramblePrepareCommand;
            this.WordScrambleBeginCommand = this.existingCommand.WordScrambleBeginCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;
            this.UserFailCommand = this.existingCommand.UserFailOutcome.Command;
        }

        private WordScrambleGameEditorControlViewModel()
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
            Dictionary<UserRoleEnum, int> roleProbabilities = new Dictionary<UserRoleEnum, int>() { { UserRoleEnum.User, 0 }, { UserRoleEnum.Subscriber, 0 }, { UserRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new WordScrambleGameCommand(name, triggers, requirements, this.MinimumParticipants, this.TimeLimit, this.CustomWordsFilePath, this.WordScrambleTimeLimit,
                this.StartedCommand, this.UserJoinCommand, this.WordScramblePrepareCommand, this.WordScrambleBeginCommand, new GameOutcome("Success", 0, roleProbabilities, this.UserSuccessCommand),
                new GameOutcome("Failure", 0, roleProbabilities, this.UserFailCommand), this.NotEnoughPlayersCommand);
            if (this.existingCommand != null)
            {
                ChannelSession.Settings.GameCommands.Remove(this.existingCommand);
                newCommand.ID = this.existingCommand.ID;
            }
            ChannelSession.Settings.GameCommands.Add(newCommand);
        }

        public override async Task<bool> Validate()
        {
            if (this.TimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Time Limit is not a valid number greater than 0");
                return false;
            }

            if (this.MinimumParticipants <= 0)
            {
                await DialogHelper.ShowMessage("The Minimum Participants is not a valid number greater than 0");
                return false;
            }

            if (this.WordScrambleTimeLimit <= 0)
            {
                await DialogHelper.ShowMessage("The Word Scramble Time Limit is not a valid number greater than 0");
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
