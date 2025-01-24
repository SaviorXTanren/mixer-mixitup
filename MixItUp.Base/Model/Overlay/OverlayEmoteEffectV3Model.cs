using MixItUp.Base.Model.Commands;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Trovo.New;
using MixItUp.Base.Services.Twitch.New;
using MixItUp.Base.Services.YouTube.New;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Chat.Trovo;
using MixItUp.Base.ViewModel.Chat.Twitch;
using MixItUp.Base.ViewModel.Chat.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    public enum OverlayEmoteEffectV3AnimationType
    {
        Rain,
        Float,
        Fade,
        Zoom,
        Explosion,
        FallingLeaves,
        ShootingStars,

        Random = 1000,
    }

    public class OverlayEmoteEffectV3Model : OverlayItemV3ModelBase
    {
        public const string EmojiURLPrefix = "emoji://";

        public const string EmotesPropertyName = "Emotes";
        public const string IncludeDelayPropertyName = "IncludeDelay";

        public static readonly string DefaultHTML = OverlayResources.OverlayEmoteEffectDefaultHTML;
        public static readonly string DefaultCSS = OverlayResources.OverlayEmoteEffectDefaultCSS;
        public static readonly string DefaultJavascript = OverlayResources.OverlayEmoteEffectBaseDefaultJavascript + Environment.NewLine + Environment.NewLine + OverlayResources.OverlayEmoteEffectDefaultJavascript;

        public static readonly IEnumerable<OverlayEmoteEffectV3AnimationType> ValidAnimationTypes = EnumHelper.GetEnumList<OverlayEmoteEffectV3AnimationType>().Where(e => e != OverlayEmoteEffectV3AnimationType.Random);

        public static readonly HashSet<OverlayEmoteEffectV3AnimationType> DontDelayAnimations = new HashSet<OverlayEmoteEffectV3AnimationType>()
        {
            OverlayEmoteEffectV3AnimationType.Explosion
        };

        public static IEnumerable<string> GetEmoteURLs(string text, CommandParametersModel parameters, bool allowURLs, bool allowEmoji)
        {
            List<string> emoteURLs = new List<string>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                string[] splits = text.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                if (splits != null && splits.Length > 0)
                {
                    foreach (string split in splits)
                    {
                        if (StreamingPlatforms.ContainsPlatform(parameters.Platform, StreamingPlatformTypeEnum.Twitch))
                        {
                            if (ServiceManager.Get<TwitchSession>().Emotes.TryGetValue(split, out TwitchChatEmoteViewModel twitchEmote))
                            {
                                emoteURLs.Add(twitchEmote.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }

                            TwitchBitsCheerViewModel twitchBitCheer = TwitchBitsCheerViewModel.GetBitCheermote(split);
                            if (twitchBitCheer != null)
                            {
                                emoteURLs.Add(twitchBitCheer.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }
                        }

                        if (StreamingPlatforms.ContainsPlatform(parameters.Platform, StreamingPlatformTypeEnum.YouTube))
                        {
                            if (ServiceManager.Get<YouTubeSession>().EmoteDictionary.TryGetValue(split, out YouTubeChatEmoteViewModel youtubeEmote))
                            {
                                emoteURLs.Add(youtubeEmote.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }
                        }

                        if (StreamingPlatforms.ContainsPlatform(parameters.Platform, StreamingPlatformTypeEnum.Trovo))
                        {
                            if (ServiceManager.Get<TrovoSession>().ChannelEmotes.TryGetValue(split, out TrovoChatEmoteViewModel trovoChannelEmote))
                            {
                                emoteURLs.Add(trovoChannelEmote.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }
                            else if (ServiceManager.Get<TrovoSession>().EventEmotes.TryGetValue(split, out TrovoChatEmoteViewModel trovoEventEmote))
                            {
                                emoteURLs.Add(trovoEventEmote.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }
                            else if (ServiceManager.Get<TrovoSession>().GlobalEmotes.TryGetValue(split, out TrovoChatEmoteViewModel trovoGlobalEmote))
                            {
                                emoteURLs.Add(trovoGlobalEmote.OverlayAnimatedOrStaticImageURL);
                                continue;
                            }
                        }

                        if (ServiceManager.Get<BetterTTVService>().BetterTTVEmotes.TryGetValue(split, out BetterTTVEmoteModel bttvEmote))
                        {
                            emoteURLs.Add(bttvEmote.OverlayAnimatedOrStaticImageURL);
                            continue;
                        }
                        else if (ServiceManager.Get<FrankerFaceZService>().FrankerFaceZEmotes.TryGetValue(split, out FrankerFaceZEmoteModel ffzEmote))
                        {
                            emoteURLs.Add(ffzEmote.OverlayAnimatedOrStaticImageURL);
                            continue;
                        }

                        if (Uri.IsWellFormedUriString(split, UriKind.Absolute))
                        {
                            if (allowURLs)
                            {
                                emoteURLs.Add(split);
                            }
                            continue;
                        }

                        if (allowEmoji)
                        {
                            emoteURLs.AddRange(OverlayEmoteEffectV3Model.GetEmojisFromText(split));
                        }
                    }
                }
            }

            return emoteURLs;
        }

        public static IEnumerable<string> GetEmojisFromText(string text)
        {
            List<string> emoteURLs = new List<string>();
            for (int i = 0; i < text.Length; i++)
            {
                if (ModerationService.EmojiRegex.IsMatch(text[i].ToString()))
                {
                    emoteURLs.Add(EmojiURLPrefix + text[i]);
                }
                else if (i + 1 < text.Length && ModerationService.EmojiRegex.IsMatch(text[i].ToString() + text[i + 1].ToString()))
                {
                    emoteURLs.Add(EmojiURLPrefix + text[i] + text[i + 1]);
                    i++;
                }
            }
            return emoteURLs;
        }

        [DataMember]
        public string EmoteText { get; set; }

        [DataMember]
        public OverlayEmoteEffectV3AnimationType AnimationType { get; set; }

        [DataMember]
        public int PerEmoteShown { get; set; }
        [DataMember]
        public int MaxAmountShown { get; set; }

        [DataMember]
        public int EmoteWidth { get; set; }
        [DataMember]
        public int EmoteHeight { get; set; }

        [DataMember]
        public bool AllowURLs { get; set; }
        [DataMember]
        public bool AllowEmoji { get; set; }

        public OverlayEmoteEffectV3Model() : base(OverlayItemV3Type.EmoteEffect) { }

        public override Dictionary<string, object> GetGenerationProperties()
        {
            Dictionary<string, object> properties = base.GetGenerationProperties();
            properties[EmotesPropertyName] = string.Empty;

            OverlayEmoteEffectV3AnimationType animationType = this.AnimationType;
            if (this.AnimationType == OverlayEmoteEffectV3AnimationType.Random)
            {
                animationType = OverlayEmoteEffectV3Model.ValidAnimationTypes.Random();
            }

            properties[nameof(this.AnimationType)] = animationType.ToString();
            properties[OverlayEmoteEffectV3Model.IncludeDelayPropertyName] = (OverlayEmoteEffectV3Model.DontDelayAnimations.Contains(animationType) ? false : true).ToString().ToLower();
            properties[nameof(this.EmoteWidth)] = this.EmoteWidth;
            properties[nameof(this.EmoteHeight)] = this.EmoteHeight;
            properties[nameof(this.PerEmoteShown)] = this.PerEmoteShown;
            properties[nameof(this.MaxAmountShown)] = this.MaxAmountShown;

            return properties;
        }

        public override async Task ProcessGenerationProperties(Dictionary<string, object> properties, CommandParametersModel parameters)
        {
            string text = await SpecialIdentifierStringBuilder.ProcessSpecialIdentifiers(this.EmoteText, parameters);
            IEnumerable<string> emoteURLs = OverlayEmoteEffectV3Model.GetEmoteURLs(text, parameters, this.AllowURLs, this.AllowEmoji);
            properties[EmotesPropertyName] = $"\"{string.Join("\", \"", emoteURLs)}\"";
        }
    }
}
