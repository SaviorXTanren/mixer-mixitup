using Mixer.Base.Model.Client;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class OverlayTextToSpeech
    {
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Voice { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Rate { get; set; }
    }

    public interface IOverlayEndpointService
    {
        string Name { get; }
        int Port { get; }

        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred;

        Task<bool> Initialize();

        Task Disconnect();

        Task<int> TestConnection();

        void StartBatching();

        Task EndBatching();

        Task ShowItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);
        Task UpdateItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers);
        Task HideItem(OverlayItemModelBase item);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        void SetLocalFile(string fileID, string filePath);
    }

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

    public class OverlayEndpointService : IOverlayEndpointService
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

        public string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.Port); } }
        public string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.Port); } }

        public int TotalConnectedClients { get { return this.webSocketServer.TotalConnectedClients; } }

        public OverlayEndpointService(string name, int port)
        {
            this.Name = name;
            this.Port = port;

            this.httpListenerServer = new OverlayHttpListenerServer(this.HttpListenerServerAddress);
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

                    if (this.Name.Equals(ChannelSession.Services.Overlay.DefaultOverlayName) && !string.IsNullOrWhiteSpace(ChannelSession.Settings.OverlaySourceName))
                    {
                        string overlayServerAddress = string.Format(OverlayEndpointService.RegularOverlayHttpListenerServerAddressFormat, this.Port);
                        if (ChannelSession.Services.OBSStudio.IsConnected)
                        {
                            await ChannelSession.Services.OBSStudio.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.OBSStudio.SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.OBSStudio.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.XSplit.IsConnected)
                        {
                            await ChannelSession.Services.XSplit.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.XSplit.SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.XSplit.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ChannelSession.Services.StreamlabsOBS.IsConnected)
                        {
                            await ChannelSession.Services.StreamlabsOBS.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ChannelSession.Services.StreamlabsOBS.SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ChannelSession.Services.StreamlabsOBS.SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
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

            this.httpListenerServer.Stop();
            await this.webSocketServer.Stop();
        }

        public async Task<int> TestConnection() { return await this.webSocketServer.TestConnection(); }

        public void StartBatching()
        {
            this.isBatching = true;
        }

        public async Task EndBatching()
        {
            this.isBatching = false;
            if (batchPackets.Count > 0)
            {
                await this.webSocketServer.Send(new OverlayPacket("Batch", JArray.FromObject(this.batchPackets.ToList())));
            }
            this.batchPackets.Clear();
        }

        public async Task ShowItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (item != null)
            {
                try
                {
                    JObject jobj = await item.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                    if (jobj != null)
                    {
                        if (item is OverlayImageItemModel || item is OverlayVideoItemModel || item is OverlaySoundItemModel)
                        {
                            this.SetLocalFile(jobj["FileID"].ToString(), jobj["FilePath"].ToString());
                            jobj["FullLink"] = OverlayItemModelBase.GetFileFullLink(jobj["FileID"].ToString(), jobj["FileType"].ToString(), jobj["FilePath"].ToString());
                        }
                        await this.SendPacket("Show", jobj);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        public async Task UpdateItem(OverlayItemModelBase item, UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            if (item != null)
            {
                try
                {
                    JObject jobj = await item.GetProcessedItem(user, arguments, extraSpecialIdentifiers);
                    if (jobj != null)
                    {
                        await this.SendPacket("Update", jobj);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }
        }

        public async Task HideItem(OverlayItemModelBase item)
        {
            await this.SendPacket("Hide", JObject.FromObject(item));
        }

        public async Task SendTextToSpeech(OverlayTextToSpeech textToSpeech) { await this.SendPacket("TextToSpeech", textToSpeech); }

        public void SetLocalFile(string fileID, string filePath)
        {
            this.httpListenerServer.SetLocalFile(fileID, filePath);
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

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }
    }

    public class OverlayHttpListenerServer : LocalHttpListenerServer
    {
        private const string OverlayFolderPath = "Overlay\\";
        private const string OverlayWebpageFilePath = OverlayFolderPath + "OverlayPage.html";

        private const string WebSocketWrapperScriptReplacementString = "<script src=\"webSocketWrapper.js\"></script>";
        private const string WebSocketWrapperFilePath = OverlayFolderPath + "WebSocketWrapper.js";

        private const string OverlayFilesWebPath = "overlay/files/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public OverlayHttpListenerServer(string address)
            : base(address)
        {
            this.webPageInstance = File.ReadAllText(OverlayWebpageFilePath);

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
            try
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
                    string[] splits = fileID.Split(new char[] { '/', '\\' });
                    if (splits.Length == 2)
                    {
                        string fileType = splits[0];
                        fileID = splits[1];
                        if (this.localFiles.ContainsKey(fileID) && File.Exists(this.localFiles[fileID]))
                        {
                            listenerContext.Response.Headers["Access-Control-Allow-Origin"] = "*";
                            listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                            listenerContext.Response.StatusDescription = HttpStatusCode.OK.ToString();
                            listenerContext.Response.ContentType = fileType + "/" + Path.GetExtension(this.localFiles[fileID]).Replace(".", "");

                            byte[] fileData = File.ReadAllBytes(this.localFiles[fileID]);
                            await listenerContext.Response.OutputStream.WriteAsync(fileData, 0, fileData.Length);
                        }
                    }
                }
                else
                {
                    await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "");
                }
            }
            catch (HttpListenerException ex) { Logger.Log(LogLevel.Debug, ex); }
            catch (Exception ex) { Logger.Log(ex); }
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

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            return base.ProcessReceivedPacket(packetJSON);
        }
    }
}
