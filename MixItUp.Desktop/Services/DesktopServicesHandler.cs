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
            this.ImageManipulationService = new DesktopImageManipulationService();
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
            this.OverlayServers = new OverlayServiceManager();
            this.MixrElixr = new MixrElixrService();

            this.StreamlabsOBS = new StreamlabsOBSService();
            this.XSplit = new XSplitService("http://localhost:8211/");
        }

        public override async Task Close()
        {
            await this.DisconnectOverlayServer();
            await this.DisconnectOvrStream();
            await this.DisconnectOBSStudio();
            await this.DisconnectDeveloperAPI();
            await this.DisconnectTelemetryService();
        }

        public override async Task<bool> InitializeOverlayServer()
        {
            foreach (var kvp in ChannelSession.AllOverlayNameAndPorts)
            {
                if (!await ChannelSession.Services.OverlayServers.AddOverlay(kvp.Key, kvp.Value))
                {
                    await this.DisconnectOverlayServer();
                    return false;
                }
            }
            this.OverlayServers.Initialize();
            return true;
        }

        public override async Task DisconnectOverlayServer()
        {
            this.OverlayServers.Disable();
            await ChannelSession.Services.OverlayServers.RemoveAllOverlays();
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

        public override async Task<bool> InitializeOvrStream()
        {
            if (this.OvrStreamWebsocket == null)
            {
                this.OvrStreamWebsocket = new OvrStreamService(ChannelSession.Settings.OvrStreamServerIP);
                if (await this.OvrStreamWebsocket.Connect())
                {
                    return true;
                }
                else
                {
                    await this.DisconnectOvrStream();
                }
            }
            return false;
        }

        public override async Task DisconnectOvrStream()
        {
            if (this.OvrStreamWebsocket != null)
            {
                await this.OvrStreamWebsocket.Disconnect();
                this.OvrStreamWebsocket = null;
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

        public override async Task<bool> InitializeTwitter()
        {
            this.Twitter = (ChannelSession.Settings.TwitterOAuthToken != null) ? new TwitterService(ChannelSession.Settings.TwitterOAuthToken) : new TwitterService();
            if (await this.Twitter.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectTwitter();
            }
            return false;
        }

        public override async Task DisconnectTwitter()
        {
            if (this.Twitter != null)
            {
                await this.Twitter.Disconnect();
                this.Twitter = null;
                ChannelSession.Settings.TwitterOAuthToken = null;
            }
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
