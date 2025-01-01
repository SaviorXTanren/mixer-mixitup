using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class PolyPopTriggerActionPacket
    {
        public string type { get; set; }

        public string title { get; set; }

        public JObject variables { get; set; } = new JObject();

        public string Type { get { return this.type; } set { this.type = value; } }

        public PolyPopTriggerActionPacket(string title, Dictionary<string, string> variables)
        {
            this.type = "ALERT";
            this.title = title;

            this.variables["text"] = new JObject();
            foreach (var kvp in variables)
            {
                this.variables["text"][kvp.Key] = kvp.Value;
            }
        }
    }

    public class PolyPopWebSocketServer : WebSocketServerBase
    {
        public PolyPopWebSocketServer(HttpListenerContext listenerContext) : base(listenerContext) { this.OnDisconnectOccurred += PolyPopWebServer_OnDisconnectOccurred; }

        public event EventHandler Connected { add { this.OnConnectedOccurred += value; } remove { this.OnConnectedOccurred -= value; } }
        public event EventHandler Disconnected = delegate { };

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            Logger.Log(LogLevel.Debug, "PolyPop Web Socket Packet Received - " + packetJSON);

            await base.ProcessReceivedPacket(packetJSON);
        }

        private void PolyPopWebServer_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            Logger.Log(LogLevel.Debug, "PolyPop Disconnected");

            this.Disconnected(sender, new EventArgs());
        }
    }

    public class PolyPopService : WebSocketHttpListenerServerBase, IExternalService
    {
        public const string PolyPopWebSocketServerAddressFormat = "http://127.0.0.1:{0}/";

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        public PolyPopService()
        {
            base.OnConnectedOccurred += PolyPopService_OnConnectedOccurred;
            base.OnDisconnectOccurred += PolyPopService_OnDisconnectOccurred;
        }

        public string Name { get { return MixItUp.Base.Resources.PolyPop; } }

        public bool IsEnabled { get { return ChannelSession.Settings.PolyPopPortNumber > 0; } }

        public bool IsConnected { get; private set; }

        public Task<Result> Connect()
        {
            this.IsConnected = false;
            if (this.Start(string.Format(PolyPopWebSocketServerAddressFormat, ChannelSession.Settings.PolyPopPortNumber)))
            {
                this.IsConnected = true;
                ServiceManager.Get<ITelemetryService>().TrackService("PolyPop");
                return Task.FromResult(new Result());
            }
            return Task.FromResult(new Result(MixItUp.Base.Resources.PolyPopFailedToConnect));
        }

        public async Task Disconnect()
        {
            this.IsConnected = false;
            await this.Stop();
        }

        public async Task TriggerAlert(string alertName, Dictionary<string, string> variables)
        {
            await this.Send(JSONSerializerHelper.SerializeToString(new PolyPopTriggerActionPacket(alertName, variables)));
        }

        protected override WebSocketServerBase CreateWebSocketServer(HttpListenerContext listenerContext)
        {
            return new PolyPopWebSocketServer(listenerContext);
        }

        private void PolyPopService_OnConnectedOccurred(object sender, WebSocketServerBase e)
        {
            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.PolyPop);
            this.Connected(sender, new EventArgs());
        }

        private void PolyPopService_OnDisconnectOccurred(WebSocketServerBase sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.PolyPop);
            this.Disconnected(sender, new EventArgs());
        }
    }
}