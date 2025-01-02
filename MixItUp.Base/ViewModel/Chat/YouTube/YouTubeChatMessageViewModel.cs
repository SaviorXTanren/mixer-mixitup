using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.YouTube.New;
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
            this.ProcessMessageContents(message.Snippet.DisplayMessage);
        }

        public YouTubeChatMessageViewModel(UserV2ViewModel user, string message)
            : base(string.Empty, StreamingPlatformTypeEnum.YouTube, user)
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
                    if (ServiceManager.Get<YouTubeSession>().EmoteDictionary.ContainsKey(part))
                    {
                        this.MessageParts[this.MessageParts.Count - 1] = ServiceManager.Get<YouTubeSession>().EmoteDictionary[part];
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
