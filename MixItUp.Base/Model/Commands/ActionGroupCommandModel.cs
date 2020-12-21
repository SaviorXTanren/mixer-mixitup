using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public class ActionGroupCommandModel : CommandModelBase
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        [DataMember]
        public bool RunOneRandomly { get; set; }

        public ActionGroupCommandModel(string name, bool runOneRandomly)
            : base(name, CommandTypeEnum.ActionGroup)
        {
            this.RunOneRandomly = runOneRandomly;
        }

        internal ActionGroupCommandModel(MixItUp.Base.Commands.ActionGroupCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.ActionGroup;
            this.RunOneRandomly = command.IsRandomized;
        }

        protected ActionGroupCommandModel() : base() { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return ActionGroupCommandModel.commandLockSemaphore; } }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.RunOneRandomly)
            {
                await CommandModelBase.RunActions(new List<ActionModelBase>() { this.Actions.Random() }, parameters);
            }
            else
            {
                await CommandModelBase.RunActions(this.Actions, parameters);
            }
        }
    }
}
