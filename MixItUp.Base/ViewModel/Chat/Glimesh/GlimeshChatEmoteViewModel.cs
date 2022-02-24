using Glimesh.Base.Models.Clients.Chat;

namespace MixItUp.Base.ViewModel.Chat.Glimesh
{
    public class GlimeshChatEmoteViewModel : IChatEmoteViewModel
    {
        public string ID { get { return this.Name; } }
        public string Name { get; set; }
        public string ImageURL { get; set; }

        public GlimeshChatEmoteViewModel(ChatMessageTokenModel token)
        {
            this.Name = token.text;
            this.ImageURL = token.src;
        }
    }
}
