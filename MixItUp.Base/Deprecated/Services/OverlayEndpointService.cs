using MixItUp.Base.Model.Commands;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    [Obsolete]
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

    [Obsolete]
    public class OverlayPacket
    {
        public string type { get; set; }

        public JObject data;
        public JArray array;

        public string Type { get { return this.type; } set { this.type = value; } }

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

    [Obsolete]
    public class OverlayEndpointService
    {
        public const string RegularOverlayHttpListenerServerAddressFormat = "http://localhost:{0}/overlay/";
        public const string RegularOverlayWebSocketServerAddressFormat = "http://localhost:{0}/ws/";

        public const string AdministratorOverlayHttpListenerServerAddressFormat = "http://*:{0}/overlay/";
        public const string AdministratorOverlayWebSocketServerAddressFormat = "http://*:{0}/ws/";

        public event EventHandler OnWebSocketConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred = delegate { };

        public Guid ID { get { return Guid.Empty; } }
        public int PortNumber { get { return 0; } }
        public string Name { get { return string.Empty; } }

        private OverlayHttpListenerServer httpListenerServer;
        private OverlayWebSocketHttpListenerServer webSocketServer;

        private List<OverlayPacket> batchPackets = new List<OverlayPacket>();

        public string HttpListenerServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayHttpListenerServerAddressFormat : RegularOverlayHttpListenerServerAddressFormat, this.PortNumber); } }
        public string WebSocketServerAddress { get { return string.Format(ChannelSession.IsElevated ? AdministratorOverlayWebSocketServerAddressFormat : RegularOverlayWebSocketServerAddressFormat, this.PortNumber); } }

        public int TotalConnectedClients { get { return this.webSocketServer.TotalConnectedClients; } }

        public OverlayEndpointService()
        {
            this.httpListenerServer = new OverlayHttpListenerServer();
            this.webSocketServer = new OverlayWebSocketHttpListenerServer();
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

        }

        public Task EndBatching()
        {

            if (batchPackets.Count > 0)
            {
                //await this.webSocketServer.Send(new OverlayPacket("Batch", JArray.FromObject(this.batchPackets.ToList())));
            }
            this.batchPackets.Clear();
            return Task.CompletedTask;
        }

        public async Task SendJObject(string type, string id, JObject jobj, CommandParametersModel parameters)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(id) && jobj != null)
                {
                    await PerformTextReplacements(jobj, parameters);
                    jobj["ID"] = id;
                    jobj["Type"] = type;
                    await this.SendPacket(type, jobj);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private async Task PerformTextReplacements(JObject jobj, CommandParametersModel parameters)
        {
            if (jobj != null)
            {
                foreach (string key in jobj.GetKeys())
                {
                    if (jobj[key].Type == JTokenType.String)
                    {
                        SpecialIdentifierStringBuilder siString = new SpecialIdentifierStringBuilder(jobj[key].ToString(), encode: false);
                        await siString.ReplaceCommonSpecialModifiers(parameters);
                        jobj[key] = siString.ToString();
                    }
                    else if (jobj[key].Type == JTokenType.Object)
                    {
                        await this.PerformTextReplacements((JObject)jobj[key], parameters);
                    }
                }
            }
        }



















        public Task ShowItem(OverlayItemModelBase item, CommandParametersModel parameters)
        {
            //if (item != null)
            //{
            //    try
            //    {
            //        JObject jobj = await item.GetProcessedItem(parameters);
            //        if (jobj != null)
            //        {
            //            if (item is OverlayImageItemModel || item is OverlayVideoItemModel || item is OverlaySoundItemModel)
            //            {
            //                string filepath = jobj["FilePath"].ToString();
            //                if (!ServiceManager.Get<IFileService>().IsURLPath(filepath) && !ServiceManager.Get<IFileService>().FileExists(filepath))
            //                {
            //                    Logger.Log(LogLevel.Error, $"Overlay Action - File does not exist: {filepath}");
            //                }

            //                this.SetLocalFile(jobj["FileID"].ToString(), filepath);
            //                jobj["FullLink"] = OverlayItemModelBase.GetFileFullLink(jobj["FileID"].ToString(), jobj["FileType"].ToString(), filepath);
            //            }
            //            await this.SendPacket("Show", jobj);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log(ex);
            //    }
            //}
            return Task.CompletedTask;
        }

        public Task UpdateItem(OverlayItemModelBase item, CommandParametersModel parameters)
        {
            //if (item != null)
            //{
            //    try
            //    {
            //        JObject jobj = await item.GetProcessedItem(parameters);
            //        if (jobj != null)
            //        {
            //            if (item is OverlayImageItemModel || item is OverlayVideoItemModel || item is OverlaySoundItemModel)
            //            {
            //                this.SetLocalFile(jobj["FileID"].ToString(), jobj["FilePath"].ToString());
            //                jobj["FullLink"] = OverlayItemModelBase.GetFileFullLink(jobj["FileID"].ToString(), jobj["FileType"].ToString(), jobj["FilePath"].ToString());
            //            }
            //            await this.SendPacket("Update", jobj);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log(ex);
            //    }
            //}
            return Task.CompletedTask;
        }

        public async Task HideItem(OverlayItemModelBase item)
        {
            await this.SendPacket("Hide", JObject.FromObject(item));
        }

        public async Task SendTextToSpeech(OverlayTextToSpeech textToSpeech) { await this.SendPacket("TextToSpeech", textToSpeech); }

        public string GetURLForLocalFile(string filePath, string fileType)
        {
            return this.httpListenerServer.GetURLForFile(filePath, fileType);
        }

        public void SetLocalFile(string fileID, string filePath)
        {
            this.httpListenerServer.SetLocalFile(fileID, filePath);
        }

        private Task SendPacket(string type, object contents)
        {
            //OverlayPacket packet = new OverlayPacket(type, JObject.FromObject(contents));
            //if (this.isBatching)
            //{
            //    this.batchPackets.Add(packet);
            //}
            //else
            //{
            //    //await this.webSocketServer.Send(packet);
            //}
            return Task.CompletedTask;
        }

        private void WebSocketServer_OnConnectedOccurred(object sender, WebSocketServerBase e)
        {
            this.OnWebSocketConnectedOccurred(this, new EventArgs());
        }

        private void WebSocketServer_OnDisconnectOccurred(WebSocketServerBase sender, WebSocketCloseStatus closeStatus)
        {
            this.OnWebSocketDisconnectedOccurred(this, closeStatus);
        }
    }

    public class OverlayHttpListenerServer : LocalHttpListenerServer
    {
        private const string OverlayFolderPath = "Overlay\\";
        private const string OverlayWebpageFilePath = OverlayFolderPath + "Overlay.html";

        private const string OverlayFilesWebPath = "overlay/files/";

        private string webPageInstance;

        private Dictionary<string, string> localFiles = new Dictionary<string, string>();

        public OverlayHttpListenerServer()
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

    public class OverlayWebSocketHttpListenerServer : WebSocketHttpListenerServerBase
    {
        public OverlayWebSocketHttpListenerServer() { }

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
