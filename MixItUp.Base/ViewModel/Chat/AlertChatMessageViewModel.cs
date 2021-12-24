using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;

namespace MixItUp.Base.ViewModel.Chat
{
    public class AlertChatMessageViewModel : ChatMessageViewModel
    {
        public string Color { get; private set; }

        public AlertChatMessageViewModel(string message, string color = null) : this(StreamingPlatformTypeEnum.None, message, color) { }

        public AlertChatMessageViewModel(StreamingPlatformTypeEnum platform, string message, string color = null) : this(ChannelSession.User, message, color) { }

        public AlertChatMessageViewModel(UserV2ViewModel user, string message, string color = null)
            : base(string.Empty, (user != null) ? user.Platform : StreamingPlatformTypeEnum.None, user)
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
