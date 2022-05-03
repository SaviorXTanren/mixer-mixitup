using MixItUp.Base.Services.YouTube;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatEmoteViewModel : IChatEmoteViewModel
    {
        public YouTubeChatEmoteModel Emote { get; set; }

        public string ID { get { return this.Emote.emojiId; } }

        public string Name { get { return this.Emote.shortcuts?.FirstOrDefault(); } }

        public string ImageURL { get { return this.Emote.image?.thumbnails?.FirstOrDefault()?.url; } }

        public YouTubeChatEmoteViewModel(YouTubeChatEmoteModel emote)
        {
            this.Emote = emote;
        }
    }
}
