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
        public int MaxFailures { get; set; }
        [DataMember]
        public int InitialAmount { get; set; }
        [DataMember]
        public bool AllowWordGuess { get; set; }
        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommandModel SuccessfulGuessCommand { get; set; }
        [DataMember]
        public CustomCommandModel FailedGuessCommand { get; set; }

        [DataMember]
        public CustomCommandModel GameWonCommand { get; set; }
        [DataMember]
        public CustomCommandModel GameLostCommand { get; set; }

        [DataMember]
        public string StatusArgument { get; set; }
        [DataMember]
        public CustomCommandModel StatusCommand { get; set; }

        [JsonIgnore]
        public string currentWord { get; set; }
        [JsonIgnore]
        public int totalAmount { get; set; }
        [JsonIgnore]
        public HashSet<char> successfulGuesses { get; set; } = new HashSet<char>();
        [JsonIgnore]
        public HashSet<char> failedGuesses { get; set; } = new HashSet<char>();
        [JsonIgnore]
        private int failedGuessesCount = 0;

        public HangmanGameCommandModel(string name, HashSet<string> triggers, int maxFailures, int initialAmount, bool allowWordGuess, string customWordsFilePath, CustomCommandModel successfulGuessCommand, CustomCommandModel failedGuessCommand,
            CustomCommandModel gameWonCommand, CustomCommandModel gameLostCommand, string statusArgument, CustomCommandModel statusCommand)
            : base(name, triggers, GameCommandTypeEnum.Hangman)
        {
            this.MaxFailures = maxFailures;
            this.InitialAmount = initialAmount;
            this.AllowWordGuess = allowWordGuess;
            this.CustomWordsFilePath = customWordsFilePath;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
            this.GameWonCommand = gameWonCommand;
            this.GameLostCommand = gameLostCommand;
            this.StatusArgument = statusArgument;
            this.StatusCommand = statusCommand;
        }

        [Obsolete]
        public HangmanGameCommandModel() : base() { }

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

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            this.SetPrimaryCurrencyRequirementArgumentIndex(argumentIndex: 1);

            if (string.IsNullOrEmpty(this.currentWord) || this.failedGuesses.Count >= this.MaxFailures)
            {
                await this.ClearData();
            }

            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                this.AddSpecialIdentifiersToParameters(parameters);
                await this.RunSubCommand(this.StatusCommand, parameters);

                return new Result(success: false);
            }

            if (parameters.Arguments.Count == 0)
            {
                return new Result(MixItUp.Base.Resources.GameCommandHangmanMissingGuess);
            }

            if (!this.AllowWordGuess && parameters.Arguments[0].Length != 1)
            {
                return new Result(MixItUp.Base.Resources.GameCommandHangmanNotLetter);
            }

            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            this.totalAmount += this.GetPrimaryBetAmount(parameters);

            if (parameters.Arguments[0].Length > 1)
            {
                this.AddSpecialIdentifiersToParameters(parameters);
                if (string.Equals(this.currentWord, parameters.Arguments[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    await this.GameWon(parameters);
                }
                else
                {
                    await this.FailedGuess(parameters);
                }
            }
            else
            {
                char letter = parameters.Arguments.ElementAt(0).ToUpper().First();
                if (this.currentWord.Contains(letter))
                {
                    this.successfulGuesses.Add(letter);
                    this.AddSpecialIdentifiersToParameters(parameters);

                    if (this.currentWord.All(c => this.successfulGuesses.Contains(c)))
                    {
                        await this.GameWon(parameters);
                    }
                    else
                    {
                        await this.RunSubCommand(this.SuccessfulGuessCommand, parameters);
                    }
                }
                else
                {
                    this.failedGuesses.Add(letter);
                    await this.FailedGuess(parameters);
                }
            }

            await this.PerformCooldown(parameters);
        }

        private async Task GameWon(CommandParametersModel parameters)
        {
            this.PerformPrimarySetPayout(parameters.User, this.totalAmount);
            this.SetGameWinners(parameters, new List<CommandParametersModel>() { parameters });
            await this.RunSubCommand(this.GameWonCommand, parameters);
            await this.ClearData();
        }

        private async Task FailedGuess(CommandParametersModel parameters)
        {
            this.failedGuessesCount++;
            this.AddSpecialIdentifiersToParameters(parameters);

            if (this.failedGuessesCount >= this.MaxFailures)
            {
                await this.RunSubCommand(this.GameLostCommand, parameters);
                await this.ClearData();
            }
            else
            {
                await this.RunSubCommand(this.FailedGuessCommand, parameters);
            }
        }

        private async Task ClearData()
        {
            this.currentWord = await this.GetRandomWord(this.CustomWordsFilePath);
            this.successfulGuesses.Clear();
            this.failedGuesses.Clear();
            this.failedGuessesCount = 0;
            this.totalAmount = this.InitialAmount;
        }

        private void AddSpecialIdentifiersToParameters(CommandParametersModel parameters)
        {
            parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = this.totalAmount.ToString();
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.totalAmount.ToString();
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanAnswerSpecialIdentifier] = this.currentWord;
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanFailedGuessesSpecialIdentifier] = string.Join(", ", this.failedGuesses);
            List<string> currentLetters = new List<string>();
            foreach (char c in this.currentWord)
            {
                currentLetters.Add(this.successfulGuesses.Contains(c) ? c.ToString() : "_");
            }
            parameters.SpecialIdentifiers[HangmanGameCommandModel.GameHangmanCurrentSpecialIdentifier] = string.Join(" ", currentLetters);
        }
    }
}
