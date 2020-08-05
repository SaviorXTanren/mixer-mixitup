using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace MixItUp.Base.Model.Commands
{
    [DataContract]
    public abstract class GameCommandModelBase : ChatCommandModel
    {
        private static SemaphoreSlim commandLockSemaphore = new SemaphoreSlim(1);

        public GameCommandModelBase(string name, HashSet<string> triggers, bool includeExclamation, bool wildcards) : base(name, CommandTypeEnum.Game, triggers, includeExclamation, wildcards) { }

        protected override SemaphoreSlim CommandLockSemaphore { get { return GameCommandModelBase.commandLockSemaphore; } }

        public override bool DoesCommandHaveWork { get { return true; } }
    }
}
