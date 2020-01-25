using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatMessageViewModel : ChatMessageViewModel
    {
        private const char SOHCharacter = (char)1;
        private static readonly string SlashMeAction = SOHCharacter.ToString() + "ACTION ";

        public bool IsSlashMe { get; set; }

        public string WhisperThreadID { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, new UserViewModel(message))
        {
            if (message.Message.StartsWith(SlashMeAction) && message.Message.Last() == SOHCharacter)
            {
                this.IsSlashMe = true;
                string text = message.Message.Replace(SlashMeAction, string.Empty);
                this.AddStringMessagePart(text.Substring(0, text.Length - 1));
            }
            else
            {
                this.AddStringMessagePart(message.Message);
            }
        }

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, new UserViewModel(whisper))
        {
            this.WhisperThreadID = whisper.thread_id;

            this.AddStringMessagePart(whisper.body);
        }
    }
}