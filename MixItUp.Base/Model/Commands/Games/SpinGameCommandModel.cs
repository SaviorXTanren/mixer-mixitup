using System.Collections.Generic;
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
            : base(name, triggers)
        {
            this.Outcomes = new List<GameOutcomeModel>(outcomes);
        }

        private SpinGameCommandModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            await this.PerformOutcome(parameters, this.SelectRandomOutcome(parameters.User, this.Outcomes), this.GetBetAmount(parameters));
        }
    }
}
