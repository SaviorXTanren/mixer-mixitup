using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class WhisperAction : ActionBase
    {
        public string ChatText { get; set; }

        public WhisperAction(string chatText) : base("Whisper") { }

        public override async Task Perform()
        {
            if (MixerAPIHandler.ChatClient != null && !string.IsNullOrEmpty(this.ChatText))
            {
                await MixerAPIHandler.ChatClient.SendMessage(this.ChatText);
            }
        }
    }
}
