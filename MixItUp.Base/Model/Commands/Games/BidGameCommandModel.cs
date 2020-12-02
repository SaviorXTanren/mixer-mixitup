using MixItUp.Base.Model.User;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class BidGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public UserRoleEnum StartRoleRequirement { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public CustomCommandModel GameCompleteCommand { get; set; }

        [JsonIgnore]
        private bool gameActive = false;
        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private CommandParametersModel lastBidParameters;
        [JsonIgnore]
        private int lastBidAmount;

        public BidGameCommandModel(string name, HashSet<string> triggers, UserRoleEnum startRoleRequirement, int timeLimit, CustomCommandModel startedCommand, CustomCommandModel userJoinCommand,
            CustomCommandModel notEnoughPlayersCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers)
        {
            this.StartRoleRequirement = startRoleRequirement;
            this.TimeLimit = timeLimit;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        private BidGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.gameActive)
            {
                return await base.ValidateRequirements(parameters);
            }
            else
            {
                if (parameters.User.HasPermissionsTo(this.StartRoleRequirement))
                {
                    this.gameActive = true;
                    this.runParameters = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await Task.Delay(this.TimeLimit * 1000);

                        if (this.lastBidParameters != null)
                        {
                            await this.GameCompleteCommand.Perform(this.lastBidParameters);
                        }
                        else
                        {
                            await this.NotEnoughPlayersCommand.Perform(this.runParameters);
                        }

                        this.CooldownRequirement.Perform(this.runParameters);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.StartedCommand.Perform(parameters);
                    return false;
                }
                await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.RoleErrorInsufficientRole, this.StartRoleRequirement));
            }
            return false;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            int betAmount = this.GetBetAmount(parameters);
            if (betAmount > this.lastBidAmount)
            {
                await this.Requirements.Refund(this.lastBidParameters);

                this.lastBidParameters = parameters;
                this.lastBidAmount = betAmount;

                await this.UserJoinCommand.Perform(parameters);
                this.ResetCooldown();
            }
            else
            {
                await ChannelSession.Services.Chat.SendMessage(string.Format(MixItUp.Base.Resources.GameCurrencyRequirementAmountGreaterThan, this.lastBidAmount, this.GameCurrencyRequirement.Currency.Name));
                await this.Requirements.Refund(parameters);
            }
        }

        private void ClearData()
        {
            this.gameActive = false;
            this.runParameters = null;
            this.lastBidParameters = null;
            this.lastBidAmount = 0;
        }
    }
}