using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Model.Overlay.Widgets;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.Overlay;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class OverlayV3Packet
    {
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
        [DataMember]
        public string HTML { get; set; }
        [DataMember]
        public int Layer { get; set; }

        public OverlayItemDataV3Model(string id)
        {
            this.ID = id;
            this.URL = $"/{OverlayV3HttpListenerServer.OverlayDataPrefix}/{this.ID}";
        }

        public OverlayItemDataV3Model(string id, string html)
        {
            this.ID = id;
            this.HTML = html;
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

        public const string LocalFilePropertyName = "LocalFile:\\\\";
        public const string LocalFilePropertyRegexPattern = "{LocalFile:\\\\[^}]*}";

        public event EventHandler<OverlayV3Packet> OnPacketReceived = delegate { };

        public static string ReplaceProperty(string text, string name, object value)
        {
            return text.Replace($"{{{name}}}", (value != null) ? value.ToString() : string.Empty);
        }

        public static async Task<string> PerformBasicOverlayItemProcessing(OverlayEndpointV3Service endpoint, OverlayItemV3ModelBase item)
        {
            string iframeHTML = endpoint.GetItemIFrameHTML();
            iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(item.HTML), item.HTML);
            iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(item.CSS), item.CSS);
            iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, nameof(item.Javascript), item.Javascript);

            Dictionary<string, object> properties = item.GetGenerationProperties();
            await item.ProcessGenerationProperties(properties, new CommandParametersModel());
            foreach (var property in properties)
            {
                iframeHTML = OverlayV3Service.ReplaceProperty(iframeHTML, property.Key, property.Value);
            }

            return iframeHTML;
        }

        public string Name { get { return Resources.Overlay; } }
        public int PortNumber { get { return ChannelSession.Settings.OverlayPortNumber; } }

        public bool IsConnected { get; private set; }
        public int TotalConnectedClients { get { return this.webSocketListenerServer.TotalConnectedClients; } }

        public string HttpAddress { get { return string.Format(RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.PortNumber); } }

        private OverlayV3HttpListenerServer httpListenerServer;
        private OverlayV3WebSocketHttpListenerServer webSocketListenerServer;

        private Dictionary<Guid, OverlayEndpointV3Service> overlayEndpoints = new Dictionary<Guid, OverlayEndpointV3Service>();

        public Task<Result> Enable()
        {
            if (ChannelSession.Settings != null)
            {
                ChannelSession.Settings.EnableOverlay = true;
            }
            return Task.FromResult(new Result());
        }

        public async Task<Result> Connect()
        {
            try
            {
                this.IsConnected = false;

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
                        if (widget.IsEnabled)
                        {
#pragma warning disable CS0612 // Type or member is obsolete
                            await widget.Initialize();
#pragma warning restore CS0612 // Type or member is obsolete
                        }

                        if (widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
                        {
                            this.ConnectOverlayWidgetEndpointService(widget);
                        }
                    }

                    ServiceManager.Get<ITelemetryService>().TrackService("Overlay");
                    this.IsConnected = true;
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(string.Format(Resources.OverlayAddFailed, Resources.Default));
        }

        public async Task Disconnect()
        {
            foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
            {
                await this.DisconnectOverlayEndpointService(overlayEndpoint.ID);
            }

            foreach (OverlayWidgetV3Model widget in this.GetWidgets())
            {
                if (widget.IsEnabled)
                {
#pragma warning disable CS0612 // Type or member is obsolete
                    await widget.Uninitialize();
#pragma warning restore CS0612 // Type or member is obsolete
                }
            }

            if (this.webSocketListenerServer != null)
            {
                this.webSocketListenerServer.OnConnectedOccurred -= WebSocketListenerServer_OnConnectedOccurred;
                await this.webSocketListenerServer.Stop();
            }

            if (this.httpListenerServer != null)
            {
                this.httpListenerServer.Stop();
            }

            this.IsConnected = false;
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
            if (string.IsNullOrEmpty(overlayEndpoint.Head))
            {
                overlayEndpoint.Head = OverlayResources.OverlayEndpointDefaultHead;
            }

            if (string.IsNullOrEmpty(overlayEndpoint.HTML))
            {
                overlayEndpoint.HTML = OverlayResources.OverlayEndpointDefaultHTML;
            }

            if (string.IsNullOrEmpty(overlayEndpoint.CSS))
            {
                overlayEndpoint.CSS = OverlayResources.OverlayEndpointDefaultCSS;
            }

            if (string.IsNullOrEmpty(overlayEndpoint.Javascript))
            {
                overlayEndpoint.Javascript = OverlayResources.OverlayEndpointDefaultJavascript;
            }

            OverlayEndpointV3Service endpointService = new OverlayEndpointV3Service(overlayEndpoint);
            endpointService.Connect();
            this.overlayEndpoints[overlayEndpoint.ID] = endpointService;
        }

        public async Task DisconnectOverlayEndpointService(Guid id)
        {
            if (this.overlayEndpoints.TryGetValue(id, out OverlayEndpointV3Service service))
            {
                await service.Disconnect();
            }
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
                await this.DisconnectOverlayEndpointService(widget.ID);
            }
        }

        public void ConnectOverlayWidgetEndpointService(OverlayWidgetV3Model widget)
        {
            OverlayEndpointV3Service endpointService = this.GetOverlayEndpointService(widget.ID);
            if (endpointService == null)
            {
                OverlayWidgetEndpointV3Service widgetEndpoint = new OverlayWidgetEndpointV3Service(widget);
                widgetEndpoint.Connect();
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
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values.ToList())
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (OverlayEndpointV3Service overlay in this.overlayEndpoints.Values.ToList())
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
                    Logger.Log(LogLevel.Debug, $"Overlay - Setting HTML - {id} - {html}");
                    this.httpListenerServer.SetHTMLData(id, html);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public string LocalFilePropertyReplacement(string text)
        {
            string result = text;
            foreach (Match match in Regex.Matches(text, LocalFilePropertyRegexPattern))
            {
                string m = match.Value;
                string filePath = match.Value.Substring(1, m.Length - 2).Replace(LocalFilePropertyName, string.Empty);
                string url = this.GetURLForFile(filePath, "local");
                result = result.Replace(match.Value, url);
            }
            return result;
        }

        public string GetURLForFile(string filePath, string fileType) { return this.httpListenerServer.GetURLForFile(filePath, fileType); }

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

        public Dictionary<Guid, OverlayItemV3ModelBase> PacketListeningItems { get; private set; } = new Dictionary<Guid, OverlayItemV3ModelBase>();

        public virtual Guid ID { get { return this.Model.ID; } }
        public virtual string Name { get { return this.Model.Name; } }

        public string HttpAddress
        {
            get
            {
                if (this.ID == Guid.Empty)
                {
                    return $"{ServiceManager.Get<OverlayV3Service>().HttpAddress}{OverlayV3HttpListenerServer.OverlayPathPrefix}";
                }
                return $"{ServiceManager.Get<OverlayV3Service>().HttpAddress}{OverlayV3HttpListenerServer.OverlayPathPrefix}/{this.ID}";
            }
        }
        public virtual string WebSocketConnectionURL { get { return $"/ws/{this.ID}/"; } }

        public int ConnectedClients { get { return this.webSocketServers.Count; } }

        private List<OverlayV3Packet> batchPackets = new List<OverlayV3Packet>();
        private bool isBatching = false;

        private string mainHTML;
        private string itemIFrameHTML;

        private SemaphoreSlim semaphore = new SemaphoreSlim(1);

        private List<OverlayV3WebSocketServer> webSocketServers = new List<OverlayV3WebSocketServer>();

        public OverlayEndpointV3Service(OverlayEndpointV3Model model)
        {
            this.Model = model;
        }

        public void Connect()
        {
            this.mainHTML = OverlayResources.OverlayMainHTML;
            this.mainHTML = OverlayV3Service.ReplaceProperty(this.mainHTML, nameof(WebSocketConnectionURL), WebSocketConnectionURL);

            this.RefreshItemIFrameHTMLCache();
        }

        public async Task Disconnect()
        {
            foreach (OverlayV3WebSocketServer server in this.webSocketServers)
            {
                await server.Disconnect();
            }
            this.webSocketServers.Clear();
        }

        public void AddWebsocketServer(OverlayV3WebSocketServer webSocketServer)
        {
            this.webSocketServers.Add(webSocketServer);

            webSocketServer.OnPacketReceived += WebSocketServer_OnPacketReceived;
            webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;
        }

        public string GetMainHTML() { return this.mainHTML; }

        public string GetItemIFrameHTML() { return this.itemIFrameHTML; }

        public async Task Add(string id, string html, int layer = 0)
        {
            try
            {
                if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(html))
                {
                    html = ServiceManager.Get<OverlayV3Service>().LocalFilePropertyReplacement(html);
                    ServiceManager.Get<OverlayV3Service>().SetHTMLData(id, html);
                    await this.Send(new OverlayV3Packet(nameof(this.Add), new OverlayItemDataV3Model(id, html)
                    {
                        Layer = layer
                    }));
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
                    Logger.Log(LogLevel.Debug, $"Overlay - Removing HTML - {id}");
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
                    Logger.Log(LogLevel.Debug, $"Overlay - Calling Function - {id} - {functionName} - {{{string.Join(" ", parameters)}}}");
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

        public async Task<Guid> PlayAudio(string filePath, double volume)
        {
            Guid id = Guid.NewGuid();
            OverlaySoundV3Model overlayItem = new OverlaySoundV3Model(filePath, volume)
            {
                ID = id
            };
            await this.Add(id.ToString(), await OverlayV3Service.PerformBasicOverlayItemProcessing(this, overlayItem));
            return id;
        }

        public void StartBatching()
        {
            this.isBatching = true;
        }

        public async Task EndBatching()
        {
            this.isBatching = false;

            await this.semaphore.WaitAsync();

            IEnumerable<OverlayV3Packet> packets = this.batchPackets.ToList();
            this.batchPackets.Clear();

            this.semaphore.Release();

            if (packets.Count() > 0)
            {
                foreach (var webSocketServer in this.webSocketServers)
                {
                    await webSocketServer.Send(packets);
                }
            }
        }

        public string GetURLForFile(string filePath, string fileType) { return ServiceManager.Get<OverlayV3Service>().GetURLForFile(filePath, fileType); }

        public void RefreshItemIFrameHTMLCache()
        {
            this.itemIFrameHTML = OverlayResources.OverlayItemIFrameHTML;

            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, "AnimateCSS", OverlayResources.animateCSS);
            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, "WoahCSS", OverlayResources.WoahCSS);

            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, nameof(this.Model.Head), this.Model.Head);
            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, nameof(this.Model.HTML), this.Model.HTML);
            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, nameof(this.Model.CSS), this.Model.CSS);
            this.itemIFrameHTML = OverlayV3Service.ReplaceProperty(this.itemIFrameHTML, nameof(this.Model.Javascript), this.Model.Javascript);
        }

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
                await this.semaphore.WaitAsync();

                this.batchPackets.Add(packet);

                this.semaphore.Release();
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

        private async void WebSocketServer_OnPacketReceived(object sender, OverlayV3Packet packet)
        {
            this.OnPacketReceived(this, packet);

            this.PacketReceived(packet);

            if (packet.Data.TryGetValue("ID", out JToken idString) && idString != null && Guid.TryParse(idString.ToString(), out Guid id))
            {
                if (string.Equals(packet.Type, OverlaySoundV3Model.SoundFinishedPacketType))
                {
                    ServiceManager.Get<IAudioService>().OverlaySoundFinished(id);
                }
                else
                {
                    if (OverlayWidgetV3ViewModel.WidgetsInEditing.TryGetValue(id, out OverlayWidgetV3ViewModel widgetViewModel))
                    {
                        await widgetViewModel.ProcessPacket(packet);
                    }
                    else if (PacketListeningItems.TryGetValue(id, out OverlayItemV3ModelBase item))
                    {
                        await item.ProcessPacket(packet);
                    }
                    else
                    {
                        OverlayWidgetV3Model widget = ServiceManager.Get<OverlayV3Service>().GetWidget(id);
                        if (widget != null)
                        {
                            await widget.Item.ProcessPacket(packet);
                        }
                    }
                }
            }
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
                OverlayWidgetV3Model widget = ServiceManager.Get<OverlayV3Service>().GetWidget(this.ID);
                if (widget != null && widget.IsEnabled && widget.Item.DisplayOption == OverlayItemV3DisplayOptionsType.SingleWidgetURL)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    widget.SendInitial();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }
    }

    public class OverlayV3HttpListenerServer : LocalHttpListenerServer
    {
        public const string OverlayPathPrefix = "overlay";
        public const string OverlayDataPrefix = "data";
        public const string OverlayFilesPrefix = "files";
        public const string OverlayScriptsPrefix = "scripts";

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();
        private Dictionary<string, string> htmlData = new Dictionary<string, string>();

        public OverlayV3HttpListenerServer() { }

        public string GetURLForFile(string filePath, string fileType)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return string.Empty;
            }

            if (ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                return filePath;
            }

            string id = Guid.NewGuid().ToString();
            var existing = this.localFiles.FirstOrDefault(kvp => string.Equals(kvp.Value, filePath));
            if (!string.IsNullOrEmpty(existing.Key))
            {
                id = existing.Key;
            }
            else
            {
                this.localFiles[id] = filePath;
            }

            return $"/{OverlayFilesPrefix}/{fileType}/{id}?nonce={Guid.NewGuid()}";
        }

        public void SetHTMLData(string id, string data)
        {
            lock (this.htmlData)
            {
                this.htmlData[id] = data;
            }
        }

        public void RemoveHTMLData(string id)
        {
            lock (this.htmlData)
            {
                this.htmlData.Remove(id);
            }
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

                    string data = null;
                    lock (this.htmlData)
                    {
                        this.htmlData.TryGetValue(id, out data);
                    }

                    if (!string.IsNullOrEmpty(data))
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        Task.Run(async () =>
                        {
                            await Task.Delay(1000);
                            lock (this.htmlData)
                            {
                                this.htmlData.Remove(id);
                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                        await this.CloseConnection(listenerContext, HttpStatusCode.OK, data);
                    }
                }
                else if (url.StartsWith(OverlayScriptsPrefix))
                {
                    string name = url.Replace(OverlayScriptsPrefix, string.Empty);
                    name = name.Trim(new char[] { '/' });

                    string data = null;
                    if (string.Equals(name, "jquery-3.6.0.min.js"))
                    {
                        data = OverlayResources.jqueryJS;
                    }
                    else if (string.Equals(name, "video.min.js"))
                    {
                        data = OverlayResources.videoJS;
                    }
                    else if (string.Equals(name, "animate.min.css"))
                    {
                        data = OverlayResources.animateCSS;
                    }

                    if (data != null)
                    {
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
                            filePath = ServiceManager.Get<IFileService>().ExpandEnvironmentVariablesInFilePath(filePath);
                            FileInfo fileInfo = new FileInfo(filePath);

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
