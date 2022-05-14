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
    public enum RouletteGameCommandBetType
    {
        [Obsolete]
        Traditional,
        NumberRange,
        Custom
    }

    [DataContract]
    public class RouletteGameCommandModel : GameCommandModelBase
    {
        public const string GameRouletteBetTypeSpecialIdentifier = "gamebettype";
        public const string GameRouletteValidBetTypesSpecialIdentifier = "gamevalidbettypes";
        public const string GameRouletteWinningBetTypeSpecialIdentifier = "gamewinningbettype";

        [DataMember]
        public int MinimumParticipants { get; set; }
        [DataMember]
        public int TimeLimit { get; set; }
        [DataMember]
        public RouletteGameCommandBetType BetType { get; set; }
        [DataMember]
        public HashSet<string> BetOptions { get; set; }

        [DataMember]
        public CustomCommandModel StartedCommand { get; set; }
        [DataMember]
        public CustomCommandModel UserJoinCommand { get; set; }
        [DataMember]
        public CustomCommandModel NotEnoughPlayersCommand { get; set; }

        [DataMember]
        public GameOutcomeModel UserSuccessOutcome { get; set; }
        [DataMember]
        public CustomCommandModel UserFailureCommand { get; set; }
        [DataMember]
        public CustomCommandModel GameCompleteCommand { get; set; }

        [JsonIgnore]
        private CommandParametersModel runParameters;
        [JsonIgnore]
        private Dictionary<UserV2ViewModel, CommandParametersModel> runUsers = new Dictionary<UserV2ViewModel, CommandParametersModel>();

        public RouletteGameCommandModel(string name, HashSet<string> triggers, int minimumParticipants, int timeLimit, RouletteGameCommandBetType betType, HashSet<string> betOptions, CustomCommandModel startedCommand,
            CustomCommandModel userJoinCommand, CustomCommandModel notEnoughPlayersCommand, GameOutcomeModel userSuccessOutcome, CustomCommandModel userFailureCommand, CustomCommandModel gameCompleteCommand)
            : base(name, triggers, GameCommandTypeEnum.Roulette)
        {
            this.MinimumParticipants = minimumParticipants;
            this.TimeLimit = timeLimit;
            this.BetType = betType;
            this.BetOptions = betOptions;
            this.StartedCommand = startedCommand;
            this.UserJoinCommand = userJoinCommand;
            this.NotEnoughPlayersCommand = notEnoughPlayersCommand;
            this.UserSuccessOutcome = userSuccessOutcome;
            this.UserFailureCommand = userFailureCommand;
            this.GameCompleteCommand = gameCompleteCommand;
        }

        [Obsolete]
        public RouletteGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>();
            commands.Add(this.StartedCommand);
            commands.Add(this.UserJoinCommand);
            commands.Add(this.NotEnoughPlayersCommand);
            commands.Add(this.UserSuccessOutcome.Command);
            commands.Add(this.UserFailureCommand);
            commands.Add(this.GameCompleteCommand);
            return commands;
        }

        public override Task<Result> CustomValidation(CommandParametersModel parameters)
        {
            this.SetPrimaryCurrencyRequirementArgumentIndex(argumentIndex: 1);

            if (parameters.Arguments.Count == 0 || !this.IsValidBetTypes(parameters.Arguments[0]))
            {
                return Task.FromResult(new Result(string.Format(MixItUp.Base.Resources.GameCommandRouletteValidBetTypes, this.GetValidBetTypes())));
            }

            return Task.FromResult(new Result());
        }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RefundCooldown(parameters);

            if (!this.runUsers.ContainsKey(parameters.User))
            {
                string betType = parameters.Arguments[0].ToLower();
                this.runUsers[parameters.User] = parameters;

                if (this.runParameters == null)
                {
                    this.runParameters = parameters;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
                    {
                        await DelayNoThrow(this.TimeLimit * 1000, cancellationToken);

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

                        string winningBetType = this.BetOptions.Random();
                        if (this.BetType == RouletteGameCommandBetType.NumberRange)
                        {
                            IEnumerable<int> minMaxNumbers = this.BetOptions.Select(s => int.Parse(s));
                            winningBetType = RandomHelper.GenerateRandomNumber(minMaxNumbers.Min(), minMaxNumbers.Max()).ToString();
                        }

                        List<CommandParametersModel> winners = new List<CommandParametersModel>();
                        int totalPayout = 0;
                        foreach (CommandParametersModel participant in this.runUsers.Values.ToList())
                        {
                            participant.SpecialIdentifiers[RouletteGameCommandModel.GameRouletteWinningBetTypeSpecialIdentifier] = winningBetType;
                            if (string.Equals(winningBetType, participant.Arguments[0], StringComparison.CurrentCultureIgnoreCase))
                            {
                                winners.Add(participant);
                                totalPayout += await this.RunOutcome(participant, this.UserSuccessOutcome);
                            }
                            else
                            {
                                await this.RunSubCommand(this.UserFailureCommand, participant);
                            }
                        }

                        this.SetGameWinners(this.runParameters, winners);
                        this.runParameters.SpecialIdentifiers[GameCommandModelBase.GameAllPayoutSpecialIdentifier] = totalPayout.ToString();
                        this.runParameters.SpecialIdentifiers[RouletteGameCommandModel.GameRouletteWinningBetTypeSpecialIdentifier] = winningBetType;

                        await this.RunSubCommand(this.GameCompleteCommand, this.runParameters);

                        await this.PerformCooldown(this.runParameters);
                        this.ClearData();
                    }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                    this.runParameters.SpecialIdentifiers[RouletteGameCommandModel.GameRouletteValidBetTypesSpecialIdentifier] = this.GetValidBetTypes();
                    await this.RunSubCommand(this.StartedCommand, this.runParameters);
                }

                parameters.SpecialIdentifiers[RouletteGameCommandModel.GameRouletteBetTypeSpecialIdentifier] = betType;
                await this.RunSubCommand(this.UserJoinCommand, parameters);
                return;
            }
            else
            {
                await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameCommandAlreadyUnderway, parameters);
            }
            await this.Requirements.Refund(parameters);
        }

        private void ClearData()
        {
            this.runParameters = null;
            this.runUsers.Clear();
        }

        private string GetValidBetTypes()
        {
            string validBetTypes = string.Empty;
            if (this.BetType == RouletteGameCommandBetType.NumberRange)
            {
                IEnumerable<int> numbers = this.BetOptions.Select(s => int.Parse(s));
                validBetTypes = numbers.Min() + "-" + numbers.Max();
            }
            else if (this.BetType == RouletteGameCommandBetType.Custom)
            {
                validBetTypes = string.Join(", ", this.BetOptions);
            }
            return validBetTypes;
        }

        private bool IsValidBetTypes(string value)
        {
            if (this.BetType == RouletteGameCommandBetType.NumberRange)
            {
                var min = int.Parse(this.BetOptions.ElementAt(0));
                var max = int.Parse(this.BetOptions.ElementAt(1));
                if (int.TryParse(value, out var intValue))
                {
                    return intValue >= min && intValue <= max;
                }
            }
            else if (this.BetType == RouletteGameCommandBetType.Custom)
            {
                return this.BetOptions.Contains(value, StringComparer.InvariantCultureIgnoreCase);
            }

            return false;
        }
    }
}