using MixItUp.Base.Model;
using MixItUp.Base.Model.Trovo.Chat;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatMessageViewModel : UserChatMessageViewModel
    {
        public static readonly HashSet<ChatMessageTypeEnum> ApplicableMessageTypes = new HashSet<ChatMessageTypeEnum>()
        {
            ChatMessageTypeEnum.Normal, ChatMessageTypeEnum.MagicChatSuperCapChat, ChatMessageTypeEnum.MagicChatColorfulChat, ChatMessageTypeEnum.MagicChatSpellChat, ChatMessageTypeEnum.MagicChatBulletScreenChat
        };

        public ChatMessageTypeEnum MessageType { get; set; }

        public ChatMessageModel TrovoMessage { get; set; }

        public TrovoChatMessageViewModel(ChatMessageModel message, UserV2ViewModel user = null)
            : base(message.message_id, StreamingPlatformTypeEnum.Trovo, user)
        {
            this.MessageType = message.type;
            this.TrovoMessage = message;

            this.ProcessMessageContents(message.content);
        }

        public TrovoChatMessageViewModel(UserV2ViewModel user, string message)
            : base(string.Empty, StreamingPlatformTypeEnum.Trovo, user)
        {
            this.ProcessMessageContents(message);
        }

        private void ProcessMessageContents(string message)
        {
            string[] parts = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                this.AddStringMessagePart(part);
                if (part.StartsWith(":"))
                {
                    string emote = part.Substring(1);
                    if (ServiceManager.Get<TrovoSession>().ChannelEmotes.ContainsKey(emote))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TrovoSession>().ChannelEmotes[emote];
                    }
                    else if (ServiceManager.Get<TrovoSession>().EventEmotes.ContainsKey(emote))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TrovoSession>().EventEmotes[emote];
                    }
                    else if (ServiceManager.Get<TrovoSession>().GlobalEmotes.ContainsKey(emote))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TrovoSession>().GlobalEmotes[emote];
                    }
                }
            }
        }
    }
}
