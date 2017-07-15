using Mixer.Base.Web;
using Newtonsoft.Json;
using System.Net;

namespace MixItUp.Base.Overlay
{
    public class OverlayImage
    {
        public string filePath;
        public string fileData;
        public int duration;
        public int horizontal;
        public int vertical;
    }

    public class OverlayWebServer : HttpListenerServerBase
    {
        public OverlayWebServer(string address) : base(address) { }

        private OverlayImage overlayImage;

        public void SetOverlayImage(OverlayImage overlayImage)
        {
            this.overlayImage = overlayImage;
        }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            result = string.Empty;
            if (this.overlayImage != null)
            {
                result = JsonConvert.SerializeObject(this.overlayImage);
                this.overlayImage = null;
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NoContent;
        }
    }
}
