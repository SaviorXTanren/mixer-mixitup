using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

        public async Task<bool> TestConnection()
        {
            return (await this.GetActiveScene() != null);
        }

        public async Task<IEnumerable<StreamlabsOBSScene>> GetScenes()
        {
            return await this.GetArrayResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("getScenes", "ScenesService"));
        }

        public async Task<StreamlabsOBSScene> GetActiveScene()
        {
            return await this.GetResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("activeScene", "ScenesService"));
        }

        public async Task MakeSceneActive(StreamlabsOBSScene scene)
        {
            await this.SendAndReceive(new StreamlabsOBSRequest("makeActive", scene.ResourceID));
        }

        public async Task<IEnumerable<StreamlabsOBSSceneItem>> GetSceneItems(StreamlabsOBSScene scene)
        {
            return await this.GetArrayResult<StreamlabsOBSSceneItem>(new StreamlabsOBSRequest("getItems", scene.ResourceID));
        }

        public async Task SetSceneItemVisibility(StreamlabsOBSSceneItem sceneItem, bool visibility)
        {
            StreamlabsOBSRequest request = new StreamlabsOBSRequest("setVisibility", sceneItem.ResourceID);
            request.Arguments.Add(visibility);
            await this.SendAndReceive(request);
        }

        public async Task SetSceneItemDimensions(StreamlabsOBSSceneItem sceneItem, StreamingSourceDimensions dimensions)
        {
            StreamlabsOBSRequest request = new StreamlabsOBSRequest("setTransform", sceneItem.ResourceID);

            JObject positionJObj = new JObject();
            positionJObj["x"] = dimensions.X;
            positionJObj["y"] = dimensions.Y;

            JObject scaleObj = new JObject();
            scaleObj["x"] = dimensions.XScale;
            scaleObj["y"] = dimensions.YScale;

            JObject jobj = new JObject();
            jobj["position"] = positionJObj;
            jobj["scale"] = scaleObj;
            jobj["rotation"] = dimensions.Rotation;

            request.Arguments.Add(jobj);
            await this.SendAndReceive(request);
        }

        public async Task StartStopStream()
        {
            string status = await this.GetResult<string>(new StreamlabsOBSRequest("toggleStreaming", "StreamingService"));
        }

        private async Task<T> GetResult<T>(StreamlabsOBSRequest request)
        {
            JObject responseJObj = await this.SendAndReceive(request);
            StreamlabsOBSResponse response = responseJObj.ToObject<StreamlabsOBSResponse>();
            if (response != null && response.Result != null)
            {
                return response.Result.ToObject<T>();
            }
            return default(T);
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

            JObject result = new JObject();
            try
            {
                JObject requestJObj = JObject.FromObject(request);
                using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(ConnectionString))
                {
                    string requestString = requestJObj.ToString(Formatting.None);
                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestString);

                    await Task.WhenAny(Task.Run(async () =>
                    {
                        namedPipeClient.Connect();
                        await namedPipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);

                        byte[] responseBytes = new byte[1000000];
                        int count = await namedPipeClient.ReadAsync(responseBytes, 0, responseBytes.Length);

                        string responseString = Encoding.ASCII.GetString(responseBytes, 0, count);
                        result = JObject.Parse(responseString);
                    }), Task.Delay(5000));
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            finally
            {
                this.idSempahoreLock.Release();
            }
            return result;
        }
    }
}
