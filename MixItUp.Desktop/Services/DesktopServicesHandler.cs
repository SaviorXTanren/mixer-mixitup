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
using MixItUp.XSplit;
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
            this.TextToSpeechService = new WindowsTextToSpeechService();
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
            this.OverlayServers = new OverlayServiceManager();
            this.MixrElixr = new MixrElixrService();
        }

        public override async Task Close()
        {
            await this.DisconnectOverlayServer();
            await this.DisconnectOvrStream();
            await this.DisconnectOBSStudio();
            await this.DisconnectStreamlabsOBSService();
            await this.DisconnectXSplitServer();
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
            if (this.OBSWebsocket == null)
            {
                this.OBSWebsocket = new OBSService(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                if (await this.OBSWebsocket.Connect())
                {
                    this.OBSWebsocket.Connected += OBSWebsocket_Connected;
                    this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;
                    return true;
                }
                this.OBSWebsocket = null;
            }
            return false;
        }

        public override async Task DisconnectOBSStudio()
        {
            if (this.OBSWebsocket != null)
            {
                this.OBSWebsocket.Connected -= OBSWebsocket_Connected;
                this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                await this.OBSWebsocket.Disconnect();
                this.OBSWebsocket = null;
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

        public override async Task<bool> InitializeStreamlabsOBSService()
        {
            if (this.StreamlabsOBSService == null)
            {
                this.StreamlabsOBSService = new StreamlabsOBSService();
                if (await this.StreamlabsOBSService.Connect())
                {
                    this.StreamlabsOBSService.Connected += StreamlabsOBSService_Connected;
                    this.StreamlabsOBSService.Disconnected += StreamlabsOBSService_Disconnected;
                    return true;
                }
                this.StreamlabsOBSService = null;
            }
            return false;
        }

        public override async Task DisconnectStreamlabsOBSService()
        {
            if (this.StreamlabsOBSService != null)
            {
                this.StreamlabsOBSService.Connected -= StreamlabsOBSService_Connected;
                this.StreamlabsOBSService.Disconnected -= StreamlabsOBSService_Disconnected;
                await this.StreamlabsOBSService.Disconnect();
                this.StreamlabsOBSService = null;
            }
        }

        public override async Task<bool> InitializeXSplitServer()
        {
            if (this.XSplitServer == null)
            {
                this.XSplitServer = new XSplitWebSocketHttpListenerServer("http://localhost:8211/");
                if (await this.XSplitServer.Connect())
                {
                    this.XSplitServer.Connected += XSplitServer_Connected;
                    this.XSplitServer.Disconnected += XSplitServer_Disconnected;
                    return true;
                }
            }
            await this.DisconnectXSplitServer();
            return true;
        }

        public override async Task DisconnectXSplitServer()
        {
            if (this.XSplitServer != null)
            {
                this.XSplitServer.Connected -= XSplitServer_Connected;
                this.XSplitServer.Disconnected -= XSplitServer_Disconnected;
                await this.XSplitServer.Disconnect();
                this.XSplitServer = null;
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

        public override async Task<bool> InitializeDiscord()
        {
            this.Discord = (ChannelSession.Settings.DiscordOAuthToken != null) ? new DiscordService(ChannelSession.Settings.DiscordOAuthToken) : new DiscordService();
            if (await this.Discord.Connect() && this.Discord.Server != null && this.Discord.User != null)
            {
                return true;
            }
            else
            {
                await this.DisconnectDiscord();
            }
            return false;
        }

        public override async Task DisconnectDiscord()
        {
            if (this.Discord != null)
            {
                await this.Discord.Disconnect();
                this.Discord = null;
                ChannelSession.Settings.DiscordOAuthToken = null;
            }
        }

        public override async Task<bool> InitializePatreon()
        {
            this.Patreon = (ChannelSession.Settings.PatreonOAuthToken != null) ? new PatreonService(ChannelSession.Settings.PatreonOAuthToken) : new PatreonService();
            if (await this.Patreon.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectPatreon();
            }
            return false;
        }

        public override async Task DisconnectPatreon()
        {
            if (this.Patreon != null)
            {
                await this.Patreon.Disconnect();
                this.Patreon = null;
                ChannelSession.Settings.PatreonOAuthToken = null;
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

        private void XSplitServer_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("XSplit");
        }

        private void XSplitServer_Disconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("XSplit");
        }

        private void StreamlabsOBSService_Connected(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Streamlabs OBS");
        }

        private void StreamlabsOBSService_Disconnected(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("Streamlabs OBS");
        }
    }
}
