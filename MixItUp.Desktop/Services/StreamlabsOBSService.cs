using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class StreamlabsOBSService : IStreamlabsOBSService
    {
        private const string ConnectionString = "slobs";

        private int currentID = 1;
        private SemaphoreSlim idSempahoreLock = new SemaphoreSlim(1);

        public async Task<IEnumerable<StreamlabsOBSScene>> GetScenes()
        {
            return await this.GetArrayResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("getScenes", "ScenesService"));
        }

        private async Task<List<T>> GetArrayResult<T>(StreamlabsOBSRequest request)
        {
            List<T> results = new List<T>();
            JObject responseJObj = await this.SendAndReceive(request);
            StreamlabsOBSArrayResponse response = responseJObj.ToObject<StreamlabsOBSArrayResponse>();
            if (response != null && response.Result != null)
            {
                foreach (JToken token in response.Result)
                {
                    results.Add(token.ToObject<T>());
                }
            }
            return results;
        }

        private async Task<JObject> SendAndReceive(StreamlabsOBSRequest request)
        {
            await this.idSempahoreLock.WaitAsync();

            request.ID = this.currentID;
            this.currentID++;

            this.idSempahoreLock.Release();

            return await AsyncRunner.RunSyncAsAsync(() =>
            {
                JObject requestJObj = JObject.FromObject(request);
                using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(ConnectionString))
                {
                    string requestString = requestJObj.ToString(Formatting.None);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestString);

                    namedPipeClient.Connect();
                    namedPipeClient.Write(requestBytes, 0, requestBytes.Length);

                    byte[] responseBytes = new byte[1000000];

                    int count = namedPipeClient.Read(responseBytes, 0, responseBytes.Length);
                    string responseString = Encoding.ASCII.GetString(responseBytes, 0, count);
                    return JObject.Parse(responseString);
                }
            });
        }
    }
}
