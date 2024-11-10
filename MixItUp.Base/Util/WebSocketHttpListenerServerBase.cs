using MixItUp.Base.Util;
using MixItUp.Base.Web;
using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class WebSocketHttpListenerServerBase : LocalHttpListenerServer
    {
        public delegate void DisconnectHandler(WebSocketServerBase webSocketServer, WebSocketCloseStatus status);

        public event EventHandler<WebSocketServerBase> OnConnectedOccurred = delegate { };
        public event DisconnectHandler OnDisconnectOccurred = delegate { };

        private LockedList<WebSocketServerBase> webSocketServers = new LockedList<WebSocketServerBase>();

        public WebSocketHttpListenerServerBase() : base() { }

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

        public async Task Send(string packet)
        {
            try
            {
                foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
                {
                    Logger.Log(LogLevel.Debug, "Sending Web Socket Packet - " + packet);

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
                    if (webSocketServer != null)
                    {
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
            this.OnConnectedOccurred(sender, (WebSocketServerBase)sender);
        }

        private void WebSocketServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            this.webSocketServers.Remove((WebSocketServerBase)sender);
            this.OnDisconnectOccurred((WebSocketServerBase)sender, e);
        }
    }
}
