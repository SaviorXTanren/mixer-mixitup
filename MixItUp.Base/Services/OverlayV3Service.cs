﻿using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
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
    public class OverlayV3Packet
    {
        [DataMember]
        public string ID { get; set;}
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public JObject Data { get; set; } = new JObject();

        public OverlayV3Packet() { }

        public OverlayV3Packet(string json)
        {
            this.Type = string.Empty;

            JObject jobj = JObject.Parse(json);
            if (jobj.TryGetValue("Type", out JToken type))
            {
                this.Type = type.ToString();
            }
            if (jobj.TryGetValue("Data", out JToken data) && data is JObject)
            {
                this.Data = (JObject)data;
            }
        }

        public OverlayV3Packet(string type, object data)
        {
            this.Type = type;
            this.Data = JObject.FromObject(data);
        }
    }

    [DataContract]
    public class OverlayURLDataV3Model
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string URL { get; set; }

        public OverlayURLDataV3Model(OverlayOutputV3Model output)
        {
            this.ID = output.TextID;
            this.URL = "data/" + this.ID;
        }
    }

    public static class OverlayV3WebPage
    {
        public const string OverlayFolderPath = "Overlay\\";

        public static async Task<string> GetHTMLFromFile(string filePath) { return await GetHTMLFromTemplate(await ServiceManager.Get<IFileService>().ReadFile(filePath)); }

        public static async Task<string> GetHTMLFromTemplate(string template)
        {
            string output = template;

            output = await OverlayOutputV3Model.ReplaceScriptTag(output, "jquery-3.6.0.min.js", OverlayFolderPath + "jquery-3.6.0.min.js");
            output = await OverlayOutputV3Model.ReplaceScriptTag(output, "webSocketWrapper.js", OverlayFolderPath + "webSocketWrapper.js");
            output = await OverlayOutputV3Model.ReplaceScriptTag(output, "video.min.js", OverlayFolderPath + "video.min.js");

            output = await OverlayOutputV3Model.ReplaceCSSStyleSheetTag(output, "animate.min.css", OverlayFolderPath + "animate.min.css");

            return output;
        }
    }

    public class OverlayV3Service : IExternalService
    {
        public string Name { get { return Resources.Overlay; } }

        public bool IsConnected { get; private set; }

        private Dictionary<Guid, OverlayEndpointV3Service> overlays = new Dictionary<Guid, OverlayEndpointV3Service>();

        public Task<Result> Enable()
        {
            ChannelSession.Settings.EnableOverlay = true;
            this.IsConnected = true;
            return Task.FromResult(new Result());
        }

        public async Task<Result> Connect()
        {
            try
            {
                foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
                {
                    if (!await this.AddOverlayEndpoint(overlayEndpoint))
                    {
                        await this.Disconnect();
                        return new Result(string.Format(Resources.OverlayAddFailed, overlayEndpoint.Name));
                    }
                }
                ServiceManager.Get<ITelemetryService>().TrackService("Overlay");
                return new Result();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public async Task Disconnect()
        {
            foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
            {
                await this.RemoveOverlayEndpoint(overlayEndpoint.ID);
            }
        }

        public Task<Result> Disable()
        {
            ChannelSession.Settings.EnableOverlay = false;
            return Task.FromResult(new Result());
        }

        public async Task<int> TestConnections()
        {
            int count = 0;
            foreach (OverlayEndpointV3Service overlay in this.overlays.Values)
            {
                count += await overlay.TestConnection();
            }
            return count;
        }

        public void StartBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlays.Values)
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlays.Values)
            {
                await overlay.EndBatching();
            }
        }

        public async Task<bool> AddOverlayEndpoint(OverlayEndpointV3Model overlayEndpoint)
        {
            OverlayEndpointV3Service overlay = new OverlayEndpointV3Service(overlayEndpoint);
            if (await overlay.Initialize())
            {
                overlay.OnWebSocketConnectedOccurred += Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred += Overlay_OnWebSocketDisconnectedOccurred;
                this.overlays[overlayEndpoint.ID] = overlay;
                return true;
            }
            await this.RemoveOverlayEndpoint(overlayEndpoint.ID);
            return false;
        }

        public async Task RemoveOverlayEndpoint(Guid id)
        {
            OverlayEndpointV3Service overlay = this.GetOverlayEndpointService(id);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(id);
            }
        }

        public IEnumerable<OverlayEndpointV3Model> GetOverlayEndpoints() { return ChannelSession.Settings.OverlayEndpointsV3.ToList(); }

        public OverlayEndpointV3Model GetOverlayEndpoint(Guid id)
        {
            return this.GetOverlayEndpoints().FirstOrDefault(oe => oe.ID == id) ?? this.GetDefaultOverlayEndpoint();
        }

        public OverlayEndpointV3Model GetDefaultOverlayEndpoint() { return this.GetOverlayEndpoint(Guid.Empty); }

        public OverlayEndpointV3Service GetDefaultOverlayEndpointService()
        {
            OverlayEndpointV3Model overlayEndpoint = this.GetDefaultOverlayEndpoint();
            if (overlayEndpoint != null)
            {
                return this.GetOverlayEndpointService(overlayEndpoint.ID);
            }
            return null;
        }

        public OverlayEndpointV3Service GetOverlayEndpointService(Guid id)
        {
            if (this.overlays.ContainsKey(id))
            {
                return this.overlays[id];
            }
            return null;
        }

        public async Task AddWidget(OverlayWidgetV3Model item)
        {
            ChannelSession.Settings.OverlayWidgetsV3.Add(item);
            await item.Enable();
        }

        public async Task RemoveWidget(OverlayWidgetV3Model item)
        {
            await item.Disable();
            ChannelSession.Settings.OverlayWidgetsV3.Remove(item);
        }

        public IEnumerable<OverlayWidgetV3Model> GetWidgets() { return ChannelSession.Settings.OverlayWidgetsV3.ToList(); }

        public string GetURLForFile(string filePath, string fileType) { return this.GetDefaultOverlayEndpointService().GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { this.GetDefaultOverlayEndpointService().SetLocalFile(id, filePath); }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            OverlayEndpointV3Service overlay = (OverlayEndpointV3Service)sender;

            Logger.Log("Client connected to Overlay Endpoint - " + overlay.Name);

            try
            {
                overlay.StartBatching();
                foreach (OverlayWidgetV3Model widget in this.GetWidgets())
                {
                    if (widget.IsEnabled && widget.Item.OverlayEndpointID == overlay.ID)
                    {
                        await overlay.SendAdd(widget.Item, new CommandParametersModel());
                    }
                }
                await overlay.EndBatching();
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        private void Overlay_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            OverlayEndpointV3Service overlay = (OverlayEndpointV3Service)sender;

            Logger.Log("Client disconnect from Overlay Endpoint - " + overlay.Name);
        }
    }

    public class OverlayEndpointV3Service
    {
        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/overlay/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/overlay/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/overlay/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/overlay/ws/";

        public const string RegularOverlayWidgetHttpListenerServerAddressFormat = "http://localhost:{0}/widget/";
        public const string RegularOverlayWidgetWebSocketServerAddressFormat = "http://localhost:{0}/widget/ws/";

        public const string AdministratorOverlayWidgetHttpListenerServerAddressFormat = "http://*:{0}/widget/";
        public const string AdministratorOverlayWidgetWebSocketServerAddressFormat = "http://*:{0}/widget/ws/";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public OverlayEndpointV3Model Model { get; private set; }

        public Guid ID { get { return this.Model.ID; } }
        public int PortNumber { get { return this.Model.PortNumber; } }
        public string Name { get { return this.Model.Name; } }

        public string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.PortNumber); } }

        public int TotalConnectedClients { get { return this.webSocketServer.TotalConnectedClients; } }

        private OverlayV3HttpListenerServer httpListenerServer;
        private OverlayV3WebSocketHttpListenerServer webSocketServer;

        private List<OverlayV3Packet> batchPackets = new List<OverlayV3Packet>();
        private bool isBatching = false;

        public OverlayEndpointV3Service(OverlayEndpointV3Model model)
        {
            this.Model = model;

            this.httpListenerServer = new OverlayV3HttpListenerServer();
            this.webSocketServer = new OverlayV3WebSocketHttpListenerServer();
        }

        public async Task<bool> Initialize()
        {
            try
            {
                this.httpListenerServer.Start(this.HttpListenerServerAddress);
                if (this.webSocketServer.Start(this.WebSocketServerAddress))
                {
                    this.webSocketServer.OnConnectedOccurred += WebSocketServer_OnConnectedOccurred;
                    this.webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;
                    this.webSocketServer.OnPacketReceived += WebSocketServer_OnPacketReceived;

                    if (this.ID == Guid.Empty && !string.IsNullOrWhiteSpace(ChannelSession.Settings.OverlaySourceName))
                    {
                        string overlayServerAddress = string.Format(OverlayEndpointService.RegularOverlayHttpListenerServerAddressFormat, this.PortNumber);
                        if (ServiceManager.Get<IOBSStudioService>().IsConnected)
                        {
                            await ServiceManager.Get<IOBSStudioService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ServiceManager.Get<IOBSStudioService>().SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ServiceManager.Get<IOBSStudioService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ServiceManager.Get<XSplitService>().IsConnected)
                        {
                            await ServiceManager.Get<XSplitService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ServiceManager.Get<XSplitService>().SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ServiceManager.Get<XSplitService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                        }

                        if (ServiceManager.Get<StreamlabsDesktopService>().IsConnected)
                        {
                            await ServiceManager.Get<StreamlabsDesktopService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                            await ServiceManager.Get<StreamlabsDesktopService>().SetWebBrowserSourceURL(null, ChannelSession.Settings.OverlaySourceName, overlayServerAddress);
                            await ServiceManager.Get<StreamlabsDesktopService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
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
            this.webSocketServer.OnPacketReceived -= WebSocketServer_OnPacketReceived;

            this.httpListenerServer.Stop();
            await this.webSocketServer.Stop();
        }

        public async Task<int> TestConnection() { return await this.webSocketServer.TestConnection(); }

        public async Task SendBasic(OverlayItemV3ModelBase item, CommandParametersModel parameters)
        {
            try
            {
                if (item != null)
                {
                    OverlayOutputV3Model output = await item.GetProcessedItem(parameters);

                    await this.httpListenerServer.SetHTMLData(output.ID.ToString(), output.GenerateBasicHTMLOutput());

                    await this.Send(new OverlayV3Packet("Basic", new OverlayURLDataV3Model(output)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task SendResponsiveVoice(OverlayResponsiveVoiceTextToSpeechV3Model item)
        {
            try
            {
                if (item != null)
                {
                    await this.Send(new OverlayV3Packet("ResponsiveVoice", item));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task SendAdd(OverlayItemV3ModelBase item, CommandParametersModel parameters)
        {
            try
            {
                if (item != null)
                {
                    OverlayOutputV3Model output = await item.GetProcessedItem(parameters);

                    await this.httpListenerServer.SetHTMLData(output.ID.ToString(), output.GenerateWidgetHTMLOutput());

                    await this.Send(new OverlayV3Packet("Add", new OverlayURLDataV3Model(output)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task SendUpdate(OverlayItemV3ModelBase item, JObject jobj)
        {
            try
            {
                if (item != null)
                {
                    await this.Send(new OverlayV3Packet("Update", jobj));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task SendRemove(OverlayItemV3ModelBase item)
        {
            try
            {
                if (item != null)
                {
                    await this.Send(new OverlayV3Packet("Remove", new OverlayURLDataV3Model(item)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public void StartBatching()
        {
            this.isBatching = true;
        }

        public async Task EndBatching()
        {
            this.isBatching = false;
            if (this.batchPackets.Count > 0)
            {
                await this.webSocketServer.Send(this.batchPackets.ToList());
                this.batchPackets.Clear();
            }
        }

        public string GetURLForFile(string filePath, string fileType) { return this.httpListenerServer.GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { this.httpListenerServer.SetLocalFile(id, filePath); }

        private async Task Send(OverlayV3Packet packet)
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

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }

        private void WebSocketServer_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            this.OnPacketReceived(sender, packet);
        }
    }

    public class OverlayV3HttpListenerServer : LocalHttpListenerServer
    {
        private const string OverlayWebpageFilePath = OverlayV3WebPage.OverlayFolderPath + "Overlay.html";

        private const string OverlayDataWebPath = "overlay/data/";
        private const string OverlayFilesWebPath = "overlay/files/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();
        private Dictionary<string, string> htmlData = new Dictionary<string, string>();

        public OverlayV3HttpListenerServer() { }

        public string GetURLForFile(string filePath, string fileType)
        {
            if (ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                return filePath;
            }

            string id = Guid.NewGuid().ToString();
            this.localFiles[id] = filePath;

            return string.Format("/{0}{1}/{2}?nonce={3}", OverlayFilesWebPath, fileType, id, Guid.NewGuid());
        }

        public void SetLocalFile(string id, string filePath)
        {
            if (!ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                this.localFiles[id] = filePath;
            }
        }

        public async Task SetHTMLData(string id, string data)
        {
            this.htmlData[id] = await OverlayV3WebPage.GetHTMLFromTemplate(data);
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            try
            {
                string url = listenerContext.Request.Url.LocalPath;
                url = url.Trim(new char[] { '/' });

                if (url.Equals("overlay"))
                {
                    if (string.IsNullOrEmpty(this.webPageInstance))
                    {
                        this.webPageInstance = await OverlayV3WebPage.GetHTMLFromFile(OverlayWebpageFilePath);
                    }
                    await this.CloseConnection(listenerContext, HttpStatusCode.OK, this.webPageInstance);
                }
                else if (url.StartsWith(OverlayDataWebPath))
                {
                    string id = url.Replace(OverlayDataWebPath, string.Empty);
                    if (this.htmlData.TryGetValue(id, out string data))
                    {
                        this.htmlData.Remove(id);
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, data);
                    }
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
                            listenerContext.Response.Headers["Accept-Ranges"] = "bytes";

                            string filePath = this.localFiles[fileID];
                            FileInfo fileInfo = new FileInfo(this.localFiles[fileID]);

                            // If they overlay requests a range, let's chunk this file
                            string range = listenerContext.Request.Headers["Range"];
                            if (range != null)
                            {
                                // The total file size
                                long filesize = fileInfo.Length;

                                // Format is: bytes=0-123
                                //  0  : start byte
                                //  123: end byte (can be empty, means to give me what you want)
                                range = range.Replace("bytes=", string.Empty);
                                string[] markers = range.Split('-');
                                long startByte = long.Parse(markers[0]);
                                // Max of 1MB past startByte
                                long endByte = Math.Min(filesize, startByte + 1024 * 1024);
                                if (markers.Length > 1 && !string.IsNullOrEmpty(markers[1]))
                                {
                                    // If they requested less bytes, then provide less instead
                                    endByte = Math.Min(long.Parse(markers[1]), endByte);
                                }

                                int byteRange = (int)(endByte - startByte);

                                // Write out necessary headers
                                listenerContext.Response.Headers["Content-Range"] = $"bytes {startByte}-{endByte - 1}/{filesize}";
                                listenerContext.Response.StatusCode = (int)HttpStatusCode.PartialContent;
                                listenerContext.Response.StatusDescription = HttpStatusCode.PartialContent.ToString();
                                listenerContext.Response.ContentLength64 = byteRange;

                                // Only read/write the range of bytes requested
                                byte[] fileData = new byte[byteRange];
                                using (BinaryReader reader = new BinaryReader(new FileStream(filePath, FileMode.Open, FileAccess.Read)))
                                {
                                    reader.BaseStream.Seek(startByte, SeekOrigin.Begin);
                                    reader.Read(fileData, 0, byteRange);
                                }
                                await listenerContext.Response.OutputStream.WriteAsync(fileData, 0, fileData.Length);
                            }
                            else
                            {
                                byte[] fileData = File.ReadAllBytes(filePath);
                                listenerContext.Response.ContentLength64 = fileData.Length;
                                await listenerContext.Response.OutputStream.WriteAsync(fileData, 0, fileData.Length);
                            }
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

    public class OverlayV3WebSocketHttpListenerServer : WebSocketHttpListenerServerBase
    {
        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public OverlayV3WebSocketHttpListenerServer() { }

        public async Task Send(IEnumerable<OverlayV3Packet> packets) { await base.Send(JSONSerializerHelper.SerializeToString(packets)); }

        public async Task Send(OverlayV3Packet packet) { await base.Send(JSONSerializerHelper.SerializeToString(packet)); }

        public void PacketReceived(OverlayV3Packet packet) { this.OnPacketReceived(this, packet); }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new OverlayV3WebSocketServer(this, listenerContext);
        }
    }

    public class OverlayV3WebSocketServer : WebSocketServerBase
    {
        private OverlayV3WebSocketHttpListenerServer server;

        public OverlayV3WebSocketServer(OverlayV3WebSocketHttpListenerServer server, HttpListenerContext listenerContext)
            : base(listenerContext)
        {
            this.server = server;
        }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                this.server.PacketReceived(new OverlayV3Packet(packetJSON));
            }
            catch (Exception)
            {
                Logger.Log("Bad Overlay Packet Parsing: " + packetJSON);
            }

            return base.ProcessReceivedPacket(packetJSON);
        }
    }
}