namespace MixItUp.Base.ViewModel.Services
{
    public class StreamDeckServiceControlViewModel : ServiceControlViewModelBase
    {
        public override string WikiPageName { get { return "stream-deck"; } }

        public StreamDeckServiceControlViewModel() : base(Resources.StreamDeck) { }
    }
}
