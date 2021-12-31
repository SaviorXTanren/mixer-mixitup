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
        private HashSet<UserV2ViewModel> collectUsers = new HashSet<UserV2ViewModel>();

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

        [Obsolete]
        public VolcanoGameCommandModel() : base() { }

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

        public override async Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            if (this.collectActive)
            {
                if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.CollectArgument, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (!this.collectUsers.Contains(parameters.User))
                    {
                        this.collectUsers.Add(parameters.User);
                        int payout = this.GenerateRandomNumber(this.TotalAmount, this.CollectMinimumPercentage / 100.0d, this.CollectMaximumPercentage / 100.0d);
                        this.PerformPrimarySetPayout(parameters.User, payout);

                        parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();
                        this.SetGameWinners(parameters, new List<CommandParametersModel>() { parameters });
                        await this.RunSubCommand(this.CollectCommand, parameters);
                    }
                    else
                    {
                        return new Result(MixItUp.Base.Resources.GameCommandVolcanoAlreadyCollected);
                    }
                }
                else
                {
                    return new Result(MixItUp.Base.Resources.GameCommandVolcanoCollectUnderway);
                }
                return new Result(success: false);
            }
            else if (parameters.Arguments.Count == 1 && string.Equals(parameters.Arguments[0], this.StatusArgument, StringComparison.CurrentCultureIgnoreCase))
            {
                parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();
                if (this.TotalAmount >= this.Stage3MinimumAmount)
                {
                    await this.RunSubCommand(this.Stage3StatusCommand, parameters);
                }
                else if (this.TotalAmount >= this.Stage2MinimumAmount)
                {
                    await this.RunSubCommand(this.Stage2StatusCommand, parameters);
                }
                else
                {
                    await this.RunSubCommand(this.Stage1StatusCommand, parameters);
                }
                return new Result(success: false);
            }
            return new Result();
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            this.TotalAmount += this.GetPrimaryBetAmount(parameters);
            parameters.SpecialIdentifiers[GameCommandModelBase.GameTotalAmountSpecialIdentifier] = this.TotalAmount.ToString();

            if (this.TotalAmount >= this.Stage3MinimumAmount)
            {
                if (this.GenerateProbability() <= this.PayoutProbability)
                {
                    this.collectUsers.Add(parameters.User);
                    int payout = this.GenerateRandomNumber(this.TotalAmount, this.PayoutMinimumPercentage / 100.0d, this.PayoutMaximumPercentage / 100.0d);
                    this.PerformPrimarySetPayout(parameters.User, payout);

                    this.SetGameWinners(parameters, new List<CommandParametersModel>() { parameters });
                    parameters.SpecialIdentifiers[GameCommandModelBase.GamePayoutSpecialIdentifier] = payout.ToString();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        this.collectActive = true;
                        await DelayNoThrow(this.CollectTimeLimit * 1000, cancellationToken);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    await this.RunSubCommand(this.PayoutCommand, parameters);
                }
                else
                {
                    await this.RunSubCommand(this.Stage3DepositCommand, parameters);
                }
            }
            else if (this.TotalAmount >= this.Stage2MinimumAmount)
            {
                await this.RunSubCommand(this.Stage2DepositCommand, parameters);
            }
            else
            {
                await this.RunSubCommand(this.Stage1DepositCommand, parameters);
            }
            await this.PerformCooldown(parameters);

            ChannelSession.Settings.Commands.ManualValueChanged(this.ID);
        }

        private void ClearData()
        {
            this.TotalAmount = 0;
            this.collectActive = false;
            this.collectUsers.Clear();

            ChannelSession.Settings.Commands.ManualValueChanged(this.ID);
        }
    }
}
