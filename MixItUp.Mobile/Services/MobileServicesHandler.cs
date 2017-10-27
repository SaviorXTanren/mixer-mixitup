using System.Threading.Tasks;
using MixItUp.Base.Services;

namespace MixItUp.Mobile.Services
{
    public class MobileServicesHandler : ServicesHandlerBase
    {
        public override async Task Close()
        {
            await this.DisconnectOverlayServer();

            await this.DisconnectOBSStudio();

            await this.DisconnectXSplitServer();
        }

        public override Task<bool> InitializeFileService()
        {
            if (this.FileService != null)
            {
                this.FileService = new AndroidFileService();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }

        public override Task<bool> InitializeInputService() { return Task.FromResult(false); }

        public override Task<bool> InitializeAudioService() { return Task.FromResult(false); }

        public override Task<bool> InitializeOverlayServer() { return Task.FromResult(false); }
        public override Task DisconnectOverlayServer() { return Task.FromResult(0); }

        public override Task<bool> InitializeOBSWebsocket() { return Task.FromResult(false); }
        public override Task DisconnectOBSStudio() { return Task.FromResult(0); }

        public override Task<bool> InitializeXSplitServer() { return Task.FromResult(false); }
        public override Task DisconnectXSplitServer() { return Task.FromResult(0); }
    }
}
