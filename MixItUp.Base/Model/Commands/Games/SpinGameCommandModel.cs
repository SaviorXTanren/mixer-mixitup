using System;
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

        [Obsolete]
        public SpinGameCommandModel() : base() { }

        public override IEnumerable<CommandModelBase> GetInnerCommands() { return this.Outcomes.Select(o => o.Command); }

        public override async Task CustomRun(CommandParametersModel parameters)
        {
            await this.RunOutcome(parameters, this.SelectRandomOutcome(parameters.User, this.Outcomes));
            await this.PerformCooldown(parameters);
        }
    }
}
