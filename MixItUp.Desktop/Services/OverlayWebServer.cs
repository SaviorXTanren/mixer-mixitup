using Mixer.Base.Model.Client;
using Mixer.Base.Web;
using MixItUp.Base;
using MixItUp.Base.Actions;
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

    public class OverlayWebServer : IOverlayService
    {
        public const string RegularOverlayHttpListenerServerAddress = "http://localhost:8111/overlay/";
        public const string RegularOverlayWebSocketServerAddress = "http://localhost:8111/ws/";

        public const string AdministratorOverlayHttpListenerServerAddress = "http://*:8111/overlay/";
        public const string AdministratorOverlayWebSocketServerAddress = "http://*:8111/ws/";

        public event EventHandler OnWebSocketConnectedOccurred { add { this.webSocketServer.OnConnectedOccurred += value; } remove { this.webSocketServer.OnConnectedOccurred -= value; } }
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred { add { this.webSocketServer.OnDisconnectOccurred += value; } remove { this.webSocketServer.OnDisconnectOccurred -= value; } }

        private OverlayHttpListenerServer httpListenerServer;
        private OverlayWebSocketServer webSocketServer;

        private List<OverlayPacket> batchPackets = new List<OverlayPacket>();
        private bool isBatching = false;

        private static bool IsElevated
        {
            get
            {
                WindowsIdentity id = WindowsIdentity.GetCurrent();
                return id.Owner != id.User;
            }
        }

        public OverlayWebServer()
        {
            this.httpListenerServer = new OverlayHttpListenerServer(IsElevated ? AdministratorOverlayHttpListenerServerAddress : RegularOverlayHttpListenerServerAddress);
            this.webSocketServer = new OverlayWebSocketServer(IsElevated ? AdministratorOverlayWebSocketServerAddress : RegularOverlayWebSocketServerAddress);
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
                            await ChannelSession.Services.OBSWebsocket.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.OBSWebsocket.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, RegularOverlayHttpListenerServerAddress);
                            await ChannelSession.Services.OBSWebsocket.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.XSplitServer != null)
                        {
                            await ChannelSession.Services.XSplitServer.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.XSplitServer.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, RegularOverlayHttpListenerServerAddress);
                            await ChannelSession.Services.XSplitServer.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.StreamlabsOBSService != null)
                        {
                            await ChannelSession.Services.StreamlabsOBSService.SetSourceVisibility(ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.StreamlabsOBSService.SetWebBrowserSourceURL(ChannelSession.Settings.OverlaySourceName, RegularOverlayHttpListenerServerAddress);
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

        public async Task SendImage(OverlayImageEffect effect)
        {
            this.httpListenerServer.SetLocalFile(effect.ID, effect.FilePath);
            await this.SendPacket("image", effect);
        }

        public async Task SendText(OverlayTextEffect effect) { await this.SendPacket("text", effect); }

        public async Task SendYoutubeVideo(OverlayYoutubeEffect effect) { await this.SendPacket("youtube", effect); }

        public async Task SendLocalVideo(OverlayVideoEffect effect)
        {
            this.httpListenerServer.SetLocalFile(effect.ID, effect.FilePath);
            await this.SendPacket("video", effect);
        }

        public async Task SendHTML(OverlayHTMLEffect effect) { await this.SendPacket("htmlText", effect); }

        public async Task SendWebPage(OverlayWebPageEffect effect) { await this.SendPacket("webPage", effect); }

        public async Task SendTextToSpeech(OverlayTextToSpeech textToSpeech) { await this.SendPacket("textToSpeech", textToSpeech); }

        public async Task SendSongRequest(OverlaySongRequest songRequest) { await this.SendPacket("songRequest", songRequest); }

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

        private const string OverlayFilesWebPath = "overlay/files/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public OverlayHttpListenerServer(string address)
            : base(address)
        {
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
                    await listenerContext.Response.OutputStream.WriteAsync(imageData, 0, imageData.Length);

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
                if (packetObj["type"] != null)
                {
                    if (packetObj["type"].ToString().Equals("songRequestComplete"))
                    {
                        if (ChannelSession.Services.SongRequestService != null)
                        {
                            await ChannelSession.Services.SongRequestService.SkipToNextSong();
                        }
                    }
                }
            }
            await base.ProcessReceivedPacket(packetJSON);
        }
    }
}
