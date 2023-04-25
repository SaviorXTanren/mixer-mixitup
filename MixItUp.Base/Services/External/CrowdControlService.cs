using Google.Apis.YouTubePartner.v1.Data;
using MixItUp.Base.Services.Twitch;
using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class CrowdControlEffectUpdateMessage
    {
        public string channelID { get; set; }
        public string id { get; set; }
        public int state { get; set; }
        public object effect { get; set; }
        public int completedAt { get; set; }
        public string type { get; set; }
    }

    public class CrowdControlCoinExchangeMessage
    {
        public string channelID { get; set; }
        public bool playSFX { get; set; }
        public bool anonymous { get; set; }
        public int timestamp { get; set; }
        public string type { get; set; }
        public int amount { get; set; }
        public CrowdControlViewer viewer { get; set; }
    }

    public class CrowdControlViewer
    {
        public string name { get; set; }
        public string twitchID { get; set; }
        public string twitchAvatarURL { get; set; }
    }

    public class CrowdControlWebSocket : ClientWebSocketBase
    {
        protected override async Task ProcessReceivedPacket(string packet)
        {
            if (!string.IsNullOrEmpty(packet) && !packet.StartsWith("0"))
            {
                if (packet.Equals("2"))
                {
                    await this.Send("3");
                }
                else
                {
                    //CrowdControlEffectUpdateMessage effectUpdate = JSONSerializerHelper.DeserializeFromString<CrowdControlEffectUpdateMessage>(data.ToString());
                    //if (effectUpdate != null)
                    //{
                    //      "effect-initial"
                    //}

                    //CrowdControlCoinExchangeMessage coinExchange = JSONSerializerHelper.DeserializeFromString<CrowdControlCoinExchangeMessage>(data.ToString());
                    //if (coinExchange != null)
                    //{
                    //      "coin-message"
                    //}
                }
            }

            Logger.Log(packet);
        }
    }

    public class CrowdControlService : IExternalService
    {
        private const string ConnectionURL = "wss://overlay-socket.crowdcontrol.live/socket.io/?EIO=4&transport=websocket";

        public string Name { get { return Resources.CrowdControl; } }

        public bool IsConnected { get; private set; }

        private CrowdControlWebSocket socket;

        public CrowdControlService() { }

        public async Task<Result> Connect()
        {
            if (!ServiceManager.Get<TwitchSessionService>().IsConnected)
            {
                return new Result(Resources.CrowdControlTwitchAccountMustBeConnected);
            }

            try
            {
                this.socket = new CrowdControlWebSocket();
                this.socket.OnDisconnectOccurred += Socket_OnDisconnectOccurred;
                if (await this.socket.Connect(CrowdControlService.ConnectionURL))
                {
                    //await this.socket.Send("[\"events\",\"DamagedPlushie\"]");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(Resources.CrowdControlFailedToConnectToService);
        }

        private async void Socket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            await this.Disconnect();
        }

        public async Task Disconnect()
        {
            if (this.socket != null)
            {
                await this.socket.Disconnect();
            }
            this.socket = null;
        }
    }
}
