using Mixer.Base.Web;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.Serialization;

namespace MixItUp.Base.Overlay
{
    [DataContract]
    public class OverlayImage
    {
        [DataMember]
        public string imagePath;
        [DataMember]
        public int duration;
        [DataMember]
        public int horizontal;
        [DataMember]
        public int vertical;
        [JsonIgnore]
        public string imageData;
    }

    [DataContract]
    public class OverlayText
    {
        [DataMember]
        public string text;
        [DataMember]
        public string color;
        [DataMember]
        public int duration;
        [DataMember]
        public int horizontal;
        [DataMember]
        public int vertical;
    }

    public class OverlayWebServer : HttpListenerServerBase
    {
        public OverlayWebServer(string address) : base(address) { }

        private string currentData = null;

        public void SetImage(OverlayImage image) { this.currentData = JsonConvert.SerializeObject(image); }

        public void SetText(OverlayText text) { this.currentData = JsonConvert.SerializeObject(text); }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            result = string.Empty;
            if (this.currentData != null)
            {
                result = this.currentData;
                this.currentData = null;
                return HttpStatusCode.OK;
            }
            return HttpStatusCode.NoContent;
        }
    }
}
