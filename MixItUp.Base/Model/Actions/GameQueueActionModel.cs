using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Requirements;
using MixItUp.Base.Services;
using MixItUp.Base.ViewModel.User;
using System;
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

        [Obsolete]
        public GameQueueActionModel() { }

        protected override async Task PerformInternal(CommandParametersModel parameters)
        {
            if (this.ActionType == GameQueueActionType.EnableDisableQueue)
            {
                if (ServiceManager.Get<GameQueueService>().IsEnabled)
                {
                    await ServiceManager.Get<GameQueueService>().Disable();
                }
                else
                {
                    await ServiceManager.Get<GameQueueService>().Enable();
                }
            }
            else if (this.ActionType == GameQueueActionType.EnableQueue)
            {
                await ServiceManager.Get<GameQueueService>().Enable();
            }
            else if (this.ActionType == GameQueueActionType.DisableQueue)
            {
                await ServiceManager.Get<GameQueueService>().Disable();
            }
            else
            {
                if (!ServiceManager.Get<GameQueueService>().IsEnabled)
                {
                    await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.GameQueueNotEnabled, parameters);
                    return;
                }

                UserV2ViewModel targetUser = parameters.User;
                if (!string.IsNullOrEmpty(this.TargetUsername))
                {
                    string username = await ReplaceStringWithSpecialModifiers(this.TargetUsername, parameters);
                    targetUser = await ServiceManager.Get<UserService>().GetUserByPlatform(parameters.Platform, platformUsername: username);
                    if (targetUser == null)
                    {
                        await ServiceManager.Get<ChatService>().SendMessage(MixItUp.Base.Resources.UserNotFound, parameters);
                        return;
                    }
                }

                CommandParametersModel gameQueueParameters = parameters.Duplicate();
                gameQueueParameters.User = targetUser;

                if (this.ActionType == GameQueueActionType.JoinQueue)
                {
                    await ServiceManager.Get<GameQueueService>().Join(gameQueueParameters);
                }
                else if (this.ActionType == GameQueueActionType.JoinFrontOfQueue)
                {
                    await ServiceManager.Get<GameQueueService>().JoinFront(gameQueueParameters);
                }
                else if (this.ActionType == GameQueueActionType.QueuePosition)
                {
                    await ServiceManager.Get<GameQueueService>().PrintUserPosition(gameQueueParameters);
                }
                else if (this.ActionType == GameQueueActionType.QueueStatus)
                {
                    await ServiceManager.Get<GameQueueService>().PrintStatus(gameQueueParameters);
                }
                else if (this.ActionType == GameQueueActionType.LeaveQueue)
                {
                    await ServiceManager.Get<GameQueueService>().Leave(gameQueueParameters);
                }
                if (this.ActionType == GameQueueActionType.SelectFirst)
                {
                    await ServiceManager.Get<GameQueueService>().SelectFirst();
                }
                else if (this.ActionType == GameQueueActionType.SelectRandom)
                {
                    await ServiceManager.Get<GameQueueService>().SelectRandom();
                }
                else if (this.ActionType == GameQueueActionType.SelectFirstType)
                {
                    if (this.RoleRequirement != null)
                    {
                        await ServiceManager.Get<GameQueueService>().SelectFirstType(this.RoleRequirement);
                    }
                    else
                    {
                        await ServiceManager.Get<GameQueueService>().SelectFirst();
                    }
                }
                else if (this.ActionType == GameQueueActionType.ClearQueue)
                {
                    await ServiceManager.Get<GameQueueService>().Clear();
                }
            }
        }
    }
}
