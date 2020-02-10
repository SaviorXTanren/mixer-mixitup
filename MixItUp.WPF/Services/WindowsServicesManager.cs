using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using MixItUp.Base.Util;
using MixItUp.WPF.Services.DeveloperAPI;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsServicesManager : ServicesManagerBase
    {
        public void Initialize()
        {
            this.Secrets = new SecretsService();
            this.MixItUpService = new MixItUpService();
            this.MixerStatus = new MixerStatusService();

            this.User = new UserService();
            this.Chat = new ChatService();
            this.Events = new EventService();
            this.MixPlay = new MixerMixPlayService();

            this.Settings = new SettingsService();
            this.Statistics = new StatisticsService();
            this.Database = new WindowsDatabaseService();
            this.FileService = new WindowsFileService();
            this.InputService = new WindowsInputService();
            this.TimerService = new TimerService();
            this.GameQueueService = new GameQueueService();
            this.Image = new WindowsImageService();
            this.AudioService = new WindowsAudioService();
            this.GiveawayService = new GiveawayService();
            this.TranslationService = new TranslationService();
            this.SerialService = new SerialService();
            this.RemoteService = new LocalStreamerRemoteService("https://mixitup-remote-server.azurewebsites.net/api/", "https://mixitup-remote-server.azurewebsites.net/RemoteHub");
            this.DeveloperAPI = new WindowsDeveloperAPIService();
            this.Telemetry = new WindowsTelemetryService();

            this.Streamlabs = new StreamlabsService();
            this.StreamElements = new StreamElementsService();
            this.StreamJar = new StreamJarService();
            this.TipeeeStream = new TipeeeStreamService(new WindowsSocketIOConnection());
            this.TreatStream = new TreatStreamService(new WindowsSocketIOConnection());
            this.Streamloots = new StreamlootsService();
            this.JustGiving = new JustGivingService();
            this.Tiltify = new TiltifyService();
            this.ExtraLife = new ExtraLifeService();
            this.IFTTT = new IFTTTService();
            this.Patreon = new PatreonService();
            this.Discord = new DiscordService();
            this.Twitter = new TwitterService();
            this.OvrStream = new WindowsOvrStreamService();
            this.Overlay = new OverlayService();
            this.MixrElixr = new MixrElixrService();

            this.OBSStudio = new WindowsOBSService();
            this.StreamlabsOBS = new StreamlabsOBSService();
            this.XSplit = new XSplitService("http://localhost:8211/");

            this.Settings.Initialize();
            SerializerHelper.Initialize(this.FileService);
        }

        public override void SetSecrets(SecretsService secretsService)
        {
            this.Secrets = secretsService;
        }

        public override async Task Close()
        {
            await this.Overlay.Disconnect();
            await this.OvrStream.Disconnect();
            await this.OBSStudio.Disconnect();
            await this.DeveloperAPI.Disconnect();
            await this.Telemetry.Disconnect();
        }
    }
}