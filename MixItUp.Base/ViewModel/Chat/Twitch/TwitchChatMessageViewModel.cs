using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatMessageViewModel : ChatMessageViewModel
    {
        public string WhisperThreadID { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, new UserViewModel(message))
        {
            this.AddStringMessagePart(message.Message);
        }

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, new UserViewModel(whisper))
        {
            this.WhisperThreadID = whisper.thread_id;

            this.AddStringMessagePart(whisper.body);
        }
    }
}