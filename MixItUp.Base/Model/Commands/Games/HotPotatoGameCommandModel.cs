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
        public int LowerLimit { get; set; }
        [DataMember]
        public int UpperLimit { get; set; }
        [DataMember]
        public bool AllowUserTargeting { get; set; }

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

        public HotPotatoGameCommandModel(string name, HashSet<string> triggers, int lowerLimit, int upperLimit, bool allowUserTargeting,
            CustomCommandModel startedCommand, CustomCommandModel tossPotatoCommand, CustomCommandModel potatoExplodeCommand)
            : base(name, triggers)
        {
            this.LowerLimit = lowerLimit;
            this.UpperLimit = upperLimit;
            this.AllowUserTargeting = allowUserTargeting;
            this.StartedCommand = startedCommand;
            this.TossPotatoCommand = tossPotatoCommand;
            this.PotatoExplodeCommand = potatoExplodeCommand;
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
            if (this.startParameters == null || (this.gameActive && this.lastTossParameters?.TargetUser == parameters.User))
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
                        this.gameActive = true;
                        this.startParameters = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        AsyncRunner.RunAsyncBackground(async (token) =>
                        {
                            await Task.Delay(1000 * RandomHelper.GenerateRandomNumber(this.LowerLimit, this.UpperLimit));

                            this.gameActive = false;
                            await this.PotatoExplodeCommand.Perform(this.lastTossParameters);

                            this.CooldownRequirement.Perform(this.startParameters);
                            this.ClearData();
                        }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        await this.StartedCommand.Perform(parameters);
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

        private void ClearData()
        {
            this.gameActive = false;
            this.startParameters = null;
            this.lastTossParameters = null;
        }
    }
}