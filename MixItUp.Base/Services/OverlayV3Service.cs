﻿using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
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
    public class OverlayOutputV3Model
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;

        public string TextID { get { return this.ID.ToString(); } }
    }

    [DataContract]
    public class OverlayItemDataV3Model
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string URL { get; set; }

        public OverlayItemDataV3Model(string id)
        {
            this.ID = id;
            this.URL = $"/{OverlayV3HttpListenerServer.OverlayDataPrefix}/{this.ID}";
        }
    }

    [DataContract]
    public class OverlayFunctionV3Model
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string FunctionName { get; set; }
        [DataMember]
        public Dictionary<string, object> Parameters { get; set; }

        public OverlayFunctionV3Model(string id, string functionName, Dictionary<string, object> parameters)
        {
            this.ID = id;
            this.FunctionName = functionName;
            this.Parameters = parameters;
        }
    }

    public class OverlayV3Service : IExternalService
    {
        public const int DefaultOverlayPort = 8111;

        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/ws/";

        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public static string ReplaceProperty(string text, string name, object value)
        {
            return text.Replace($"{{{name}}}", (value != null) ? value.ToString() : string.Empty);
        }

        public static string ReplaceScriptTag(string text, string fileName, string contents)
        {
            return text.Replace($"<script src=\"{fileName}\"></script>", $"<script>{contents}</script>");
        }

        public static string ReplaceCSSStyleSheetTag(string text, string fileName, string contents)
        {
            return text.Replace($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{fileName}\">", $"<style>{contents}</style>");
        }

        public static string ReplaceRemoteFiles(string html)
        {
            html = OverlayV3Service.ReplaceScriptTag(html, "jquery-3.6.0.min.js", OverlayResources.jqueryJS);
            html = OverlayV3Service.ReplaceScriptTag(html, "video.min.js", OverlayResources.videoJS);

            html = OverlayV3Service.ReplaceCSSStyleSheetTag(html, "animate.min.css", OverlayResources.animateCSS);

            return html;
        }

        public string Name { get { return Resources.Overlay; } }
        public int PortNumber { get { return ChannelSession.Settings.OverlayPortNumber; } }

        public bool IsConnected { get; private set; }
        public int TotalConnectedClients { get { return this.webSocketListenerServer.TotalConnectedClients; } }

        public string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.PortNumber); } }

        private OverlayV3HttpListenerServer httpListenerServer;
        private OverlayV3WebSocketHttpListenerServer webSocketListenerServer;

        private Dictionary<Guid, OverlayEndpointV3Service> overlayEndpoints = new Dictionary<Guid, OverlayEndpointV3Service>();

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
                this.httpListenerServer = new OverlayV3HttpListenerServer();
                this.webSocketListenerServer = new OverlayV3WebSocketHttpListenerServer();

                this.httpListenerServer.Start(this.HttpListenerServerAddress);
                if (this.webSocketListenerServer.Start(this.WebSocketServerAddress))
                {
                    this.webSocketListenerServer.OnConnectedOccurred += WebSocketListenerServer_OnConnectedOccurred;

                    if (ServiceManager.Get<IOBSStudioService>().IsConnected)
                    {
                        await ServiceManager.Get<IOBSStudioService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                        await ServiceManager.Get<IOBSStudioService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                    }

                    if (ServiceManager.Get<XSplitService>().IsConnected)
                    {
                        await ServiceManager.Get<XSplitService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                        await ServiceManager.Get<XSplitService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                    }

                    if (ServiceManager.Get<StreamlabsDesktopService>().IsConnected)
                    {
                        await ServiceManager.Get<StreamlabsDesktopService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: false);
                        await ServiceManager.Get<StreamlabsDesktopService>().SetSourceVisibility(null, ChannelSession.Settings.OverlaySourceName, visibility: true);
                    }

                    foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
                    {
                        this.ConnectOverlayEndpointService(overlayEndpoint);
                    }

                    foreach (OverlayWidgetV3Model widget in this.GetWidgets())
                    {
                        if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
                        {
                            this.ConnectOverlayWidgetEndpointService(widget);
                        }
                    }

                    ServiceManager.Get<ITelemetryService>().TrackService("Overlay");
                    return new Result();
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return new Result(string.Format(Resources.OverlayAddFailed, Resources.Default));
        }

        public async Task Disconnect()
        {
            foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
            {
                this.DisconnectOverlayEndpointService(overlayEndpoint.ID);
            }

            this.webSocketListenerServer.OnConnectedOccurred -= WebSocketListenerServer_OnConnectedOccurred;

            this.httpListenerServer.Stop();
            await this.webSocketListenerServer.Stop();
        }

        public Task<Result> Disable()
        {
            ChannelSession.Settings.EnableOverlay = false;
            return Task.FromResult(new Result());
        }

        #region Endpoints

        public IEnumerable<OverlayEndpointV3Model> GetOverlayEndpoints() { return ChannelSession.Settings.OverlayEndpointsV3.ToList(); }

        public OverlayEndpointV3Model GetOverlayEndpoint(Guid id)
        {
            return this.GetOverlayEndpoints().FirstOrDefault(oe => oe.ID == id) ?? this.GetDefaultOverlayEndpoint();
        }

        public OverlayEndpointV3Model GetDefaultOverlayEndpoint() { return this.GetOverlayEndpoint(Guid.Empty); }

        public void ConnectOverlayEndpointService(OverlayEndpointV3Model overlayEndpoint)
        {
            OverlayEndpointV3Service endpointService = new OverlayEndpointV3Service(overlayEndpoint);
            endpointService.Initialize();
            this.overlayEndpoints[overlayEndpoint.ID] = endpointService;
        }

        public void DisconnectOverlayEndpointService(Guid id)
        {
            this.overlayEndpoints.Remove(id);
        }

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
            if (this.overlayEndpoints.ContainsKey(id))
            {
                return this.overlayEndpoints[id];
            }
            return null;
        }

        #endregion Endpoints

        #region Widgets

        public IEnumerable<OverlayWidgetV3Model> GetWidgets()
        {
            return ChannelSession.Settings.OverlayWidgetsV3;
        }

        public OverlayWidgetV3Model GetWidget(Guid id)
        {
            return this.GetWidgets().FirstOrDefault(w => w.ID == id);
        }

        public async Task AddWidget(OverlayWidgetV3Model widget)
        {
            if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
            {
                this.ConnectOverlayWidgetEndpointService(widget);
            }

            ChannelSession.Settings.OverlayWidgetsV3.Add(widget);
            await widget.Enable();
        }

        public async Task RemoveWidget(OverlayWidgetV3Model widget)
        {
            ChannelSession.Settings.OverlayWidgetsV3.Remove(widget);
            await widget.Disable();

            if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
            {
                this.DisconnectOverlayEndpointService(widget.ID);
            }
        }

        public void ConnectOverlayWidgetEndpointService(OverlayWidgetV3Model widget)
        {
            OverlayEndpointV3Service endpointService = this.GetOverlayEndpointService(widget.ID);
            if (endpointService == null)
            {
                OverlayWidgetEndpointV3Service widgetEndpoint = new OverlayWidgetEndpointV3Service(widget);
                widgetEndpoint.Initialize();
                this.overlayEndpoints[widgetEndpoint.ID] = widgetEndpoint;
            }
        }

        #endregion Widgets

        public async Task<int> TestConnections()
        {
            return await this.webSocketListenerServer.TestConnection();
        }

        public void StartBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values)
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values)
            {
                await overlay.EndBatching();
            }
        }

        public void SetHTMLData(string id, string html)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(html))
                {
                    this.httpListenerServer.SetHTMLData(id, html);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public string GetURLForFile(string filePath, string fileType) { return this.httpListenerServer.GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { this.httpListenerServer.SetLocalFile(id, filePath); }

        private void WebSocketListenerServer_OnConnectedOccurred(object sender, WebSocketServerBase webSocketServer)
        {
            if (webSocketServer is OverlayV3WebSocketServer)
            {
                OverlayV3WebSocketServer server = (OverlayV3WebSocketServer)webSocketServer;
                OverlayEndpointV3Service endpointService = this.GetOverlayEndpointService(server.WebSocketEndpointID);
                if (endpointService != null)
                {
                    endpointService.AddWebsocketServer(server);
                }
            }
        }
    }

    public class OverlayEndpointV3Service
    {
        public const string WebSocketConnectionStartInitialPacket = "ConnectionStart";

        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public OverlayEndpointV3Model Model { get; private set; }

        public virtual Guid ID { get { return this.Model.ID; } }
        public virtual string Name { get { return this.Model.Name; } }

        public string HttpAddress
        {
            get
            {
                if (this.ID == Guid.Empty)
                {
                    return $"{ServiceManager.Get<OverlayV3Service>().HttpListenerServerAddress}{OverlayV3HttpListenerServer.OverlayPathPrefix}";
                }
                return $"{ServiceManager.Get<OverlayV3Service>().HttpListenerServerAddress}{OverlayV3HttpListenerServer.OverlayPathPrefix}/{this.ID}";
            }
        }
        public virtual string WebSocketConnectionURL { get { return $"/ws/{this.ID}/"; } }

        private List<OverlayV3Packet> batchPackets = new List<OverlayV3Packet>();
        private bool isBatching = false;

        private string mainHTML;
        private string itemIFrameHTML;

        private List<OverlayV3WebSocketServer> webSocketServers = new List<OverlayV3WebSocketServer>();

        public OverlayEndpointV3Service(OverlayEndpointV3Model model)
        {
            this.Model = model;
        }

        public void Initialize()
        {
            this.mainHTML = OverlayV3Service.ReplaceRemoteFiles(OverlayResources.OverlayMainHTML);
            this.mainHTML = OverlayV3Service.ReplaceProperty(this.mainHTML, nameof(WebSocketConnectionURL), WebSocketConnectionURL);

            this.itemIFrameHTML = OverlayResources.OverlayItemIFrameHTML; //OverlayV3Service.ReplaceRemoteFiles(OverlayResources.OverlayItemIFrameHTML);
        }

        public void AddWebsocketServer(OverlayV3WebSocketServer webSocketServer)
        {
            this.webSocketServers.Add(webSocketServer);

            webSocketServer.OnPacketReceived += WebSocketServer_OnPacketReceived;
            webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;
        }

        public string GetMainHTML() { return this.mainHTML; }

        public string GetItemIFrameHTML() { return this.itemIFrameHTML; }

        public async Task Add(string id, string html)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(html))
                {
                    ServiceManager.Get<OverlayV3Service>().SetHTMLData(id, html);
                    await this.Send(new OverlayV3Packet(nameof(this.Add), new OverlayItemDataV3Model(id)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task Remove(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    await this.Send(new OverlayV3Packet(nameof(this.Remove), new OverlayItemDataV3Model(id)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task Clear()
        {
            try
            {
                await this.Send(new OverlayV3Packet(nameof(this.Clear), new OverlayItemDataV3Model(Guid.Empty.ToString())));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task Function(string id, string functionName, Dictionary<string, object> parameters)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(functionName))
                {
                    await this.Send(new OverlayV3Packet(nameof(this.Function), new OverlayFunctionV3Model(id, functionName, parameters)));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task ResponsiveVoice(OverlayResponsiveVoiceTextToSpeechV3Model item)
        {
            try
            {
                if (item != null)
                {
                    await this.Send(new OverlayV3Packet(nameof(this.ResponsiveVoice), item));
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
                foreach (var webSocketServer in this.webSocketServers)
                {
                    await webSocketServer.Send(this.batchPackets.ToList());
                }
                this.batchPackets.Clear();
            }
        }

        public string GetURLForFile(string filePath, string fileType) { return ServiceManager.Get<OverlayV3Service>().GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { ServiceManager.Get<OverlayV3Service>().SetLocalFile(id, filePath); }

        protected virtual void PacketReceived(OverlayV3Packet packet)
        {
            if (string.Equals(packet.Type, WebSocketConnectionStartInitialPacket))
            {
                foreach (OverlayWidgetV3Model widget in ServiceManager.Get<OverlayV3Service>().GetWidgets())
                {
                    if (widget.IsEnabled && widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.OverlayEndpoint && this.ID == widget.OverlayEndpointID)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        widget.SendInitial();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
        }

        private async Task Send(OverlayV3Packet packet)
        {
            if (this.isBatching)
            {
                this.batchPackets.Add(packet);
            }
            else
            {
                foreach (var webSocketServer in this.webSocketServers)
                {
                    await webSocketServer.Send(packet);
                }
            }
        }

        private string GenerateOutputHTML(OverlayOutputV3Model output)
        {
            string content = OverlayResources.OverlayItemIFrameHTML;
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.HTML), output.HTML);
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.CSS), output.CSS);
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.Javascript), output.Javascript);
            return content;
        }

        private void WebSocketServer_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            this.OnPacketReceived(this, packet);

            this.PacketReceived(packet);
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            if (sender is OverlayV3WebSocketServer)
            {
                OverlayV3WebSocketServer webSocketServer = (OverlayV3WebSocketServer)sender;

                webSocketServer.OnPacketReceived -= WebSocketServer_OnPacketReceived;
                webSocketServer.OnDisconnectOccurred -= WebSocketServer_OnDisconnectOccurred;

                this.webSocketServers.Remove(webSocketServer);
            }
        }
    }

    public class OverlayWidgetEndpointV3Service : OverlayEndpointV3Service
    {
        public OverlayWidgetV3Model Widget { get; set; }

        public override Guid ID { get { return this.Widget.ID; } }
        public override string Name { get { return this.Widget.Name; } }

        public OverlayWidgetEndpointV3Service(OverlayWidgetV3Model widget)
            : base(ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint())
        {
            this.Widget = widget;
        }

        protected override void PacketReceived(OverlayV3Packet packet)
        {
            if (string.Equals(packet.Type, WebSocketConnectionStartInitialPacket))
            {
                foreach (OverlayWidgetV3Model widget in ServiceManager.Get<OverlayV3Service>().GetWidgets())
                {
                    if (widget.IsEnabled && widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL && this.ID == widget.ID)
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        widget.SendInitial();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    }
                }
            }
        }
    }

    public class OverlayV3HttpListenerServer : LocalHttpListenerServer
    {
        public const string OverlayPathPrefix = "overlay";
        public const string OverlayDataPrefix = "data";
        public const string OverlayFilesPrefix = "files";

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

            return $"/{OverlayFilesPrefix}/{fileType}/{id}?nonce={Guid.NewGuid()}";
        }

        public void SetLocalFile(string id, string filePath)
        {
            if (!ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                this.localFiles[id] = filePath;
            }
        }

        public void SetHTMLData(string id, string data)
        {
            this.htmlData[id] = OverlayV3Service.ReplaceRemoteFiles(data);
        }

        public void RemoveHTMLData(string id)
        {
            this.htmlData.Remove(id);
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            try
            {
                string url = listenerContext.Request.Url.LocalPath;
                url = url.Trim(new char[] { '/' });

                if (url.StartsWith(OverlayPathPrefix))
                {
                    Guid pathID = Guid.Empty;
                    if (!url.Equals(OverlayPathPrefix))
                    {
                        if (!Guid.TryParse(url.Replace(OverlayPathPrefix + "/", ""), out pathID))
                        {
                            await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "Invalid Overlay ID specified");
                            return;
                        }
                    }

                    OverlayEndpointV3Service endpointService = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(pathID);
                    if (endpointService != null)
                    {
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, endpointService.GetMainHTML());
                        return;
                    }

                    await this.CloseConnection(listenerContext, HttpStatusCode.BadRequest, "Invalid Overlay ID specified");
                }
                else if (url.StartsWith(OverlayDataPrefix))
                {
                    string id = url.Replace(OverlayDataPrefix, string.Empty);
                    id = id.Trim(new char[] { '/' });
                    if (this.htmlData.TryGetValue(id, out string data))
                    {
                        //this.htmlData.Remove(id);
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, data);
                    }
                }
                else if (url.StartsWith(OverlayFilesPrefix))
                {
                    string fileID = url.Replace(OverlayFilesPrefix, string.Empty);
                    fileID = fileID.Trim(new char[] { '/' });

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
        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public Guid WebSocketEndpointID { get; private set; }

        private OverlayV3WebSocketHttpListenerServer server;

        public OverlayV3WebSocketServer(OverlayV3WebSocketHttpListenerServer server, HttpListenerContext listenerContext)
            : base(listenerContext)
        {
            this.server = server;

            string[] splits = listenerContext.Request.Url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (splits.Length == 2 && Guid.TryParse(splits[1], out Guid id))
            {
                this.WebSocketEndpointID = id;
            }
        }

        public async Task Send(IEnumerable<OverlayV3Packet> packets) { await base.Send(JSONSerializerHelper.SerializeToString(packets)); }

        public async Task Send(OverlayV3Packet packet) { await base.Send(JSONSerializerHelper.SerializeToString(packet)); }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                OverlayV3Packet packet = new OverlayV3Packet(packetJSON);
                this.server.PacketReceived(packet);
                this.OnPacketReceived(this, packet);
            }
            catch (Exception)
            {
                Logger.Log("Bad Overlay Packet Parsing: " + packetJSON);
            }

            return base.ProcessReceivedPacket(packetJSON);
        }
    }
}