using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Actions
{
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

    [DataContract]
    public class GameQueueActionModel : ActionModelBase
    {
        [DataMember]
        public GameQueueActionType ActionType { get; set; }

        [DataMember]
        public RoleRequirementModel RoleRequirement { get; set; }

        [DataMember]
        public string TargetUsername { get; set; }

        public GameQueueActionModel(GameQueueActionType gameQueueType, RoleRequirementModel roleRequirement = null, string targetUsername = null)
            : base(ActionTypeEnum.GameQueue)
        {
            this.ActionType = gameQueueType;
            this.RoleRequirement = roleRequirement;
            this.TargetUsername = targetUsername;
        }

#pragma warning disable CS0612 // Type or member is obsolete
        internal GameQueueActionModel(MixItUp.Base.Actions.GameQueueAction action)
            : base(ActionTypeEnum.GameQueue)
        {
            this.ActionType = (GameQueueActionType)(int)action.GameQueueType;
            this.RoleRequirement = (action.RoleRequirement != null) ? new RoleRequirementModel(action.RoleRequirement) : null;
            this.TargetUsername = action.TargetUsername;
        }
#pragma warning restore CS0612 // Type or member is obsolete

        private GameQueueActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == GameQueueActionType.EnableDisableQueue)
            {
                if (ChannelSession.Services.GameQueueService.IsEnabled)
                {
                    await ChannelSession.Services.GameQueueService.Disable();
                }
                else
                {
                    await ChannelSession.Services.GameQueueService.Enable();
                }
            }
            else if (this.ActionType == GameQueueActionType.EnableQueue)
            {
                await ChannelSession.Services.GameQueueService.Enable();
            }
            else if (this.ActionType == GameQueueActionType.DisableQueue)
            {
                await ChannelSession.Services.GameQueueService.Disable();
            }
            else
            {
                if (!ChannelSession.Services.GameQueueService.IsEnabled)
                {
                    await ChannelSession.Services.Chat.SendMessage("The game queue is not currently enabled");
                    return;
                }

                UserViewModel targetUser = parameters.User;
                if (!string.IsNullOrEmpty(this.TargetUsername))
                {
                    string username = await ReplaceStringWithSpecialModifiers(this.TargetUsername, parameters);
                    targetUser = ChannelSession.Services.User.GetActiveUserByUsername(username, parameters.Platform);
                    if (targetUser == null)
                    {
                        await ChannelSession.Services.Chat.SendMessage("The user could not be found");
                        return;
                    }
                }

                if (this.ActionType == GameQueueActionType.JoinQueue)
                {
                    await ChannelSession.Services.GameQueueService.Join(targetUser);
                }
                else if (this.ActionType == GameQueueActionType.JoinFrontOfQueue)
                {
                    await ChannelSession.Services.GameQueueService.JoinFront(targetUser);
                }
                else if (this.ActionType == GameQueueActionType.QueuePosition)
                {
                    await ChannelSession.Services.GameQueueService.PrintUserPosition(targetUser);
                }
                else if (this.ActionType == GameQueueActionType.QueueStatus)
                {
                    await ChannelSession.Services.GameQueueService.PrintStatus();
                }
                else if (this.ActionType == GameQueueActionType.LeaveQueue)
                {
                    await ChannelSession.Services.GameQueueService.Leave(targetUser);
                }
                if (this.ActionType == GameQueueActionType.SelectFirst)
                {
                    await ChannelSession.Services.GameQueueService.SelectFirst();
                }
                else if (this.ActionType == GameQueueActionType.SelectRandom)
                {
                    await ChannelSession.Services.GameQueueService.SelectRandom();
                }
                else if (this.ActionType == GameQueueActionType.SelectFirstType)
                {
                    if (this.RoleRequirement != null)
                    {
                        await ChannelSession.Services.GameQueueService.SelectFirstType(this.RoleRequirement);
                    }
                    else
                    {
                        await ChannelSession.Services.GameQueueService.SelectFirst();
                    }
                }
                else if (this.ActionType == GameQueueActionType.ClearQueue)
                {
                    await ChannelSession.Services.GameQueueService.Clear();
                }
            }
        }
    }
}
