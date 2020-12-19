using MixItUp.Base.Util;
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

        [DataMember]
        public string CurrentWord { get; set; }
        [DataMember]
        public int TotalAmount { get; set; }
        [DataMember]
        public HashSet<char> SuccessfulGuesses { get; set; } = new HashSet<char>();
        [DataMember]
        public HashSet<char> FailedGuesses { get; set; } = new HashSet<char>();

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
            if (string.IsNullOrEmpty(this.CurrentWord) || this.FailedGuesses.Count >= this.MaxFailures)
            {
                await this.ClearData();
            }

            if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
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
                        this.PerformPayout(parameters, this.TotalAmount);
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
            this.CurrentWord = await this.GetRandomWord(this.CustomWordsFilePath);
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
