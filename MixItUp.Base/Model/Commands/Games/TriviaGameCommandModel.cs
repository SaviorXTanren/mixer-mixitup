using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class TriviaGameQuestionModel
    {
        [DataMember]
        public string Question { get; set; }
        [DataMember]
        public List<string> Answers { get; set; } = new List<string>();

        [DataMember]
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public int TotalAnswers { get { return this.Answers.Count; } }

        [JsonIgnore]
        public string CorrectAnswer { get { return this.Answers[0]; } }

        public TriviaGameQuestionModel() { }
    }

    [DataContract]
    public class TriviaGameCommandModel : GameCommandModelBase
    {
        public const string GameTriviaQuestionSpecialIdentifier = "gamequestion";
        public const string GameTriviaSpecificAnswerHeaderSpecialIdentifier = "gameanswer";
        public const string GameTriviaAnswersSpecialIdentifier = "gameanswers";
        public const string GameTriviaCorrectAnswerSpecialIdentifier = "gamecorrectanswer";

        private const string OpenTDBUrl = "https://opentdb.com/api.php?amount=1&type=multiple";

        private static readonly TriviaGameQuestionModel FallbackQuestion = new TriviaGameQuestionModel()
        {
            Question = "Looks like we failed to get a question, who's to blame?",
            Answers = new List<string>() { "SaviorXTanren", "SaviorXTanren", "SaviorXTanren", "SaviorXTanren" }
        };

        private class OpenTDBResults
        {
            public int response_code { get; set; }
            public List<OpenTDBQuestion> results { get; set; }
        }

        private class OpenTDBQuestion
        {
            public string category { get; set; }
            public string type { get; set; }
            public string difficulty { get; set; }
            public string question { get; set; }
            public string correct_answer { get; set; }
            public List<string> incorrect_answers { get; set; }
        }

        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public bool UseRandomOnlineQuestions { get; set; }
        [DataMember]
        public int WinAmount { get; set; }

        [DataMember]
        public List<TriviaGameQuestionModel> CustomQuestions { get; set; } = new List<TriviaGameQuestionModel>();

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }

        [DataMember]
        public CustomCommandModel CorrectAnswerCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserSuccessCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserFailureCommand { get; set; }

        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private TriviaGameQuestionModel question;
        [JsonIgnore]
        private Dictionary<int, string> numbersToAnswers = new Dictionary<int, string>();
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, int> runUserSelections = new Dictionary<UserV2ViewModel, int>();

        public TriviaGameCommandModel(string name, HashSet<string> triggers, int timeLimit, bool useRandomOnlineQuestions, int winAmount, IEnumerable<TriviaGameQuestionModel> customQuestions,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel correctAnswerCommand, CustomCommandModel userSuccessCommand, CustomCommandModel userFailureCommand)
            : base(name, triggers, GameCommandTypeEnum.Trivia)
        {
            this.TimeLimit = timeLimit;
            this.UseRandomOnlineQuestions = useRandomOnlineQuestions;
            this.WinAmount = winAmount;
            this.CustomQuestions = new List<TriviaGameQuestionModel>(customQuestions);
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.CorrectAnswerCommand = correctAnswerCommand;
            this.UserSuccessCommand = userSuccessCommand;
            this.UserFailureCommand = userFailureCommand;
        }

        [Obsolete]
        public TriviaGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.CorrectAnswerCommand);
            commands.Add(this.UserSuccessCommand);
            commands.Add(this.UserFailureCommand);
            return commands;
        }

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            if (this.runParameters != null)
            {
                return new Result(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
            }
            return await base.CustomValidation(parameters);
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            this.runParameters = parameters;

            bool useCustomQuestion = (RandomHelper.GenerateProbability() >= 50);
            if (this.CustomQuestions.Count > 0 && !this.UseRandomOnlineQuestions)
            {
                useCustomQuestion = true;
            }
            else if (this.CustomQuestions.Count == 0 && this.UseRandomOnlineQuestions)
            {
                useCustomQuestion = false;
            }

            if (!useCustomQuestion)
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient())
                {
                    OpenTDBResults openTDBResults = await client.GetAsync<OpenTDBResults>(OpenTDBUrl);
                    if (openTDBResults != null && openTDBResults.results != null && openTDBResults.results.Count > 0)
                    {
                        List<string> answers = new List<string>();
                        answers.Add(HttpUtility.HtmlDecode(openTDBResults.results[0].correct_answer));
                        answers.AddRange(openTDBResults.results[0].incorrect_answers.Select(a => HttpUtility.HtmlDecode(a)));
                        this.question = new TriviaGameQuestionModel()
                        {
                            Question = HttpUtility.HtmlDecode(openTDBResults.results[0].question),
                            Answers = answers,
                        };
                    }
                }
            }

            if (this.CustomQuestions.Count > 0 && (useCustomQuestion || this.question == null))
            {
                this.question = this.CustomQuestions.Where(q => q.Enabled).Random();
            }

            if (this.question == null)
            {
                this.question = FallbackQuestion;
            }

            int i = 1;
            foreach (string answer in this.question.Answers.Shuffle())
            {
                this.numbersToAnswers[i] = answer;
                i++;
            }

            int correctAnswerNumber = 0;
            foreach (var kvp in this.numbersToAnswers)
            {
                if (kvp.Value.Equals(this.question.CorrectAnswer))
                {
                    correctAnswerNumber = kvp.Key;
                    break;
                }
            }

            this.runParameters.SpecialIdentifiers[TriviaGameCommandModel.GameTriviaQuestionSpecialIdentifier] = this.question.Question;
            foreach (var kvp in this.numbersToAnswers)
            {
                this.runParameters.SpecialIdentifiers[TriviaGameCommandModel.GameTriviaSpecificAnswerHeaderSpecialIdentifier + kvp.Key] = kvp.Value;
            }
            this.runParameters.SpecialIdentifiers[TriviaGameCommandModel.GameTriviaAnswersSpecialIdentifier] = string.Join(", ", this.numbersToAnswers.OrderBy(a => a.Key).Select(a => $"{a.Key}) {a.Value}"));
            this.runParameters.SpecialIdentifiers[TriviaGameCommandModel.GameTriviaCorrectAnswerSpecialIdentifier] = this.question.CorrectAnswer;

            IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forTrivia: true);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;

                await this.DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;

                List<CommandParametersModel> winners = new List<CommandParametersModel>();
                foreach (var kvp in this.runUserSelections.ToList())
                {
                    CommandParametersModel participant = this.runUsers[kvp.Key];
                    if (kvp.Value == correctAnswerNumber)
                    {
                        winners.Add(participant);
                        this.PerformPrimarySetPayout(participant.User, this.WinAmount);
                        participant.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = this.WinAmount.ToString();
                        await this.RunSubCommand(this.UserSuccessCommand, participant);
                    }
                    else
                    {
                        await this.RunSubCommand(this.UserFailureCommand, participant);
                    }
                }

                this.SetGameWinners(this.runParameters, winners);
                this.runParameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = this.WinAmount.ToString();
                this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameAllPayoutSpecialIdentifier] = (this.WinAmount * winners.Count).ToString();
                await this.RunSubCommand(this.CorrectAnswerCommand, this.runParameters);

                if (widgets.Count() > 0)
                {
                    foreach (OverlayPollV3Model widget in widgets)
                    {
                        await widget.End(correctAnswerNumber.ToString());
                    }
                }

                await this.PerformCooldown(this.runParameters);
                this.ClearData();
            }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            await this.RunSubCommand(this.StartedCommand, this.runParameters);

            if (widgets.Count() > 0)
            {
                foreach (OverlayPollV3Model widget in widgets)
                {
                    await widget.NewTriviaCommand(this.question.Question, this.TimeLimit, this.numbersToAnswers);
                }
            }
        }

        private async void ChatService_OnChatMessageReceived(object sender, ViewModel.Chat.ChatMessageViewModel message)
        {
            try
            {
                if (!this.runUsers.ContainsKey(message.User) && !string.IsNullOrEmpty(message.PlainTextMessage) && int.TryParse(message.PlainTextMessage, out int choice) && this.numbersToAnswers.ContainsKey(choice))
                {
                    CommandParametersModel parameters = new CommandParametersModel(message);
                    this.runUsers[message.User] = parameters;
                    this.runUserSelections[message.User] = choice;
                    await this.RunSubCommand(this.UserJoinCommand, parameters);

                    IEnumerable<OverlayPollV3Model> widgets = OverlayPollV3Model.GetPollOverlayWidgets(forTrivia: true);
                    if (widgets.Count() > 0)
                    {
                        foreach (OverlayPollV3Model widget in widgets)
                        {
                            await widget.UpdateTriviaCommand(choice);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void ClearData()
        {
            this.runParameters = null;
            this.question = null;
            this.numbersToAnswers.Clear();
            this.runUsers.Clear();
            this.runUserSelections.Clear();
        }
    }
}