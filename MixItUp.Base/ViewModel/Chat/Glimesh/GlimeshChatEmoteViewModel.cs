using Glimesh.Base.Models.Clients.Chat;

namespace MixItUp.Base.ViewModel.Chat.Glimesh
{
    public class GlimeshChatEmoteViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public GlimeshChatEmoteViewModel(ChatMessageTokenModel token)
        {
            this.Name = token.text;
            this.Url = token.url;
        }

        public bool IsGif { get { return this.Url.Contains(".gif"); } }
    }
}
