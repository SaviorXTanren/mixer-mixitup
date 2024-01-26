using MixItUp.Base.Model.Overlay;
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
            this.URL = $"{OverlayV3HttpListenerServer.OverlayDataPrefix}/{this.ID}";
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

        public bool IsConnected { get; private set; }

        private Dictionary<Guid, OverlayEndpointV3Service> overlayEndpoints = new Dictionary<Guid, OverlayEndpointV3Service>();

        private Dictionary<Guid, OverlayWidgetEndpointV3Service> widgetEndpoints = new Dictionary<Guid, OverlayWidgetEndpointV3Service>();

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

                foreach (OverlayWidgetV3Model widget in this.GetWidgets())
                {
                    if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
                    {
                        if (!await this.AddOverlayWidgetEndpoint(widget))
                        {
                            await this.Disconnect();
                            return new Result(string.Format(Resources.OverlayAddFailed, widget.Name));
                        }
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

            foreach (var widgetEndpont in this.widgetEndpoints)
            {
                await this.RemoveOverlayWidgetEndpoint(widgetEndpont.Key);
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
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values)
            {
                count += await overlay.TestConnection();
            }

            foreach (OverlayEndpointV3Service overlay in this.widgetEndpoints.Values)
            {
                count += await overlay.TestConnection();
            }
            return count;
        }

        public void StartBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values)
            {
                overlay.StartBatching();
            }

            foreach (OverlayEndpointV3Service overlay in this.widgetEndpoints.Values)
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

            foreach (OverlayEndpointV3Service overlay in this.widgetEndpoints.Values)
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
                this.overlayEndpoints[overlayEndpoint.ID] = overlay;
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
                this.overlayEndpoints.Remove(id);
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
            if (this.overlayEndpoints.ContainsKey(id))
            {
                return this.overlayEndpoints[id];
            }
            return null;
        }

        public IEnumerable<OverlayWidgetV3Model> GetWidgets()
        {
            return ChannelSession.Settings.OverlayWidgetsV3;
        }

        public async Task AddWidget(OverlayWidgetV3Model widget)
        {
            if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
            {
                await this.AddOverlayWidgetEndpoint(widget);
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
                await this.RemoveOverlayWidgetEndpoint(widget.ID);
            }
        }

        public async Task<bool> AddOverlayWidgetEndpoint(OverlayWidgetV3Model widget)
        {
            OverlayWidgetEndpointV3Service widgetEndpoint = this.GetOverlayWidgetEndpointService(widget.ID);
            if (widgetEndpoint != null)
            {
                return true;
            }

            widgetEndpoint = new OverlayWidgetEndpointV3Service(widget);
            if (await widgetEndpoint.Initialize())
            {
                widgetEndpoint.OnWebSocketConnectedOccurred += Overlay_OnWebSocketConnectedOccurred;
                widgetEndpoint.OnWebSocketDisconnectedOccurred += Overlay_OnWebSocketDisconnectedOccurred;
                this.widgetEndpoints[widgetEndpoint.ID] = widgetEndpoint;
                return true;
            }
            await this.RemoveOverlayWidgetEndpoint(widgetEndpoint.ID);
            return false;
        }

        public async Task RemoveOverlayWidgetEndpoint(Guid id)
        {
            OverlayWidgetEndpointV3Service widgetEndpoint = this.GetOverlayWidgetEndpointService(id);
            if (widgetEndpoint != null)
            {
                widgetEndpoint.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                widgetEndpoint.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await widgetEndpoint.Disconnect();
                this.widgetEndpoints.Remove(id);
            }
        }

        public OverlayWidgetEndpointV3Service GetOverlayWidgetEndpointService(Guid id)
        {
            if (this.widgetEndpoints.ContainsKey(id))
            {
                return this.widgetEndpoints[id];
            }
            return null;
        }

        public string GetURLForFile(string filePath, string fileType) { return this.GetDefaultOverlayEndpointService().GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { this.GetDefaultOverlayEndpointService().SetLocalFile(id, filePath); }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            OverlayEndpointV3Service overlay = (OverlayEndpointV3Service)sender;

            Logger.Log("Client connected to Overlay Endpoint - " + overlay.Name);

            try
            {
                overlay.StartBatching();
                //foreach (OverlayWidgetV3ModelBase widget in this.GetWidgets())
                //{
                //    if (widget.IsEnabled && widget.OverlayEndpointID == overlay.ID)
                //    {
                //        //await overlay.Add(widget.Item, new CommandParametersModel());
                //    }
                //}
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
        public const string WebSocketConnectionStartInitialPacket = "ConnectionStart";

        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/overlay/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/overlay/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/overlay/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/overlay/ws/";

        public const string OverlayWebSocketConnectionURL = "/overlay/ws/";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred = delegate { };

        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public OverlayEndpointV3Model Model { get; private set; }

        public virtual Guid ID { get { return this.Model.ID; } }
        public virtual int PortNumber { get { return this.Model.PortNumber; } }
        public virtual string Name { get { return this.Model.Name; } }

        public virtual string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public virtual string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.PortNumber); } }
        public virtual string WebSocketConnectionURL { get { return OverlayWebSocketConnectionURL; } }

        public int TotalConnectedClients { get { return this.webSocketServer.TotalConnectedClients; } }

        private OverlayV3HttpListenerServer httpListenerServer;
        private OverlayV3WebSocketHttpListenerServer webSocketServer;

        private List<OverlayV3Packet> batchPackets = new List<OverlayV3Packet>();
        private bool isBatching = false;

        private string itemIFrameHTML;

        public OverlayEndpointV3Service(OverlayEndpointV3Model model)
        {
            this.Model = model;
        }

        public async Task<bool> Initialize()
        {
            try
            {
                string webPageInstance = OverlayV3Service.ReplaceRemoteFiles(OverlayResources.OverlayMainHTML);
                webPageInstance = OverlayV3Service.ReplaceProperty(webPageInstance, nameof(WebSocketConnectionURL), WebSocketConnectionURL);

                this.itemIFrameHTML = OverlayResources.OverlayItemIFrameHTML; //OverlayV3Service.ReplaceRemoteFiles(OverlayResources.OverlayItemIFrameHTML);

                this.httpListenerServer = new OverlayV3HttpListenerServer(webPageInstance);
                this.webSocketServer = new OverlayV3WebSocketHttpListenerServer();

                this.httpListenerServer.Start(this.HttpListenerServerAddress);
                if (this.webSocketServer.Start(this.WebSocketServerAddress))
                {
                    this.webSocketServer.OnConnectedOccurred += WebSocketServer_OnConnectedOccurred;
                    this.webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;
                    this.webSocketServer.OnPacketReceived += WebSocketServer_OnPacketReceived;

                    if (this.ID == Guid.Empty && !string.IsNullOrWhiteSpace(ChannelSession.Settings.OverlaySourceName))
                    {
                        string overlayServerAddress = string.Format(OverlayEndpointV3Service.RegularOverlayHttpListenerServerAddressFormat, this.PortNumber);
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

        public string GetItemIFrameHTML() { return this.itemIFrameHTML; }

        public async Task Add(string id, string html)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(html))
                {
                    this.httpListenerServer.SetHTMLData(id, html);
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
                await this.webSocketServer.Send(this.batchPackets.ToList());
                this.batchPackets.Clear();
            }
        }

        public string GetURLForFile(string filePath, string fileType) { return this.httpListenerServer.GetURLForFile(filePath, fileType); }

        public void SetLocalFile(string id, string filePath) { this.httpListenerServer.SetLocalFile(id, filePath); }

        protected void PacketReceived(OverlayV3Packet packet)
        {
            this.OnPacketReceived(this, packet);
        }

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

        private string GenerateOutputHTML(OverlayOutputV3Model output)
        {
            string content = OverlayResources.OverlayItemIFrameHTML;
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.HTML), output.HTML);
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.CSS), output.CSS);
            content = OverlayV3Service.ReplaceProperty(content, nameof(output.Javascript), output.Javascript);
            return content;
        }

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }

        protected virtual void WebSocketServer_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            this.PacketReceived(packet);

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
    }

    public class OverlayWidgetEndpointV3Service : OverlayEndpointV3Service
    {
        public const string RegularOverlayWidgetHttpListenerServerAddressFormat = "http://localhost:{0}/widget/{1}/";
        public const string RegularOverlayWidgetWebSocketServerAddressFormat = "http://localhost:{0}/widget/{1}/ws/";

        public const string AdministratorOverlayWidgetHttpListenerServerAddressFormat = "http://*:{0}/widget/{1}/";
        public const string AdministratorOverlayWidgetWebSocketServerAddressFormat = "http://*:{0}/widget/{1}/ws/";

        public const string OverlayWidgetWebSocketConnectionURL = "/widget/{0}/ws/";

        public OverlayWidgetV3Model Widget { get; set; }

        public override Guid ID { get { return this.Widget.ID; } }
        public override string Name { get { return this.Widget.Name; } }

        public override string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWidgetHttpListenerServerAddressFormat : RegularOverlayWidgetHttpListenerServerAddressFormat, this.PortNumber, this.ID); } }
        public override string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? RegularOverlayWidgetWebSocketServerAddressFormat : AdministratorOverlayWidgetWebSocketServerAddressFormat, this.PortNumber, this.ID); } }
        public override string WebSocketConnectionURL { get { return string.Format(OverlayWidgetWebSocketConnectionURL, this.ID); } }

        public OverlayWidgetEndpointV3Service(OverlayWidgetV3Model widget)
            : base(ServiceManager.Get<OverlayV3Service>().GetDefaultOverlayEndpoint())
        {
            this.Widget = widget;
        }

        protected override void WebSocketServer_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            this.PacketReceived(packet);

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
        private static readonly string OverlayDataFullPath = $"{OverlayPathPrefix}/{OverlayDataPrefix}/";

        public const string OverlayFilesPrefix = "files";
        private static readonly string OverlayFilesFullPath = $"{OverlayPathPrefix}/{OverlayFilesPrefix}/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();
        private Dictionary<string, string> htmlData = new Dictionary<string, string>();

        public OverlayV3HttpListenerServer(string webPageInstance)
        {
            this.webPageInstance = webPageInstance;
        }

        public string GetURLForFile(string filePath, string fileType)
        {
            if (ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                return filePath;
            }

            string id = Guid.NewGuid().ToString();
            this.localFiles[id] = filePath;

            return string.Format("/{0}{1}/{2}?nonce={3}", OverlayFilesFullPath, fileType, id, Guid.NewGuid());
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

                if (url.Equals(OverlayPathPrefix))
                {
                    await this.CloseConnection(listenerContext, HttpStatusCode.OK, this.webPageInstance);
                }
                else if (url.StartsWith(OverlayDataFullPath))
                {
                    string id = url.Replace(OverlayDataFullPath, string.Empty);
                    if (this.htmlData.TryGetValue(id, out string data))
                    {
                        //this.htmlData.Remove(id);
                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, data);
                    }
                }
                else if (url.StartsWith(OverlayFilesFullPath))
                {
                    string fileID = url.Replace(OverlayFilesFullPath, "");
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
