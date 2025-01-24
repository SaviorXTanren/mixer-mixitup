using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
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
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();
        [JsonIgnore]
        private string runWord;
        [JsonIgnore]
        private string runWordScrambled;
        [JsonIgnore]
        private CancellationTokenSource runCancellationTokenSource;

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

        [Obsolete]
        public WordScrambleGameCommandModel() : base() { }

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

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            if (string.IsNullOrEmpty(this.runWord))
            {
                this.runWord = await this.GetRandomWord(this.CustomWordsFilePath);
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;
                this.GetPrimaryCurrencyRequirement()?.SetTemporaryAmount(this.runBetAmount);
                this.runCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);
                    if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleAnswerSpecialIdentifier] = this.runWord;
                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommandModel.GameWordScrambleWordSpecialIdentifier] = this.runWordScrambled = this.runWord.Shuffle();

                    if (this.runUsers.Count < this.MinimumParticipants)
                    {
                        await this.RunSubCommand(this.NotEnoughPlayersCommand, this.runParameters);
                        foreach (var kvp in this.runUsers.ToList())
                        {
                            await this.Requirements.Refund(kvp.Value);
                        }
                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                        return;
                    }

                    await this.RunSubCommand(this.WordScramblePrepareCommand, this.runParameters);

                    await DelayNoThrow(5000, cancellationToken);
                    if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;

                    await this.RunSubCommand(this.WordScrambleBeginCommand, this.runParameters);

                    await DelayNoThrow(this.WordScrambleTimeLimit * 1000, cancellationToken);
                    if (cancellationToken != null && cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;

                    if (!string.IsNullOrEmpty(this.runWord))
                    {
                        await this.RunSubCommand(this.UserFailureCommand, this.runParameters);
                        await this.PerformCooldown(this.runParameters);
                    }
                    this.ClearData();
                }, this.runCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                await this.RunSubCommand(this.StartedCommand, this.runParameters);
                await this.RunSubCommand(this.UserJoinCommand, this.runParameters);
                return;
            }
            else if (!this.runUsers.ContainsKey(parameters.User))
            {
                this.runUsers[parameters.User] = parameters;
                await this.RunSubCommand(this.UserJoinCommand, parameters);
                return;
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway, parameters);
            }
            await this.Requirements.Refund(parameters);
        }

        private async void ChatService_OnChatMessageReceived(object sender, ViewModel.Chat.ChatMessageViewModel message)
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
                    this.SetGameWinners(winner, new List<CommandParametersModel>() { winner });

                    await this.PerformCooldown(this.runParameters);
                    this.ClearData();
                    await this.RunSubCommand(this.UserSuccessCommand, winner);
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
            this.GetPrimaryCurrencyRequirement()?.ResetTemporaryAmount();
            try
            {
                if (this.runCancellationTokenSource != null)
                {
                    this.runCancellationTokenSource.Cancel();
                }
            }
            catch (Exception) { }
            this.runCancellationTokenSource = null;
        }
    }
}