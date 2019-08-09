using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class ServicesHandlerBase
    {
        public IMixItUpService MixItUpService { get; protected set; }

        public IMixerStatusService MixerStatus { get; protected set; }

        public IChatService ChatService { get; protected set; }

        public ISettingsService Settings { get; protected set; }
        public IFileService FileService { get; protected set; }
        public IAudioService AudioService { get; protected set; }
        public IInputService InputService { get; protected set; }
        public ITimerService TimerService { get; protected set; }
        public IGameQueueService GameQueueService { get; protected set; }
        public IImageManipulationService ImageManipulationService { get; protected set; }
        public ITextToSpeechService TextToSpeechService { get; protected set; }
        public ITranslationService TranslationService { get; protected set; }
        public ISongRequestService SongRequestService { get; protected set; }
        public IGiveawayService GiveawayService { get; protected set; }
        public ISerialService SerialService { get; protected set; }
        public LocalRemoteServiceBase RemoteService { get; protected set; }

        public IOverlayServiceManager OverlayServers { get; protected set; }
        public IStreamingSoftwareService OBSWebsocket { get; protected set; }
        public IStreamingSoftwareService StreamlabsOBSService { get; protected set; }
        public IStreamingSoftwareService XSplitServer { get; protected set; }
        public IDeveloperAPIService DeveloperAPI { get; protected set; }
        public IStreamlabsService Streamlabs { get; protected set; }
        public ITwitterService Twitter { get; protected set; }
        public SpotifyService Spotify { get; protected set; }
        public IDiscordService Discord { get; protected set; }
        public ITiltifyService Tiltify { get; protected set; }
        public IExtraLifeService ExtraLife { get; protected set; }
        public ITelemetryService Telemetry { get; protected set; }
        public IScoutService Scout { get; protected set; }
        public ITipeeeStreamService TipeeeStream { get; protected set; }
        public ITreatStreamService TreatStream { get; protected set; }
        public IStreamJarService StreamJar { get; protected set; }
        public IPatreonService Patreon { get; protected set; }
        public IOvrStreamService OvrStreamWebsocket { get; protected set; }
        public IIFTTTService IFTTT { get; protected set; }
        public IStreamlootsService Streamloots { get; protected set; }

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

        public abstract Task<bool> InitializeTelemetryService();
        public abstract Task DisconnectTelemetryService();

        public abstract Task<bool> InitializeStreamlabs();
        public abstract Task DisconnectStreamlabs();

        public abstract Task<bool> InitializeTwitter();
        public abstract Task DisconnectTwitter();

        public abstract Task<bool> InitializeSpotify();
        public abstract Task DisconnectSpotify();

        public abstract Task<bool> InitializeDiscord();
        public abstract Task DisconnectDiscord();

        public abstract Task<bool> InitializeTiltify(string authorizationCode = null);
        public abstract Task DisconnectTiltify();

        public abstract Task<bool> InitializeExtraLife();
        public abstract Task DisconnectExtraLife();

        public abstract Task<bool> InitializeTipeeeStream();
        public abstract Task DisconnectTipeeeStream();

        public abstract Task<bool> InitializeTreatStream();
        public abstract Task DisconnectTreatStream();

        public abstract Task<bool> InitializeStreamJar();
        public abstract Task DisconnectStreamJar();

        public abstract Task<bool> InitializePatreon();
        public abstract Task DisconnectPatreon();

        public abstract Task<bool> InitializeOvrStream();
        public abstract Task DisconnectOvrStream();

        public abstract Task<bool> InitializeIFTTT(string key = null);
        public abstract Task DisconnectIFTTT();

        public abstract Task<bool> InitializeStreamloots(string streamlootsID = null);
        public abstract Task DisconnectStreamloots();
    }
}
