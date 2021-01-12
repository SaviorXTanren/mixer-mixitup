using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands.Games
{
    [DataContract]
    public class SpinGameCommandModel : GameCommandModelBase
    {
        [DataMember]
        public List<GameOutcomeModel> Outcomes { get; set; } = new List<GameOutcomeModel>();

        public SpinGameCommandModel(string name, HashSet<string> triggers, IEnumerable<GameOutcomeModel> outcomes)
            : base(name, triggers, GameCommandTypeEnum.Spin)
        {
            this.Outcomes = new List<GameOutcomeModel>(outcomes);
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal SpinGameCommandModel(Base.Commands.SpinGameCommand command)
            : base(command, GameCommandTypeEnum.Spin)
        {
            this.Outcomes = new List<GameOutcomeModel>(command.Outcomes.Select(o => new GameOutcomeModel(o)));
        }

        internal SpinGameCommandModel(Base.Commands.VendingMachineGameCommand command)
            : base(command, GameCommandTypeEnum.Spin)
        {
            this.Outcomes = new List<GameOutcomeModel>(command.Outcomes.Select(o => new GameOutcomeModel(o)));
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private SpinGameCommandModel() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands() { return this.Outcomes.Select(o => o.Command); }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            await this.PerformOutcome(parameters, this.SelectRandomOutcome(parameters.User, this.Outcomes));
        }
    }
}
