using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using MixItUp.Desktop.Audio;
using MixItUp.Desktop.Files;
using MixItUp.Desktop.Services.DeveloperAPI;
using MixItUp.Input;
using MixItUp.OBS;
using MixItUp.OvrStream;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopServicesHandler : ServicesHandlerBase
    {
        public void Initialize()
        {
            this.Secrets = new SecretsService();
            this.MixItUpService = new MixItUpService();
            this.MixerStatus = new MixerStatusService();

            this.User = new UserService();
            this.Chat = new ChatService();
            this.Events = new EventService();

            this.Settings = new DesktopSettingsService();
            this.Statistics = new StatisticsService();
            this.Database = new DatabaseService();
            this.FileService = new WindowsFileService();
            this.InputService = new WindowsInputService();
            this.TimerService = new TimerService();
            this.GameQueueService = new GameQueueService();
            this.Image = new WindowsImageService();
            this.AudioService = new AudioService();
            this.GiveawayService = new GiveawayService();
            this.TranslationService = new TranslationService();
            this.SerialService = new SerialService();
            this.RemoteService = new RemoteService("https://mixitup-remote-server.azurewebsites.net/api/", "https://mixitup-remote-server.azurewebsites.net/RemoteHub");
            this.DeveloperAPI = new WindowsDeveloperAPIService();
            this.Telemetry = new DesktopTelemetryService();

            this.Streamlabs = new StreamlabsService();
            this.StreamJar = new StreamJarService();
            this.TipeeeStream = new TipeeeStreamService(new SocketIOConnection());
            this.TreatStream = new TreatStreamService(new SocketIOConnection());
            this.Streamloots = new StreamlootsService();
            this.JustGiving = new JustGivingService();
            this.Tiltify = new TiltifyService();
            this.ExtraLife = new ExtraLifeService();
            this.IFTTT = new IFTTTService();
            this.Patreon = new PatreonService();
            this.Discord = new DiscordService();
            this.Twitter = new TwitterService();
            this.OvrStream = new OvrStreamService();
            this.Overlay = new OverlayService();
            this.MixrElixr = new MixrElixrService();

            this.OBSStudio = new OBSService();
            this.StreamlabsOBS = new StreamlabsOBSService();
            this.XSplit = new XSplitService("http://localhost:8211/");
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