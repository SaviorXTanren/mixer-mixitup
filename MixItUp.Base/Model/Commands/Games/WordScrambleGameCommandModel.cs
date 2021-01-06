using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class WordScrambleGameCommandModel : GameCommandModelBase
    {
        public const string GameWordScrambleWordSpecialIdentifier = "gamewordscrambleword";
        public const string GameWordScrambleAnswerSpecialIdentifier = "gamewordscrambleanswer";

        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public int WordScrambleTimeLimit { get; set; }

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public CustomCommandModel WordScramblePrepareCommand { get; set; }
        [DataMember]
        public CustomCommandModel WordScrambleBeginCommand { get; set; }

        [DataMember]
        public CustomCommandModel UserSuccessCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserFailureCommand { get; set; }

        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private int runBetAmount;
        [JsonIgnore]
        private Dictionary<UserViewModel, CommandParametersModel> runUsers = new Dictionary<UserViewModel, CommandParametersModel>();
        [JsonIgnore]
        private string runWord;
        [JsonIgnore]
        private string runWordScrambled;

        public WordScrambleGameCommandModel(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, int wordScrambleTimeLimit, string customWordsFilePath,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand,
            CustomCommandModel wordScramblePrepareCommand, CustomCommandModel wordScrambleBeginCommand, CustomCommandModel userSuccessCommand, CustomCommandModel userFailCommand)
            : base(name, triggers, GameCommandTypeEnum.WordScramble)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.WordScrambleTimeLimit = wordScrambleTimeLimit;
            this.CustomWordsFilePath = customWordsFilePath;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.WordScramblePrepareCommand = wordScramblePrepareCommand;
            this.WordScrambleBeginCommand = wordScrambleBeginCommand;
            this.UserSuccessCommand = userSuccessCommand;
            this.UserFailureCommand = userFailCommand;
        }

        internal WordScrambleGameCommandModel(Base.Commands.WordScrambleGameCommand command)
            : base(command, GameCommandTypeEnum.WordScramble)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.WordScrambleTimeLimit = command.WordScrambleTimeLimit;
            this.CustomWordsFilePath = command.CustomWordsFilePath;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.UserJoinCommand = new CustomCommandModel(command.UserJoinCommand) { IsEmbedded = true };
            this.NotEnoughPlayersCommand = new CustomCommandModel(command.NotEnoughPlayersCommand) { IsEmbedded = true };
            this.WordScramblePrepareCommand = new CustomCommandModel(command.WordScramblePrepareCommand) { IsEmbedded = true };
            this.WordScrambleBeginCommand = new CustomCommandModel(command.WordScrambleBeginCommand) { IsEmbedded = true };
            this.UserSuccessCommand = new CustomCommandModel(command.UserSuccessOutcome.Command) { IsEmbedded = true };
            this.UserFailureCommand = new CustomCommandModel(command.UserFailOutcome.Command) { IsEmbedded = true };
        }

        private WordScrambleGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.WordScramblePrepareCommand);
            commands.Add(this.WordScrambleBeginCommand);
            commands.Add(this.UserSuccessCommand);
            commands.Add(this.UserFailureCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runWord == null)
            {
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;
                this.GetPrimaryCurrencyRequirement().SetTemporaryAmount(this.runBetAmount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await Task.Delay(this.TimeLimit * 1000);

                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleAnswerSpecialIdentifier] = this.runWord = await this.GetRandomWord(this.CustomWordsFilePath);
                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleWordSpecialIdentifier] = this.runWordScrambled = this.runWord.Shuffle();

                    if (this.runUsers.Count < this.MinimumParticipants)
                    {
                        await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                        foreach (var kvp in this.runUsers.ToList())
                        {
                            await this.Requirements.Refund(kvp.Value);
                        }
                        await this.CooldownRequirement.Perform(this.runParameters);
                        this.ClearData();
                        return;
                    }

                    await this.WordScramblePrepareCommand.Perform(this.runParameters);

                    await Task.Delay(5000);

                    GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;

                    await this.WordScrambleBeginCommand.Perform(this.runParameters);

                    await Task.Delay(this.WordScrambleTimeLimit * 1000);

                    GlobalEvents.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;

                    if (!string.IsNullOrEmpty(this.runWord))
                    {
                        this.UserFailureCommand.Perform(this.runParameters);
                    }
                    await this.CooldownRequirement.Perform(this.runParameters);
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                await this.StartedCommand.Perform(this.runParameters);
                await this.UserJoinCommand.Perform(this.runParameters);
                return;
            }
            else if (string.IsNullOrEmpty(this.runWord) && !this.runUsers.ContainsKey(parameters.User))
            {
                this.runUsers[parameters.User] = parameters;
                await this.UserJoinCommand.Perform(this.runParameters);
                this.ResetCooldown();
                return;
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
            }
            await this.Requirements.Refund(parameters);
        }

        private async void GlobalEvents_OnChatMessageReceived(object sender, ViewModel.Chat.ChatMessageViewModel message)
        {
            try
            {
                if (!string.IsNullOrEmpty(this.runWord) && this.runUsers.ContainsKey(message.User) && string.Equals(this.runWord, message.PlainTextMessage, StringComparison.CurrentCultureIgnoreCase))
                {
                    CommandParametersModel winner = this.runUsers[message.User];

                    int payout = this.runBetAmount * this.runUsers.Count;
                    this.PerformPrimarySetPayout(message.User, payout);

                    winner.SpecialIdentifiers[HitmanGameCommandModel.GamePayoutSpecialIdentifier] = payout.ToString();
                    winner.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleWordSpecialIdentifier] = this.runWordScrambled;
                    winner.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleAnswerSpecialIdentifier] = this.runWord;

                    await this.CooldownRequirement.Perform(this.runParameters);
                    this.ClearData();
                    await this.UserSuccessCommand.Perform(winner);
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
            this.runBetAmount = 0;
            this.runWord = null;
            this.runWordScrambled = null;
            this.runUsers.Clear();
            this.GetPrimaryCurrencyRequirement().ResetTemporaryAmount();
        }
    }
}