using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Desktop.Audio;
using MixItUp.Desktop.Files;
using MixItUp.Input;
using MixItUp.OBS;
using MixItUp.Overlay;
using MixItUp.XSplit;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class DesktopServicesHandler : ServicesHandlerBase
    {
        public void Initialize()
        {
            this.InitializeAudioService();
            this.InitializeFileService();
            this.InitializeInputService();
            this.InitializeTextToSpeechService();
        }

        public override async Task Close()
        {
            await this.DisconnectOverlayServer();

            await this.DisconnectOBSStudio();

            await this.DisconnectXSplitServer();
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

        public override async Task<bool> InitializeOverlayServer()
        {
            if (this.OverlayServer == null)
            {
                this.OverlayServer = new OverlayWebServer("http://localhost:8111/");
                return await this.OverlayServer.Initialize();
            }
            return false;
        }

        public override async Task DisconnectOverlayServer()
        {
            if (this.OverlayServer != null)
            {
                await this.OverlayServer.Close();
                this.OverlayServer = null;
            }
        }

        public override async Task<bool> InitializeOBSWebsocket()
        {
            if (this.OBSWebsocket == null)
            {
                this.OBSWebsocket = new OBSService();
                bool result = await this.OBSWebsocket.Initialize(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                if (!result)
                {
                    this.OBSWebsocket = null;
                }
                return result;
            }
            return false;
        }

        public override async Task DisconnectOBSStudio()
        {
            if (this.OBSWebsocket != null)
            {
                await this.OBSWebsocket.Close();
                this.OBSWebsocket = null;
            }
        }

        public override async Task<bool> InitializeXSplitServer()
        {
            if (this.XSplitServer == null)
            {
                this.XSplitServer = new XSplitWebServer("http://localhost:8201/");
                return await this.XSplitServer.Initialize();
            }
            return true;
        }

        public override async Task DisconnectXSplitServer()
        {
            if (this.XSplitServer != null)
            {
                await this.XSplitServer.Close();
            }
        }
    }
}
