using MixItUp.Base.Model;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatMessageViewModel : UserChatMessageViewModel
    {
        private const char SOHCharacter = (char)1;
        private static readonly string SlashMeAction = SOHCharacter.ToString() + "ACTION ";

        private const string TagMessageID = "msg-id";
        private const string MessageIDHighlightedMessage = "highlighted-message";

        private static HashSet<string> messageEmotesHashSet = new HashSet<string>();
        private static Dictionary<string, TwitchChatEmoteViewModel> messageEmotesCache = new Dictionary<string, TwitchChatEmoteViewModel>();

        public bool IsSlashMe { get; set; }

        public bool HasBits { get; set; }

        public bool IsHighlightedMessage { get; set; }

        public string WhisperThreadID { get; set; }
        public UserV2ViewModel WhisperRecipient { get; set; }

        public string ReplyThreadID { get; set; }

        public string PlainTextMessageNoCheermotes { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message, UserV2ViewModel user)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, user)
        {
            if (this.User != null)
            {
                this.User.GetPlatformData<TwitchUserPlatformV2Model>(StreamingPlatformTypeEnum.Twitch).SetUserProperties(message);
            }

            foreach (var kvp in message.EmotesDictionary)
            {
                if (!messageEmotesHashSet.Contains(kvp.Key) && kvp.Value.Count > 0)
                {
                    Tuple<int, int> instance = kvp.Value.FirstOrDefault();
                    if (0 <= instance.Item1 && instance.Item1 < message.Message.Length && 0 <= instance.Item2 && instance.Item2 < message.Message.Length)
                    {
                        string emoteCode = message.Message.Substring(instance.Item1, instance.Item2 - instance.Item1 + 1);
                        messageEmotesCache[emoteCode] = new TwitchChatEmoteViewModel(kvp.Key, emoteCode);
                        messageEmotesHashSet.Add(kvp.Key);
                    }
                }
            }

            this.HasBits = (int.TryParse(message.Bits, out int bits) && bits > 0);
            this.IsHighlightedMessage = message.RawPacket.Tags.ContainsKey(TagMessageID) && message.RawPacket.Tags[TagMessageID].Equals(MessageIDHighlightedMessage);

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

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper, UserV2ViewModel user, UserV2ViewModel recipient)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.WhisperThreadID = whisper.thread_id;

            this.WhisperRecipient = recipient;
            this.TargetUsername = recipient.Username;

            this.ProcessMessageContents(whisper.body);
        }

        public TwitchChatMessageViewModel(ChatUserNoticePacketModel userNotice, UserV2ViewModel user)
            : base(userNotice.MessageID, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.IsHighlightedMessage = true;
            this.ProcessMessageContents(userNotice.RawPacket.Get1SkippedParameterText);
        }

        public TwitchChatMessageViewModel(PubSubBitsEventV2Model bitsCheer, UserV2ViewModel user)
            : base(bitsCheer.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.HasBits = true;

            this.ProcessMessageContents((!string.IsNullOrEmpty(bitsCheer.chat_message)) ? bitsCheer.chat_message : string.Empty);
        }

        public TwitchChatMessageViewModel(ChatClearMessagePacketModel messageDeletion, UserV2ViewModel user)
            : base(messageDeletion.ID, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ProcessMessageContents(messageDeletion.Message);
        }

        public TwitchChatMessageViewModel(UserV2ViewModel user, string message, string replyMessageID = null)
            : base(string.Empty, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ReplyThreadID = replyMessageID;

            this.ProcessMessageContents(message);
        }

        private void ProcessMessageContents(string message)
        {
            List<string> messageNoCheermotes = new List<string>();

            if (!string.IsNullOrEmpty(message))
            {
                string[] parts = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    this.AddStringMessagePart(part);
                    if (this.HasBits)
                    {
                        TwitchBitsCheerViewModel bitCheermote = TwitchBitsCheerViewModel.GetBitCheermote(part);
                        if (bitCheermote != null)
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = bitCheermote;
                            continue;
                        }
                        else
                        {
                            messageNoCheermotes.Add(part);
                        }
                    }

                    if (ServiceManager.Get<TwitchChatService>().Emotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TwitchChatService>().Emotes[part];
                    }
                    else if (messageEmotesCache.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = messageEmotesCache[part];
                    }
                    else if (ChannelSession.Settings.ShowBetterTTVEmotes && ServiceManager.Get<BetterTTVService>().BetterTTVEmotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<BetterTTVService>().BetterTTVEmotes[part];
                    }
                    else if (ChannelSession.Settings.ShowFrankerFaceZEmotes && ServiceManager.Get<FrankerFaceZService>().FrankerFaceZEmotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<FrankerFaceZService>().FrankerFaceZEmotes[part];
                    }
                }
            }

            if (this.HasBits)
            {
                this.PlainTextMessageNoCheermotes = string.Join(" ", messageNoCheermotes);
            }
            else
            {
                this.PlainTextMessageNoCheermotes = this.PlainTextMessage;
            }
        }
    }
}