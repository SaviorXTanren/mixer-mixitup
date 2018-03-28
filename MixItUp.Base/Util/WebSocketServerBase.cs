using Mixer.Base.Model.Client;
using Mixer.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class WebSocketServerBase : WebSocketBase
    {
        private SemaphoreSlim asyncSemaphore = new SemaphoreSlim(1);

        private string address;

        private HttpListener httpListener;

        private bool connectionTestSuccessful;

        public WebSocketServerBase(string address) { this.address = address; }

        public async Task<bool> Initialize()
        {
            try
            {
                this.httpListener = new HttpListener();
                this.httpListener.Prefixes.Add(this.address);
                this.httpListener.Start();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                this.WaitForWebSocketConnection();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                return true;
            }
            catch (Exception ex) { Logger.Log(ex); }

            await this.DisconnectServer();

            return false;
        }

        public async Task DisconnectServer(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            await this.Disconnect(closeStatus);

            try
            {
                if (this.httpListener != null)
                {
                    using (this.httpListener)
                    {
                        this.httpListener.GetType().InvokeMember("RemoveAll", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, this.httpListener, new object[] { false });
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            this.httpListener = null;
        }

        public async Task Send(WebSocketPacket packet) { await this.Send(JsonConvert.SerializeObject(packet)); }

        public async Task<bool> TestConnection()
        {
            this.connectionTestSuccessful = false;

            await this.Send(new WebSocketPacket() { type = "test" });

            await this.WaitForResponse(() => this.connectionTestSuccessful);

            return this.connectionTestSuccessful;
        }

        protected override async Task SendInternal(byte[] buffer)
        {
            try
            {
                if (this.IsOpen())
                {
                    await this.webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override Task ProcessReceivedPacket(string packetJSON)
        {
            if (!string.IsNullOrEmpty(packetJSON))
            {
                JObject packetObj = JObject.Parse(packetJSON);
                if (packetObj["type"].ToString().Equals("exception"))
                {
                    Logger.Log("WebSocket Client Exception: " + packetObj["data"].ToString());
                }
                else if (packetObj["type"].ToString().Equals("test"))
                {
                    this.connectionTestSuccessful = true;
                }
            }
            return Task.FromResult(0);
        }

        protected async Task WaitForWebSocketConnection()
        {
            try
            {
                while (this.httpListener != null && this.httpListener.IsListening)
                {
                    try
                    {
                        var hc = await this.httpListener.GetContextAsync();
                        if (hc.Request.IsWebSocketRequest)
                        {
                            if (this.webSocket == null)
                            {
                                HttpListenerWebSocketContext wsc = await hc.AcceptWebSocketAsync(null);
                                this.SetWebSocket(wsc.WebSocket);

                                if (ChannelSession.Settings.DiagnosticLogging)
                                {
                                    await this.Send(new WebSocketPacket() { type = "debug" });
                                }

                                await this.Receive();

                                hc.Response.StatusCode = (int)HttpStatusCode.OK;
                            }
                            else
                            {
                                hc.Response.StatusCode = (int)HttpStatusCode.Conflict;
                            }
                        }
                        else
                        {
                            hc.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        }
                        hc.Response.Close();
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        protected override async Task<WebSocketCloseStatus> Receive()
        {
            WebSocketCloseStatus closeStatus = await base.Receive();

            await this.Disconnect(closeStatus);

            return closeStatus;
        }
    }
}
