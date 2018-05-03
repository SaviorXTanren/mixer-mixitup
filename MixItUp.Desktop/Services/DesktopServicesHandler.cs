using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Desktop.Audio;
using MixItUp.Desktop.Files;
using MixItUp.Input;
using MixItUp.OBS;
using MixItUp.Overlay;
using MixItUp.XSplit;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopServicesHandler : ServicesHandlerBase
    {
        public void Initialize()
        {
            this.MixerStatus = new MixerStatusService();

            this.InitializeSettingsService();
            this.InitializeAudioService();
            this.InitializeFileService();
            this.InitializeInputService();
            this.InitializeTextToSpeechService();
            this.InitializeSongRequestService();
            this.InitializeTranslationService();
        }

        public override async Task Close()
        {
            await this.DisconnectOverlayServer();

            await this.DisconnectOBSStudio();

            await this.DisconnectXSplitServer();
        }

        public override Task<bool> InitializeSettingsService()
        {
            if (this.Settings == null)
            {
                this.Settings = new DesktopSettingsService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeFileService()
        {
            if (this.FileService == null)
            {
                this.FileService = new WindowsFileService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeInputService()
        {
            if (this.InputService == null)
            {
                this.InputService = new WindowsInputService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeAudioService()
        {
            if (this.AudioService == null)
            {
                this.AudioService = new WindowsAudioService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeTextToSpeechService()
        {
            if (this.TextToSpeechService == null)
            {
                this.TextToSpeechService = new WindowsTextToSpeechService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeTranslationService()
        {
            if (this.TranslationService == null)
            {
                this.TranslationService = new TranslationService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeSongRequestService()
        {
            if (this.SongRequestService == null)
            {
                this.SongRequestService = new DesktopSongRequestService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
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
                this.OBSWebsocket = new OBSService();
                if (await this.OBSWebsocket.Initialize(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword))
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
                this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                await this.OBSWebsocket.Close();
                this.OBSWebsocket = null;
            }
        }

        public override async Task<bool> InitializeXSplitServer()
        {
            if (this.XSplitServer == null)
            {
                this.XSplitServer = new XSplitWebServer("http://localhost:8211/");
                if (await this.XSplitServer.Initialize())
                {
                    this.XSplitServer.OnWebSocketConnectedOccurred += XSplitServer_OnWebSocketConnectedOccurred;
                    this.XSplitServer.OnWebSocketDisconnectedOccurred += XSplitServer_OnWebSocketDisconnectedOccurred;
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
                this.XSplitServer.OnWebSocketConnectedOccurred -= XSplitServer_OnWebSocketConnectedOccurred;
                this.XSplitServer.OnWebSocketDisconnectedOccurred -= XSplitServer_OnWebSocketDisconnectedOccurred;
                await this.XSplitServer.DisconnectServer();
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
            if (await this.GameWisp.Connect())
            {
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
            if (await this.Discord.Connect())
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

        private void OverlayServer_OnWebSocketConnectedOccurred(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("Overlay");
        }

        private void OverlayServer_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("Overlay");
        }

        private void OBSWebsocket_Connected(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("OBS");
        }

        private async void OBSWebsocket_Disconnected(object sender, System.EventArgs e)
        {
            ChannelSession.DisconnectionOccurred("OBS");
            await this.DisconnectOBSStudio();
        }

        private void XSplitServer_OnWebSocketConnectedOccurred(object sender, System.EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("XSplit");
        }

        private void XSplitServer_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("XSplit");
        }
    }
}
