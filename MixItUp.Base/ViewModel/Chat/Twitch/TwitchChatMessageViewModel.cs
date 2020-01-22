using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using Twitch.Base.Models.Clients.Chat;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatMessageViewModel : ChatMessageViewModel
    {
        public TwitchChatMessageViewModel(ChatMessagePacketModel message)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, new UserViewModel(message))
        {
            this.AddStringMessagePart(message.Message);
        }
    }
}