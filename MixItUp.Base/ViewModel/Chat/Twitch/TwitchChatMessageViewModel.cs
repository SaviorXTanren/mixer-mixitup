using MixItUp.Base.Model;
using MixItUp.Base.Model.User.Platform;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Chat;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public override string ID { get; protected set; }
        public override string Name { get; protected set; }
        public override string ImageURL
        {
            get
            {
                if (ChannelSession.Settings.ChatFontSize <= 30)
                {
                    return this.DarkSmallImageUrl;
                }
                else if (ChannelSession.Settings.ChatFontSize <= 70)
                {
                    return this.DarkMediumImageUrl;
                }
                else
                {
                    return this.DarkLargeImageUrl;
                }
            }
            protected set { }
        }

        public override bool IsGIFImage { get { return this.IsAnimated; } }

        public string DarkSmallImageUrl { get; private set; }
        public string DarkMediumImageUrl { get; private set; }
        public string DarkLargeImageUrl { get; private set; }
        public bool IsAnimated { get; private set; }

        public TwitchChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.ID = emote.id;
            this.Name = emote.name;
            if (emote.HasAnimated)
            {
                this.IsAnimated = true;
                this.DarkSmallImageUrl = emote.BuildImageURL(ChatEmoteModel.AnimatedFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale1Name);
                this.DarkMediumImageUrl = emote.BuildImageURL(ChatEmoteModel.AnimatedFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale2Name);
                this.DarkLargeImageUrl = emote.BuildImageURL(ChatEmoteModel.AnimatedFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
            }
            else
            {
                this.DarkSmallImageUrl = emote.BuildImageURL(ChatEmoteModel.StaticFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale1Name);
                this.DarkMediumImageUrl = emote.BuildImageURL(ChatEmoteModel.StaticFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale2Name);
                this.DarkLargeImageUrl = emote.BuildImageURL(ChatEmoteModel.StaticFormatName, ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
            }
        }

        public TwitchChatEmoteViewModel(string emoteID, string emoteCode)
        {
            this.ID = emoteID;
            this.Name = emoteCode;
            this.DarkSmallImageUrl = this.BuildV2EmoteURL(ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale1Name);
            this.DarkMediumImageUrl = this.BuildV2EmoteURL(ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale2Name);
            this.DarkLargeImageUrl = this.BuildV2EmoteURL(ChatEmoteModel.DarkThemeName, ChatEmoteModel.Scale3Name);
        }

        private string BuildV2EmoteURL(string theme, string size) { return $"https://static-cdn.jtvnw.net/emoticons/v2/{this.ID}/default/{theme}/{size}"; }
    }

    public class TwitchBitsCheerViewModel : ChatEmoteViewModelBase
    {
        public override string ID { get; protected set; }
        public override string Name { get; protected set; }
        public override string ImageURL { get; protected set; }

        public int Amount { get; set; }
        public TwitchBitsCheermoteTierViewModel Tier { get; set; }

        public TwitchBitsCheerViewModel(string text, int amount, TwitchBitsCheermoteTierViewModel tier)
        {
            this.Amount = amount;
            this.Tier = tier;

            this.ID = this.Name = text;
            this.ImageURL = (ChannelSession.AppSettings.IsDarkBackground) ? this.Tier.DarkImage : this.Tier.LightImage;
        }
    }

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
                        TwitchBitsCheerViewModel bitCheermote = this.GetBitCheermote(part);
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
                    else if (ChannelSession.Settings.ShowBetterTTVEmotes && ServiceManager.Get<TwitchChatService>().BetterTTVEmotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TwitchChatService>().BetterTTVEmotes[part];
                    }
                    else if (ChannelSession.Settings.ShowFrankerFaceZEmotes && ServiceManager.Get<TwitchChatService>().FrankerFaceZEmotes.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<TwitchChatService>().FrankerFaceZEmotes[part];
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

        private TwitchBitsCheerViewModel GetBitCheermote(string part)
        {
            foreach (TwitchBitsCheermoteViewModel cheermote in ServiceManager.Get<TwitchChatService>().BitsCheermotes)
            {
                if (part.StartsWith(cheermote.ID, StringComparison.InvariantCultureIgnoreCase) && int.TryParse(part.ToLower().Replace(cheermote.ID.ToLower(), ""), out int amount) && amount > 0)
                {
                    TwitchBitsCheermoteTierViewModel tier = cheermote.GetAppropriateTier(amount);
                    if (tier != null)
                    {
                        return new TwitchBitsCheerViewModel(part, amount, tier);
                    }
                }
            }
            return null;
        }
    }
}