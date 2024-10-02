namespace MixItUp.Base.ViewModel.Chat
{
    public abstract class ChatEmoteViewModelBase
    {
        public virtual string ID { get; protected set; }
        public virtual string Name { get; protected set; }
        public virtual string ImageURL { get; protected set; }
        public virtual string AnimatedImageURL { get; protected set; }
        public virtual string OverlayAnimatedImageURL { get { return this.AnimatedImageURL; } protected set { } }

        public virtual bool IsAnimated { get; protected set; }

        public string OverlayAnimatedOrStaticImageURL { get { return (this.IsAnimated && !string.IsNullOrEmpty(this.OverlayAnimatedImageURL)) ? this.OverlayAnimatedImageURL : this.ImageURL; } }
    }
}
