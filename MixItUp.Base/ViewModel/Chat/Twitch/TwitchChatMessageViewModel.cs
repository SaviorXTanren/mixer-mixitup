using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.V5.Emotes;

namespace MixItUp.Base.ViewModel.Chat.Twitch
{
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
        private static Dictionary<string, EmoteModel> messageEmotesCache = new Dictionary<string, EmoteModel>();

        public bool IsSlashMe { get; set; }

        public bool HasBits { get; set; }

        public bool IsHighlightedMessage { get; set; }

        public string WhisperThreadID { get; set; }

        public TwitchChatMessageViewModel(ChatMessagePacketModel message, UserViewModel user = null)
            : base(message.ID, StreamingPlatformTypeEnum.Twitch, (user != null) ? user : new UserViewModel(message))
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
                        messageEmotesCache[emoteCode] = new EmoteModel()
                        {
                            id = emoteID,
                            code = emoteCode
                        };
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

        public TwitchChatMessageViewModel(PubSubWhisperEventModel whisper, UserViewModel user = null)
            : base(whisper.message_id, StreamingPlatformTypeEnum.Twitch, (user != null) ? user : new UserViewModel(whisper))
        {
            this.WhisperThreadID = whisper.thread_id;

            UserViewModel recipient = new UserViewModel(whisper.recipient);
            this.TargetUsername = recipient.Username;

            this.ProcessMessageContents(whisper.body);
        }

        public TwitchChatMessageViewModel(UserViewModel user, string message)
            : base(string.Empty, StreamingPlatformTypeEnum.Twitch, user)
        {
            this.ProcessMessageContents(message);
        }

        private void ProcessMessageContents(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                string[] parts = message.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    this.AddStringMessagePart(part);
                    if (ServiceManager.Get<TwitchChatService>() != null)
                    {
                        if (this.HasBits)
                        {
                            foreach (TwitchBitsCheermoteViewModel cheermote in ServiceManager.Get<TwitchChatService>().BitsCheermotes)
                            {
                                if (part.StartsWith(cheermote.ID) && int.TryParse(part.Replace(cheermote.ID, ""), out int amount) && amount > 0)
                                {
                                    TwitchBitsCheermoteTierViewModel tier = cheermote.GetAppropriateTier(amount);
                                    if (tier != null)
                                    {
                                        this.MessageParts[this.MessageParts.Count - 1] = new TwitchBitsCheerViewModel(part, amount, tier);
                                        continue;
                                    }
                                }
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
            }
        }
    }
}