using Mixer.Base.Web;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class RequestListenerWebServerBase : HttpListenerServerBase
    {
        private static object lockObj = new object();

        public RequestListenerWebServerBase(string address) : base(address) { }

        private List<string> currentData = new List<string>();
        private bool connectionTestOccurring = false;

        protected void AddToData(string data)
        {
            lock (lockObj)
            {
                this.currentData.Add(data);
            }
        }

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
