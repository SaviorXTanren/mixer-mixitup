using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class ServicesHandlerBase
    {
        public IMixItUpService MixItUpService { get; protected set; }

        public IMixerStatusService MixerStatus { get; protected set; }

        public ISettingsService Settings { get; protected set; }
        public IFileService FileService { get; protected set; }
        public IAudioService AudioService { get; protected set; }
        public IInputService InputService { get; protected set; }
        public ITextToSpeechService TextToSpeechService { get; protected set; }
        public ITranslationService TranslationService { get; protected set; }
        public ISongRequestService SongRequestService { get; protected set; }

        public IOverlayService OverlayServer { get; protected set; }
        public IStreamingSoftwareService OBSWebsocket { get; protected set; }
        public IStreamingSoftwareService StreamlabsOBSService { get; protected set; }
        public IStreamingSoftwareService XSplitServer { get; protected set; }
        public IDeveloperAPIService DeveloperAPI { get; protected set; }
        public IStreamlabsService Streamlabs { get; protected set; }
        public IGameWispService GameWisp { get; protected set; }
        public IGawkBoxService GawkBox { get; protected set; }
        public ITwitterService Twitter { get; protected set; }
        public ISpotifyService Spotify { get; protected set; }
        public IDiscordService Discord { get; protected set; }
        public ITiltifyService Tiltify { get; protected set; }

        public abstract Task Close();

        public abstract Task<bool> InitializeOverlayServer();
        public abstract Task DisconnectOverlayServer();

        public abstract Task<bool> InitializeOBSWebsocket();
        public abstract Task DisconnectOBSStudio();

        public abstract Task<bool> InitializeStreamlabsOBSService();
        public abstract Task DisconnectStreamlabsOBSService();

        public abstract Task<bool> InitializeXSplitServer();
        public abstract Task DisconnectXSplitServer();

        public abstract Task<bool> InitializeDeveloperAPI();
        public abstract Task DisconnectDeveloperAPI();

        public abstract Task<bool> InitializeStreamlabs();
        public abstract Task DisconnectStreamlabs();

        public abstract Task<bool> InitializeGameWisp();
        public abstract Task DisconnectGameWisp();

        public abstract Task<bool> InitializeGawkBox(string gawkBoxID = "");
        public abstract Task DisconnectGawkBox();

        public abstract Task<bool> InitializeTwitter();
        public abstract Task DisconnectTwitter();

        public abstract Task<bool> InitializeSpotify();
        public abstract Task DisconnectSpotify();

        public abstract Task<bool> InitializeDiscord();
        public abstract Task DisconnectDiscord();

        public abstract Task<bool> InitializeTiltify(string authorizationCode = null);
        public abstract Task DisconnectTiltify();
    }
}
