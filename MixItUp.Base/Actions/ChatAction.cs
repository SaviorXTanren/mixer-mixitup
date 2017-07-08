using Mixer.Base.ViewModel;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class ChatAction : ActionBase
    {
        public string ChatText { get; set; }

        public ChatAction(string chatText) : base("Chat") { }

        public override async Task Perform(UserViewModel user)
        {
            if (MixerAPIHandler.ChatClient != null)
            {
                await MixerAPIHandler.ChatClient.SendMessage(this.ChatText);
            }
        }
    }
}
