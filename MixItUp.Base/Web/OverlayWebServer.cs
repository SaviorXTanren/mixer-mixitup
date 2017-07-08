using MixItUp.Base.Web;
using Newtonsoft.Json;
using System.Net;

namespace MixItUp.Base.Overlay
{
    public class OverlayImage
    {
        public string filePath;
        public int duration;
        public int horizontal;
        public int vertical;
        public int width;
        public int height;
    }

    public class OverlayWebServer : RESTWebServer
    {
        private OverlayImage overlayImage;

        public void SetOverlayImage(OverlayImage overlayImage)
        {
            this.overlayImage = overlayImage;
        }

        protected override HttpStatusCode RequestReceived(string data, out string result)
        {
            result = string.Empty;
            if (this.overlayImage != null)
            {
                result = JsonConvert.SerializeObject(this.overlayImage);
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NoContent;
        }
    }
}
