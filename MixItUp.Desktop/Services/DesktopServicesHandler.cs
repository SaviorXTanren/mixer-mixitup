using MixItUp.Base;
using MixItUp.Base.Services;
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

            this.ChatService = new ChatService();

            this.Settings = new DesktopSettingsService();
            this.FileService = new WindowsFileService();
            this.InputService = new WindowsInputService();
            this.TimerService = new TimerService();
            this.GameQueueService = new GameQueueService();
            this.ImageManipulationService = new DesktopImageManipulationService();
            this.AudioService = new AudioService();
            this.TextToSpeechService = new WindowsTextToSpeechService();
            this.SongRequestService = new SongRequestService();
            this.GiveawayService = new GiveawayService();
            this.TranslationService = new TranslationService();
            this.SerialService = new SerialService();
            this.RemoteService = new RemoteService("https://mixitup-remote-server.azurewebsites.net/api/", "https://mixitup-remote-server.azurewebsites.net/RemoteHub");

            this.ExtraLife = new ExtraLifeService();
            this.OverlayServers = new OverlayServiceManager();
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

        public override async Task<bool> InitializeExtraLife()
        {
            if (await this.ExtraLife.Connect(ChannelSession.Settings.ExtraLifeTeamID, ChannelSession.Settings.ExtraLifeParticipantID, ChannelSession.Settings.ExtraLifeIncludeTeamDonations))
            {
                return true;
            }
            else
            {
                await this.DisconnectExtraLife();
            }
            return false;
        }

        public override async Task DisconnectExtraLife()
        {
            if (this.ExtraLife != null)
            {
                await this.ExtraLife.Disconnect();
                ChannelSession.Settings.ExtraLifeTeamID = 0;
                ChannelSession.Settings.ExtraLifeParticipantID = 0;
                ChannelSession.Settings.ExtraLifeIncludeTeamDonations = false;
            }
        }

        public override async Task<bool> InitializeTipeeeStream()
        {
            this.TipeeeStream = (ChannelSession.Settings.TipeeeStreamOAuthToken != null) ? new TipeeeStreamService(ChannelSession.Settings.TipeeeStreamOAuthToken) : new TipeeeStreamService();
            if (await this.TipeeeStream.Connect())
            {
                this.TipeeeStream.OnWebSocketConnectedOccurred += TipeeeStream_OnWebSocketConnectedOccurred;
                this.TipeeeStream.OnWebSocketDisconnectedOccurred += TipeeeStream_OnWebSocketDisconnectedOccurred;
                return true;
            }
            else
            {
                await this.DisconnectTipeeeStream();
            }
            return false;
        }

        public override async Task DisconnectTipeeeStream()
        {
            if (this.TipeeeStream != null)
            {
                this.TipeeeStream.OnWebSocketConnectedOccurred -= TipeeeStream_OnWebSocketConnectedOccurred;
                this.TipeeeStream.OnWebSocketDisconnectedOccurred -= TipeeeStream_OnWebSocketDisconnectedOccurred;

                await this.TipeeeStream.Disconnect();
                this.TipeeeStream = null;
                ChannelSession.Settings.TipeeeStreamOAuthToken = null;
            }
        }

        public override async Task<bool> InitializeTreatStream()
        {
            this.TreatStream = (ChannelSession.Settings.TreatStreamOAuthToken != null) ? new TreatStreamService(ChannelSession.Settings.TreatStreamOAuthToken) : new TreatStreamService();
            if (await this.TreatStream.Connect())
            {
                this.TreatStream.OnWebSocketConnectedOccurred += TipeeeStream_OnWebSocketConnectedOccurred;
                this.TreatStream.OnWebSocketDisconnectedOccurred += TipeeeStream_OnWebSocketDisconnectedOccurred;
                return true;
            }
            else
            {
                await this.DisconnectTreatStream();
            }
            return false;
        }

        public override async Task DisconnectTreatStream()
        {
            if (this.TreatStream != null)
            {
                this.TreatStream.OnWebSocketConnectedOccurred -= TipeeeStream_OnWebSocketConnectedOccurred;
                this.TreatStream.OnWebSocketDisconnectedOccurred -= TipeeeStream_OnWebSocketDisconnectedOccurred;

                await this.TreatStream.Disconnect();
                this.TreatStream = null;
                ChannelSession.Settings.TreatStreamOAuthToken = null;
            }
        }

        public override async Task<bool> InitializeStreamJar()
        {
            this.StreamJar = (ChannelSession.Settings.StreamJarOAuthToken != null) ? new StreamJarService(ChannelSession.Settings.StreamJarOAuthToken) : new StreamJarService();
            if (await this.StreamJar.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectStreamJar();
            }
            return false;
        }

        public override async Task DisconnectStreamJar()
        {
            if (this.StreamJar != null)
            {
                await this.StreamJar.Disconnect();
                this.StreamJar = null;
                ChannelSession.Settings.StreamJarOAuthToken = null;
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

        public override Task<bool> InitializeIFTTT(string key = null)
        {
            this.IFTTT = (ChannelSession.Settings.IFTTTOAuthToken != null) ? new IFTTTService(ChannelSession.Settings.IFTTTOAuthToken) : new IFTTTService(key);
            return Task.FromResult(true);
        }

        public override Task DisconnectIFTTT()
        {
            if (this.IFTTT != null)
            {
                this.IFTTT = null;
                ChannelSession.Settings.IFTTTOAuthToken = null;
            }
            return Task.FromResult(0);
        }

        public override async Task<bool> InitializeStreamloots(string streamlootsID = null)
        {
            this.Streamloots = (ChannelSession.Settings.StreamlootsOAuthToken != null) ? new StreamlootsService(ChannelSession.Settings.StreamlootsOAuthToken) : new StreamlootsService(streamlootsID);
            if (await this.Streamloots.Connect())
            {
                return true;
            }
            else
            {
                await this.DisconnectStreamloots();
            }
            return false;
        }

        public override async Task DisconnectStreamloots()
        {
            if (this.Streamloots != null)
            {
                await this.Streamloots.Disconnect();
                this.Streamloots = null;
                ChannelSession.Settings.StreamlootsOAuthToken = null;
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

        private void GameWisp_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("GameWisp");
        }

        private void GameWisp_OnWebSocketDisconnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("GameWisp");
        }

        private void TipeeeStream_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Tipeee Stream");
        }

        private void TipeeeStream_OnWebSocketDisconnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("Tipeee Stream");
        }
    }
}
