using Mixer.Base.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Actions
{
    public class WhisperAction : ActionBase
    {
        public string ChatText { get; set; }

        public WhisperAction(string chatText)
            : base(ActionTypeEnum.Whisper)
        {
            this.ChatText = chatText;
        }

        public override async Task Perform(UserViewModel user, IEnumerable<string> arguments)
        {
            if (MixerAPIHandler.ChatClient != null && !string.IsNullOrEmpty(this.ChatText))
            {
                await MixerAPIHandler.ChatClient.Whisper(user.UserName, this.ChatText);
            }
        }
        public override SerializableAction Serialize()
        {
            return new SerializableAction()
            {
                Type = this.Type,
                Values = new List<string>() { this.ChatText }
            };
        }
    }
}
