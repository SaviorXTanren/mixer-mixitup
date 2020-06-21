using Mixer.Base.Model.Client;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class WebSocketHttpListenerServerBase : LocalHttpListenerServer
    {
        public event EventHandler OnConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnDisconnectOccurred = delegate { };

        private LockedList<WebSocketServerBase> webSocketServers = new LockedList<WebSocketServerBase>();

        public WebSocketHttpListenerServerBase(string address) : base(address) { }

        public int TotalConnectedClients { get { return this.webSocketServers.Count; } }

        public new async Task Stop()
        {
            foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
            {
                await webSocketServer.Disconnect();
            }
            this.webSocketServers.Clear();
            base.Stop();
        }

        public async Task Send(WebSocketPacket packet)
        {
            try
            {
                foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
                {
                    Logger.Log(LogLevel.Debug, "Sending Web Socket Packet - " + JSONSerializerHelper.SerializeToString(packet));

                    await webSocketServer.Send(packet);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        public async Task<int> TestConnection()
        {
            int count = 0;
            foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
            {
                if (await webSocketServer.TestConnection())
                {
                    count++;
                }
            }
            return count;
        }

        protected override async Task ProcessConnection(HttpListenerContext listenerContext)
        {
            try
            {
                if (listenerContext.Request.IsWebSocketRequest)
                {
                    WebSocketServerBase webSocketServer = this.CreateWebSocketServer(listenerContext);
                    this.webSocketServers.Add(webSocketServer);
                    webSocketServer.OnConnectedOccurred += WebSocketServer_OnConnectedOccurred;
                    webSocketServer.OnDisconnectOccurred += WebSocketServer_OnDisconnectOccurred;

                    await webSocketServer.Initialize();

                    listenerContext.Response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    listenerContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                listenerContext.Response.Close();
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        protected abstract WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext);

        private void WebSocketServer_OnConnectedOccurred(object sender, EventArgs e)
        {
            this.OnConnectedOccurred(sender, e);
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            this.webSocketServers.Remove((WebSocketServerBase)sender);
            this.OnDisconnectOccurred(sender, e);
        }
    }
}
