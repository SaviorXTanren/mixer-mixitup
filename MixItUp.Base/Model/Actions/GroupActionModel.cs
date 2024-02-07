using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
    [DataContract]
    public class GroupActionModel : ActionModelBase
    {
        [DataMember]
        public List<ActionModelBase> Actions { get; set; } = new List<ActionModelBase>();

        public GroupActionModel(IEnumerable<ActionModelBase> actions) : this(ActionTypeEnum.Group, actions) { }

        public GroupActionModel(ActionTypeEnum type) : base(type) { }

        public GroupActionModel(ActionTypeEnum type, IEnumerable<ActionModelBase> actions)
            : base(type)
        {
            this.Actions = new List<ActionModelBase>(actions);
        }

        [Obsolete]
        public GroupActionModel() : base() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            await this.RunSubActions(parameters);
        }

        public async Task RunSubActions(CommandParametersModel parameters)
        {
            await ServiceManager.Get<CommandService>().RunDirectly(new CommandInstanceModel(this.Actions, parameters));
        }
    }
}
