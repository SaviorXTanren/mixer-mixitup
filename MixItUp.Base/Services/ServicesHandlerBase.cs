using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class ServicesHandlerBase
    {
        public IAudioService AudioService { get; protected set; }
        public IInputService InputService { get; protected set; }
        public IOverlayService OverlayServer { get; protected set; }
        public IOBSService OBSWebsocket { get; protected set; }
        public IXSplitService XSplitServer { get; protected set; }

        public abstract Task Close();

        public abstract Task<bool> InitializeInputService();

        public abstract Task<bool> InitializeAudioService();

        public abstract Task<bool> InitializeOverlayServer();
        public abstract Task DisconnectOverlayServer();

        public abstract Task<bool> InitializeOBSWebsocket();
        public abstract Task DisconnectOBSStudio();

        public abstract Task<bool> InitializeXSplitServer();
        public abstract Task DisconnectXSplitServer();
    }
}
