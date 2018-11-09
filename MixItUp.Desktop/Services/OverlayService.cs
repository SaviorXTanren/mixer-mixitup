using Mixer.Base.Model.Client;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.Principal;
using System.Text.RegularExpressions;
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

    public class OverlayService : IOverlayService
    {
        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/overlay/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/overlay/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/ws/";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred = delegate { };

        public string Name { get; private set; }
        public int Port { get; private set; }

        private OverlayHttpListenerServer httpListenerServer;
        private OverlayWebSocketHttpListenerServer webSocketServer;

        private List<OverlayPacket> batchPackets = new List<OverlayPacket>();
        private bool isBatching = false;

        public string HttpListenerServerAddress { get { return string.Format(OverlayService.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.Port); } }
        public string WebSocketServerAddress { get { return string.Format(OverlayService.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.Port); } }

        private static bool IsElevated
        {
            get
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }
        }

        public OverlayService(string name, int port)
        {
            this.Name = name;
            this.Port = port;

            this.httpListenerServer = new OverlayHttpListenerServer(this.HttpListenerServerAddress, this.Port);
            this.webSocketServer = new OverlayWebSocketHttpListenerServer(this.WebSocketServerAddress);
        }

        public async Task<bool> Initialize()
        {
            try
            {
                this.httpListenerServer.Start();
                if (this.webSocketServer.Start())
                {
                    this.webSocketServer.OnConnectedOccurred += WebSocketServer_OnConnectedOccurred;
                    this.webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;

                    if (this.Name.Equals(ChannelSession.Services.OverlayServers.DefaultOverlayName) && !string.IsNullOrWhiteSpace(ChannelSession.Settings.OverlaySourceName))
                    {
                        string overlayServerAddress = string.Format(OverlayService.RegularOverlayHttpListenerServerAddressFormat, this.Port);
                        if (ChannelSession.Services.OBSWebsocket != null)
                        {
                            await ChannelSession.Services.OBSWebsocket.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.OBSWebsocket.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.OBSWebsocket.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.XSplitServer != null)
                        {
                            await ChannelSession.Services.XSplitServer.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.XSplitServer.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.XSplitServer.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.StreamlabsOBSService != null)
                        {
                            await ChannelSession.Services.StreamlabsOBSService.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.StreamlabsOBSService.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.StreamlabsOBSService.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: true);
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
            this.webSocketServer.OnConnectedOccurred -= WebSocketServer_OnConnectedOccurred;
            this.webSocketServer.OnDisconnectOccurred -= WebSocketServer_OnDisconnectOccurred;

            this.httpListenerServer.End();
            await this.webSocketServer.End();
        }

        public void StartBatching()
        {
            this.isBatching = true;
        }

        public async Task EndBatching()
        {
            this.isBatching = false;
            if (batchPackets.Count > 0)
            {
                await this.webSocketServer.Send(new OverlayPacket("batch", JArray.FromObject(this.batchPackets)));
            }
            this.batchPackets.Clear();
        }

        public async Task<bool> TestConnection() { return await this.webSocketServer.TestConnection(); }

        public async Task SendItem(OverlayItemBase item, OverlayItemPosition position, OverlayItemEffects effects)
        {
            if (item is OverlayImageItem)
            {
                await this.SendImage((OverlayImageItem)item, position, effects);
            }
            else if (item is OverlayTextItem)
            {
                await this.SendText((OverlayTextItem)item, position, effects);
            }
            else if (item is OverlayYouTubeItem)
            {
                await this.SendYouTubeVideo((OverlayYouTubeItem)item, position, effects);
            }
            else if (item is OverlayVideoItem)
            {
                await this.SendLocalVideo((OverlayVideoItem)item, position, effects);
            }
            else if (item is OverlayHTMLItem)
            {
                await this.SendHTML((OverlayHTMLItem)item, position, effects);
            }
            else if (item is OverlayWebPageItem)
            {
                await this.SendWebPage((OverlayWebPageItem)item, position, effects);
            }
            else
            {
                await this.SendEffectPacket("custom", item, position, effects);
            }
        }

        public async Task SendImage(OverlayImageItem item, OverlayItemPosition position, OverlayItemEffects effects)
        {
            this.httpListenerServer.SetLocalFile(item.FileID, item.FilePath);
            await this.SendEffectPacket("image", item, position, effects);
        }

        public async Task SendText(OverlayTextItem item, OverlayItemPosition position, OverlayItemEffects effects) { await this.SendEffectPacket("text", item, position, effects); }

        public async Task SendYouTubeVideo(OverlayYouTubeItem item, OverlayItemPosition position, OverlayItemEffects effects) { await this.SendEffectPacket("youtube", item, position, effects); }

        public async Task SendLocalVideo(OverlayVideoItem item, OverlayItemPosition position, OverlayItemEffects effects)
        {
            this.httpListenerServer.SetLocalFile(item.FileID, item.FilePath);
            await this.SendEffectPacket("video", item, position, effects);
        }

        public async Task SendHTML(OverlayHTMLItem item, OverlayItemPosition position, OverlayItemEffects effects) { await this.SendEffectPacket("htmlText", item, position, effects); }

        public async Task SendWebPage(OverlayWebPageItem item, OverlayItemPosition position, OverlayItemEffects effects) { await this.SendEffectPacket("webPage", item, position, effects); }

        public async Task SendTextToSpeech(OverlayTextToSpeech textToSpeech) { await this.SendPacket("textToSpeech", textToSpeech); }

        public async Task SendSongRequest(OverlaySongRequest songRequest) { await this.SendPacket("songRequest", songRequest); }

        private async Task SendEffectPacket(string type, OverlayItemBase item, OverlayItemPosition position, OverlayItemEffects effects)
        {
            JObject jobj = JObject.FromObject(item);
            if (position != null) { jobj.Merge(JObject.FromObject(position)); }
            if (effects != null) { jobj.Merge(JObject.FromObject(effects)); }
            await this.SendPacket(type, jobj);
        }

        private async Task SendPacket(string type, object contents)
        {
            OverlayPacket packet = new OverlayPacket(type, JObject.FromObject(contents));
            if (this.isBatching)
            {
                this.batchPackets.Add(packet);
            }
            else
            {
                await this.webSocketServer.Send(packet);
            }
        }

        public async Task RemoveItem(OverlayItemBase item)
        {
            await this.SendPacket("remove", JObject.FromObject(item));
        }

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }
    }

    public class OverlayHttpListenerServer : HttpListenerServerBase
    {
        private const string OverlayFolderPath = "Overlay\\";
        private const string OverlayWebpageFilePath = OverlayFolderPath + "OverlayPage.html";

        private const string CSSIncludeReplacementStringFormat = "<link rel=\"stylesheet\" type=\"text/css\" href=\".*\">";

        private const string JQueryScriptReplacementString = "<script src=\"jquery.min.js\"></script>";
        private const string JQueryFilePath = OverlayFolderPath + "jquery.min.js";

        private const string WebSocketWrapperScriptReplacementString = "<script src=\"webSocketWrapper.js\"></script>";
        private const string WebSocketWrapperFilePath = OverlayFolderPath + "WebSocketWrapper.js";

        private const string OverlayConnectionPortReplacementString = "openWebsocketConnection(\"0000\");";
        private const string OverlayConnectionPortFormat = "openWebsocketConnection(\"{0}\");";

        private const string OverlayFilesWebPath = "overlay/files/";

        private int port;
        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public OverlayHttpListenerServer(string address, int port)
            : base(address)
        {
            this.port = port;
            this.webPageInstance = File.ReadAllText(OverlayWebpageFilePath);

            string[] splits = CSSIncludeReplacementStringFormat.Split(new string[] { ".*" }, StringSplitOptions.RemoveEmptyEntries);
            Regex cssRegex = new Regex(CSSIncludeReplacementStringFormat);
            foreach (Match match in cssRegex.Matches(this.webPageInstance))
            {
                string cssFileName = match.Value;
                cssFileName = cssFileName.Replace(splits[0], "");
                cssFileName = cssFileName.Replace(splits[1], "");
                string cssFileContents = File.ReadAllText(OverlayFolderPath + cssFileName);

                this.webPageInstance = this.webPageInstance.Replace(match.Value, string.Format("<style>{0}</style>", cssFileContents));
            }

            string jqueryText = File.ReadAllText(JQueryFilePath);
            this.webPageInstance = this.webPageInstance.Replace(JQueryScriptReplacementString, string.Format("<script>{0}</script>", jqueryText));

            string webSocketWrapperText = File.ReadAllText(WebSocketWrapperFilePath);
            this.webPageInstance = this.webPageInstance.Replace(WebSocketWrapperScriptReplacementString, string.Format("<script>{0}</script>", webSocketWrapperText));

            this.webPageInstance = this.webPageInstance.Replace(OverlayConnectionPortReplacementString, string.Format(OverlayConnectionPortFormat, this.port));
        }

        public void SetLocalFile(string id, string filepath)
        {
            if (!Uri.IsWellFormedUriString(filepath, UriKind.RelativeOrAbsolute))
            {
                this.localFiles[id] = filepath;
            }
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            string url = listenerContext.Request.Url.LocalPath;
            url = url.Trim(new char[] { '/' });

            if (url.Equals("overlay"))
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.OK, this.webPageInstance);
            }
            else if (url.StartsWith(OverlayFilesWebPath))
            {
                string fileID = url.Replace(OverlayFilesWebPath, "");
                if (this.localFiles.ContainsKey(fileID) && File.Exists(this.localFiles[fileID]))
                {
                    listenerContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    listenerContext.Response.StatusDescription = HttpStatusCode.OK.ToString();
                    if (this.localFiles[fileID].EndsWith(".mp4") || this.localFiles[fileID].EndsWith(".webm"))
                    {
                        listenerContext.Response.ContentType = "video/" + Path.GetExtension(this.localFiles[fileID]).Replace(".", "");
                    }
                    else
                    {
                        listenerContext.Response.ContentType = "image/" + Path.GetExtension(this.localFiles[fileID]).Replace(".", "");
                    }

                    byte[] imageData = File.ReadAllBytes(this.localFiles[fileID]);

                    try
                    {
                        await listenerContext.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);
                    }
                    catch (HttpListenerException ex) { Logger.LogDiagnostic(ex); }
                    catch (Exception ex) {  Logger.Log(ex); }
                }
            }
            else
            {
                await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "");
            }
        }
    }

    public class OverlayWebSocketHttpListenerServer : WebSocketHttpListenerServerBase
    {
        public OverlayWebSocketHttpListenerServer(string address) : base(address) { }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new OverlayWebSocketServer(listenerContext);
        }
    }

    public class OverlayWebSocketServer : WebSocketServerBase
    {
        public OverlayWebSocketServer(HttpListenerContext listenerContext) : base(listenerContext) { }

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            if (!string.IsNullOrEmpty(packetJSON))
            {
                JObject packetObj = JObject.Parse(packetJSON);
                if (packetObj["type"] != null)
                {
                    if (packetObj["type"].ToString().Equals("songRequestStatus"))
                    {
                        if (ChannelSession.Services.SongRequestService != null)
                        {
                            SongRequestItem item = null;
                            if (packetObj["data"] != null)
                            {
                                item = packetObj["data"].ToObject<SongRequestItem>();
                            }

                            await ChannelSession.Services.SongRequestService.StatusUpdate(item);
                        }
                    }
                }
            }
            await base.ProcessReceivedPacket(packetJSON);
        }
    }
}
