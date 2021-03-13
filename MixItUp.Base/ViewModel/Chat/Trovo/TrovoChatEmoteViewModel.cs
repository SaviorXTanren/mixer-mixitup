using Trovo.Base.Models.Chat;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatEmoteViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public bool IsGif { get; set; }

        public TrovoChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.Name = emote.name;
            this.Url = emote.url;
        }

        public TrovoChatEmoteViewModel(GlobalChatEmoteModel emote)
            : this((ChatEmoteModel)emote)
        {
            if (!string.IsNullOrEmpty(emote.gifp))
            {
                this.Url = emote.gifp;
                this.IsGif = true;
            }
        }
    }
}
