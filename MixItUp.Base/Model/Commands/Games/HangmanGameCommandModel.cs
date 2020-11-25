using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class HangmanGameCommandModel : GameCommandModelBase
    {
        public const string GameHangmanCurrentSpecialIdentifier = "gamehangmancurrent";
        public const string GameHangmanFailedGuessesSpecialIdentifier = "gamehangmanfailedguesses";
        public const string GameHangmanAnswerSpecialIdentifier = "gamehangmananswer";

        [DataMember]
        public string StatusArgument { get; set; }
        [DataMember]
        public int MaxFailures { get; set; }
        [DataMember]
        public int InitialAmount { get; set; }

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommandModel StatusCommand { get; set; }

        [DataMember]
        public CustomCommandModel SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommandModel FailedGuessCommand { get; set; }

        [DataMember]
        public CustomCommandModel GameWonCommand { get; set; }
        [DataMember]
        public CustomCommandModel GameLostCommand { get; set; }

        [DataMember]
        public string CurrentWord { get; set; }
        [DataMember]
        public int TotalAmount { get; set; }
        [DataMember]
        public HashSet<char> SuccessfulGuesses { get; set; } = new HashSet<char>();
        [DataMember]
        public HashSet<char> FailedGuesses { get; set; } = new HashSet<char>();

        [JsonIgnore]
        private CommandParametersModel runParameters;

        public HangmanGameCommandModel(string name, HashSet<string> triggers, string statusArgument, int maxFailures, int initialAmount, string customWordsFilePath, CustomCommandModel statusCommand,
            CustomCommandModel successfulGuessCommand, CustomCommandModel failedGuessCommand, CustomCommandModel gameWonCommand, CustomCommandModel gameLostCommand)
            : base(name, triggers)
        {
            this.StatusArgument = statusArgument;
            this.MaxFailures = maxFailures;
            this.InitialAmount = initialAmount;
            this.CustomWordsFilePath = customWordsFilePath;
            this.StatusCommand = statusCommand;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
            this.GameWonCommand = gameWonCommand;
            this.GameLostCommand = gameLostCommand;
        }

        private HangmanGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StatusCommand);
            commands.Add(this.SuccessfulGuessCommand);
            commands.Add(this.FailedGuessCommand);
            commands.Add(this.GameWonCommand);
            commands.Add(this.GameLostCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                if (string.IsNullOrEmpty(this.CurrentWord) || this.FailedGuesses.Count >= this.MaxFailures)
                {
                    await this.ClearData();
                }

                this.AddSpecialIdentifiersToParameters(parameters);
                await this.StatusCommand.Perform(parameters);

                return false;
            }
            else
            {
                return await base.ValidateRequirements(parameters);
            }
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (string.IsNullOrEmpty(this.CurrentWord) || this.FailedGuesses.Count >= this.MaxFailures)
            {
                await this.ClearData();
            }

            if (parameters.Arguments.Count == 1 && parameters.Arguments[0].Length == 1)
            {
                this.TotalAmount += this.GetBetAmount(parameters);
                char letter = parameters.Arguments.ElementAt(0).ToUpper().First();
                if (this.CurrentWord.Contains(letter))
                {
                    this.SuccessfulGuesses.Add(letter);
                    this.AddSpecialIdentifiersToParameters(parameters);

                    if (this.CurrentWord.All(c => this.SuccessfulGuesses.Contains(c)))
                    {
                        this.GameCurrencyRequirement.Currency.AddAmount(parameters.User.Data, this.TotalAmount);
                        await this.GameWonCommand.Perform(parameters);
                        await this.ClearData();
                    }
                    else
                    {
                        await this.SuccessfulGuessCommand.Perform(parameters);
                    }
                }
                else
                {
                    this.FailedGuesses.Add(letter);
                    this.AddSpecialIdentifiersToParameters(parameters);

                    if (this.FailedGuesses.Count >= this.MaxFailures)
                    {
                        await this.GameLostCommand.Perform(parameters);
                        await this.ClearData();
                    }
                    else
                    {
                        await this.FailedGuessCommand.Perform(parameters);
                    }
                }
                return;
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandHangmanNotLetter);
            }
            await this.Requirements.Refund(parameters);
        }

        private async Task ClearData()
        {
            HashSet<string> wordsToUse = new HashSet<string>(GameCommandModelBase.DefaultWords.Where(s => s.Length > 4));
            if (!string.IsNullOrEmpty(this.CustomWordsFilePath) && ChannelSession.Services.FileService.FileExists(this.CustomWordsFilePath))
            {
                string fileData = await ChannelSession.Services.FileService.ReadFile(this.CustomWordsFilePath);
                if (!string.IsNullOrEmpty(fileData))
                {
                    wordsToUse = new HashSet<string>();
                    foreach (string split in fileData.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        wordsToUse.Add(split);
                    }
                }
            }
            this.CurrentWord = wordsToUse.Random().ToUpper();
            this.SuccessfulGuesses.Clear();
            this.FailedGuesses.Clear();
            this.TotalAmount = this.InitialAmount;
        }

        private void AddSpecialIdentifiersToParameters(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanAnswerSpecialIdentifier] = this.CurrentWord;
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanFailedGuessesSpecialIdentifier] = string.Join(", ", this.FailedGuesses);
            List<string> currentLetters = new List<string>();
            foreach (char c in this.CurrentWord)
            {
                currentLetters.Add(this.SuccessfulGuesses.Contains(c) ? c.ToString() : "_");
            }
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanCurrentSpecialIdentifier] = string.Join(" ", currentLetters);
        }
    }
}
