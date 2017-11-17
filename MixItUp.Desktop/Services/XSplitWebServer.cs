using MixItUp.Base.Util;
using MixItUp.Base.Services;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MixItUp.XSplit
{
    public class XSplitWebServer : RequestListenerWebServerBase, IXSplitService
    {
        public XSplitWebServer(string address) : base(address) { }

        public Task<bool> Initialize()
        {
            this.Start();
            return Task.FromResult(true);
        }

        public void SetCurrentScene(XSplitScene scene) { this.AddToData(JsonConvert.SerializeObject(scene)); }

        public void SetSourceVisibility(XSplitSource source) { this.AddToData(JsonConvert.SerializeObject(source)); }

        public void SetWebBrowserSource(XSplitWebBrowserSource source) { this.AddToData(JsonConvert.SerializeObject(source)); }

        public Task Close()
        {
            this.End();
            return Task.FromResult(0);
        }
    }
}
