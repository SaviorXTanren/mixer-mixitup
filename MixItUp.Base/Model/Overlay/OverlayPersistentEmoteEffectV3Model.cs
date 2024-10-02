using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public class OverlayPersistentEmoteEffectComboV3Model
    {
        public DateTimeOffset LastSeen { get; set; }
        public int Count { get; set; }
    }

    public class OverlayPersistentEmoteEffectV3Model : OverlayItemV3ModelBase
    {
        public const string EmotePropertyName = "Emote";
        public const string AmountPropertyName = "Amount";

        public static readonly string DefaultHTML = OverlayResources.OverlayEmoteEffectDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayEmoteEffectDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEmoteEffectBaseDefaultJavascript + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayPersistentEmoteEffectDefaultJavascript;

        [DataMember]
        public OverlayEmoteEffectV3AnimationType AnimationType { get; set; }

        [DataMember]
        public int Duration { get; set; }

        [DataMember]
        public int EmoteWidth { get; set; }
        [DataMember]
        public int EmoteHeight { get; set; }

        [DataMember]
        public int PerEmoteShown { get; set; }

        [DataMember]
        public int ComboCount { get; set; }
        [DataMember]
        public int ComboTimeframe { get; set; }

        [DataMember]
        public bool AllowEmoji { get; set; }

        [DataMember]
        public bool IgnoreDuplicates { get; set; }

        private Dictionary<string, OverlayPersistentEmoteEffectComboV3Model> comboLastSeen = new Dictionary<string, OverlayPersistentEmoteEffectComboV3Model>();

        public OverlayPersistentEmoteEffectV3Model() : base(OverlayItemV3Type.PersistentEmoteEffect) { }

        public override async Task Initialize()
        {
            await base.Initialize();

            ChatService.OnChatMessageReceived -= OnChatMessageReceived;
            ChatService.OnChatMessageReceived += OnChatMessageReceived;

            this.comboLastSeen.Clear();
        }

        public override async Task Uninitialize()
        {
            await base.Uninitialize();

            ChatService.OnChatMessageReceived -= OnChatMessageReceived;
        }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();

            properties[nameof(this.Duration)] = this.Duration;
            properties[nameof(this.EmoteWidth)] = this.EmoteWidth;
            properties[nameof(this.EmoteHeight)] = this.EmoteHeight;
            properties[nameof(this.PerEmoteShown)] = this.PerEmoteShown;

            return properties;
        }

        public async Task ShowEmote(string emoteURL, int amount)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            OverlayEmoteEffectV3AnimationType animationType = this.AnimationType;
            if (this.AnimationType == OverlayEmoteEffectV3AnimationType.Random)
            {
                animationType = OverlayEmoteEffectV3Model.ValidAnimationTypes.Random();
            }

            properties[EmotePropertyName] = emoteURL;
            properties[nameof(this.AnimationType)] = animationType.ToString();
            properties[OverlayEmoteEffectV3Model.IncludeDelayPropertyName] = OverlayEmoteEffectV3Model.DontDelayAnimations.Contains(animationType) ? false : true;
            properties[AmountPropertyName] = amount;
            await this.CallFunction("showEmote", properties);
        }

        private async void OnChatMessageReceived(object sender, ChatMessageViewModel message)
        {
            ICollection<string> emoteURLs = new List<string>();
            if (this.IgnoreDuplicates)
            {
                emoteURLs = new HashSet<string>();
            }

            foreach (object messagePart in message.MessageParts)
            {
                if (messagePart is ChatEmoteViewModelBase)
                {
                    emoteURLs.Add(((ChatEmoteViewModelBase)messagePart).OverlayAnimatedOrStaticImageURL);
                }
                else if (messagePart is string)
                {
                    foreach (string emoji in OverlayEmoteEffectV3Model.GetEmojisFromText((string)messagePart))
                    {
                        emoteURLs.Add(emoji);
                    }
                }
            }

            foreach (string emoteURL in emoteURLs)
            {
                if (!this.comboLastSeen.ContainsKey(emoteURL))
                {
                    this.comboLastSeen[emoteURL] = new OverlayPersistentEmoteEffectComboV3Model();
                }

                if (this.comboLastSeen[emoteURL].LastSeen.TotalSecondsFromNow() > this.ComboTimeframe)
                {
                    this.comboLastSeen[emoteURL].LastSeen = DateTimeOffset.Now;
                    this.comboLastSeen[emoteURL].Count = 1;
                }
                else
                {
                    this.comboLastSeen[emoteURL].Count++;
                }

                if (this.comboLastSeen[emoteURL].Count == this.ComboCount)
                {
                    await this.ShowEmote(emoteURL, this.comboLastSeen[emoteURL].Count);
                }
                else if (this.comboLastSeen[emoteURL].Count > this.ComboCount)
                {
                    await this.ShowEmote(emoteURL, 1);
                }
            }
        }
    }
}
