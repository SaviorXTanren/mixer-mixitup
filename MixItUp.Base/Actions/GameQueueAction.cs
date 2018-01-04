using Mixer.Base.Util;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
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
        [Name("Remove User in Front of Queue")]
        RemoveFirst
    }

    [DataContract]
    public class GameQueueAction : ActionBase
    {
        private static SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        protected override SemaphoreSlim AsyncSemaphore { get { return GameQueueAction.asyncSemaphore; } }

        [DataMember]
        public GameQueueActionType GameQueueType { get; set; }

        public GameQueueAction() : base(ActionTypeEnum.GameQueue) { }

        public GameQueueAction(GameQueueActionType gameQueueType)
            : this()
        {
            this.GameQueueType = gameQueueType;
        }

        protected override async Task PerformInternal(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.Chat != null)
            {
                if (!ChannelSession.GameQueueEnabled)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "The game queue is not currently enabled");
                    return;
                }

                if (!user.IsFollower)
                {
                    await user.SetDetails(true);
                }

                if (ChannelSession.Settings.GameQueueMustFollow && !user.IsFollower)
                {
                    await ChannelSession.Chat.Whisper(user.UserName, "You must be a follower of the channel to use the game queue");
                    return;
                }

                UserCurrencyDataViewModel rankData = null;
                if (ChannelSession.Settings.GameQueueRankRequirement != null)
                {
                    rankData = user.Data.GetCurrency(ChannelSession.Settings.GameQueueRankRequirement.CurrencyName);
                    if (!ChannelSession.Settings.GameQueueRankRequirement.DoesUserMeetRequirement(user.Data))
                    {
                        await ChannelSession.Settings.GameQueueRankRequirement.SendRankNotMetWhisper(user);
                        return;
                    }
                }

                UserCurrencyDataViewModel currencyData = null;
                if (ChannelSession.Settings.GameQueueCurrencyRequirement != null)
                {
                    currencyData = user.Data.GetCurrency(ChannelSession.Settings.GameQueueCurrencyRequirement.CurrencyName);
                    if (!ChannelSession.Settings.GameQueueCurrencyRequirement.DoesUserMeetRequirement(user.Data))
                    {
                        await ChannelSession.Settings.GameQueueCurrencyRequirement.SendCurrencyNotMetWhisper(user);
                        return;
                    }
                }

                if (this.GameQueueType == GameQueueActionType.JoinQueue)
                {
                    int position = ChannelSession.GameQueue.IndexOf(user);
                    if (position == -1)
                    {
                        if (currencyData != null)
                        {
                            currencyData.Amount -= ChannelSession.Settings.GameQueueCurrencyRequirement.RequiredAmount;
                        }

                        if (ChannelSession.Settings.GameQueueSubPriority && user.Roles.Contains(UserRole.Subscriber))
                        {
                            int totalSubs = ChannelSession.GameQueue.Count(u => u.Roles.Contains(UserRole.Subscriber));
                            ChannelSession.GameQueue.Insert(totalSubs, user);
                        }
                        else
                        {
                            ChannelSession.GameQueue.Add(user);
                        }
                    }
                    await this.PrintUserPosition(user);
                }
                else if (this.GameQueueType == GameQueueActionType.QueuePosition)
                {
                    await this.PrintUserPosition(user);
                }
                else if (this.GameQueueType == GameQueueActionType.QueueStatus)
                {
                    StringBuilder message = new StringBuilder();
                    message.Append(string.Format("There are currently {0} waiting to play with @{1}.", ChannelSession.GameQueue.Count(), ChannelSession.Channel.user.username));

                    if (ChannelSession.GameQueue.Count() > 0)
                    {
                        message.Append(" The following users are next up to play: ");

                        List<string> users = new List<string>();
                        for (int i = 0; i < ChannelSession.GameQueue.Count() && i < 5; i++)
                        {
                            users.Add("@" + ChannelSession.GameQueue[i].UserName);
                        }

                        message.Append(string.Join(", ", users));
                        message.Append(".");
                    }

                    await ChannelSession.Chat.SendMessage(message.ToString());
                }
                else if (this.GameQueueType == GameQueueActionType.LeaveQueue)
                {
                    ChannelSession.GameQueue.Remove(user);
                    await ChannelSession.Chat.Whisper(user.UserName, string.Format("You have left the queue to play with @{0}.", ChannelSession.Channel.user.username));
                }
                else if (this.GameQueueType == GameQueueActionType.RemoveFirst)
                {
                    if (ChannelSession.GameQueue.Count() > 0)
                    {
                        UserViewModel firstUser = ChannelSession.GameQueue.ElementAt(0);
                        ChannelSession.GameQueue.RemoveAt(0);
                        await ChannelSession.Chat.SendMessage(string.Format("it's time to play @{0}! Listen carefully for instructions on how to join @{1}", firstUser.UserName,
                            ChannelSession.Channel.user.username));
                    }
                }

                GlobalEvents.GameQueueUpdated();
            }
        }

        private async Task PrintUserPosition(UserViewModel user)
        {
            int position = ChannelSession.GameQueue.IndexOf(user);
            if (position != -1)
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You are #{0} in the queue to play with @{1}.", (position + 1), ChannelSession.Channel.user.username));
            }
            else
            {
                await ChannelSession.Chat.Whisper(user.UserName, string.Format("You are not currently in the queue to play with @{0}.", ChannelSession.Channel.user.username));
            }
        }
    }
}
