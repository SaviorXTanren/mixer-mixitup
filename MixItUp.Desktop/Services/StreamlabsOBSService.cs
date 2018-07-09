using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    #region Data Classes

    [DataContract]
    public class StreamlabsOBSScene
    {
        [JsonProperty("id")]
        public string ID { get; set; }

        [JsonProperty("resourceId")]
        public string ResourceID { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSSceneItem
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("resourceId")]
        public string ResourceID { get; set; }

        [JsonProperty("muted")]
        public bool Muted { get; set; }

        [JsonProperty("sourceId")]
        public string SourceID { get; set; }

        [JsonProperty("transform")]
        public StreamlabsOBSSceneItemTransform Transform { get; set; }

        [JsonProperty("visible")]
        public bool Visible { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSSceneItemTransform
    {
        [JsonProperty("position")]
        public StreamlabsOBSSceneItemTransformXY Position { get; set; }

        [JsonProperty("scale")]
        public StreamlabsOBSSceneItemTransformXY Scale { get; set; }

        [JsonProperty("rotation")]
        public double Rotation { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSSceneItemTransformXY
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSSource
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("resourceId")]
        public string ResourceID { get; set; }

        [JsonProperty("sourceId")]
        public string SourceID { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSRequest : StreamlabsOBSPacketBase
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public JObject Parameters { get; set; }

        [JsonIgnore]
        public JArray Arguments { get { return (JArray)this.Parameters["args"]; } }

        public StreamlabsOBSRequest() { this.Parameters = new JObject(); }

        public StreamlabsOBSRequest(string method, string resource)
            : this()
        {
            this.Method = method;
            this.Parameters["resource"] = resource;
            this.Parameters["args"] = new JArray();
        }
    }

    [DataContract]
    public class StreamlabsOBSResponse : StreamlabsOBSPacketBase
    {
        [JsonProperty("result")]
        public JObject Result { get; set; }

        public StreamlabsOBSResponse() { }
    }

    [DataContract]
    public class StreamlabsOBSArrayResponse : StreamlabsOBSPacketBase
    {
        [JsonProperty("result")]
        public JArray Result { get; set; }

        public StreamlabsOBSArrayResponse() { }
    }

    [DataContract]
    public abstract class StreamlabsOBSPacketBase
    {
        [JsonProperty("id")]
        public int ID { get; set; }

        [JsonProperty]
        public string jsonrpc = "2.0";

        public StreamlabsOBSPacketBase() { }
    }

    #endregion Data Classes

    public class StreamlabsOBSService : IStreamingSoftwareService, IDisposable
    {
        private const string ConnectionString = "slobs";

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        private int currentID = 1;
        private SemaphoreSlim idSempahoreLock = new SemaphoreSlim(1);

        public async Task<bool> Connect()
        {
            if (await this.TestConnection())
            {
                this.Connected(this, new EventArgs());
                return true;
            }
            return false;
        }

        public Task Disconnect()
        {
            this.Disconnected(this, new EventArgs());
            return Task.FromResult(0);
        }

        public async Task<bool> TestConnection() { return (await this.GetActiveScene() != null); }

        public async Task ShowScene(string sceneName)
        {
            IEnumerable<StreamlabsOBSScene> scenes = await this.GetArrayResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("getScenes", "ScenesService"));
            StreamlabsOBSScene scene = scenes.FirstOrDefault(s => s.Name.Equals(sceneName));
            if (scene != null)
            {
                await this.SendAndReceive(new StreamlabsOBSRequest("makeActive", scene.ResourceID));
            }
        }

        public async Task SetSourceVisibility(string sourceName, bool visibility)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSRequest request = new StreamlabsOBSRequest("setVisibility", sceneItem.ResourceID);
                request.Arguments.Add(visibility);
                await this.SendAndReceive(request);
            }
        }

        public async Task SetWebBrowserSourceURL(string sourceName, string url)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSRequest getSourceRequest = new StreamlabsOBSRequest("getSource", sceneItem.ResourceID);
                getSourceRequest.Arguments.Add(sceneItem.SourceID);
                StreamlabsOBSSource source = await this.GetResult<StreamlabsOBSSource>(getSourceRequest);
                if (source != null && source.Type.Equals("browser_source"))
                {
                    IEnumerable<JObject> properties = await this.GetArrayResult<JObject>(new StreamlabsOBSRequest("getPropertiesFormData", source.ResourceID));
                    if (properties != null)
                    {
                        foreach (JObject property in properties)
                        {
                            if (property["name"] != null && property["name"].ToString().Equals("url"))
                            {
                                property["value"] = url;
                            }
                        }

                        StreamlabsOBSRequest setSourcePropertiesRequest = new StreamlabsOBSRequest("setPropertiesFormData", source.ResourceID);
                        JArray array = new JArray();
                        foreach (JObject property in properties)
                        {
                            array.Add(property);
                        }
                        setSourcePropertiesRequest.Arguments.Add(array);
                        await this.SendAndReceive(setSourcePropertiesRequest);
                    }
                }
            }
        }

        public async Task SetSourceDimensions(string sourceName, StreamingSourceDimensions dimensions)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sourceName);
            if (sceneItem != null)
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
        }

        public async Task<StreamingSourceDimensions> GetSourceDimensions(string sourceName)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sourceName);
            if (sceneItem != null)
            {
                return new StreamingSourceDimensions()
                {
                    X = (int)sceneItem.Transform.Position.X,
                    Y = (int)sceneItem.Transform.Position.Y,
                    XScale = (int)sceneItem.Transform.Scale.X,
                    YScale = (int)sceneItem.Transform.Scale.X,
                    Rotation = (int)sceneItem.Transform.Rotation
                };
            }
            return null;
        }

        public async Task StartStopStream()
        {
            await this.SendAndReceive(new StreamlabsOBSRequest("toggleStreaming", "StreamingService"));
        }

        public Task SaveReplayBuffer() { return Task.FromResult(0); }
        public Task<bool> StartReplayBuffer() { return Task.FromResult(false); }

        private async Task<StreamlabsOBSScene> GetActiveScene()
        {
            return await this.GetResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("activeScene", "ScenesService"));
        }

        private async Task<StreamlabsOBSSceneItem> GetSceneItem(string sourceName)
        {
            StreamlabsOBSScene scene = await this.GetActiveScene();
            if (scene != null)
            {
                IEnumerable<StreamlabsOBSSceneItem> sceneItems = await this.GetArrayResult<StreamlabsOBSSceneItem>(new StreamlabsOBSRequest("getItems", scene.ResourceID));
                return sceneItems.FirstOrDefault(s => s.Name.Equals(sourceName));
            }
            return null;
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.idSempahoreLock.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
