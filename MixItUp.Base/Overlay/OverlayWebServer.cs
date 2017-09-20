using Mixer.Base.Web;
using Newtonsoft.Json;
using System.Collections.Generic;
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
        [DataMember]
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
        private static object lockObj = new object();

        public OverlayWebServer(string address) : base(address) { }

        private List<string> currentData = new List<string>();

        public void SetImage(OverlayImage image)
        {
            lock (lockObj)
            {
                this.currentData.Add(JsonConvert.SerializeObject(image));
            }
        }

        public void SetText(OverlayText text)
        {
            lock (lockObj)
            {
                this.currentData.Add(JsonConvert.SerializeObject(text));
            }
        }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            lock (lockObj)
            {
                result = string.Empty;
                if (this.currentData.Count > 0)
                {
                    result = this.currentData[0];
                    this.currentData.RemoveAt(0);
                    return HttpStatusCode.OK;
                }
                return HttpStatusCode.NoContent;
            }
        }
    }
}
