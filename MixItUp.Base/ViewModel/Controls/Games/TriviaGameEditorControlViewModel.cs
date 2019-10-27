using MixItUp.Base.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Controls.Games
{
    public class TriviaGameQuestionViewModel : UIViewModelBase
    {
        public string Question { get; set; }

        public string CorrectAnswer { get; set; }

        public List<string> WrongAnswers { get; set; } = new List<string>() { string.Empty, string.Empty, string.Empty };

        public ICommand DeleteCustomQuestionCommand { get; set; }

        private TriviaGameEditorControlViewModel control;

        public TriviaGameQuestionViewModel(TriviaGameEditorControlViewModel control)
        {
            this.control = control;

            this.DeleteCustomQuestionCommand = this.CreateCommand((parameter) =>
            {
                this.control.DeleteCustomQuestion(this);
                return Task.FromResult(0);
            });
        }

        public TriviaGameQuestionViewModel(TriviaGameEditorControlViewModel control, TriviaGameQuestion question)
            : this(control)
        {
            this.Question = question.Question;
            this.CorrectAnswer = question.CorrectAnswer;
            for (int i = 0; i < question.WrongAnswers.Count; i++)
            {
                this.WrongAnswers[i] = question.WrongAnswers[i];
            }
        }

        public string WrongAnswer1
        {
            get { return this.WrongAnswers[0]; }
            set
            {
                this.WrongAnswers[0] = value;
                this.NotifyPropertyChanged();
            }
        }

        public string WrongAnswer2
        {
            get { return this.WrongAnswers[1]; }
            set
            {
                this.WrongAnswers[1] = value;
                this.NotifyPropertyChanged();
            }
        }

        public string WrongAnswer3
        {
            get { return this.WrongAnswers[2]; }
            set
            {
                this.WrongAnswers[2] = value;
                this.NotifyPropertyChanged();
            }
        }

        public uint TotalAnswers { get { return (uint)this.WrongAnswers.Count + 1; } }

        public bool Validate()
        {
            return (!string.IsNullOrEmpty(this.Question)) && (!string.IsNullOrEmpty(this.CorrectAnswer)) &&
                ((!string.IsNullOrEmpty(this.WrongAnswer1)) || (!string.IsNullOrEmpty(this.WrongAnswer2)) || (!string.IsNullOrEmpty(this.WrongAnswer3)));
        }

        public TriviaGameQuestion GetQuestion()
        {
            TriviaGameQuestion question = new TriviaGameQuestion()
            {
                Question = this.Question,
                CorrectAnswer = this.CorrectAnswer
            };
            foreach (string wrongAnswer in this.WrongAnswers)
            {
                if (!string.IsNullOrEmpty(wrongAnswer))
                {
                    question.WrongAnswers.Add(wrongAnswer);
                }
            }
            return question;
        }
    }

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

        public bool UseRandomOnlineQuestions { get; set; }

        public ObservableCollection<TriviaGameQuestionViewModel> CustomQuestions { get; set; } = new ObservableCollection<TriviaGameQuestionViewModel>();

        public CustomCommand StartedCommand { get; set; }

        public CustomCommand UserJoinCommand { get; set; }

        public CustomCommand UserSuccessCommand { get; set; }

        public CustomCommand CorrectAnswerCommand { get; set; }

        public ICommand AddCustomQuestionCommand { get; set; }

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

            this.UseRandomOnlineQuestions = this.existingCommand.UseRandomOnlineQuestions;

            foreach (TriviaGameQuestion question in command.CustomQuestions)
            {
                this.CustomQuestions.Add(new TriviaGameQuestionViewModel(this, question));
            }
        }

        private TriviaGameEditorControlViewModel()
        {
            this.AddCustomQuestionCommand = this.CreateCommand((parameter) =>
            {
                this.CustomQuestions.Add(new TriviaGameQuestionViewModel(this));
                return Task.FromResult(0);
            });
        }

        public void DeleteCustomQuestion(TriviaGameQuestionViewModel question)
        {
            this.CustomQuestions.Remove(question);
        }

        public override void SaveGameCommand(string name, IEnumerable<string> triggers, RequirementViewModel requirements)
        {
            Dictionary<MixerRoleEnum, int> roleProbabilities = new Dictionary<MixerRoleEnum, int>() { { MixerRoleEnum.User, 0 }, { MixerRoleEnum.Subscriber, 0 }, { MixerRoleEnum.Mod, 0 } };

            GameCommandBase newCommand = new TriviaGameCommand(name, triggers, requirements, this.TimeLimit, this.StartedCommand, this.UserJoinCommand, 
                new GameOutcome("Success", 0, roleProbabilities, this.UserSuccessCommand), this.CustomQuestions.Select(q => q.GetQuestion()), this.WinAmount, this.CorrectAnswerCommand,
                this.UseRandomOnlineQuestions);
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

            foreach (TriviaGameQuestionViewModel question in this.CustomQuestions)
            {
                if (!question.Validate())
                {
                    await DialogHelper.ShowMessage("A Custom Question you supplied is missing the question, correct answer, or at least 1 wrong answer");
                    return false;
                }
            }

            if (!this.UseRandomOnlineQuestions && this.CustomQuestions.Count == 0)
            {
                await DialogHelper.ShowMessage("You must at least select the option to use random online questions or provide your own custom questions");
                return false;
            }

            return true;
        }
    }
}
