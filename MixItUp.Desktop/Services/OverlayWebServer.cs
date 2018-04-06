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
using System.Net.WebSockets;
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

        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectOccurred { add { this.webSocketServer.OnDisconnectOccurred += value; } remove { this.webSocketServer.OnDisconnectOccurred -= value; } }

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
            try
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
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task Disconnect()
        {
            this.httpListenerServer.End();
            await this.webSocketServer.DisconnectServer();
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

        public async Task SendImage(OverlayImage image) { await this.SendPacket(new OverlayPacket("image", JObject.FromObject(image))); }

        public async Task SendText(OverlayText text) { await this.SendPacket(new OverlayPacket("text", JObject.FromObject(text))); }

        public async Task SendYoutubeVideo(OverlayYoutubeVideo youtubeVideo) { await this.SendPacket(new OverlayPacket("youtube", JObject.FromObject(youtubeVideo))); }

        public async Task SendLocalVideo(OverlayLocalVideo localVideo)
        {
            localVideo.videoID = Guid.NewGuid().ToString().Replace("-", string.Empty);
            if (localVideo.filepath.EndsWith(".mp4"))
            {
                localVideo.videoType = "video/mp4";
            }
            else if (localVideo.filepath.EndsWith(".webm"))
            {
                localVideo.videoType = "video/webm";
            }
            this.httpListenerServer.SetLocalVideo(localVideo);

            await this.SendPacket(new OverlayPacket("video", JObject.FromObject(localVideo)));
        }

        public async Task SendHTMLText(OverlayHTML htmlText) { await this.SendPacket(new OverlayPacket("htmlText", JObject.FromObject(htmlText))); }

        public async Task SendTextToSpeech(OverlayTextToSpeech textToSpeech) { await this.SendPacket(new OverlayPacket("textToSpeech", JObject.FromObject(textToSpeech))); }

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

        private const string OverlayVideoWebPath = "overlay/video/";

        private string webPageInstance;

        private Dictionary<string, OverlayLocalVideo> videoFiles = new Dictionary<string, OverlayLocalVideo>();

        public OverlayHttpListenerServer(string address)
            : base(address)
        {
            this.webPageInstance = File.ReadAllText(OverlayWebpageFilePath);
            string webSocketWrapperText = File.ReadAllText(WebSocketWrapperFilePath);

            this.webPageInstance = this.webPageInstance.Replace(WebSocketWrapperScriptReplacementString, string.Format("<script>{0}</script>", webSocketWrapperText));
        }

        public void SetLocalVideo(OverlayLocalVideo video)
        {
            this.videoFiles[video.videoID] = video;
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            string url = listenerContext.Request.RawUrl;
            url = url.Trim(new char[] { '/' });

            if (url.Equals("overlay"))
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.OK, this.webPageInstance);
            }
            else if (url.StartsWith(OverlayVideoWebPath))
            {
                string videoID = url.Replace(OverlayVideoWebPath, "");
                if (this.videoFiles.ContainsKey(videoID) && File.Exists(this.videoFiles[videoID].filepath))
                {
                    listenerContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.StatusDescription = HttpStatusCode.OK.ToString();
                    listenerContext.Response.ContentType = this.videoFiles[videoID].videoType;

                    byte[] videoData = File.ReadAllBytes(this.videoFiles[videoID].filepath);
                    await listenerContext.Response.OutputStream.WriteAsync(videoData, 0, videoData.Length);

                    listenerContext.Response.Close();
                }
            }
            else
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "");
            }
        }
    }

    public class OverlayWebSocketServer : WebSocketServerBase
    {
        public OverlayWebSocketServer(string address) : base(address) { }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            if (!string.IsNullOrEmpty(packetJSON))
            {
                JObject packetObj = JObject.Parse(packetJSON);
            }
            await base.ProcessReceivedPacket(packetJSON);
        }
    }
}
