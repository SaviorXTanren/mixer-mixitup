using Mixer.Base.Model.Client;
using MixItUp.Base.Actions;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
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

    public class XSplitPacket : WebSocketPacket
    {
        public JObject data;

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
        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        public XSplitService(string address)
            : base(address)
        {
            base.OnConnectedOccurred += XSplitService_OnConnectedOccurred;
            base.OnDisconnectOccurred += XSplitService_OnDisconnectOccurred;
        }

        public string Name { get { return "XSplit"; } }

        public bool IsEnabled { get { return ChannelSession.Settings.EnableXSplitConnection; } }

        public bool IsConnected { get; private set; }

        public Task<Result> Connect()
        {
            this.IsConnected = false;
            if (this.Start())
            {
                this.IsConnected = true;
                return Task.FromResult(new Result());
            }
            return Task.FromResult(new Result("Failed to start web socket listening server"));
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
            await this.Send(new XSplitPacket("sceneTransition", JObject.FromObject(new XSplitScene() { sceneName = sceneName })));
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitSource() { sceneName = sceneName, sourceName = sourceName, sourceVisible = visibility })));
        }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitWebBrowserSource() { sceneName = sceneName, sourceName = sourceName, webBrowserUrl = url })));
        }

        public Task SetSourceDimensions(string sceneName, string sourceName, StreamingSourceDimensions dimensions) { return Task.FromResult(0); }

        public Task<StreamingSourceDimensions> GetSourceDimensions(string sceneName, string sourceName) { return Task.FromResult(new StreamingSourceDimensions()); }

        public async Task StartStopStream()
        {
            await this.Send(new XSplitPacket("startStopStream", JObject.FromObject(new XSplitOutput() { outputName = "Beam" })));
        }

        public Task SaveReplayBuffer() { return Task.FromResult(0); }
        public Task<bool> StartReplayBuffer() { return Task.FromResult(false); }

        public Task SetSceneCollection(string sceneCollectionName) { return Task.FromResult(0); }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new XSplitWebSocketServer(listenerContext);
        }

        private void XSplitService_OnConnectedOccurred(object sender, EventArgs e)
        {
            ChannelSession.ReconnectionOccurred("XSplit");
            this.Connected(sender, e);
        }

        private void XSplitService_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred("XSplit");
            this.Disconnected(sender, new EventArgs());
        }
    }
}
