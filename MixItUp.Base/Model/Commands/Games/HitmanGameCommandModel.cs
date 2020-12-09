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
        public CustomCommandModel UserFailCommand { get; set; }

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
            CustomCommandModel hitmanAppearsCommand, CustomCommandModel userSuccessCommand, CustomCommandModel userFailCommand)
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
            this.UserFailCommand = userFailCommand;
        }

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
            commands.Add(this.UserFailCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await Task.Delay(this.TimeLimit * 1000);

                    this.runParameters.SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName = await this.GetRandomWord(this.CustomWordsFilePath);

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

                    await this.HitmanApproachingCommand.Perform(this.runParameters);

                    await Task.Delay(5000);

                    GlobalEvents.OnChatMessageReceived += GlobalEvents_OnChatMessageReceived;

                    await this.HitmanAppearsCommand.Perform(this.runParameters);

                    await Task.Delay(this.HitmanTimeLimit * 1000);

                    GlobalEvents.OnChatMessageReceived -= GlobalEvents_OnChatMessageReceived;

                    if (this.gameActive && !string.IsNullOrEmpty(this.runHitmanName))
                    {
                        this.UserFailCommand.Perform(this.runParameters);
                    }
                    this.gameActive = false;
                    await this.CooldownRequirement.Perform(this.runParameters);
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.gameActive = true;
                await this.StartedCommand.Perform(this.runParameters);
                await this.UserJoinCommand.Perform(this.runParameters);
                this.ResetCooldown();
                return;
            }
            else if (string.IsNullOrEmpty(this.runHitmanName) && !this.runUsers.ContainsKey(parameters.User))
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
            if (!string.IsNullOrEmpty(this.runHitmanName) && this.runUsers.ContainsKey(message.User) && string.Equals(this.runHitmanName, message.PlainTextMessage, StringComparison.CurrentCultureIgnoreCase))
            {
                this.gameActive = false;
                int payout = this.runBetAmount * this.runUsers.Count;
                this.PerformPayout(new CommandParametersModel(message.User), payout);

                this.runUsers[message.User].SpecialIdentifiers[HitmanGameCommandModel.GamePayoutSpecialIdentifier] = payout.ToString();
                this.runUsers[message.User].SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName;

                await this.CooldownRequirement.Perform(this.runParameters);
                this.ClearData();
                await this.UserSuccessCommand.Perform(this.runUsers[message.User]);
            }
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runHitmanName = null;
            this.runUsers.Clear();
        }
    }
}
