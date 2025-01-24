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
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();
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

        [Obsolete]
        public HitmanGameCommandModel() : base() { }

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

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);
            if (this.runParameters == null)
            {
                this.runBetAmount = this.GetPrimaryBetAmount(parameters);
                this.runParameters = parameters;
                this.runUsers[parameters.User] = parameters;

                this.GetPrimaryCurrencyRequirement()?.SetTemporaryAmount(this.runBetAmount);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                {
                    await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

                    this.runParameters.SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName = await this.GetRandomWord(this.CustomWordsFilePath);

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

                    await this.RunSubCommand(this.HitmanApproachingCommand, this.runParameters);

                    await DelayNoThrow(5000, cancellationToken);

                    ChatService.OnChatMessageReceived += ChatService_OnChatMessageReceived;

                    await this.RunSubCommand(this.HitmanAppearsCommand, this.runParameters);

                    for (int i = 0; i < this.HitmanTimeLimit && this.gameActive; i++)
                    {
                        await DelayNoThrow(1000, cancellationToken);
                    }

                    ChatService.OnChatMessageReceived -= ChatService_OnChatMessageReceived;

                    if (this.gameActive && !string.IsNullOrEmpty(this.runHitmanName))
                    {
                        await this.RunSubCommand(this.UserFailureCommand, this.runParameters);
                        await this.PerformCooldown(this.runParameters);
                    }
                    this.gameActive = false;
                    this.ClearData();
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.gameActive = true;
                await this.RunSubCommand(this.StartedCommand, this.runParameters);
                await this.RunSubCommand(this.UserJoinCommand, this.runParameters);
                return;
            }
            else if (string.IsNullOrEmpty(this.runHitmanName) && !this.runUsers.ContainsKey(parameters.User))
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
                if (!string.IsNullOrEmpty(this.runHitmanName) && this.runUsers.ContainsKey(message.User) && string.Equals(this.runHitmanName, message.PlainTextMessage, StringComparison.CurrentCultureIgnoreCase))
                {
                    CommandParametersModel winner = this.runUsers[message.User];

                    this.gameActive = false;
                    int payout = this.runBetAmount * this.runUsers.Count;
                    this.PerformPrimarySetPayout(message.User, payout);

                    winner.SpecialIdentifiers[HitmanGameCommandModel.GamePayoutSpecialIdentifier] = payout.ToString();
                    winner.SpecialIdentifiers[HitmanGameCommandModel.GameHitmanNameSpecialIdentifier] = this.runHitmanName;
                    this.SetGameWinners(this.runParameters, new List<CommandParametersModel>() { this.runParameters });

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
            this.gameActive = false;
            this.runParameters = null;
            this.runBetAmount = 0;
            this.runHitmanName = null;
            this.runUsers.Clear();
            this.GetPrimaryCurrencyRequirement()?.ResetTemporaryAmount();
        }
    }
}
