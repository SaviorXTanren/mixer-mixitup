using MixItUp.Base.Util;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace MixItUp.Base.XSplit
{
    [DataContract]
    public class XSplitScene
    {
        [DataMember]
        public string sceneName;
    }

    [DataContract]
    public class XSplitSource
    {
        [DataMember]
        public string sourceName;
        [DataMember]
        public bool sourceVisible;
    }

    public class XSplitWebServer : RequestListenerWebServerBase
    {
        public XSplitWebServer(string address) : base(address) { }

        public void SetCurrentScene(XSplitScene scene) { this.AddToData(JsonConvert.SerializeObject(scene)); }

        public void UpdateSource(XSplitSource source) { this.AddToData(JsonConvert.SerializeObject(source)); }
    }
}
