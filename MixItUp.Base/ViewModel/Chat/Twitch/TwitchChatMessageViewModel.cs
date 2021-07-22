using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Chat;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
    public class TwitchChatEmoteViewModel
    {
        public ChatEmoteModel Emote { get; set; }

        public string SimpleEmoteID { get; set; }
        public string SimpleEmoteCode { get; set; }
        public string SimpleEmoteURL { get { return $"https://static-cdn.jtvnw.net/emoticons/v1/{this.SimpleEmoteID}/1.0"; } }

        public bool IsFullEmote { get { return this.Emote != null; } }

        public string Name { get { return this.IsFullEmote ? this.Emote.name : this.SimpleEmoteCode; } }

        public string ImageURL { get { return this.IsFullEmote ? this.V2DarkImageURL : this.SimpleEmoteURL; } }
        public string DefaultImageURL { get { return this.IsFullEmote ? this.Emote.Size1URL : this.SimpleEmoteURL; } }
        public string V2LightImageURL { get { return this.IsFullEmote ? this.BuildV2EmoteURL("light") : this.SimpleEmoteURL; } }
        public string V2DarkImageURL { get { return this.IsFullEmote ? this.BuildV2EmoteURL("dark") : this.SimpleEmoteURL; } }

        public TwitchChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.Emote = emote;
        }

        public TwitchChatEmoteViewModel(string emoteID, string emoteCode)
        {
            this.SimpleEmoteID = emoteID;
            this.SimpleEmoteCode = emoteCode;
        }

        private string BuildV2EmoteURL(string theme)
        {
            return $"https://static-cdn.jtvnw.net/emoticons/v2/{this.Emote.id}/default/{theme}/1.0";
        }
    }

    public class TwitchBitsCheerViewModel
    {
        public string Text { get; set; }

        public int Amount { get; set; }

        public TwitchBitsCheermoteTierViewModel Tier { get; set; }

        public TwitchBitsCheerViewModel(string text, int amount, TwitchBitsCheermoteTierViewModel tier)
        {
            this.Text = text;
            this.Amount = amount;
            this.Tier = tier;
        }
    }

    public class TwitchChatMessageViewModel : UserChatMessageViewModel
    {
        private const char SOHCharacter = (char)1;
        private static readonly string SlashMeAction = SOHCharacter.ToString() + "ACTION ";

        private const string TagMessageID = "msg-id";
        private const string MessageIDHighlightedMessage = "highlighted-message";

        private static HashSet<long> messageEmotesHashSet = new HashSet<long>();
        private static Dictionary<string, TwitchChatEmoteViewModel> messageEmotesCache = new Dictionary<string, TwitchChatEmoteViewModel>();

        public bool IsSlashMe { get; set; }

        public bool HasBits { get; set; }

        public bool IsHighlightedMessage { get; set; }

        public string WhisperThreadID { get; set; }
        public UserViewModel WhisperRecipient { get; set; }

        public string ReplyThreadID { get; set; }

        public string PlainTextMessageNoCheermotes { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message, UserViewModel user)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.User.SetTwitchChatDetails(message);

            foreach (var kvp in message.EmotesDictionary)
            {
                if (!messageEmotesHashSet.Contains(kvp.Key) && kvp.Value.Count > 0)
                {
                    long emoteID = kvp.Key;
                    Tuple<int, int> instance = kvp.Value.FirstOrDefault();
                    if (0 <= instance.Item1 && instance.Item1 < message.Message.Length && 0 <= instance.Item2 && instance.Item2 < message.Message.Length)
                    {
                        string emoteCode = message.Message.Substring(instance.Item1, instance.Item2 - instance.Item1 + 1);
                        messageEmotesCache[emoteCode] = new TwitchChatEmoteViewModel(emoteID.ToString(), emoteCode);
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

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper, UserViewModel user, UserViewModel recipient)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.WhisperThreadID = whisper.thread_id;

            this.WhisperRecipient = recipient;
            this.TargetUsername = recipient.Username;

            this.ProcessMessageContents(whisper.body);
        }

        public TwitchChatMessageViewModel(UserViewModel user, PubSubBitsEventV2Model bitsCheer)
            : base(bitsCheer.message_id, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.HasBits = true;

            this.ProcessMessageContents((!string.IsNullOrEmpty(bitsCheer.chat_message)) ? bitsCheer.chat_message : string.Empty);
        }

        public TwitchChatMessageViewModel(ChatClearMessagePacketModel messageDeletion, UserViewModel user)
            : base(messageDeletion.ID, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ProcessMessageContents(messageDeletion.Message);
        }

        public TwitchChatMessageViewModel(UserViewModel user, string message, string replyMessageID = null)
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
                    if (ChannelSession.Services.Chat.TwitchChatService != null)
                    {
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

                        if (ChannelSession.Services.Chat.TwitchChatService.Emotes.ContainsKey(part))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = ChannelSession.Services.Chat.TwitchChatService.Emotes[part];
                        }
                        else if (messageEmotesCache.ContainsKey(part))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = messageEmotesCache[part];
                        }
                        else if (ChannelSession.Settings.ShowBetterTTVEmotes && ChannelSession.Services.Chat.TwitchChatService.BetterTTVEmotes.ContainsKey(part))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = ChannelSession.Services.Chat.TwitchChatService.BetterTTVEmotes[part];
                        }
                        else if (ChannelSession.Settings.ShowFrankerFaceZEmotes && ChannelSession.Services.Chat.TwitchChatService.FrankerFaceZEmotes.ContainsKey(part))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = ChannelSession.Services.Chat.TwitchChatService.FrankerFaceZEmotes[part];
                        }
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
            foreach (TwitchBitsCheermoteViewModel cheermote in ChannelSession.Services.Chat.TwitchChatService.BitsCheermotes)
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