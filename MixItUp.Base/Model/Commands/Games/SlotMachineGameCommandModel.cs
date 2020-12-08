using MixItUp.Base.Model.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class SlotMachineGameCommandModel : GameCommandModelBase
    {
        public const string GameSlotsOutcomeSpecialIdentifier = "gameslotsoutcome";

        [DataMember]
        public List<SlotGameOutcomeModel> Outcomes { get; set; } = new List<SlotGameOutcomeModel>();

        [DataMember]
        public List<string> Symbols { get; set; } = new List<string>();

        [DataMember]
        public CustomCommandModel FailureOutcomeCommand { get; set; }

        public SlotMachineGameCommandModel(string name, HashSet<string> triggers, IEnumerable<SlotGameOutcomeModel> outcomes, IEnumerable<string> symbols, CustomCommandModel failureOutcomeCommand)
            : base(name, triggers, GameCommandTypeEnum.SlotMachine)
        {
            this.Outcomes = new List<SlotGameOutcomeModel>(outcomes);
            this.Symbols = new List<string>(symbols);
            this.FailureOutcomeCommand = failureOutcomeCommand;
        }

        private SlotMachineGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands()
        {
            List<CommandModelBase> commands = new List<CommandModelBase>() { this.FailureOutcomeCommand };
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

            foreach (SlotGameOutcomeModel outcome in this.Outcomes)
            {
                if (outcome.ValidateSymbols(symbols))
                {
                    await this.PerformOutcome(parameters, outcome, this.GetBetAmount(parameters));
                    return;
                }
            }
            await this.FailureOutcomeCommand.Perform(parameters);
        }
    }

    [DataContract]
    public class SlotGameOutcomeModel : GameOutcomeModel
    {
        [DataMember]
        public List<string> Symbols { get; set; } = new List<string>();

        [DataMember]
        public bool AnyOrder { get; set; }

        public SlotGameOutcomeModel(string name, Dictionary<UserRoleEnum, RoleProbabilityPayoutModel> roleProbabilityPayouts, CustomCommandModel command, IEnumerable<string> symbols, bool anyOrder)
            : base(name, roleProbabilityPayouts, command)
        {
            this.Symbols = new List<string>(symbols);
            this.AnyOrder = anyOrder;
        }

        private SlotGameOutcomeModel() { }

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
}
