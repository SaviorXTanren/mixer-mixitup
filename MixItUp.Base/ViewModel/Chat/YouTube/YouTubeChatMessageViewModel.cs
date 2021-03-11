using Google.Apis.YouTube.v3.Data;
using MixItUp.Base.Model;
using MixItUp.Base.ViewModel.User;
using System;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatMessageViewModel : UserChatMessageViewModel
    {
        public YouTubeChatMessageViewModel(LiveChatMessage message, UserViewModel user = null)
            : base(message.Id, StreamingPlatformTypeEnum.YouTube, (user != null) ? user : new UserViewModel(message))
        {
            string[] parts = message.Snippet.TextMessageDetails.MessageText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                this.AddStringMessagePart(part);
            }
        }
    }
}
