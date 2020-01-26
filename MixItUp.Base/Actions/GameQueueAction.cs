using Mixer.Base.Model.User;
using Mixer.Base.Util;
using MixItUp.Base.ViewModel.Requirement;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum GameQueueActionType
    {
        [Name("User Join Queue")]
        JoinQueue,
        [Name("User's Queue Position")]
        QueuePosition,
        [Name("Queue Status")]
        QueueStatus,
        [Name("User Leave Queue")]
        LeaveQueue,
        [Name("Select User at Front of Queue")]
        SelectFirst,
        [Name("Select Random User in Queue")]
        SelectRandom,
        [Name("Select First User of Type in Queue")]
        SelectFirstType,
        [Name("Enable/Disable Queue")]
        EnableDisableQueue,
        [Name("Clear Queue")]
        ClearQueue,
        [Name("User Join Front of Queue")]
        JoinFrontOfQueue,
        [Name("Enable Queue")]
        EnableQueue,
        [Name("Disable Queue")]
        DisableQueue,
    }

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

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Services.Chat != null)
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
                        await ChannelSession.Services.Chat.Whisper(user, "The game queue is not currently enabled");
                        return;
                    }

                    if (!string.IsNullOrEmpty(this.TargetUsername))
                    {
                        string username = await this.ReplaceStringWithSpecialModifiers(this.TargetUsername, user, arguments);
                        UserViewModel targetUser = ChannelSession.Services.User.GetUserByUsername(username);
                        if (targetUser != null)
                        {
                            user = targetUser;
                        }
                        else
                        {
                            await ChannelSession.Services.Chat.Whisper(user, "The user could not be found");
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
                        await ChannelSession.Services.GameQueueService.SelectFirstType(this.RoleRequirement);
                    }
                    else if (this.GameQueueType == GameQueueActionType.ClearQueue)
                    {
                        await ChannelSession.Services.GameQueueService.Clear();
                    }
                }
            }
        }
    }
}
