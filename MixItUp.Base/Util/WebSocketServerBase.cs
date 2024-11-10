using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
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
                    await this.Send(JsonConvert.SerializeObject(new JObject() { { "Type", "Debug" } }));
                }

                this.OnConnectedOccurred(this, new EventArgs());

                await this.Receive();

                return true;
            }
            catch (Exception ex) { Logger.Log(ex); }
            return false;
        }

        public async Task<bool> TestConnection()
        {
            this.connectionTestSuccessful = false;

            await this.Send(JsonConvert.SerializeObject(new JObject() { { "Type", "Test" } }));

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
                try
                {
                    JObject packetObj = JObject.Parse(packetJSON);
                    if (packetObj["Type"].ToString().Equals("Exception"))
                    {
                        Logger.Log("WebSocket Client Exception: " + packetObj["Data"].ToString());
                    }
                    else if (packetObj["Type"].ToString().Equals("Test"))
                    {
                        this.connectionTestSuccessful = true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    Logger.Log(LogLevel.Error, "WebSocket Packet Error: " + packetJSON);
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
