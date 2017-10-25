using Mixer.Base.Util;
using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public enum GameQueueActionType
    {
        [Name("Join Queue")]
        JoinQueue,
        [Name("Queue Position")]
        QueuePosition,
        [Name("Queue Status")]
        QueueStatus,
        [Name("Leave Queue")]
        LeaveQueue,
        [Name("Remove Front User")]
        RemoveFirst
    }

    [DataContract]
    public class GameQueueAction : ActionBase
    {
        [DataMember]
        public GameQueueActionType GameQueueType { get; set; }

        public GameQueueAction() { }

        public GameQueueAction(GameQueueActionType gameQueueType)
            : base(ActionTypeEnum.GameQueue)
        {
            this.GameQueueType = gameQueueType;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.GameQueueEnabled && ChannelSession.BotChat != null)
            {
                if (this.GameQueueType == GameQueueActionType.JoinQueue)
                {
                    int position = ChannelSession.GameQueue.IndexOf(user);
                    if (position == -1)
                    {
                        ChannelSession.GameQueue.Add(user);
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

                    await ChannelSession.BotChat.SendMessage(message.ToString());
                }
                else if (this.GameQueueType == GameQueueActionType.LeaveQueue)
                {
                    ChannelSession.GameQueue.Remove(user);
                    await ChannelSession.BotChat.Whisper(user.UserName, string.Format("You have left the queue to play with @{0}.", ChannelSession.Channel.user.username));
                }
                else if (this.GameQueueType == GameQueueActionType.RemoveFirst)
                {
                    if (ChannelSession.GameQueue.Count() > 0)
                    {
                        UserViewModel firstUser = ChannelSession.GameQueue.ElementAt(0);
                        ChannelSession.GameQueue.RemoveAt(0);
                        await ChannelSession.BotChat.SendMessage(string.Format("it's time to play @{0}! Listen carefully for instructions on how to join @{1}", firstUser.UserName,
                            ChannelSession.Channel.user.username));
                    }
                }

                ChannelSession.GameQueueUpdated();
            }
        }

        private async Task PrintUserPosition(UserViewModel user)
        {
            int position = ChannelSession.GameQueue.IndexOf(user);
            if (position != -1)
            {
                await ChannelSession.BotChat.Whisper(user.UserName, string.Format("You are #{0} in the queue to play with @{1}.", (position + 1), ChannelSession.Channel.user.username));
            }
            else
            {
                await ChannelSession.BotChat.Whisper(user.UserName, string.Format("You are not currently in the queue to play with @{0}.", ChannelSession.Channel.user.username));
            }
        }
    }
}
