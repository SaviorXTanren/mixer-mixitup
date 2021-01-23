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
    public class HitmanGameCommandModel : GameCommandModelBase
    {
        public const string GameHitmanNameSpecialIdentifier = "gamehitmanname";

        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public int HitmanTimeLimit { get; set; }

        [DataMember]
        public string CustomWordsFilePath { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public CustomCommandModel HitmanApproachingCommand { get; set; }
        [DataMember]
        public CustomCommandModel HitmanAppearsCommand { get; set; }

        [DataMember]
        public CustomCommandModel UserSuccessCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserFailureCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private int runBetAmount;
        [JsonIgnore]
        private Dictionary<UserViewModel, CommandParametersModel> runUsers = new Dictionary<UserViewModel, CommandParametersModel>();
        [JsonIgnore]
        private string runHitmanName;

        public HitmanGameCommandModel(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, int hitmanTimeLimit, string customWordsFilePath,
            CustomCommandModel startedCommand, CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, CustomCommandModel hitmanApproachingCommand,
            CustomCommandModel hitmanAppearsCommand, CustomCommandModel userSuccessCommand, CustomCommandModel userFailureCommand)
            : base(name, triggers, GameCommandTypeEnum.Hitman)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.HitmanTimeLimit = hitmanTimeLimit;
            this.CustomWordsFilePath = customWordsFilePath;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.HitmanApproachingCommand = hitmanApproachingCommand;
            this.HitmanAppearsCommand = hitmanAppearsCommand;
            this.UserSuccessCommand = userSuccessCommand;
            this.UserFailureCommand = userFailureCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal HitmanGameCommandModel(Base.Commands.HitmanGameCommand command)
            : base(command, GameCommandTypeEnum.Hitman)
        {
            this.MinimumParticipants = command.MinimumParticipants;
            this.TimeLimit = command.TimeLimit;
            this.HitmanTimeLimit = command.HitmanTimeLimit;
            this.CustomWordsFilePath = command.CustomWordsFilePath;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.UserJoinCommand = new CustomCommandModel(command.UserJoinCommand) { IsEmbedded = true };
            this.NotEnoughPlayersCommand = new CustomCommandModel(command.NotEnoughPlayersCommand) { IsEmbedded = true };
            this.HitmanApproachingCommand = new CustomCommandModel(command.HitmanApproachingCommand) { IsEmbedded = true };
            this.HitmanAppearsCommand = new CustomCommandModel(command.HitmanAppearsCommand) { IsEmbedded = true };
            this.UserSuccessCommand = new CustomCommandModel(command.UserSuccessOutcome.Command) { IsEmbedded = true };
            this.UserFailureCommand = new CustomCommandModel(command.UserFailOutcome.Command) { IsEmbedded = true };
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private HitmanGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.HitmanApproachingCommand);
            commands.Add(this.HitmanAppearsCommand);
            commands.Add(this.UserSuccessCommand);
            commands.Add(this.UserFailureCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;

                this.GetPrimaryCurrencyRequirement().SetTemporaryAmount(this.runBetAmount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                    this.runParameters.SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName = await this.GetRandomWord(this.CustomWordsFilePath);

                    if (this.runUsers.Count < this.MinimumParticipants)
                    {
                        await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                        foreach (var kvp in this.runUsers.ToList())
                        {
                            await this.Requirements.Refund(kvp.Value);
                        }
                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                        return;
                    }

                    await this.HitmanApproachingCommand.Perform(this.runParameters);

                    await DelayNoThrow(5000, cancellationToken);

                    GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;

                    await this.HitmanAppearsCommand.Perform(this.runParameters);

                    for (int i = 0; i < this.HitmanTimeLimit && this.gameActive; i++)
                    {
                        await DelayNoThrow(1000, cancellationToken);
                    }

                    GlobalEvents.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;

                    if (this.gameActive && !string.IsNullOrEmpty(this.runHitmanName))
                    {
                        this.UserFailureCommand.Perform(this.runParameters);
                        await this.PerformCooldown(this.runParameters);
                    }
                    this.gameActive = false;
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.gameActive = true;
                await this.StartedCommand.Perform(this.runParameters);
                await this.UserJoinCommand.Perform(this.runParameters);
                return;
            }
            else if (string.IsNullOrEmpty(this.runHitmanName) && !this.runUsers.ContainsKey(parameters.User))
            {
                this.runUsers[parameters.User] = parameters;
                await this.UserJoinCommand.Perform(parameters);
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
                if (!string.IsNullOrEmpty(this.runHitmanName) && this.runUsers.ContainsKey(message.User) && string.Equals(this.runHitmanName, message.PlainTextMessage, StringComparison.CurrentCultureIgnoreCase))
                {
                    CommandParametersModel winner = this.runUsers[message.User];

                    this.gameActive = false;
                    int payout = this.runBetAmount * this.runUsers.Count;
                    this.PerformPrimarySetPayout(message.User, payout);

                    winner.SpecialIdentifiers[HitmanGameCommandModel.GamePayoutSpecialIdentifier] = payout.ToString();
                    winner.SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName;

                    await this.PerformCooldown(this.runParameters);
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
            this.gameActive = false;
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runHitmanName = null;
            this.runUsers.Clear();
            this.GetPrimaryCurrencyRequirement().ResetTemporaryAmount();
        }
    }
}
