using MixItUp.Base.Model.Commands;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class PulsoidHeartRate
    {
        public long measured_at { get; set; }
        public JObject data { get; set; }

        public int HeartRate
        {
            get
            {
                if (this.data != null && this.data.TryGetValue("heart_rate", out JToken value) && int.TryParse(value.ToString(), out int result))
                {
                    return result;
                }
                return 0;
            }
        }
    }

    public class PulsoidWebSocket : ClientWebSocketBase
    {
        public PulsoidHeartRate LastHeartRate { get; private set; }

        private DateTimeOffset lastTrigger = DateTimeOffset.MinValue;

        protected override async Task ProcessReceivedPacket(string packetJSON)
        {
            try
            {
                PulsoidHeartRate packet = JSONSerializerHelper.DeserializeFromString<PulsoidHeartRate>(packetJSON);
                if (packet != null)
                {
                    Logger.Log("Pulsoid Service - Heart Rate Received: " + JSONSerializerHelper.SerializeToString(packet.data));

                    if (ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers.Count > 0 && this.LastHeartRate != null)
                    {
                        foreach (var range in ChannelSession.Settings.PulsoidCommandHeartRateRangeTriggers)
                        {
                            if (MathHelper.InRangeInclusive(packet.HeartRate, range.Item1, range.Item2) &&
                                !MathHelper.InRangeInclusive(this.LastHeartRate.HeartRate, range.Item1, range.Item2))
                            {
                                await this.TriggerCommand(packet);
                                break;
                            }
                        }
                    }
                    else
                    {
                        await this.TriggerCommand(packet);
                    }
                    this.LastHeartRate = packet;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Log("Pulsoid Service - Failed Packet Processing: " + packetJSON);
            }
        }

        private async Task TriggerCommand(PulsoidHeartRate packet)
        {
            if (this.lastTrigger.TotalSecondsFromNow() >= ChannelSession.Settings.PulsoidCommandTriggerDelay)
            {
                CommandParametersModel parameters = new CommandParametersModel();
                parameters.SpecialIdentifiers["pulsoidheartrate"] = packet.HeartRate.ToString();
                if (await ServiceManager.Get<EventService>().PerformEvent(EventTypeEnum.PulsoidHeartRateChanged, parameters))
                {
                    this.lastTrigger = DateTimeOffset.Now;
                }
            }
        }
    }

    /// <summary>
    /// https://docs.pulsoid.net
    /// </summary>
    public class PulsoidService : OAuthExternalServiceBase
    {
        public const string ClientID = "21c57a62-d890-4f3d-8879-4b1171d51858";

        public const string BaseAddress = "https://dev.pulsoid.net/api/v1/";

        public const string TokenUrl = "https://pulsoid.net/oauth2/token";

        public const string WebsocketUrl = "wss://dev.pulsoid.net/api/v1/data/real_time";

        public override string Name { get { return Resources.Pulsoid; } }

        public string AuthorizationURL { get { return $"https://pulsoid.net/oauth2/authorize?client_id={PulsoidService.ClientID}&redirect_uri={OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL}&response_type=code&scope=data:heart_rate:read&state={Guid.NewGuid()}"; } }

        private PulsoidWebSocket socket;

        public PulsoidService() : base(PulsoidService.BaseAddress) { }

        public PulsoidHeartRate LastHeartRate { get { return this.socket?.LastHeartRate; } }

        public override async Task<Result> Connect()
        {
            try
            {
                string authorizationCode = await this.ConnectViaOAuthRedirect(this.AuthorizationURL);
                if (!string.IsNullOrEmpty(authorizationCode))
                {
                    var body = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "authorization_code"),
                        new KeyValuePair<string, string>("client_id", PulsoidService.ClientID),
                        new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PulsoidSecret")),
                        new KeyValuePair<string, string>("redirect_uri", OAuthExternalServiceBase.DEFAULT_OAUTH_LOCALHOST_URL),
                        new KeyValuePair<string, string>("code", authorizationCode),
                    };

                    this.token = await this.GetWWWFormUrlEncodedOAuthToken(PulsoidService.TokenUrl, body);
                    if (this.token != null)
                    {
                        if (!await this.ConnectWebSocket())
                        {
                            await this.Disconnect();
                            return new Result(false);
                        }

                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
            return new Result(false);
        }

        public override async Task Disconnect()
        {
            await this.DisconnectWebSocket();

            this.token = null;
        }

        public async Task<PulsoidHeartRate> GetHeartRate()
        {
            HttpResponseMessage response = await this.GetAsync("data/heart_rate/latest");
            if (response.IsSuccessStatusCode)
            {
                return await response.ProcessResponse<PulsoidHeartRate>();
            }
            else if (response.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return new PulsoidHeartRate();
            }
            return null;
        }

        protected override async Task<Result> InitializeInternal()
        {
            PulsoidHeartRate heartRate = await this.GetHeartRate();
            if (heartRate != null)
            {
                return new Result();
            }
            return new Result(Resources.PulsoidUnableToGetHeartRateData);
        }

        protected override async Task RefreshOAuthToken()
        {
            if (this.token != null)
            {
                var body = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("client_id", PulsoidService.ClientID),
                    new KeyValuePair<string, string>("client_secret", ServiceManager.Get<SecretsService>().GetSecret("PulsoidSecret")),
                    new KeyValuePair<string, string>("refresh_token", this.token.refreshToken),
                };

                this.token = await this.GetWWWFormUrlEncodedOAuthToken(PulsoidService.TokenUrl, body);
            }
        }

        private async Task<bool> ConnectWebSocket()
        {
            await this.DisconnectWebSocket();

            this.socket = new PulsoidWebSocket();
            if (ChannelSession.IsDebug())
            {
                this.socket.OnSentOccurred += Socket_OnSentOccurred;
                this.socket.OnTextReceivedOccurred += Socket_OnTextReceivedOccurred;
            }
            this.socket.OnDisconnectOccurred += Socket_OnDisconnectOccurred;

            if (!await this.socket.Connect($"{PulsoidService.WebsocketUrl}?access_token={this.token.accessToken}"))
            {
                return false;
            }
            return true;
        }

        private async Task DisconnectWebSocket()
        {
            if (this.socket != null)
            {
                if (ChannelSession.IsDebug())
                {
                    this.socket.OnSentOccurred -= Socket_OnSentOccurred;
                    this.socket.OnTextReceivedOccurred -= Socket_OnTextReceivedOccurred;
                }
                this.socket.OnDisconnectOccurred -= Socket_OnDisconnectOccurred;
                await this.socket.Disconnect();

                this.socket = null;
            }
        }

        private async void Socket_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.Pulsoid);

            do
            {
                await this.DisconnectWebSocket();

                await Task.Delay(5000);
            }
            while (!await this.ConnectWebSocket());

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.Pulsoid);
        }

        private void Socket_OnSentOccurred(object sender, string e)
        {
            Logger.Log("Pulsoid Service - Packet Sent: " + e);
        }

        private void Socket_OnTextReceivedOccurred(object sender, string e)
        {
            Logger.Log("Pulsoid Service - Packet Received: " + e);
        }
    }
}
