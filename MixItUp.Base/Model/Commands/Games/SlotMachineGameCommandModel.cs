using MixItUp.Base.Model.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class SlotMachineGameOutcomeModel : GameOutcomeModel
    {
        [DataMember]
        public List<string> Symbols { get; set; } = new List<string>();

        [DataMember]
        public bool AnyOrder { get; set; }

        public SlotMachineGameOutcomeModel(string name, Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> roleProbabilityPayouts, CustomCommandModel command, IEnumerable<string> symbols, bool anyOrder)
            : base(name, roleProbabilityPayouts, command)
        {
            this.Symbols = new List<string>(symbols);
            this.AnyOrder = anyOrder;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal SlotMachineGameOutcomeModel(Base.Commands.SlotsGameOutcome outcome)
            : base(outcome)
        {
            this.Symbols = new List<string>() { outcome.Symbol1, outcome.Symbol2, outcome.Symbol3 };
            this.AnyOrder = outcome.AnyOrder;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private SlotMachineGameOutcomeModel() { }

        public bool ValidateSymbols(List<string> inputSymbols)
        {
            if (this.AnyOrder)
            {
                Dictionary<string, int> symbolQuantities = this.Symbols.GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
                Dictionary<string, int> inputSymbolQuantities = inputSymbols.GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());
                foreach (var kvp in inputSymbolQuantities)
                {
                    if (!symbolQuantities.ContainsKey(kvp.Key) || symbolQuantities[kvp.Key] != kvp.Value)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                for (int i = 0; i < this.Symbols.Count && i < inputSymbols.Count; i++)
                {
                    if (!string.Equals(this.Symbols[i], inputSymbols[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    [DataContract]
    public class SlotMachineGameCommandModel : GameCommandModelBase
    {
        public const string GameSlotsOutcomeSpecialIdentifier = "gameslotsoutcome";

        [DataMember]
        public List<SlotMachineGameOutcomeModel> Outcomes { get; set; } = new List<SlotMachineGameOutcomeModel>();

        [DataMember]
        public List<string> Symbols { get; set; } = new List<string>();

        [DataMember]
        public CustomCommandModel FailureCommand { get; set; }

        public SlotMachineGameCommandModel(string name, HashSet<string> triggers, IEnumerable<string> symbols, CustomCommandModel failureCommand, IEnumerable<SlotMachineGameOutcomeModel> outcomes)
            : base(name, triggers, GameCommandTypeEnum.SlotMachine)
        {
            this.Symbols = new List<string>(symbols);
            this.FailureCommand = failureCommand;
            this.Outcomes = new List<SlotMachineGameOutcomeModel>(outcomes);
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal SlotMachineGameCommandModel(Base.Commands.SlotMachineGameCommand command)
            : base(command, GameCommandTypeEnum.SlotMachine)
        {
            this.Symbols = new List<string>(command.AllSymbols);
            this.FailureCommand = new CustomCommandModel(command.FailureOutcomeCommand) { IsEmbedded = true };
            this.Outcomes = new List<SlotMachineGameOutcomeModel>(command.Outcomes.Select(o => new SlotMachineGameOutcomeModel((Base.Commands.SlotsGameOutcome)o)));
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private SlotMachineGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>() { this.FailureCommand };
            commands.AddRange(this.Outcomes.Select(o => o.Command));
            return commands;
        }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            List<string> symbols = new List<string>();
            symbols.Add(this.Symbols[this.GenerateRandomNumber(this.Symbols.Count)]);
            symbols.Add(this.Symbols[this.GenerateRandomNumber(this.Symbols.Count)]);
            symbols.Add(this.Symbols[this.GenerateRandomNumber(this.Symbols.Count)]);
            parameters.SpecialIdentifiers[SlotMachineGameCommandModel.GameSlotsOutcomeSpecialIdentifier] = string.Join(" ", symbols);

            await this.PerformCooldown(parameters);
            foreach (SlotMachineGameOutcomeModel outcome in this.Outcomes)
            {
                if (outcome.ValidateSymbols(symbols))
                {
                    await this.PerformOutcome(parameters, outcome);
                    return;
                }
            }
            await this.FailureCommand.Perform(parameters);
        }
    }
}
