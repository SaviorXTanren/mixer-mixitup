using Mixer.Base.Model.Client;
using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.XSplit
{
    #region Data Classes

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

    public class XSplitWebServer : WebSocketServerBase, IStreamingSoftwareService
    {
        public XSplitWebServer(string address) : base(address) { this.OnDisconnectOccurred += XSplitWebServer_OnDisconnectOccurred; }

        public event EventHandler Connected { add { this.OnConnectedOccurred += value; } remove { this.OnConnectedOccurred -= value; } }
        public event EventHandler Disconnected = delegate { };

        public async Task<bool> Connect() { return await this.Initialize(); }

        public async Task Disconnect() { await base.Disconnect(); }

        public async Task ShowScene(string sceneName)
        {
            await this.Send(new XSplitPacket("sceneTransition", JObject.FromObject(new XSplitScene() { sceneName = sceneName })));
        }

        public async Task SetSourceVisibility(string sourceName, bool visibility)
        {
            await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitSource() { sourceName = sourceName, sourceVisible = visibility })));
        }

        public async Task SetWebBrowserSourceURL(string sourceName, string url)
        {
            await this.Send(new XSplitPacket("sourceUpdate", JObject.FromObject(new XSplitWebBrowserSource() { sourceName = sourceName, webBrowserUrl = url })));
        }

        public Task SetSourceDimensions(string sourceName, StreamingSourceDimensions dimensions) { return Task.FromResult(0); }

        public Task<StreamingSourceDimensions> GetSourceDimensions(string sourceName) { return Task.FromResult(new StreamingSourceDimensions()); }

        public Task StartStopStream() { return Task.FromResult(0); }

        public Task SaveReplayBuffer() { return Task.FromResult(0); }
        public Task<bool> StartReplayBuffer() { return Task.FromResult(false); }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            await base.ProcessReceivedPacket(packetJSON);
        }

        private void XSplitWebServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            this.Disconnected(sender, new EventArgs());
        }
    }
}
