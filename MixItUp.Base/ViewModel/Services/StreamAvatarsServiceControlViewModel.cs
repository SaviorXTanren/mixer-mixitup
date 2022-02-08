namespace MixItUp.Base.ViewModel.Services
{
    public class StreamAvatarsServiceControlViewModel : ServiceControlViewModelBase
    {
        public override string WikiPageName { get { return "stream-avatars"; } }

        public StreamAvatarsServiceControlViewModel() : base(Resources.StreamAvatars) { }
    }
}
