using MixItUp.Base.Util;
using Newtonsoft.Json;
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

    public class OverlayWebServer : RequestListenerWebServerBase
    {
        public OverlayWebServer(string address) : base(address) { }

        public void SetImage(OverlayImage image) { this.AddToData(JsonConvert.SerializeObject(image)); }

        public void SetText(OverlayText text) { this.AddToData(JsonConvert.SerializeObject(text)); }
    }
}
