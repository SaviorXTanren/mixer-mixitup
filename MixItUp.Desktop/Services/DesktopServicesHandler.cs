using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Services.Mixer;
using MixItUp.Desktop.Audio;
using MixItUp.Desktop.Files;
using MixItUp.Desktop.Services.DeveloperAPI;
using MixItUp.Input;
using MixItUp.OBS;
using MixItUp.OvrStream;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopServicesHandler : ServicesHandlerBase
    {
        public void Initialize()
        {
            this.MixItUpService = new MixItUpService();
            this.MixerStatus = new MixerStatusService();

            this.User = new UserService();
            this.Chat = new ChatService();

            this.Settings = new DesktopSettingsService();
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

            this.StreamlabsOBS = new StreamlabsOBSService();
            this.XSplit = new XSplitService("http://localhost:8211/");
        }

        public override async Task Close()
        {
            await this.Overlay.Disconnect();
            await this.OvrStream.Disconnect();
            await this.DisconnectOBSStudio();
            await this.DisconnectDeveloperAPI();
            await this.DisconnectTelemetryService();
        }

        public override async Task<bool> InitializeOBSWebsocket()
        {
            if (this.OBSStudio == null)
            {
                this.OBSStudio = new OBSService(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                ExternalServiceResult result = await this.OBSStudio.Connect();
                if (result.Success)
                {
                    this.OBSStudio.Connected += OBSWebsocket_Connected;
                    this.OBSStudio.Disconnected += OBSWebsocket_Disconnected;
                    return true;
                }
                this.OBSStudio = null;
            }
            return false;
        }

        public override async Task DisconnectOBSStudio()
        {
            if (this.OBSStudio != null)
            {
                this.OBSStudio.Connected -= OBSWebsocket_Connected;
                this.OBSStudio.Disconnected -= OBSWebsocket_Disconnected;
                await this.OBSStudio.Disconnect();
                this.OBSStudio = null;
            }
        }

        public override async Task<bool> InitializeDeveloperAPI()
        {
            return await Task.Run(() =>
            {
                if (this.DeveloperAPI == null)
                {
                    this.DeveloperAPI = new WindowsDeveloperAPIService();
                    this.DeveloperAPI.Start();
                }
                return true;
            });
        }

        public override async Task<bool> InitializeTelemetryService()
        {
            return await Task.Run(() =>
            {
                if (this.Telemetry == null)
                {
                    this.Telemetry = new DesktopTelemetryService();
                    this.Telemetry.Start();
                }
                return true;
            });
        }

        public override async Task DisconnectDeveloperAPI()
        {
            await Task.Run(() =>
            {
                if (this.DeveloperAPI != null)
                {
                    this.DeveloperAPI.End();
                    this.DeveloperAPI = null;
                }
            });
        }

        public override async Task DisconnectTelemetryService()
        {
            await Task.Run(() =>
            {
                if (this.Telemetry != null)
                {
                    this.Telemetry.End();
                    this.Telemetry = null;
                }
            });
        }

        private void OverlayServer_OnWebSocketConnectedOccurred(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Overlay");
        }

        private void OverlayServer_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Overlay");
        }

        private void OBSWebsocket_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("OBS");
        }

        private void OBSWebsocket_Disconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("OBS");
        }

        private void OvrStreamWebsocket_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("OvrStream");
        }

        private void OvrStreamWebsocket_Disconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("OvrStream");
        }
    }
}
