using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class HotPotatoGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public int LowerTimeLimit { get; set; }
        [DataMember]
        public int UpperTimeLimit { get; set; }
        [DataMember]
        public bool ResetTimeOnToss { get; set; }

        [DataMember]
        public GamePlayerSelectionType PlayerSelectionType { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }

        [DataMember]
        public CustomCommandModel TossPotatoCommand { get; set; }
        [DataMember]
        public CustomCommandModel PotatoExplodeCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel startParameters;
        [JsonIgnore]
        private CommandParametersModel lastTossParameters;
        [JsonIgnore]
        private CancellationTokenSource lastHitCancellationTokenSource;

        public HotPotatoGameCommandModel(string name, HashSet<string> triggers, int lowerTimeLimit, int upperTimeLimit, bool resetTimeOnToss, GamePlayerSelectionType playerSelectionType,
            CustomCommandModel startedCommand, CustomCommandModel tossPotatoCommand, CustomCommandModel potatoExplodeCommand)
            : base(name, triggers, GameCommandTypeEnum.HotPotato)
        {
            this.LowerTimeLimit = lowerTimeLimit;
            this.UpperTimeLimit = upperTimeLimit;
            this.ResetTimeOnToss = resetTimeOnToss;
            this.PlayerSelectionType = playerSelectionType;
            this.StartedCommand = startedCommand;
            this.TossPotatoCommand = tossPotatoCommand;
            this.PotatoExplodeCommand = potatoExplodeCommand;
        }

        internal HotPotatoGameCommandModel(Base.Commands.BeachBallGameCommand command)
            : base(command, GameCommandTypeEnum.HotPotato)
        {
            this.LowerTimeLimit = command.LowerLimit;
            this.UpperTimeLimit = command.UpperLimit;
            this.ResetTimeOnToss = true;
            this.PlayerSelectionType = GamePlayerSelectionType.Targeted;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.TossPotatoCommand = new CustomCommandModel(command.BallHitCommand) { IsEmbedded = true };
            this.PotatoExplodeCommand = new CustomCommandModel(command.BallMissedCommand) { IsEmbedded = true };
        }

        internal HotPotatoGameCommandModel(Base.Commands.HotPotatoGameCommand command)
            : base(command, GameCommandTypeEnum.HotPotato)
        {
            this.LowerTimeLimit = command.LowerLimit;
            this.UpperTimeLimit = command.UpperLimit;
            this.ResetTimeOnToss = false;
            this.PlayerSelectionType = GamePlayerSelectionType.Targeted;
            this.StartedCommand = new CustomCommandModel(command.StartedCommand) { IsEmbedded = true };
            this.TossPotatoCommand = new CustomCommandModel(command.TossPotatoCommand) { IsEmbedded = true };
            this.PotatoExplodeCommand = new CustomCommandModel(command.PotatoExplodeCommand) { IsEmbedded = true };
        }

        private HotPotatoGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.TossPotatoCommand);
            commands.Add(this.PotatoExplodeCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.startParameters == null || this.gameActive)
            {
                await this.SetSelectedUser(this.PlayerSelectionType, parameters);
                if (parameters.TargetUser != null && this.lastTossParameters?.TargetUser == parameters.User)
                {
                    if (this.startParameters == null)
                    {
                        this.gameActive = true;
                        this.startParameters = parameters;

                        if (this.ResetTimeOnToss)
                        {
                            this.RestartTossTime();
                        }
                        else
                        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            AsyncRunner.RunAsyncBackground(async (token) =>
                            {
                                await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerTimeLimit, this.UpperTimeLimit));

                                this.gameActive = false;
                                await this.PotatoExplodeCommand.Perform(this.lastTossParameters);

                                this.CooldownRequirement.Perform(this.startParameters);
                                this.ClearData();
                            }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                        await this.StartedCommand.Perform(parameters);
                    }
                    else
                    {
                        if (this.ResetTimeOnToss)
                        {
                            this.RestartTossTime();
                        }
                        await this.TossPotatoCommand.Perform(parameters);
                    }

                    this.lastTossParameters = parameters;
                    this.ResetCooldown();
                    return;
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser);
                }
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway);
            }
            await this.Requirements.Refund(parameters);
        }

        private void RestartTossTime()
        {
            if (this.lastHitCancellationTokenSource != null)
            {
                this.lastHitCancellationTokenSource.Cancel();
                this.lastHitCancellationTokenSource = null;
            }
            this.lastHitCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AsyncRunner.RunAsyncBackground(async (token) =>
            {
                await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerTimeLimit, this.UpperTimeLimit));

                if (this.gameActive && !token.IsCancellationRequested)
                {
                    this.gameActive = false;
                    await this.PotatoExplodeCommand.Perform(this.lastTossParameters);
                    this.CooldownRequirement.Perform(this.startParameters);
                    this.ClearData();
                }
            }, this.lastHitCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.startParameters = null;
            this.lastTossParameters = null;
        }
    }
}