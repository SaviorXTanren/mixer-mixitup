using MixItUp.Base.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    [DataContract]
    public class ChatAction : ActionBase
    {
        [DataMember]
        public string ChatText { get; set; }

        [DataMember]
        public bool IsWhisper { get; set; }

        public ChatAction() { }

        public ChatAction(string chatText, bool isWhisper)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (ChannelSession.ChatClient != null)
            {
                string message = this.ChatText;
                message = message.Replace("$user", "@" + user.UserName);
                for (int i = 0; i < arguments.Count(); i++)
                {
                    message = message.Replace("$arg" + (i + 1), arguments.ElementAt(i));
                }

                if (this.IsWhisper)
                {
                    await ChannelSession.BotChatClient.Whisper(user.UserName, this.ChatText);
                }
                else
                {
                    await ChannelSession.BotChatClient.SendMessage(message);
                }
            }
        }
    }
}
