using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public abstract class ServicesHandlerBase
    {
        public IMixItUpService MixItUpService { get; protected set; }

        public IMixerStatusService MixerStatus { get; protected set; }

        public IUserService User { get; protected set; }

        public IChatService Chat { get; protected set; }
        public IEventService Events { get; protected set; }

        public ISettingsService Settings { get; protected set; }
        public SecretsService Secrets { get; protected set; }
        public StatisticsService Statistics { get; protected set; }
        public IDatabaseService Database { get; protected set; }
        public IFileService FileService { get; protected set; }
        public IAudioService AudioService { get; protected set; }
        public IInputService InputService { get; protected set; }
        public ITimerService TimerService { get; protected set; }
        public IGameQueueService GameQueueService { get; protected set; }
        public IImageService Image { get; protected set; }
        public ITranslationService TranslationService { get; protected set; }
        public IGiveawayService GiveawayService { get; protected set; }
        public ISerialService SerialService { get; protected set; }
        public LocalRemoteServiceBase RemoteService { get; protected set; }

        public IOverlayService Overlay { get; protected set; }
        public IStreamingSoftwareService OBSStudio { get; protected set; }
        public IStreamingSoftwareService StreamlabsOBS { get; protected set; }
        public IStreamingSoftwareService XSplit { get; protected set; }
        public IDeveloperAPIService DeveloperAPI { get; protected set; }
        public IStreamlabsService Streamlabs { get; protected set; }
        public IStreamElementsService StreamElements { get; protected set; }
        public ITwitterService Twitter { get; protected set; }
        public IDiscordService Discord { get; protected set; }
        public ITiltifyService Tiltify { get; protected set; }
        public IExtraLifeService ExtraLife { get; protected set; }
        public ITelemetryService Telemetry { get; protected set; }
        public ITipeeeStreamService TipeeeStream { get; protected set; }
        public ITreatStreamService TreatStream { get; protected set; }
        public IStreamJarService StreamJar { get; protected set; }
        public IPatreonService Patreon { get; protected set; }
        public IOvrStreamService OvrStream { get; protected set; }
        public IIFTTTService IFTTT { get; protected set; }
        public IStreamlootsService Streamloots { get; protected set; }
        public IMixrElixrService MixrElixr { get; protected set; }
        public IJustGivingService JustGiving { get; protected set; }

        public abstract void SetSecrets(SecretsService secretsService);

        public abstract Task Close();
    }
}