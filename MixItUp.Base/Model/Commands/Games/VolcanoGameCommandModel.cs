using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class VolcanoGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public string StatusArgument { get; set; }

        [DataMember]
        public CustomCommandModel Stage1DepositCommand { get; set; }
        [DataMember]
        public CustomCommandModel Stage1StatusCommand { get; set; }

        [DataMember]
        public int Stage2MinimumAmount { get; set; }
        [DataMember]
        public CustomCommandModel Stage2DepositCommand { get; set; }
        [DataMember]
        public CustomCommandModel Stage2StatusCommand { get; set; }

        [DataMember]
        public int Stage3MinimumAmount { get; set; }
        [DataMember]
        public CustomCommandModel Stage3DepositCommand { get; set; }
        [DataMember]
        public CustomCommandModel Stage3StatusCommand { get; set; }

        [DataMember]
        public int PayoutProbability { get; set; }
        [DataMember]
        public double PayoutMinimumPercentage { get; set; }
        [DataMember]
        public double PayoutMaximumPercentage { get; set; }
        [DataMember]
        public CustomCommandModel PayoutCommand { get; set; }

        [DataMember]
        public string CollectArgument { get; set; }
        [DataMember]
        public int CollectTimeLimit { get; set; }
        [DataMember]
        public double CollectMinimumPercentage { get; set; }
        [DataMember]
        public double CollectMaximumPercentage { get; set; }
        [DataMember]
        public CustomCommandModel CollectCommand { get; set; }

        [DataMember]
        public int TotalAmount { get; set; }

        [JsonIgnore]
        private bool collectActive = false;
        [JsonIgnore]
        private HashSet<UserViewModel> collectUsers = new HashSet<UserViewModel>();

        public VolcanoGameCommandModel(string name, HashSet<string> triggers, string statusArgument, CustomCommandModel stage1DepositCommand, CustomCommandModel stage1StatusCommand,
            int stage2MinimumAmount, CustomCommandModel stage2DepositCommand, CustomCommandModel stage2StatusCommand,
            int stage3MinimumAmount, CustomCommandModel stage3DepositCommand, CustomCommandModel stage3StatusCommand,
            int payoutProbability, double payoutMinimumPercentage, double payoutMaximumPercentage, CustomCommandModel payoutCommand,
            string collectArgument, int collectTimeLimit, double collectMinimumPercentage, double collectMaximumPercentage, CustomCommandModel collectCommand)
            : base(name, triggers, GameCommandTypeEnum.Volcano)
        {
            this.StatusArgument = statusArgument;
            this.Stage1DepositCommand = stage1DepositCommand;
            this.Stage1StatusCommand = stage1StatusCommand;
            this.Stage2MinimumAmount = stage2MinimumAmount;
            this.Stage2DepositCommand = stage2DepositCommand;
            this.Stage2StatusCommand = stage2StatusCommand;
            this.Stage3MinimumAmount = stage3MinimumAmount;
            this.Stage3DepositCommand = stage3DepositCommand;
            this.Stage3StatusCommand = stage3StatusCommand;
            this.PayoutProbability = payoutProbability;
            this.PayoutMinimumPercentage = payoutMinimumPercentage;
            this.PayoutMaximumPercentage = payoutMaximumPercentage;
            this.PayoutCommand = payoutCommand;
            this.CollectArgument = collectArgument;
            this.CollectTimeLimit = collectTimeLimit;
            this.CollectMinimumPercentage = collectMinimumPercentage;
            this.CollectMaximumPercentage = collectMaximumPercentage;
            this.CollectCommand = collectCommand;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal VolcanoGameCommandModel(Base.Commands.VolcanoGameCommand command)
            : base(command, GameCommandTypeEnum.Volcano)
        {
            this.StatusArgument = command.StatusArgument;
            this.Stage1DepositCommand = new CustomCommandModel(command.Stage1DepositCommand) { IsEmbedded = true };
            this.Stage1StatusCommand = new CustomCommandModel(command.Stage1StatusCommand) { IsEmbedded = true };
            this.Stage2MinimumAmount = command.Stage2MinimumAmount;
            this.Stage2DepositCommand = new CustomCommandModel(command.Stage2DepositCommand) { IsEmbedded = true };
            this.Stage2StatusCommand = new CustomCommandModel(command.Stage2StatusCommand) { IsEmbedded = true };
            this.Stage3MinimumAmount = command.Stage3MinimumAmount;
            this.Stage3DepositCommand = new CustomCommandModel(command.Stage3DepositCommand) { IsEmbedded = true };
            this.Stage3StatusCommand = new CustomCommandModel(command.Stage3StatusCommand) { IsEmbedded = true };
            this.PayoutProbability = command.PayoutProbability;
            this.PayoutMinimumPercentage = command.PayoutPercentageMinimum;
            this.PayoutMaximumPercentage = command.PayoutPercentageMaximum;
            this.PayoutCommand = new CustomCommandModel(command.PayoutCommand) { IsEmbedded = true };
            this.CollectArgument = command.CollectArgument;
            this.CollectTimeLimit = command.CollectTimeLimit;
            this.CollectMinimumPercentage = command.CollectPayoutPercentageMinimum;
            this.CollectMaximumPercentage = command.CollectPayoutPercentageMaximum;
            this.CollectCommand = new CustomCommandModel(command.CollectArgument) { IsEmbedded = true };
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private VolcanoGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.Stage1DepositCommand);
            commands.Add(this.Stage1StatusCommand);
            commands.Add(this.Stage2DepositCommand);
            commands.Add(this.Stage2StatusCommand);
            commands.Add(this.Stage3DepositCommand);
            commands.Add(this.Stage3StatusCommand);
            commands.Add(this.PayoutCommand);
            commands.Add(this.CollectCommand);
            return commands;
        }

        protected override async Task<bool> ValidateRequirements(CommandParametersModel parameters)
        {
            if (this.collectActive)
            {
                if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.CollectArgument, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!this.collectUsers.Contains(parameters.User))
                    {
                        this.collectUsers.Add(parameters.User);
                        int payout = this.GenerateRandomNumber(this.TotalAmount, this.CollectMinimumPercentage, this.CollectMaximumPercentage);
                        this.PerformPrimarySetPayout(parameters.User, payout);

                        parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();
                        await this.CollectCommand.Perform(parameters);
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandVolcanoAlreadyCollected);
                    }
                }
                else
                {
                    await ChannelSession.Services.Chat.SendMessage(MixItUp.Base.Resources.GameCommandVolcanoCollectUnderway);
                }
                return false;
            }
            else if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                if (this.TotalAmount >= this.Stage3MinimumAmount)
                {
                    await this.Stage3StatusCommand.Perform(parameters);
                }
                else if (this.TotalAmount >= this.Stage2MinimumAmount)
                {
                    await this.Stage2StatusCommand.Perform(parameters);
                }
                else
                {
                    await this.Stage1StatusCommand.Perform(parameters);
                }
                return false;
            }
            else
            {
                return await base.ValidateRequirements(parameters);
            }
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            this.TotalAmount += this.GetPrimaryBetAmount(parameters);
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();

            if (this.TotalAmount >= this.Stage3MinimumAmount)
            {
                if (this.GenerateProbability() <= this.PayoutProbability)
                {
                    this.collectUsers.Add(parameters.User);
                    int payout = this.GenerateRandomNumber(this.TotalAmount, this.PayoutMinimumPercentage, this.PayoutMaximumPercentage);
                    this.PerformPrimarySetPayout(parameters.User, payout);
                    parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        this.collectActive = true;
                        await Task.Delay(this.CollectTimeLimit * 1000);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.PayoutCommand.Perform(parameters);
                }
                else
                {
                    await this.Stage3DepositCommand.Perform(parameters);
                }
            }
            else if (this.TotalAmount >= this.Stage2MinimumAmount)
            {
                await this.Stage2DepositCommand.Perform(parameters);
            }
            else
            {
                await this.Stage1DepositCommand.Perform(parameters);
            }
        }

        private void ClearData()
        {
            this.TotalAmount = 0;
            this.collectActive = false;
            this.collectUsers.Clear();
        }
    }
}
