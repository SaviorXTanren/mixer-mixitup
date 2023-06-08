using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatMessageViewModel : UserChatMessageViewModel
    {
        // YouTube Emojis:
        // https://emojipedia.org/youtube/
        // https://emojis.wiki/youtube/
        // https://stackoverflow.com/questions/64726611/how-to-get-a-list-of-youtube-channel-emojis
        // 
        // https://www.gstatic.com/youtube/img/emojis/emojis-svg-5.json

        public YouTubeChatMessageViewModel(LiveChatMessage message, UserV2ViewModel user = null)
            : base(message.Id, StreamingPlatformTypeEnum.YouTube, user)
        {
            string[] parts = message.Snippet.DisplayMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                this.AddStringMessagePart(part);

                if (part.StartsWith(":"))
                {
                    string emote = part.Substring(1);
                    if (ServiceManager.Has<YouTubeChatService>())
                    {
                        if (ServiceManager.Get<YouTubeChatService>().EmoteDictionary.ContainsKey(emote))
                        {
                            this.MessageParts[this.MessageParts.Count - 1] = new YouTubeChatEmoteViewModel(ServiceManager.Get<YouTubeChatService>().EmoteDictionary[emote]);
                        }
                    }
                }
                else if (ChannelSession.Settings.ShowBetterTTVEmotes && ServiceManager.Get<BetterTTVService>().BetterTTVEmotes.ContainsKey(part))
                {
                    this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<BetterTTVService>().BetterTTVEmotes[part];
                }
            }
        }
    }
}
