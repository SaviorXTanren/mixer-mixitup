using Mixer.Base.Model.Client;
using Mixer.Base.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Util
{
    public abstract class WebSocketHttpListenerServerBase : HttpListenerServerBase
    {
        public event EventHandler OnConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnDisconnectOccurred = delegate { };

        private LockedList<WebSocketServerBase> webSocketServers = new LockedList<WebSocketServerBase>();

        public WebSocketHttpListenerServerBase(string address) : base(address) { }

        public new async Task End()
        {
            foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
            {
                await webSocketServer.Disconnect();
            }
            this.webSocketServers.Clear();
            base.End();
        }

        public async Task Send(WebSocketPacket packet)
        {
            foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
            {
                await webSocketServer.Send(packet);
            }
        }

        public async Task<bool> TestConnection()
        {
            List<bool> results = new List<bool>();
            foreach (WebSocketServerBase webSocketServer in this.webSocketServers)
            {
                results.Add(await webSocketServer.TestConnection());
            }
            return results.TrueForAll(r => r);
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
