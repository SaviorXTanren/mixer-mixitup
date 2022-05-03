using Glimesh.Base.Models.Clients.Chat;

namespace MixItUp.Base.ViewModel.Chat.Glimesh
{
    public class GlimeshChatEmoteViewModel : ChatEmoteViewModelBase
    {
        public override string ID { get;  protected set; }
        public override string Name { get; protected set; }
        public override string ImageURL { get; protected set; }

        public GlimeshChatEmoteViewModel(ChatMessageTokenModel token)
        {
            this.ID = this.Name = token.text;
            this.ImageURL = token.src;
        }
    }
}
