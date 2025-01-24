using MixItUp.Base.Model.Actions;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    #region Data Classes

    [DataContract]
    public class XSplitOutput
    {
        [DataMember]
        public string outputName;
    }

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
        public string sceneName;
        [DataMember]
        public string sourceName;
        [DataMember]
        public bool sourceVisible;
    }

    [DataContract]
    public class XSplitWebBrowserSource : XSplitSource
    {
        [DataMember]
        public string webBrowserUrl;
    }

    public class XSplitPacket
    {
        public string type { get; set; }

        public JObject data;

        public string Type { get { return this.type; } set { this.type = value; } }

        public XSplitPacket(string type, JObject data)
        {
            this.type = type;
            this.data = data;
        }
    }

    #endregion Data Classes

    public class XSplitWebSocketServer : WebSocketServerBase
    {
        public XSplitWebSocketServer(HttpListenerContext listenerContext) : base(listenerContext) { this.OnDisconnectOccurred += XSplitWebServer_OnDisconnectOccurred; }

        public event EventHandler Connected { add { this.OnConnectedOccurred += value; } remove { this.OnConnectedOccurred -= value; } }
        public event EventHandler Disconnected = delegate { };

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            Logger.Log(LogLevel.Debug, "XSplit Web Socket Packet Received - " + packetJSON);

            await base.ProcessReceivedPacket(packetJSON);
        }

        private void XSplitWebServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            Logger.Log(LogLevel.Debug, "XSplit Disconnected");

            this.Disconnected(sender, new EventArgs());
        }
    }

    public class XSplitService : WebSocketHttpListenerServerBase, IStreamingSoftwareService
    {
        public const string XSplitHttpListenerServerAddress = "http://localhost:8211/";

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        public XSplitService()
        {
            base.OnConnectedOccurred += XSplitService_OnConnectedOccurred;
            base.OnDisconnectOccurred += XSplitService_OnDisconnectOccurred;
        }

        public string Name { get { return MixItUp.Base.Resources.XSplit; } }

        public bool IsEnabled { get { return ChannelSession.Settings.EnableXSplitConnection; } }

        public bool IsConnected { get; private set; }

        public Task<Result> Connect()
        {
            this.IsConnected = false;
            if (this.Start(XSplitHttpListenerServerAddress))
            {
                this.IsConnected = true;
                ServiceManager.Get<ITelemetryService>().TrackService("XSplit");
                return Task.FromResult(new Result());
            }
            return Task.FromResult(new Result(MixItUp.Base.Resources.XSplitFailedToStartServer));
        }

        public async Task Disconnect()
        {
            this.IsConnected = false;
            await this.Stop();
        }

        public new async Task<bool> TestConnection()
        {
            return (await base.TestConnection() > 0);
        }

        public async Task ShowScene(string sceneName)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(new XSplitPacket("sceneTransition", JObject.FromObject(new XSplitScene() { sceneName = sceneName }))));
        }

        public async Task<string> GetCurrentScene()
        {
            return await Task.FromResult<string>("Not Yet Implemented");
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitSource() { sceneName = sceneName, sourceName = sourceName, sourceVisible = visibility }))));
        }

        public Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility) { return Task.CompletedTask; }

        public Task SetImageSourceFilePath(string sceneName, string sourceName, string filePath) { return Task.CompletedTask; }

        public Task SetMediaSourceFilePath(string sceneName, string sourceName, string filePath) { return Task.CompletedTask; }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitWebBrowserSource() { sceneName = sceneName, sourceName = sourceName, webBrowserUrl = url }))));
        }

        public Task SetSourceDimensions(string sceneName, string sourceName, StreamingSoftwareSourceDimensionsModel dimensions) { return Task.CompletedTask; }

        public Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(string sceneName, string sourceName) { return Task.FromResult(new StreamingSoftwareSourceDimensionsModel()); }

        public Task StartStopStream() { return Task.CompletedTask; }

        public Task StartStopRecording() { return Task.CompletedTask; }

        public Task SaveReplayBuffer() { return Task.CompletedTask; }
        public Task<bool> StartReplayBuffer() { return Task.FromResult(false); }

        public Task SetSceneCollection(string sceneCollectionName) { return Task.CompletedTask; }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new XSplitWebSocketServer(listenerContext);
        }

        private void XSplitService_OnConnectedOccurred(object sender, WebSocketServerBase e)
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.XSplit);
            this.Connected(sender, new EventArgs());
        }

        private void XSplitService_OnDisconnectOccurred(WebSocketServerBase sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.XSplit);
            this.Disconnected(sender, new EventArgs());
        }
    }
}
