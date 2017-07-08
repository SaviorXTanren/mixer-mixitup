using Mixer.Base.ViewModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class WhisperAction : ActionBase
    {
        public string ChatText { get; set; }

        public WhisperAction(string chatText) : base("Whisper") { }

        public override async Task Perform(UserViewModel user)
        {
            if (MixerAPIHandler.ChatClient != null && !string.IsNullOrEmpty(this.ChatText))
            {
                await MixerAPIHandler.ChatClient.Whisper(user.UserName, this.ChatText);
            }
        }
    }
}
