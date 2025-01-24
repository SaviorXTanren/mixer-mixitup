using MixItUp.Base.Model.Trovo.Chat;

namespace MixItUp.Base.ViewModel.Chat.Trovo
{
    public class TrovoChatEmoteViewModel : ChatEmoteViewModelBase
    {
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
                this.AnimatedImageURL = emote.gifp;
                this.IsAnimated = true;
            }
        }
    }
}
