using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Commands.Games;
using MixItUp.Base.Model.Currency;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MixItUp.Base.ViewModel.Games
{
    public class TriviaGameQuestionViewModel : UIViewModelBase
    {
        public string Question
        {
            get { return this.question; }
            set
            {
                this.question = value;
                this.NotifyPropertyChanged();
            }
        }
        private string question;

        public string CorrectAnswer
        {
            get { return this.correctAnswer; }
            set
            {
                this.correctAnswer = value;
                this.NotifyPropertyChanged();
            }
        }
        private string correctAnswer;

        public string WrongAnswer1
        {
            get { return this.wrongAnswer1; }
            set
            {
                this.wrongAnswer1 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string wrongAnswer1;

        public string WrongAnswer2
        {
            get { return this.wrongAnswer2; }
            set
            {
                this.wrongAnswer2 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string wrongAnswer2;

        public string WrongAnswer3
        {
            get { return this.wrongAnswer3; }
            set
            {
                this.wrongAnswer3 = value;
                this.NotifyPropertyChanged();
            }
        }
        private string wrongAnswer3;

        public bool Enabled
        {
            get { return this.enabled; }
            set
            {
                this.enabled = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool enabled = true;

        public TriviaGameQuestionViewModel() { }

        public TriviaGameQuestionViewModel(TriviaGameQuestionModel model)
        {
            this.Question = model.Question;
            this.CorrectAnswer = model.CorrectAnswer;
            this.WrongAnswer1 = model.Answers[1];
            this.WrongAnswer2 = (model.Answers.Count > 2) ? model.Answers[2] : string.Empty;
            this.WrongAnswer3 = (model.Answers.Count > 3) ? model.Answers[3] : string.Empty;
            this.Enabled = model.Enabled;
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(this.Question))
            {
                return new Result(MixItUp.Base.Resources.GameCommandTriviaQuestionMissing);
            }

            if (string.IsNullOrEmpty(this.CorrectAnswer) || string.IsNullOrEmpty(this.WrongAnswer1))
            {
                return new Result(MixItUp.Base.Resources.GameCommandTriviaAnswersMissing);
            }

            return new Result();
        }

        public TriviaGameQuestionModel GetModel()
        {
            TriviaGameQuestionModel result = new TriviaGameQuestionModel()
            {
                Question = this.Question
            };
            result.Answers.Add(this.CorrectAnswer);
            result.Answers.Add(this.WrongAnswer1);
            if (!string.IsNullOrEmpty(this.WrongAnswer2)) { result.Answers.Add(this.WrongAnswer2); }
            if (!string.IsNullOrEmpty(this.WrongAnswer3)) { result.Answers.Add(this.WrongAnswer3); }
            result.Enabled = this.Enabled;
            return result;
        }
    }

    public class TriviaGameCommandEditorWindowViewModel : GameCommandEditorWindowViewModelBase
    {
        public int WinAmount
        {
            get { return this.winAmount; }
            set
            {
                this.winAmount = value;
                this.NotifyPropertyChanged();
            }
        }
        private int winAmount;

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

        public bool UseRandomOnlineQuestions
        {
            get { return this.useRandomOnlineQuestions; }
            set
            {
                this.useRandomOnlineQuestions = value;
                this.NotifyPropertyChanged();
            }
        }
        private bool useRandomOnlineQuestions;

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

        public CustomCommandModel CorrectAnswerCommand
        {
            get { return this.correctAnswerCommand; }
            set
            {
                this.correctAnswerCommand = value;
                this.NotifyPropertyChanged();
            }
        }
        private CustomCommandModel correctAnswerCommand;

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

        public ObservableCollection<TriviaGameQuestionViewModel> CustomQuestions { get; set; } = new ObservableCollection<TriviaGameQuestionViewModel>();

        public ICommand AddQuestionCommand { get; set; }
        public ICommand DeleteQuestionCommand { get; set; }

        public TriviaGameCommandEditorWindowViewModel(TriviaGameCommandModel command)
            : base(command)
        {
            this.WinAmount = command.WinAmount;
            this.TimeLimit = command.TimeLimit;
            this.UseRandomOnlineQuestions = command.UseRandomOnlineQuestions;
            this.StartedCommand = command.StartedCommand;
            this.UserJoinCommand = command.UserJoinCommand;
            this.CorrectAnswerCommand = command.CorrectAnswerCommand;
            this.UserSuccessCommand = command.UserSuccessCommand;
            this.UserFailureCommand = command.UserFailureCommand;

            this.CustomQuestions.AddRange(command.CustomQuestions.Select(q => new TriviaGameQuestionViewModel(q)));

            this.SetUICommands();
        }

        public TriviaGameCommandEditorWindowViewModel(CurrencyModel currency)
            : base(currency)
        {
            this.Name = MixItUp.Base.Resources.Trivia;
            this.Triggers = MixItUp.Base.Resources.Trivia.Replace(" ", string.Empty).ToLower();

            this.WinAmount = 100;
            this.TimeLimit = 30;
            this.UseRandomOnlineQuestions = true;
            this.StartedCommand = this.CreateBasicChatCommand(MixItUp.Base.Resources.GameCommandTriviaStartedExample);
            this.UserJoinCommand = this.CreateBasicCommand();
            this.CorrectAnswerCommand = this.CreateBasicChatCommand(string.Format(MixItUp.Base.Resources.GameCommandTriviaCorrectAnswerExample, this.PrimaryCurrencyName));
            this.UserSuccessCommand = this.CreateBasicCommand();
            this.UserFailureCommand = this.CreateBasicCommand();

            this.SetUICommands();
        }

        public override bool AutoAddCurrencyRequirement { get { return false; } }

        public override Task<CommandModelBase> CreateNewCommand()
        {
            return Task.FromResult<CommandModelBase>(new TriviaGameCommandModel(this.Name, this.GetChatTriggers(), this.TimeLimit, this.UseRandomOnlineQuestions, this.WinAmount, this.CustomQuestions.Select(o => o.GetModel()),
                this.StartedCommand, this.UserJoinCommand, this.CorrectAnswerCommand, this.UserSuccessCommand, this.UserFailureCommand));
        }

        public override async Task UpdateExistingCommand(CommandModelBase command)
        {
            await base.UpdateExistingCommand(command);
            TriviaGameCommandModel gCommand = (TriviaGameCommandModel)command;
            gCommand.TimeLimit = this.TimeLimit;
            gCommand.UseRandomOnlineQuestions = this.UseRandomOnlineQuestions;
            gCommand.WinAmount = this.WinAmount;
            gCommand.CustomQuestions = new List<TriviaGameQuestionModel>(this.CustomQuestions.Select(o => o.GetModel()));
            gCommand.StartedCommand = this.StartedCommand;
            gCommand.UserJoinCommand = this.UserJoinCommand;
            gCommand.CorrectAnswerCommand = this.CorrectAnswerCommand;
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

            if (this.WinAmount < 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTriviaWinAmountMustBePositive);
            }

            if (this.TimeLimit <= 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTimeLimitMustBePositive);
            }

            if (!this.UseRandomOnlineQuestions && this.CustomQuestions.Count == 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandTriviaAtLeastOneQuestion);
            }

            foreach (TriviaGameQuestionViewModel question in this.CustomQuestions)
            {
                result = question.Validate();
                if (!result.Success)
                {
                    return result;
                }
            }

            return new Result();
        }

        private void SetUICommands()
        {
            this.AddQuestionCommand = this.CreateCommand(() =>
            {
                this.CustomQuestions.Add(new TriviaGameQuestionViewModel());
            });

            this.DeleteQuestionCommand = this.CreateCommand((parameter) =>
            {
                this.CustomQuestions.Remove((TriviaGameQuestionViewModel)parameter);
            });
        }
    }
}