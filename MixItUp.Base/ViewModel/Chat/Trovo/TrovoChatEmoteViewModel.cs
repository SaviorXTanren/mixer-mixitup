using Trovo.Base.Models.Chat;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public override string ID { get; protected set; }
        public override string Name { get; protected set; }
        public override string ImageURL { get; protected set; }

        public override bool IsGIFImage { get { return this.IsGif || base.IsGIFImage; } }

        private bool IsGif { get; set; }

        public TrovoChatEmoteViewModel(ChatEmoteModel emote)
        {
            this.ID = this.Name = emote.name;
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
