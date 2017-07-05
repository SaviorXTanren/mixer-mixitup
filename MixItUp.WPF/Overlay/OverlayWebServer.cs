using MixItUp.Base.Web;
using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace MixItUp.WPF.Overlay
{
    public class OverlayWebServer : RESTWebServer
    {
        private string filePath;
        private int duration;

        public void SetOverlayImage(string filePath, int duration)
        {
            this.filePath = filePath;
            this.duration = duration;
        }

        protected override HttpStatusCode RequestReceived(string data, out string result)
        {
            result = string.Empty;
            if (!string.IsNullOrEmpty(this.filePath) && File.Exists(this.filePath))
            {
                result = JsonConvert.SerializeObject(this.filePath + "|" + this.duration);
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NoContent;
        }
    }
}
