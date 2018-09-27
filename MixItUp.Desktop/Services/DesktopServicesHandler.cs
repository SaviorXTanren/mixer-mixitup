using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Desktop.Audio;
using MixItUp.Desktop.Files;
using MixItUp.Input;
using MixItUp.OBS;
using MixItUp.Overlay;
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

            this.Settings = new DesktopSettingsService();
            this.FileService = new WindowsFileService();
            this.InputService = new WindowsInputService();
            this.AudioService = new AudioService();
            this.TextToSpeechService = new WindowsTextToSpeechService();
            this.SongRequestService = new SongRequestService();
            this.TranslationService = new TranslationService();
        }

        public override async Task Close()
        {
            await this.DisconnectOverlayServer();
            await this.DisconnectOBSStudio();
            await this.DisconnectStreamlabsOBSService();
            await this.DisconnectXSplitServer();
            await this.DisconnectDeveloperAPI();
            await this.DisconnectTelemetryService();
        }

        public override async Task<bool> InitializeOverlayServer()
        {
            if (this.OverlayServer == null)
            {
                this.OverlayServer = new OverlayWebServer();
                if (await this.OverlayServer.Initialize())
                {
                    this.OverlayServer.OnWebSocketConnectedOccurred += OverlayServer_OnWebSocketConnectedOccurred;
                    this.OverlayServer.OnWebSocketDisconnectedOccurred += OverlayServer_OnWebSocketDisconnectedOccurred;
                    return true;
                }
            }
            await this.DisconnectOverlayServer();
            return false;
        }

        public override async Task DisconnectOverlayServer()
        {
            if (this.OverlayServer != null)
            {
                this.OverlayServer.OnWebSocketConnectedOccurred -= OverlayServer_OnWebSocketConnectedOccurred;
                this.OverlayServer.OnWebSocketDisconnectedOccurred -= OverlayServer_OnWebSocketDisconnectedOccurred;

                await this.OverlayServer.Disconnect();
                this.OverlayServer = null;
            }
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
                this.XSplitServer = new XSplitWebServer("http://localhost:8211/");
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

        public override async Task<bool> InitializeStreamlabs()
        {
            this.Streamlabs = (ChannelSession.Settings.StreamlabsOAuthToken != null) ? new StreamlabsService(ChannelSession.Settings.StreamlabsOAuthToken) : new StreamlabsService();
            if (await this.Streamlabs.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectStreamlabs();
            }
            return false;
        }

        public override async Task DisconnectStreamlabs()
        {
            if (this.Streamlabs != null)
            {
                await this.Streamlabs.Disconnect();
                this.Streamlabs = null;
                ChannelSession.Settings.StreamlabsOAuthToken = null;
            }
        }

        public override async Task<bool> InitializeGameWisp()
        {
            this.GameWisp = (ChannelSession.Settings.GameWispOAuthToken != null) ? new GameWispService(ChannelSession.Settings.GameWispOAuthToken) : new GameWispService();
            if (await this.GameWisp.Connect() && this.GameWisp.ChannelInfo != null)
            {
                this.GameWisp.OnWebSocketConnectedOccurred += GameWisp_OnWebSocketConnectedOccurred;
                this.GameWisp.OnWebSocketDisconnectedOccurred += GameWisp_OnWebSocketDisconnectedOccurred;
                return true;
            }
            else
            {
                await this.DisconnectGameWisp();
            }
            return false;
        }

        public override async Task DisconnectGameWisp()
        {
            if (this.GameWisp != null)
            {
                this.GameWisp.OnWebSocketConnectedOccurred -= GameWisp_OnWebSocketConnectedOccurred;
                this.GameWisp.OnWebSocketDisconnectedOccurred -= GameWisp_OnWebSocketDisconnectedOccurred;
                await this.GameWisp.Disconnect();
                this.GameWisp = null;
                ChannelSession.Settings.GameWispOAuthToken = null;
            }
        }

        public override async Task<bool> InitializeGawkBox(string gawkBoxID = "")
        {
            this.GawkBox = (ChannelSession.Settings.GawkBoxOAuthToken != null) ? new GawkBoxService(ChannelSession.Settings.GawkBoxOAuthToken) : new GawkBoxService(gawkBoxID);
            if (await this.GawkBox.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectGawkBox();
            }
            return false;
        }

        public override async Task DisconnectGawkBox()
        {
            if (this.GawkBox != null)
            {
                await this.GawkBox.Disconnect();
                this.GawkBox = null;
                ChannelSession.Settings.GawkBoxOAuthToken = null;
            }
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

        public override async Task<bool> InitializeSpotify()
        {
            this.Spotify = (ChannelSession.Settings.SpotifyOAuthToken != null) ? new SpotifyService(ChannelSession.Settings.SpotifyOAuthToken) : new SpotifyService();
            if (await this.Spotify.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectSpotify();
            }
            return false;
        }

        public override async Task DisconnectSpotify()
        {
            if (this.Spotify != null)
            {
                await this.Spotify.Disconnect();
                this.Spotify = null;
                ChannelSession.Settings.SpotifyOAuthToken = null;
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

        public override async Task<bool> InitializeTiltify(string authorizationCode = null)
        {
            this.Tiltify = (ChannelSession.Settings.TiltifyOAuthToken != null) ? new TiltifyService(ChannelSession.Settings.TiltifyOAuthToken) : new TiltifyService(authorizationCode);
            if (await this.Tiltify.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectTiltify();
            }
            return false;
        }

        public override async Task DisconnectTiltify()
        {
            if (this.Tiltify != null)
            {
                await this.Tiltify.Disconnect();
                this.Tiltify = null;
                ChannelSession.Settings.TiltifyOAuthToken = null;
                ChannelSession.Settings.TiltifyCampaign = 0;
            }
        }

        public override async Task<bool> InitializeStreamDeck()
        {
            this.StreamDeck = (ChannelSession.Settings.StreamDeckDeviceName != null) ? new StreamDeckService(ChannelSession.Settings.StreamDeckDeviceName) : new StreamDeckService();
            if (await this.StreamDeck.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectStreamDeck();
            }
            return false;
        }

        public override async Task DisconnectStreamDeck()
        {
            if (this.StreamDeck != null)
            {
                await this.StreamDeck.Disconnect();
                this.StreamDeck = null;
                ChannelSession.Settings.StreamDeckDeviceName = null;
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

        private void GameWisp_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("GameWisp");
        }

        private void GameWisp_OnWebSocketDisconnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("GameWisp");
        }
    }
}
