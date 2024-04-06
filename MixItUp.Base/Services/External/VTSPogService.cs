using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class VTSPogWebSocketResponsePacket
    {
        public string type { get; set; }
        public JObject data { get; set; }
    }

    public class VTSPogWebSocket : ClientWebSocketBase
    {
        protected override Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "VTS POG Packet Received - " + packet);

                VTSPogWebSocketResponsePacket response = JSONSerializerHelper.DeserializeFromString<VTSPogWebSocketResponsePacket>(packet);
                if (response != null && !string.IsNullOrEmpty(response.type))
                {
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return Task.CompletedTask;
        }
    }

    public class VTSPogService : IExternalService
    {
        private const string BaseAddress = "http://localhost:3800/";
        private const string WebSocketAddress = "ws://localhost:3800/api";

        private VTSPogWebSocket websocket = null;

        public string Name { get { return Resources.VTSPog; } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            if (await this.Status())
            {
                this.websocket = new VTSPogWebSocket();
                this.websocket.OnDisconnectOccurred += Websocket_OnDisconnectOccurred;
                if (await this.websocket.Connect(VTSPogService.WebSocketAddress))
                {
                    this.IsConnected = true;
                    return new Result();
                }
            }
            return new Result(Resources.VTSPogFailedToConnect);
        }

        public async Task Disconnect()
        {
            if (this.websocket != null)
            {
                this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
                await this.websocket.Disconnect();
            }
            this.websocket = null;

            this.IsConnected = false;
        }

        public async Task<bool> ToggleTTSQueue()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    await client.GetAsync("stopTTS");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task<bool> SkipAudio()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    await client.GetAsync("skip");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task<bool> Status()
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    string result = await client.GetStringAsync("status");
                    if (string.Equals(result, "1"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.VTSPog);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.VTSPog);
        }
    }
}
