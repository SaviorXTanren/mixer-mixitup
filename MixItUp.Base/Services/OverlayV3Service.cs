using System.Net.WebSockets;
using System;
using MixItUp.Base.Util;
using System.Threading.Tasks;
using System.Net;
using StreamingClient.Base.Web;
using System.Collections.Generic;
using System.IO;
using StreamingClient.Base.Util;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Model.Overlay;
using System.Linq;
using MixItUp.Base.Services.External;

namespace MixItUp.Base.Services
{
    public class OverlayV3Packet : WebSocketPacket
    {
        public string Type { get; set; }

        public JArray Data { get; set; }
    }

    public class OverlayV3Service : ServiceBase
    {
        public const string RegularOverlayWidgetHttpListenerServerAddressFormat = "http://localhost:{0}/widget/";
        public const string RegularOverlayWidgetWebSocketServerAddressFormat = "http://localhost:{0}/widget/ws/";

        public const string AdministratorOverlayWidgetHttpListenerServerAddressFormat = "http://*:{0}/widget/";
        public const string AdministratorOverlayWidgetWebSocketServerAddressFormat = "http://*:{0}/widget/ws/";

        public override string Name { get { return Resources.Overlay; } }

        private Dictionary<Guid, OverlayV3EndpointService> overlays = new Dictionary<Guid, OverlayV3EndpointService>();

        public override Task<Result> Enable()
        {
            ChannelSession.Settings.EnableOverlay = true;
            this.State = ServiceState.Enabled;
            return Task.FromResult(new Result());
        }

        public override async Task<Result> Connect()
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

                this.State = ServiceState.Connected;
                ServiceManager.Get<ITelemetryService>().TrackService("Overlay");
                return new Result();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public override async Task<Result> Disconnect()
        {
            foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
            {
                await this.RemoveOverlayEndpoint(overlayEndpoint.ID);
            }

            this.State = ServiceState.Disconnected;

            return new Result();
        }

        public override Task<Result> Disable()
        {
            ChannelSession.Settings.EnableOverlay = false;
            this.State = ServiceState.Disabled;
            return Task.FromResult(new Result());
        }

        public async Task<bool> AddOverlayEndpoint(OverlayEndpointV3Model overlayEndpoint)
        {
            OverlayV3EndpointService overlay = new OverlayV3EndpointService(overlayEndpoint);
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
            OverlayV3EndpointService overlay = this.GetOverlayEndpointService(id);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(id);
            }
        }

        public IEnumerable<OverlayEndpointV3Model> GetOverlayEndpoints()
        {
            return ChannelSession.Settings.OverlayEndpointsV3;
        }

        public OverlayEndpointV3Model GetOverlayEndpoint(Guid id)
        {
            return this.GetOverlayEndpoints().FirstOrDefault(oe => oe.ID == id) ?? this.GetDefaultOverlayEndpoint();
        }

        public OverlayEndpointV3Model GetDefaultOverlayEndpoint()
        {
            return this.GetOverlayEndpoint(Guid.Empty);
        }

        public OverlayV3EndpointService GetDefaultOverlayEndpointService()
        {
            OverlayEndpointV3Model overlayEndpoint = this.GetDefaultOverlayEndpoint();
            if (overlayEndpoint != null)
            {
                return this.GetOverlayEndpointService(overlayEndpoint.ID);
            }
            return null;
        }

        public OverlayV3EndpointService GetOverlayEndpointService(Guid id)
        {
            if (this.overlays.ContainsKey(id))
            {
                return this.overlays[id];
            }
            return null;
        }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            OverlayV3EndpointService overlay = (OverlayV3EndpointService)sender;
            //this.OnOverlayConnectedOccurred(overlay, new EventArgs());

            //Logger.Log("Client connected to Overlay Endpoint - " + overlay.Name);

            //overlay.StartBatching();
            //foreach (OverlayWidgetModel widget in ChannelSession.Settings.OverlayWidgets.Where(ow => ow.OverlayName.Equals(overlay.Name)))
            //{
            //    try
            //    {
            //        if (widget.IsEnabled)
            //        {
            //            await widget.ShowItem();
            //            await widget.LoadCachedData();
            //            await widget.UpdateItem();
            //        }
            //    }
            //    catch (Exception ex) { Logger.Log(ex); }
            //}
            //await overlay.EndBatching();
        }

        private void Overlay_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            OverlayV3EndpointService overlay = (OverlayV3EndpointService)sender;
            //this.OnOverlayDisconnectedOccurred(overlay, closeStatus);

            Logger.Log("Client disconnect from Overlay Endpoint - " + overlay.Name);
        }
    }

    public class OverlayV3EndpointService
    {
        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/overlay/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/overlay/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/overlay/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/overlay/ws/";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred = delegate { };

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

        public OverlayV3EndpointService(OverlayEndpointV3Model model)
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

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }
    }

    public class OverlayV3HttpListenerServer : LocalHttpListenerServer
    {
        private const string OverlayFolderPath = "Overlay\\";
        private const string OverlayWebpageFilePath = OverlayFolderPath + "Overlay.html";

        private const string OverlayFilesWebPath = "overlay/files/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public OverlayV3HttpListenerServer()
        {
            this.webPageInstance = File.ReadAllText(OverlayWebpageFilePath);

            this.ReplaceScriptTag("jquery-3.6.0.min.js");
            this.ReplaceScriptTag("webSocketWrapper.js");
            this.ReplaceScriptTag("video.min.js");

            this.ReplaceCSSStyleSheetTag("animate.min.css");
        }

        public string GetURLForFile(string filePath, string fileType)
        {
            if (ServiceManager.Get<IFileService>().IsURLPath(filePath))
            {
                return filePath;
            }

            string id = Guid.NewGuid().ToString();
            this.localFiles[id] = filePath;

            return string.Format("/overlay/files/{0}/{1}?nonce={2}", fileType, id, Guid.NewGuid());
        }

        public void SetLocalFile(string id, string filepath)
        {
            if (!ServiceManager.Get<IFileService>().IsURLPath(filepath))
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

        private void ReplaceScriptTag(string fileName)
        {
            this.webPageInstance = this.webPageInstance.Replace($"<script src=\"{fileName}\"></script>", $"<script>{File.ReadAllText(OverlayFolderPath + fileName)}</script>");
        }

        private void ReplaceCSSStyleSheetTag(string fileName)
        {
            this.webPageInstance = this.webPageInstance.Replace($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{fileName}\">", $"<style>{File.ReadAllText(OverlayFolderPath + fileName)}</style>");
        }
    }

    public class OverlayV3WebSocketHttpListenerServer : WebSocketHttpListenerServerBase
    {
        public OverlayV3WebSocketHttpListenerServer() { }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new OverlayWebSocketServer(listenerContext);
        }
    }

    public class OverlayV3WebSocketServer : WebSocketServerBase
    {
        public OverlayV3WebSocketServer(HttpListenerContext listenerContext) : base(listenerContext) { }

        public async Task Send(OverlayV3Packet packet) { await this.Send(JSONSerializerHelper.SerializeToString(packet)); }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            return base.ProcessReceivedPacket(packetJSON);
        }
    }
}
