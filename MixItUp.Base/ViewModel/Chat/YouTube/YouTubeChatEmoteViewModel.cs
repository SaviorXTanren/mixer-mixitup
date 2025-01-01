using MixItUp.Base.Model.YouTube;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public YouTubeChatEmoteModel Emote { get; set; }

        public YouTubeChatEmoteViewModel(YouTubeChatEmoteModel emote)
        {
            this.Emote = emote;
            this.ID = this.Emote.emojiId;
            this.Name = this.Emote.shortcuts?.FirstOrDefault();
            this.ImageURL = this.Emote.image?.thumbnails?.FirstOrDefault()?.url;
        }
    }
}
