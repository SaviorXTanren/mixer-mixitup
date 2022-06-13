using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.Services;
using MixItUp.Base.Services.YouTube;
using MixItUp.Base.ViewModel.User;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private List<string> Arguments = new List<string>();

        public YouTubeChatMessageViewModel(LiveChatMessage message, UserV2ViewModel user = null)
            : base(message.Id, StreamingPlatformTypeEnum.YouTube, user)
        {
            string[] parts = message.Snippet.DisplayMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string currentUserTag = string.Empty;

            foreach (string part in parts)
            {
                this.AddStringMessagePart(part);
                this.Arguments.Add(part);

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
                else if (part.StartsWith("@") || !string.IsNullOrEmpty(currentUserTag))
                {
                    if (part.StartsWith("@"))
                    {
                        currentUserTag = part;
                    }
                    else
                    {
                        currentUserTag += " " + part;
                    }

                    UserV2ViewModel userTag = ServiceManager.Get<UserService>().GetActiveUserByPlatformUsername(StreamingPlatformTypeEnum.YouTube, currentUserTag);
                    if (userTag != null)
                    {
                        int spaces = currentUserTag.Count(c => c == ' ') + 1;
                        for (int i = 0; i < spaces; i++)
                        {
                            this.Arguments.RemoveAt(this.Arguments.Count - 1);
                        }
                        this.Arguments.Add(currentUserTag);

                        currentUserTag = string.Empty;
                    }
                }
            }
        }

        public override IEnumerable<string> ToArguments() { return this.Arguments; }
    }
}
