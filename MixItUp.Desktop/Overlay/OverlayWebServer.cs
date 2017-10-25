using MixItUp.Base.Services;
using MixItUp.Desktop;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MixItUp.Overlay
{
    public class OverlayWebServer : RequestSenderWebServerBase, IOverlayService
    {
        private Process nodeJSProcess;

        public OverlayWebServer(string address) : base(address) { }

        public Task<bool> Initialize()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "Overlay\\LaunchNode.bat";

            this.nodeJSProcess = Process.Start(startInfo);

            return Task.FromResult(true);
        }

        public Task TestConnection()
        {
            this.SendData("test", new JObject());
            return Task.FromResult(0);
        }

        public void SetImage(OverlayImage image) { this.SendData("image", JObject.FromObject(image)); }

        public void SetText(OverlayText text) { this.SendData("text", JObject.FromObject(text)); }

        public Task Close()
        {
            this.End();
            this.nodeJSProcess.Close();
            return Task.FromResult(0);
        }
    }
}
