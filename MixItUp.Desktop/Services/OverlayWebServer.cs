using Mixer.Base.Model.Client;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace MixItUp.Overlay
{
    public class OverlayPacket : WebSocketPacket
    {
        public JObject data;
        public JArray array;

        public OverlayPacket() { }

        public OverlayPacket(string type, JObject data)
        {
            this.type = type;
            this.data = data;
        }

        public OverlayPacket(string type, JArray array)
        {
            this.type = type;
            this.array = array;
        }
    }

    public class OverlayWebServer : IOverlayService
    {
        public const string OverlayHttpListenerServerAddress = "http://localhost:8111/overlay/";
        public const string OverlayWebSocketServerAddress = "http://localhost:8111/ws/";

        public event EventHandler Disconnected { add { this.webSocketServer.Disconnected += value; } remove { this.webSocketServer.Disconnected -= value; } }

        private OverlayHttpListenerServer httpListenerServer;
        private OverlayWebSocketServer webSocketServer;

        private List<OverlayPacket> batchPackets = new List<OverlayPacket>();
        private bool isBatching = false;

        public OverlayWebServer()
        {
            this.httpListenerServer = new OverlayHttpListenerServer(OverlayHttpListenerServerAddress);
            this.webSocketServer = new OverlayWebSocketServer(OverlayWebSocketServerAddress);
        }

        public async Task<bool> Initialize()
        {
            this.httpListenerServer.Start();
            if (await this.webSocketServer.Initialize())
            {
                if (!string.IsNullOrWhiteSpace(ChannelSession.Settings.OverlaySourceName))
                {
                    if (ChannelSession.Services.OBSWebsocket != null)
                    {
                        ChannelSession.Services.OBSWebsocket.SetSourceRender(ChannelSession.Settings.OverlaySourceName, isVisible: false);
                        ChannelSession.Services.OBSWebsocket.SetWebBrowserSource(ChannelSession.Settings.OverlaySourceName, OverlayHttpListenerServerAddress);
                        ChannelSession.Services.OBSWebsocket.SetSourceRender(ChannelSession.Settings.OverlaySourceName, isVisible: true);
                    }

                    if (ChannelSession.Services.XSplitServer != null)
                    {
                        await ChannelSession.Services.XSplitServer.SetSourceVisibility(new XSplitSource() { sourceName = ChannelSession.Settings.OverlaySourceName, sourceVisible = false });
                        await ChannelSession.Services.XSplitServer.SetWebBrowserSource(new XSplitWebBrowserSource() { sourceName = ChannelSession.Settings.OverlaySourceName, webBrowserUrl = OverlayHttpListenerServerAddress, sourceVisible = true });
                    }
                }
                return true;
            }
            return false;
        }

        public async Task Disconnect()
        {
            this.httpListenerServer.End();
            await this.webSocketServer.Disconnect();
        }

        public void StartBatching()
        {
            this.isBatching = true;
        }

        public async Task EndBatching()
        {
            this.isBatching = false;

            await this.webSocketServer.Send(new OverlayPacket("batch", JArray.FromObject(this.batchPackets)));

            this.batchPackets.Clear();
        }

        public async Task<bool> TestConnection() { return await this.webSocketServer.TestConnection(); }

        public async Task SetImage(OverlayImage image) { await this.SendPacket(new OverlayPacket("image", JObject.FromObject(image))); }

        public async Task SetText(OverlayText text) { await this.SendPacket(new OverlayPacket("text", JObject.FromObject(text))); }

        public async Task SetHTMLText(OverlayHTML htmlText) { await this.SendPacket(new OverlayPacket("htmlText", JObject.FromObject(htmlText))); }

        private async Task SendPacket(OverlayPacket packet)
        {
            if (this.isBatching)
            {
                this.batchPackets.Add(packet);
            }
            else
            {
                await this.webSocketServer.Send(packet);
            }
        }
    }

    public class OverlayHttpListenerServer : HttpListenerServerBase
    {
        private const string OverlayWebpageFilePath = "Overlay\\OverlayPage.html";
        private const string WebSocketWrapperFilePath = "Overlay\\WebSocketWrapper.js";

        private const string WebSocketWrapperScriptReplacementString = "<script src=\"webSocketWrapper.js\"></script>";

        private string webPageInstance;

        public OverlayHttpListenerServer(string address)
            : base(address)
        {
            this.webPageInstance = File.ReadAllText(OverlayWebpageFilePath);
            string webSocketWrapperText = File.ReadAllText(WebSocketWrapperFilePath);

            this.webPageInstance = this.webPageInstance.Replace(WebSocketWrapperScriptReplacementString, string.Format("<script>{0}</script>", webSocketWrapperText));
        }

        protected override HttpStatusCode RequestReceived(HttpListenerRequest request, string data, out string result)
        {
            result = this.webPageInstance;
            return HttpStatusCode.OK;
        }
    }

    public class OverlayWebSocketServer : WebSocketServerBase
    {
        public OverlayWebSocketServer(string address) : base(address) { }

        protected override async Task PacketReceived(string packet)
        {
            if (packet != null)
            {
                JObject packetObj = JObject.Parse(packet);
            }
            await base.PacketReceived(packet);
        }
    }
}
