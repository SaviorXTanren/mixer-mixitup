using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ChatAction : ActionBase
    {
        public string ChatText { get; private set; }

        public ChatAction(string chatText)
            : base("Chat")
        {
            this.ChatText = chatText;
        }

        public override async Task Perform()
        {
            if (MixerAPIHandler.ChatClient != null)
            {
                await MixerAPIHandler.ChatClient.Whisper(null, this.ChatText);
            }
        }
    }
}
