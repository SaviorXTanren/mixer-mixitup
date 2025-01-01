using MixItUp.Base.Model.Actions;
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

namespace MixItUp.Base.Services.External
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
        public double? Rotation { get; set; }
    }

    [DataContract]
    public class StreamlabsOBSSceneItemTransformXY
    {
        [JsonProperty("x")]
        public double? X { get; set; }

        [JsonProperty("y")]
        public double? Y { get; set; }
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
        public int? ID { get; set; }

        [JsonProperty]
        public string jsonrpc = "2.0";

        public StreamlabsOBSPacketBase() { }
    }

    #endregion Data Classes

    public class StreamlabsDesktopService : IStreamingSoftwareService, IDisposable
    {
        private const string ConnectionString = "slobs";

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        private int currentID = 1;
        private SemaphoreSlim idSempahoreLock = new SemaphoreSlim(1);

        public string Name { get { return MixItUp.Base.Resources.StreamlabsDesktop; } }

        public bool IsEnabled { get { return ChannelSession.Settings.EnableStreamlabsOBSConnection; } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;

            try
            {
                if (await this.TestConnection())
                {
                    await this.StartReplayBuffer();

                    this.Connected(this, new EventArgs());
                    ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.StreamlabsDesktop);
                    this.IsConnected = true;
                    ServiceManager.Get<ITelemetryService>().TrackService("Streamlabs Desktop");
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                if (ex is UnauthorizedAccessException && !ChannelSession.IsElevated)
                {
                    return new Result(Resources.StreamlabsDesktopAdminMaybe);
                }
            }
            return new Result(Resources.StreamlabsDesktopFailed);
        }

        public Task Disconnect()
        {
            this.IsConnected = false;

            this.Disconnected(this, new EventArgs());
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.StreamlabsDesktop);

            return Task.CompletedTask;
        }

        public async Task<bool> TestConnection() { return (await this.GetActiveScene() != null); }

        public async Task ShowScene(string sceneName)
        {
            StreamlabsOBSScene scene = await this.GetScene(sceneName);
            if (scene != null)
            {
                await this.SendAndReceive(new StreamlabsOBSRequest("makeActive", scene.ResourceID));
            }
        }

        public async Task<string> GetCurrentScene()
        {
            var scene = await this.GetActiveScene();
            if (scene == null)
            {
                return "Unknown";
            }

            return scene.Name;
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSRequest request = new StreamlabsOBSRequest("setVisibility", sceneItem.ResourceID);
                request.Arguments.Add(visibility);
                await this.SendAndReceive(request);
            }
        }

        public Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility) { return Task.CompletedTask; }

        public async Task SetImageSourceFilePath(string sceneName, string sourceName, string filePath)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSSource source = await this.GetItemSource(sceneItem);
                if (source != null && source.Type.Equals("image_source"))
                {
                    IEnumerable<JObject> properties = await this.GetSourceProperties(source);
                    if (properties != null)
                    {
                        JObject property = properties.FirstOrDefault(p => p.TryGetValue("name", out JToken name) && name != null && string.Equals(name.ToString(), "file"));
                        if (property != null)
                        {
                            property["value"] = filePath;
                        }

                        await this.SetSourceProperties(source, properties);
                    }
                }
            }
        }

        public async Task SetMediaSourceFilePath(string sceneName, string sourceName, string filePath)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSSource source = await this.GetItemSource(sceneItem);
                if (source != null && source.Type.Equals("ffmpeg_source"))
                {
                    IEnumerable<JObject> properties = await this.GetSourceProperties(source);
                    if (properties != null)
                    {
                        JObject property = properties.FirstOrDefault(p => p.TryGetValue("name", out JToken name) && name != null && string.Equals(name.ToString(), "local_file"));
                        if (property != null)
                        {
                            property["value"] = filePath;
                        }

                        await this.SetSourceProperties(source, properties);
                    }
                }
            }
        }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
            if (sceneItem != null)
            {
                StreamlabsOBSSource source = await this.GetItemSource(sceneItem);
                if (source != null && source.Type.Equals("browser_source"))
                {
                    IEnumerable<JObject> properties = await this.GetSourceProperties(source);
                    if (properties != null)
                    {
                        JObject property = properties.FirstOrDefault(p => p.TryGetValue("name", out JToken name) && name != null && string.Equals(name.ToString(), "url"));
                        if (property != null)
                        {
                            property["value"] = url;
                        }

                        await this.SetSourceProperties(source, properties);
                    }
                }
            }
        }

        public async Task SetSourceDimensions(string sceneName, string sourceName, StreamingSoftwareSourceDimensionsModel dimensions)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
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

        public async Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(string sceneName, string sourceName)
        {
            StreamlabsOBSSceneItem sceneItem = await this.GetSceneItem(sceneName, sourceName);
            if (sceneItem != null)
            {
                return new StreamingSoftwareSourceDimensionsModel()
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

        public async Task StartStopStream() { await this.SendAndReceive(new StreamlabsOBSRequest("toggleStreaming", "StreamingService")); }

        public async Task StartStopRecording() { await this.SendAndReceive(new StreamlabsOBSRequest("toggleRecording", "StreamingService")); }

        public async Task SaveReplayBuffer() { await this.SendAndReceive(new StreamlabsOBSRequest("saveReplay", "StreamingService")); }

        public async Task<bool> StartReplayBuffer()
        {
            await this.SendAndReceive(new StreamlabsOBSRequest("startReplayBuffer", "StreamingService"));
            return true;
        }

        public Task SetSceneCollection(string sceneCollectionName) { return Task.CompletedTask; }

        private async Task<StreamlabsOBSScene> GetActiveScene()
        {
            return await this.GetResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("activeScene", "ScenesService"));
        }

        private async Task<StreamlabsOBSScene> GetScene(string sceneName)
        {
            IEnumerable<StreamlabsOBSScene> scenes = await this.GetArrayResult<StreamlabsOBSScene>(new StreamlabsOBSRequest("getScenes", "ScenesService"));
            return scenes.FirstOrDefault(s => s.Name.Equals(sceneName));
        }

        private async Task<StreamlabsOBSSceneItem> GetSceneItem(string sceneName, string sourceName)
        {
            StreamlabsOBSScene scene = (!string.IsNullOrEmpty(sceneName)) ? await this.GetScene(sceneName) : await this.GetActiveScene();
            if (scene != null)
            {
                IEnumerable<StreamlabsOBSSceneItem> sceneItems = await this.GetArrayResult<StreamlabsOBSSceneItem>(new StreamlabsOBSRequest("getItems", scene.ResourceID));
                return sceneItems.FirstOrDefault(s => s.Name.Equals(sourceName));
            }
            return null;
        }

        private async Task<StreamlabsOBSSource> GetItemSource(StreamlabsOBSSceneItem sceneItem)
        {
            StreamlabsOBSRequest getSourceRequest = new StreamlabsOBSRequest("getSource", sceneItem.ResourceID);
            getSourceRequest.Arguments.Add(sceneItem.SourceID);
            return await this.GetResult<StreamlabsOBSSource>(getSourceRequest);
        }

        private async Task<IEnumerable<JObject>> GetSourceProperties(StreamlabsOBSSource source)
        {
            return await this.GetArrayResult<JObject>(new StreamlabsOBSRequest("getPropertiesFormData", source.ResourceID));
        }

        private async Task SetSourceProperties(StreamlabsOBSSource source, IEnumerable<JObject> properties)
        {
            StreamlabsOBSRequest setSourcePropertiesRequest = new StreamlabsOBSRequest("setPropertiesFormData", source.ResourceID);
            JArray array = new JArray();
            foreach (JObject property in properties)
            {
                array.Add(property);
            }
            setSourcePropertiesRequest.Arguments.Add(array);
            await this.SendAndReceive(setSourcePropertiesRequest);
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
            Exception exception = null;
            JObject result = new JObject();

            try
            {
                await this.idSempahoreLock.WaitAsync();

                request.ID = this.currentID;
                this.currentID++;

                JObject requestJObj = JObject.FromObject(request);
                using (NamedPipeClientStream namedPipeClient = new NamedPipeClientStream(ConnectionString))
                {
                    string requestString = requestJObj.ToString(Formatting.None);
                    Logger.Log("Streamlabs Desktop packet sent - " + requestString);

                    byte[] requestBytes = Encoding.UTF8.GetBytes(requestString);

                    await Task.WhenAny(Task.Run(async () =>
                    {
                        try
                        {
                            namedPipeClient.Connect();
                            await namedPipeClient.WriteAsync(requestBytes, 0, requestBytes.Length);

                            byte[] responseBytes = new byte[5000000];
                            int count = await namedPipeClient.ReadAsync(responseBytes, 0, responseBytes.Length);

                            string responseString = Encoding.ASCII.GetString(responseBytes, 0, count);
                            result = JObject.Parse(responseString);
                        }
                        catch (Exception ex)
                        {
                            exception = ex;
                        }
                    }), Task.Delay(5000));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.idSempahoreLock.Release();
            }

            if (exception != null)
            {
                throw exception;
            }

            if (result != null)
            {
                Logger.Log("Streamlabs Desktop packet recieved - " + result.ToString());
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
