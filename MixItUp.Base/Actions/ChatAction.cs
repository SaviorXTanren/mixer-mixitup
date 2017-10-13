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
            if (ChannelSession.BotChat != null)
            {
                string message = await this.ReplaceStringWithSpecialModifiers(this.ChatText, user, arguments);
                if (this.IsWhisper)
                {
                    await ChannelSession.BotChat.Whisper(user.UserName, message);
                }
                else
                {
                    await ChannelSession.BotChat.SendMessage(message);
                }
            }
        }
    }
}
