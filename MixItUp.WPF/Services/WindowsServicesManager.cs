using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using MixItUp.WPF.Services.DeveloperAPI;
using StreamingClient.Base.Util;
using System;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{
    public class WindowsServicesManager : ServicesManagerBase
    {
        public void Initialize()
        {
            this.Secrets = new SecretsService();

            var mixItUpService = new MixItUpService();
            this.MixItUpService = mixItUpService;
            this.CommunityCommandsService = mixItUpService;
            this.WebhookService = mixItUpService;

            this.User = new UserService();
            this.Chat = new ChatService();
            this.Events = new EventService();
            this.Alerts = new AlertsService();

            this.Command = new CommandService();
            this.Settings = new SettingsService();
            this.Statistics = new StatisticsService();
            this.Database = new WindowsDatabaseService();
            this.Moderation = new ModerationService();
            this.FileService = new WindowsFileService();
            this.InputService = new WindowsInputService();
            this.Timers = new TimerService();
            this.GameQueueService = new GameQueueService();
            this.Image = new WindowsImageService();
            this.AudioService = new WindowsAudioService();
            this.GiveawayService = new GiveawayService();
            this.SerialService = new SerialService();
            this.DeveloperAPI = new WindowsDeveloperAPIService();
            this.Telemetry = new WindowsTelemetryService();

            this.Streamlabs = new StreamlabsService(new WindowsSocketIOConnection());
            this.StreamElements = new StreamElementsService(new WindowsSocketIOConnection());
            this.Rainmaker = new RainmakerService(new WindowsSocketIOConnection());
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
            this.PixelChat = new PixelChatService();
            this.VTubeStudio = new VTubeStudioService();
            this.Voicemod = new VoicemodEmptyService();
            this.OvrStream = new WindowsOvrStreamService();
            this.Overlay = new OverlayService();

            this.OBSStudio = new WindowsOBSService();
            this.StreamlabsOBS = new StreamlabsOBSService();
            this.XSplit = new XSplitService("http://localhost:8211/");

            this.TwitchStatus = new TwitchStatusService();

            this.Settings.Initialize();
            FileSerializerHelper.Initialize(this.FileService);
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