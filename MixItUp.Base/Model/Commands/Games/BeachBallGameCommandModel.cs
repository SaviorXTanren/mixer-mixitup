using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class BeachBallGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }

        [DataMember]
        public CustomCommandModel BallHitCommand { get; set; }
        [DataMember]
        public CustomCommandModel BallMissedCommand { get; set; }

        [JsonIgnore]
        private CommandParametersModel startParameters;
        [JsonIgnore]
        private CommandParametersModel lastHitParameters;
        [JsonIgnore]
        private CancellationTokenSource lastHitCancellationTokenSource;

        public BeachBallGameCommandModel(string name, HashSet<string> triggers, int lowerLimit, int upperLimit, bool allowUserTargeting,
            CustomCommandModel startedCommand, CustomCommandModel ballHitCommand, CustomCommandModel ballMissedCommand)
            : base(name, triggers)
        {
            this.LowerLimit = lowerLimit;
            this.UpperLimit = upperLimit;
            this.AllowUserTargeting = allowUserTargeting;
            this.StartedCommand = startedCommand;
            this.BallHitCommand = ballHitCommand;
            this.BallMissedCommand = ballMissedCommand;
        }

        private BeachBallGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.BallHitCommand);
            commands.Add(this.BallMissedCommand);
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.startParameters == null || this.lastHitParameters.TargetUser == parameters.User)
            {
                if (this.AllowUserTargeting)
                {
                    await parameters.SetTargetUser();
                    if (parameters.TargetUser == parameters.User)
                    {
                        parameters.TargetUser = null;
                    }
                }

                if (parameters.TargetUser == null)
                {
                    parameters.TargetUser = this.GetRandomUser(parameters);
                }

                if (parameters.TargetUser != null)
                {
                    if (this.startParameters == null)
                    {
                        this.startParameters = parameters;
                        await this.StartedCommand.Perform(parameters);
                    }
                    this.lastHitParameters = parameters;

                    if (this.lastHitCancellationTokenSource != null)
                    {
                        this.lastHitCancellationTokenSource.Cancel();
                        this.lastHitCancellationTokenSource = null;
                    }
                    this.lastHitCancellationTokenSource = new CancellationTokenSource();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (token) =>
                    {
                        await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerLimit, this.UpperLimit));

                        if (!token.IsCancellationRequested)
                        {
                            await this.BallMissedCommand.Perform(this.lastHitParameters);
                            this.CooldownRequirement.Perform(this.startParameters);
                            this.ClearData();
                        }
                    }, this.lastHitCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

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

        private void ClearData()
        {
            this.startParameters = null;
            this.lastHitParameters = null;
            this.lastHitCancellationTokenSource = null;
        }
    }
}