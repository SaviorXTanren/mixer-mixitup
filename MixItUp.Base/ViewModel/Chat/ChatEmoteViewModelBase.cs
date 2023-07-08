namespace MixItUp.Base.ViewModel.Chat
{
    public abstract class ChatEmoteViewModelBase
    {
        public abstract string ID { get; protected set; }
        public abstract string Name { get; protected set; }
        public abstract string ImageURL { get; protected set; }

        public virtual bool IsGIFImage { get { return this.ImageURL.Contains(".gif"); } }
        public virtual bool IsSVGImage { get { return this.ImageURL.Contains(".svg"); } }
    }
}
