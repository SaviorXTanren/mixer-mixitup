using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
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
        public bool IsRandomized { get; set; }

        public ActionGroupCommandModel(string name) : base(name, CommandTypeEnum.ActionGroup) { }

        internal ActionGroupCommandModel(MixItUp.Base.Commands.ActionGroupCommand command)
            : base(command)
        {
            this.Name = command.Name;
            this.Type = CommandTypeEnum.ActionGroup;
        }

        protected override SemaphoreSlim CommandLockSemaphore { get { return ActionGroupCommandModel.commandLockSemaphore; } }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.IsRandomized)
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
