using Trovo.Base.Models.Chat;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatEmoteViewModel : IChatEmoteViewModel
    {
        public string ID { get { return this.Name; } }
        public string Name { get; set; }
        public string ImageURL { get; set; }

        public bool IsGif { get; set; }

        public TrovoChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.Name = emote.name;
            this.ImageURL = emote.url;
        }

        public TrovoChatEmoteViewModel(GlobalChatEmoteModel emote)
            : this((ChatEmoteModel)emote)
        {
            if (!string.IsNullOrEmpty(emote.gifp))
            {
                this.ImageURL = emote.gifp;
                this.IsGif = true;
            }
        }
    }
}
