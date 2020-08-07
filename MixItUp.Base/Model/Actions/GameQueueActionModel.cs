using MixItUp.Base.Model.User;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
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
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return GameQueueActionModel.asyncSemaphore; } }

        [DataMember]
        public GameQueueActionType GameQueueType { get; set; }

        [DataMember]
        public UserRoleEnum RoleRequirement { get; set; }

        [DataMember]
        public string TargetUsername { get; set; }

        public GameQueueActionModel(GameQueueActionType gameQueueType, UserRoleEnum roleRequirement = UserRoleEnum.User, string targetUsername = null)
            : base(ActionTypeEnum.GameQueue)
        {
            this.GameQueueType = gameQueueType;
            this.RoleRequirement = roleRequirement;
            this.TargetUsername = targetUsername;
        }

        protected override async Task PerformInternal(UserViewModel user, StreamingPlatformTypeEnum platform, IEnumerable<string> arguments, Dictionary<string, string> specialIdentifiers)
        {
            if (this.GameQueueType == GameQueueActionType.EnableDisableQueue)
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
            else if (this.GameQueueType == GameQueueActionType.EnableQueue)
            {
                await ChannelSession.Services.GameQueueService.Enable();
            }
            else if (this.GameQueueType == GameQueueActionType.DisableQueue)
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

                if (!string.IsNullOrEmpty(this.TargetUsername))
                {
                    string username = await this.ReplaceStringWithSpecialModifiers(this.TargetUsername, user, platform, arguments, specialIdentifiers);
                    UserViewModel targetUser = ChannelSession.Services.User.GetUserByUsername(username, platform);
                    if (targetUser != null)
                    {
                        user = targetUser;
                    }
                    else
                    {
                        await ChannelSession.Services.Chat.SendMessage("The user could not be found");
                        return;
                    }
                }

                if (this.GameQueueType == GameQueueActionType.JoinQueue)
                {
                    await ChannelSession.Services.GameQueueService.Join(user);
                }
                else if (this.GameQueueType == GameQueueActionType.JoinFrontOfQueue)
                {
                    await ChannelSession.Services.GameQueueService.JoinFront(user);
                }
                else if (this.GameQueueType == GameQueueActionType.QueuePosition)
                {
                    await ChannelSession.Services.GameQueueService.PrintUserPosition(user);
                }
                else if (this.GameQueueType == GameQueueActionType.QueueStatus)
                {
                    await ChannelSession.Services.GameQueueService.PrintStatus();
                }
                else if (this.GameQueueType == GameQueueActionType.LeaveQueue)
                {
                    await ChannelSession.Services.GameQueueService.Leave(user);
                }
                if (this.GameQueueType == GameQueueActionType.SelectFirst)
                {
                    await ChannelSession.Services.GameQueueService.SelectFirst();
                }
                else if (this.GameQueueType == GameQueueActionType.SelectRandom)
                {
                    await ChannelSession.Services.GameQueueService.SelectRandom();
                }
                else if (this.GameQueueType == GameQueueActionType.SelectFirstType)
                {
                    //await ChannelSession.Services.GameQueueService.SelectFirstType(this.RoleRequirement);
                }
                else if (this.GameQueueType == GameQueueActionType.ClearQueue)
                {
                    await ChannelSession.Services.GameQueueService.Clear();
                }
            }
        }
    }
}
