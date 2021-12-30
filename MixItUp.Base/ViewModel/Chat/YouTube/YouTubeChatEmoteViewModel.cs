using MixItUp.Base.Services.YouTube;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatEmoteViewModel
    {
        public YouTubeChatEmoteModel Emote { get; set; }

        public string Name { get { return this.Emote.shortcuts?.FirstOrDefault(); } }

        public string ImageURL { get { return this.Emote.image?.thumbnails?.FirstOrDefault()?.url; } }

        public YouTubeChatEmoteViewModel(YouTubeChatEmoteModel emote)
        {
            this.Emote = emote;
        }
    }
}
