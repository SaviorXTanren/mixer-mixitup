using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatMessageViewModel : UserChatMessageViewModel
    {
        public static TwitchChatMessageViewModel CreateWhisper(UserViewModel user, string message)
        {
            TwitchChatMessageViewModel result = new TwitchChatMessageViewModel(user);
            result.ProcessMessageContents(message);
            result.TargetUsername = user.Username;
            return result;
        }

        private const char SOHCharacter = (char)1;
        private static readonly string SlashMeAction = SOHCharacter.ToString() + "ACTION ";

        public bool IsSlashMe { get; set; }

        public string WhisperThreadID { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message, UserViewModel user = null)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, (user != null) ? user : new UserViewModel(message))
        {
            this.User.SetTwitchChatDetails(message);

            if (message.Message.StartsWith(SlashMeAction) && message.Message.Last() == SOHCharacter)
            {
                this.IsSlashMe = true;
                string text = message.Message.Replace(SlashMeAction, string.Empty);
                this.ProcessMessageContents(text.Substring(0, text.Length - 1));
            }
            else
            {
                this.ProcessMessageContents(message.Message);
            }
        }

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper, UserViewModel user = null)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, (user != null) ? user : new UserViewModel(whisper))
        {
            this.WhisperThreadID = whisper.thread_id;

            UserViewModel recipient = new UserViewModel(whisper.recipient);
            this.TargetUsername = recipient.Username;

            this.ProcessMessageContents(whisper.body);
        }

        private TwitchChatMessageViewModel(UserViewModel user) : base(string.Empty, StreamingPlatformTypeEnum.Twitch, user) { }

        private void ProcessMessageContents(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                string[] parts = message.Split(new char[] { ' ' });
                foreach (string part in parts)
                {
                    this.AddStringMessagePart(part);
                    if (ChannelSession.Services.Chat.TwitchChatService.Emotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ChannelSession.Services.Chat.TwitchChatService.Emotes[part];
                    }
                    else if (ChannelSession.Settings.ShowBetterTTVEmotes && ChannelSession.Services.Chat.TwitchChatService.BetterTTVEmotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ChannelSession.Services.Chat.TwitchChatService.BetterTTVEmotes[part];
                    }
                }
            }
        }
    }
}