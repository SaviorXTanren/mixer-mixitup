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
    public class WordScrambleGameCommand : GameCommandModelBase
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
        public CustomCommandModel UserFailCommand { get; set; }

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

        public WordScrambleGameCommand(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, int wordScrambleTimeLimit, string customWordsFilePath,
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
            this.UserFailCommand = userFailCommand;
        }

        private WordScrambleGameCommand() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.WordScramblePrepareCommand);
            commands.Add(this.WordScrambleBeginCommand);
            commands.Add(this.UserSuccessCommand);
            commands.Add(this.UserFailCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runWord == null)
            {
                this.runBetAmount = this.GetBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await Task.Delay(this.TimeLimit * 1000);

                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommand.GameWordScrambleAnswerSpecialIdentifier] = this.runWord = await this.GetRandomWord(this.CustomWordsFilePath);
                    this.runParameters.SpecialIdentifiers[WordScrambleGameCommand.GameWordScrambleWordSpecialIdentifier] = this.runWordScrambled = this.runWord.Shuffle();

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
                        this.UserFailCommand.Perform(this.runParameters);
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
            if (!string.IsNullOrEmpty(this.runWord) && this.runUsers.ContainsKey(message.User) && string.Equals(this.runWord, message.PlainTextMessage, StringComparison.CurrentCultureIgnoreCase))
            {
                int payout = this.runBetAmount * this.runUsers.Count;
                this.PerformPayout(new CommandParametersModel(message.User), payout);

                this.runUsers[message.User].SpecialIdentifiers[HitmanGameCommandModel.GamePayoutSpecialIdentifier] = payout.ToString();
                this.runUsers[message.User].SpecialIdentifiers[WordScrambleGameCommand.GameWordScrambleWordSpecialIdentifier] = this.runWordScrambled;
                this.runUsers[message.User].SpecialIdentifiers[WordScrambleGameCommand.GameWordScrambleAnswerSpecialIdentifier] = this.runWord;

                await this.CooldownRequirement.Perform(this.runParameters);
                this.ClearData();
                await this.UserSuccessCommand.Perform(this.runUsers[message.User]);
            }
        }

        private void ClearData()
        {
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runWord = null;
            this.runWordScrambled = null;
            this.runUsers.Clear();
        }
    }
}