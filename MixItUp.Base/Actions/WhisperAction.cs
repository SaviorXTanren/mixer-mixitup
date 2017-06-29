using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class WhisperAction : ActionBase
    {
        public string ChatText { get; private set; }

        public WhisperAction(string chatText)
            : base("Whisper")
        {
            this.ChatText = chatText;
        }

        public override async Task Perform()
        {
            if (MixerAPIHandler.ChatClient != null)
            {
                await MixerAPIHandler.ChatClient.SendMessage(this.ChatText);
            }
        }
    }
}
