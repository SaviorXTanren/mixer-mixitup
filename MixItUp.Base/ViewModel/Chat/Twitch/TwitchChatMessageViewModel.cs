using MixItUp.Base.Model;
using MixItUp.Base.Model.Twitch.Clients.Chat;
using MixItUp.Base.Model.Twitch.Clients.PubSub.Messages;
using MixItUp.Base.Model.Twitch.EventSub;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public string UserBadges { get; set; }
        public string UserBadgeInfo { get; set; }

        [Obsolete]
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

            this.UserBadges = message.UserBadges;
            this.UserBadgeInfo = message.UserBadgeInfo;

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

        public TwitchChatMessageViewModel(CheerNotification cheer, UserV2ViewModel user)
            : base(string.Empty, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.HasBits = true;

            this.ProcessMessageContents((!string.IsNullOrEmpty(cheer.message)) ? cheer.message : string.Empty);
        }

        public TwitchChatMessageViewModel(ChatClearMessagePacketModel messageDeletion, UserV2ViewModel user)
            : base(messageDeletion.ID, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ProcessMessageContents(messageDeletion.Message);
        }

        public TwitchChatMessageViewModel(ChatMessageDeletedNotification messageDeleted, UserV2ViewModel user)
            : base(messageDeleted.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {

        }

        public TwitchChatMessageViewModel(ChatMessageNotification notification, UserV2ViewModel user)
            : base(notification.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.IsInUsersChannel = string.IsNullOrEmpty(notification.source_broadcaster_user_id) || string.Equals(ServiceManager.Get<TwitchSession>().ChannelID, notification.source_broadcaster_user_id);

            this.HasBits = notification.cheer?.bits > 0;
            this.IsHighlightedMessage =
                notification.MessageType == ChatNotificationMessageType.power_ups_message_effect ||
                notification.MessageType == ChatNotificationMessageType.channel_points_highlighted ||
                notification.MessageType == ChatNotificationMessageType.user_intro;

            this.ReplyThreadID = notification.reply?.parent_message_id;

            this.ProcessMessageContents(notification.message);
        }

        public TwitchChatMessageViewModel(ChatNotification notification, UserV2ViewModel user)
            : base(notification.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.IsInUsersChannel = string.IsNullOrEmpty(notification.source_broadcaster_user_id) || string.Equals(ServiceManager.Get<TwitchSession>().ChannelID, notification.source_broadcaster_user_id);

            this.IsHighlightedMessage =
                notification.NoticeType == ChatNotificationType.announcement ||
                notification.NoticeType == ChatNotificationType.shared_chat_announcement;

            this.ProcessMessageContents(notification.message);
        }

        public TwitchChatMessageViewModel(ModerationNotificationDelete notification, UserV2ViewModel user)
            : base(notification.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
        }

        public TwitchChatMessageViewModel(UserV2ViewModel user, string message, string replyMessageID = null, UserV2ViewModel recipient = null)
            : base(string.Empty, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ReplyThreadID = replyMessageID;

            if (recipient != null)
            {
                this.WhisperRecipient = recipient;
                this.TargetUsername = recipient.Username;
            }

            this.ProcessMessageContents(message);
        }

        private void ProcessMessageContents(string message)
        {
            List<string> messageNoCheermotes = new List<string>();

            if (!string.IsNullOrEmpty(message))
            {
                message = message.Trim();
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

                    if (ServiceManager.Get<TwitchSession>().Emotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TwitchSession>().Emotes[part];
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

        private void ProcessMessageContents(ChatMessageNotificationMessage message)
        {
            List<string> messageNoCheermotes = new List<string>();

            foreach (ChatMessageNotificationFragment fragment in message.fragments)
            {
                if (fragment.Type == ChatNotificationMessageFragmentType.text)
                {
                    foreach (string text in fragment.text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        this.AddStringMessagePart(text);
                        if (ChannelSession.Settings.ShowBetterTTVEmotes && ServiceManager.Get<BetterTTVService>().BetterTTVEmotes.ContainsKey(text))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<BetterTTVService>().BetterTTVEmotes[text];
                        }
                        else if (ChannelSession.Settings.ShowFrankerFaceZEmotes && ServiceManager.Get<FrankerFaceZService>().FrankerFaceZEmotes.ContainsKey(text))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<FrankerFaceZService>().FrankerFaceZEmotes[text];
                        }
                    }
                }
                else if (fragment.Type == ChatNotificationMessageFragmentType.emote)
                {
                    this.AddStringMessagePart(fragment.text);
                    if (!ServiceManager.Get<TwitchSession>().Emotes.ContainsKey(fragment.text))
                    {
                        ServiceManager.Get<TwitchSession>().Emotes[fragment.text] = new TwitchChatEmoteViewModel(fragment.text, fragment.emote);
                    }
                    this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TwitchSession>().Emotes[fragment.text];
                }
                else if (fragment.Type == ChatNotificationMessageFragmentType.cheermote)
                {
                    this.AddStringMessagePart(fragment.text);
                    if (ServiceManager.Get<TwitchSession>().BitsCheermotes.TryGetValue(fragment.cheermote.prefix, out TwitchBitsCheermoteViewModel cheermote))
                    {
                        if (cheermote.Tiers.TryGetValue(fragment.cheermote.tier.ToString(), out TwitchBitsCheermoteTierViewModel tier))
                        {
                            TwitchBitsCheerViewModel bitCheermote = new TwitchBitsCheerViewModel(fragment.text, fragment.cheermote.bits, tier);
                            this.MessageParts[this.MessageParts.Count - 1] = bitCheermote;
                        }
                    }
                }
                else if (fragment.Type == ChatNotificationMessageFragmentType.mention)
                {
                    this.AddStringMessagePart(fragment.text);
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