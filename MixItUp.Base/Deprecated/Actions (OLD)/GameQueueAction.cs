using MixItUp.Base.Model.Requirements;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [Obsolete]
    public enum GameQueueActionType
    {
        JoinQueue,
        QueuePosition,
        QueueStatus,
        LeaveQueue,
        SelectFirst,
        SelectRandom,
        SelectFirstType,
        EnableDisableQueue,
        ClearQueue,
        JoinFrontOfQueue,
        EnableQueue,
        DisableQueue,
    }

    [Obsolete]
    [DataContract]
    public class GameQueueAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return GameQueueAction.asyncSemaphore; } }

        [DataMember]
        public GameQueueActionType GameQueueType { get; set; }

        [DataMember]
        public RoleRequirementViewModel RoleRequirement { get; set; }

        [DataMember]
        public string TargetUsername { get; set; }

        public GameQueueAction() : base(ActionTypeEnum.GameQueue) { }

        public GameQueueAction(GameQueueActionType gameQueueType, RoleRequirementViewModel roleRequirement = null, string targetUsername = null)
            : this()
        {
            this.GameQueueType = gameQueueType;
            this.RoleRequirement = roleRequirement;
            this.TargetUsername = targetUsername;
        }

        protected override Task PerformInternal(UserV2ViewModel user, IEnumerable<string> arguments)
        {
            return Task.CompletedTask;
        }
    }
}
