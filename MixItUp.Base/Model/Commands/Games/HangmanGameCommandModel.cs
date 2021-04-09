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

        public HangmanGameCommandModel(string name, HashSet<string> triggers, int maxFailures, int initialAmount, string customWordsFilePath, CustomCommandModel successfulGuessCommand, CustomCommandModel failedGuessCommand,
            CustomCommandModel gameWonCommand, CustomCommandModel gameLostCommand, string statusArgument, CustomCommandModel statusCommand)
            : base(name, triggers, GameCommandTypeEnum.Hangman)
        {
            this.MaxFailures = maxFailures;
            this.InitialAmount = initialAmount;
            this.CustomWordsFilePath = customWordsFilePath;
            this.SuccessfulGuessCommand = successfulGuessCommand;
            this.FailedGuessCommand = failedGuessCommand;
            this.GameWonCommand = gameWonCommand;
            this.GameLostCommand = gameLostCommand;
            this.StatusArgument = statusArgument;
            this.StatusCommand = statusCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal HangmanGameCommandModel(Base.Commands.HangmanGameCommand command)
            : base(command, GameCommandTypeEnum.Hangman)
        {
            this.MaxFailures = command.MaxFailures;
            this.InitialAmount = command.InitialAmount;
            this.CustomWordsFilePath = command.CustomWordsFilePath;
            this.SuccessfulGuessCommand = new CustomCommandModel(command.SuccessfulGuessCommand) { IsEmbedded = true };
            this.FailedGuessCommand = new CustomCommandModel(command.FailedGuessCommand) { IsEmbedded = true };
            this.GameWonCommand = new CustomCommandModel(command.GameWonCommand) { IsEmbedded = true };
            this.GameLostCommand = new CustomCommandModel(command.GameLostCommand) { IsEmbedded = true };
            this.StatusArgument = command.StatusArgument;
            this.StatusCommand = new CustomCommandModel(command.StatusArgument) { IsEmbedded = true };
        }
#pragma warning restore CS0612 // Type or member is obsolete

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

            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            if (parameters.Arguments.Count == 1 && parameters.Arguments[0].Length == 1)
            {
                this.totalAmount += this.GetPrimaryBetAmount(parameters);
                char letter = parameters.Arguments.ElementAt(0).ToUpper().First();
                if (this.currentWord.Contains(letter))
                {
                    this.successfulGuesses.Add(letter);
                    this.AddSpecialIdentifiersToParameters(parameters);

                    if (this.currentWord.All(c => this.successfulGuesses.Contains(c)))
                    {
                        this.PerformPrimarySetPayout(parameters.User, this.totalAmount);
                        await this.RunSubCommand(this.GameWonCommand, parameters);
                        await this.ClearData();
                    }
                    else
                    {
                        await this.RunSubCommand(this.SuccessfulGuessCommand, parameters);
                    }
                }
                else
                {
                    this.failedGuesses.Add(letter);
                    this.AddSpecialIdentifiersToParameters(parameters);

                    if (this.failedGuesses.Count >= this.MaxFailures)
                    {
                        await this.RunSubCommand(this.GameLostCommand, parameters);
                        await this.ClearData();
                    }
                    else
                    {
                        await this.RunSubCommand(this.FailedGuessCommand, parameters);
                    }
                }
                await this.PerformCooldown(parameters);
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
            this.currentWord = await this.GetRandomWord(this.CustomWordsFilePath);
            this.successfulGuesses.Clear();
            this.failedGuesses.Clear();
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
