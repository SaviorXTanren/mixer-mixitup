using MixItUp.Base.Services.YouTube;
using System.Linq;

namespace MixItUp.Base.ViewModel.Chat.YouTube
{
    public class YouTubeChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public YouTubeChatEmoteModel Emote { get; set; }

        public override string ID { get; protected set; }

        public override string Name { get; protected set; }

        public override string ImageURL { get; protected set; }

        public YouTubeChatEmoteViewModel(YouTubeChatEmoteModel emote)
        {
            this.Emote = emote;
            this.ID = this.Emote.emojiId;
            this.Name = this.Emote.shortcuts?.FirstOrDefault();
            this.ImageURL = this.Emote.image?.thumbnails?.FirstOrDefault()?.url;
        }
    }
}
