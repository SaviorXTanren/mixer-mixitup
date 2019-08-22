using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.ViewModel.Chat
{
    public class AlertChatMessageViewModel : ChatMessageViewModel
    {
        public string Color { get; private set; }

        public AlertChatMessageViewModel(StreamingPlatformTypeEnum platform, string message, string color = null)
            : base(string.Empty, platform, new UserViewModel(ChannelSession.MixerStreamerUser))
        {
            this.Color = color;

            this.AddStringMessagePart(string.Format("--- {0} ---", message));
        }

        public override string ToString()
        {
            return this.PlainTextMessage;
        }
    }
}
