using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public class WebSocketPacket
    {
        /// <summary>
        /// The type of packet.
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// The ID of the packet.
        /// </summary>
        public uint id { get; set; }
    }

    public abstract class WebSocketServerBase : WebSocketBase
    {
        public event EventHandler OnConnectedOccurred = delegate { };

        private HttpListenerContext context;

        private bool connectionTestSuccessful;

        public WebSocketServerBase(HttpListenerContext context) { this.context = context; }

        public async Task<bool> Initialize()
        {
            try
            {
                HttpListenerWebSocketContext wsc = await this.context.AcceptWebSocketAsync(null);
                this.SetWebSocket(wsc.WebSocket);

                if (ChannelSession.AppSettings.DiagnosticLogging)
                {
                    await this.Send(new WebSocketPacket() { type = "debug" });
                }

                this.OnConnectedOccurred(this, new EventArgs());

                await this.Receive();

                return true;
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task Send(WebSocketPacket packet) { await this.Send(JsonConvert.SerializeObject(packet)); }

        public async Task<bool> TestConnection()
        {
            this.connectionTestSuccessful = false;

            await this.Send(new WebSocketPacket() { type = "test" });

            await this.WaitForSuccess(() => this.connectionTestSuccessful);

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
            return Task.CompletedTask;
        }

        protected override async Task<WebSocketCloseStatus> Receive()
        {
            WebSocketCloseStatus closeStatus = await base.Receive();

            await this.Disconnect(closeStatus);

            return closeStatus;
        }
    }
}
