using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System;
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

        [Obsolete]
        public HotPotatoGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.TossPotatoCommand);
            commands.Add(this.PotatoExplodeCommand);
            return commands;
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            var lastTargetUser = this.lastTossParameters?.TargetUser;
            if (this.gameActive && lastTargetUser != parameters.User)
            {
                Logger.ForceLog(LogLevel.Information, $"User trying to trigger Hot Potato game isn't the currently targeted user: {parameters.User} - {lastTargetUser}");

                // The game is underway and it's not the user's turn
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway, parameters);
                await this.Requirements.Refund(parameters);
                return;
            }

            await this.SetSelectedUser(this.PlayerSelectionType, parameters);
            if (parameters.TargetUser != null && !parameters.IsTargetUserSelf)
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
                            await DelayNoThrow(1000 * RandomHelper.GenerateRandomNumber(this.LowerTimeLimit, this.UpperTimeLimit), token);

                            this.gameActive = false;
                            this.SetGameWinners(this.lastTossParameters, new List<CommandParametersModel>() { this.lastTossParameters });
                            await this.RunSubCommand(this.PotatoExplodeCommand, this.lastTossParameters);

                            await this.PerformCooldown(this.startParameters);
                            this.ClearData();
                        }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                    await this.RunSubCommand(this.StartedCommand, parameters);
                }
                else
                {
                    if (this.ResetTimeOnToss)
                    {
                        this.RestartTossTime();
                    }
                    await this.RunSubCommand(this.TossPotatoCommand, parameters);
                }

                this.lastTossParameters = parameters;
                return;
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandCouldNotFindUser, parameters);
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
                await DelayNoThrow(1000 * RandomHelper.GenerateRandomNumber(this.LowerTimeLimit, this.UpperTimeLimit), token);

                if (this.gameActive && !token.IsCancellationRequested)
                {
                    this.gameActive = false;
                    this.SetGameWinners(this.lastTossParameters, new List<CommandParametersModel>() { this.lastTossParameters });
                    await this.RunSubCommand(this.PotatoExplodeCommand, this.lastTossParameters);
                    await this.PerformCooldown(this.lastTossParameters);
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