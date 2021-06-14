using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Trovo;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using Trovo.Base.Models.Chat;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatMessageViewModel : UserChatMessageViewModel
    {
        public static readonly HashSet<ChatMessageTypeEnum> ApplicableMessageTypes = new HashSet<ChatMessageTypeEnum>()
        {
            ChatMessageTypeEnum.Normal, ChatMessageTypeEnum.MagicChatSuperCapChat, ChatMessageTypeEnum.MagicChatColorfulChat, ChatMessageTypeEnum.MagicChatSpellChat, ChatMessageTypeEnum.MagicChatBulletScreenChat,
            ChatMessageTypeEnum.SubscriptionAlert, ChatMessageTypeEnum.FollowAlert, ChatMessageTypeEnum.WelcomeMessage, ChatMessageTypeEnum.ActivityEventMessage, ChatMessageTypeEnum.WelcomeMessageFromRaid
        };

        public ChatMessageTypeEnum MessageType { get; set; }

        public TrovoChatMessageViewModel(ChatMessageModel message, UserViewModel user = null)
            : base(message.message_id, StreamingPlatformTypeEnum.Trovo, user)
        {
            this.MessageType = message.type;

            string[] parts = message.content.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                this.AddStringMessagePart(part);
                if (part.StartsWith(":"))
                {
                    string emote = part.Substring(1);
                    if (ServiceManager.Has<TrovoChatEventService>())
                    {
                        if (ServiceManager.Get<TrovoChatEventService>().ChannelEmotes.ContainsKey(emote))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = new TrovoChatEmoteViewModel(ServiceManager.Get<TrovoChatEventService>().ChannelEmotes[emote]);
                        }
                        else if (ServiceManager.Get<TrovoChatEventService>().EventEmotes.ContainsKey(emote))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = new TrovoChatEmoteViewModel(ServiceManager.Get<TrovoChatEventService>().EventEmotes[emote]);
                        }
                        else if (ServiceManager.Get<TrovoChatEventService>().GlobalEmotes.ContainsKey(emote))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = new TrovoChatEmoteViewModel(ServiceManager.Get<TrovoChatEventService>().GlobalEmotes[emote]);
                        }
                    }
                }
            }
        }
    }
}
