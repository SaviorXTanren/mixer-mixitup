using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ChatAction : ActionBase
    {
        public string ChatText { get; set; }

        public bool IsWhisper { get; set; }

        public ChatAction(string chatText, bool isWhisper)
            : base(ActionTypeEnum.Chat)
        {
            this.ChatText = chatText;
            this.IsWhisper = isWhisper;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (MixerAPIHandler.ChatClient != null)
            {
                string message = this.ChatText;
                message = message.Replace("$user", "@" + user.UserName);
                for (int i = 0; i < arguments.Count(); i++)
                {
                    message = message.Replace("$arg" + (i + 1), arguments.ElementAt(i));
                }

                if (this.IsWhisper)
                {
                    await MixerAPIHandler.ChatClient.Whisper(user.UserName, this.ChatText);
                }
                else
                {
                    await MixerAPIHandler.ChatClient.SendMessage(message);
                }
            }
        }

        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.ChatText, this.IsWhisper.ToString() }
            };
        }
    }
}
