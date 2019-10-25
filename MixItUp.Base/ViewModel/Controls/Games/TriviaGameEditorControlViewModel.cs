using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class TriviaGameEditorControlViewModel : GameEditorControlViewModelBase
    {
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

        public string WinAmountString
        {
            get { return this.WinAmount.ToString(); }
            set
            {
                this.WinAmount = this.GetPositiveIntFromString(value);
                this.NotifyPropertyChanged();
            }
        }
        public int WinAmount { get; set; } = 100;

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }

        public CustomCommand CorrectAnswerCommand { get; set; }

        public TriviaGameCommand existingCommand;

        public TriviaGameEditorControlViewModel(UserCurrencyViewModel currency)
            : this()
        {
            this.StartedCommand = this.CreateBasic2ChatCommand("@$username has started a game of trivia! Type the number of the answer to the following question: $gamequestion", "$gameanswers");

            this.UserJoinCommand = this.CreateBasicChatCommand("You've put in your guess, let's wait and see which one is right...", whisper: true);

            this.UserSuccessCommand = this.CreateBasicCurrencyCommand($"You guessed correct and won $gamepayout {currency.Name}!", currency, "$gamepayout", whisper: true);

            this.CorrectAnswerCommand = this.CreateBasicChatCommand("The correct answer was $gamecorrectanswer!");
        }

        public TriviaGameEditorControlViewModel(TriviaGameCommand command)
            : this()
        {
            this.existingCommand = command;

            this.TimeLimit = this.existingCommand.TimeLimit;

            this.StartedCommand = this.existingCommand.StartedCommand;

            this.UserJoinCommand = this.existingCommand.UserJoinCommand;

            this.UserSuccessCommand = this.existingCommand.UserSuccessOutcome.Command;

            this.CorrectAnswerCommand = this.existingCommand.CorrectAnswerCommand;
        }

        private TriviaGameEditorControlViewModel()
        {

        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new TriviaGameCommand(name, triggers, requirements, this.TimeLimit, this.StartedCommand, this.UserJoinCommand, 
                new GameOutcome("Success", 0, roleProbabilities, this.UserSuccessCommand), new List<TriviaGameQuestion>(), this.WinAmount, this.CorrectAnswerCommand);
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

            if (this.WinAmount < 0)
            {
                await DialogHelper.ShowMessage("The Win amount is not a valid number greater than or equal to 0");
                return false;
            }

            return true;
        }
    }
}
