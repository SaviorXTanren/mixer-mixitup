using Mixer.Base.Web;
using Newtonsoft.Json;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;

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

    public class XSplitWebServer : HttpListenerServerBase
    {
        public XSplitWebServer(string address) : base(address) { }

        private string currentData = null;

        private bool connectionTestOccurring = false;

        public void SetCurrentScene(XSplitScene scene) { this.currentData = JsonConvert.SerializeObject(scene); }

        public void UpdateSource(XSplitSource source) { this.currentData = JsonConvert.SerializeObject(source); }

        public async Task<bool> TestConnection()
        {
            this.connectionTestOccurring = true;
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(500);
                if (!this.connectionTestOccurring)
                {
                    return true;
                }
            }
            this.connectionTestOccurring = false;
            return false;
        }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            if (this.connectionTestOccurring)
            {
                this.connectionTestOccurring = false;
            }

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
