using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class InfiniteAlbumToken
    {
        public string Token { get; set; }
        public string UserId { get; set; }
    }

    public class InfiniteAlbumCommand
    {
        [JsonProperty("category")]
        public string Category { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("data")]
        public string Data { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class InfiniteAlbumWebSocket : ClientWebSocketBase
    {
        protected override Task ProcessReceivedPacket(string packet)
        {
            return Task.CompletedTask;
        }
    }

    public class InfiniteAlbumService : OAuthExternalServiceBase
    {
        private const string BaseAddress = "https://api.infinitealbum.io/";
        private const string WebsocketAddress = "wss://socket.api.infinitealbum.io";

        private readonly InfiniteAlbumWebSocket websocket = new InfiniteAlbumWebSocket();

        public override string Name => Resources.InfiniteAlbum;

        public InfiniteAlbumService() : base(BaseAddress)
        {
        }

        public override Task<Result> Connect()
        {
            return this.Connect(ChannelSession.Settings.InfiniteAlbumOAuthToken);
        }

        public override async Task Disconnect()
        {
            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            await this.websocket.Disconnect();

            this.token = null;
        }

        public async Task SendCommand(InfiniteAlbumCommand command)
        {
            await this.websocket.Send(JSONSerializerHelper.SerializeToString(command, includeObjectType: false));
        }

        public Task<Dictionary<string, List<InfiniteAlbumCommand>>> GetCommands()
        {
            return this.GetAsync<Dictionary<string, List<InfiniteAlbumCommand>>>($"{BaseAddress}core/service/websocket/config");
        }

        protected override async Task<Result> InitializeInternal()
        {
            if (this.token?.accessToken == null)
            {
                return new Result(false);
            }

            if (!this.websocket.IsOpen())
            {
                if (!await this.ConnectWebSocket())
                {
                    return new Result(MixItUp.Base.Resources.InfiniteAlbumConnectionFailed);
                }

                this.TrackServiceTelemetry("Infinite Album");
                return new Result();
            }

            return new Result(false);
        }

        protected override async Task<AdvancedHttpClient> GetHttpClient(bool autoRefreshToken = true)
        {
            // Hack to remove "Bearer" scheme from the Authorization header
            var client = await base.GetHttpClient(autoRefreshToken);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", this.token?.accessToken);
            return client;
        }

        protected override async Task RefreshOAuthToken()
        {
            try
            {
                if (this.token?.accessToken != null)
                {
                    JObject payload = new JObject();
                    InfiniteAlbumToken iaToken = await this.PostAsync<InfiniteAlbumToken>($"{BaseAddress}core/auth/refresh", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);

                    this.token = new OAuthTokenModel
                    {
                        accessToken = iaToken.Token,
                        clientID = iaToken.UserId,
                        expiresIn = int.MaxValue,
                    };
                }
                else if (this.token?.refreshToken != null)
                {
                    JObject payload = new JObject();
                    payload["token"] = this.token?.refreshToken;

                    InfiniteAlbumToken iaToken = await this.PostAsync<InfiniteAlbumToken>($"{BaseAddress}core/auth/temp/exchange", AdvancedHttpClient.CreateContentFromObject(payload), autoRefreshToken: false);

                    this.token = new OAuthTokenModel
                    {
                        accessToken = iaToken.Token,
                        clientID = iaToken.UserId,
                        expiresIn = int.MaxValue,
                    };
                }
                else
                {
                    this.token = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override Task<OAuthTokenModel> GetOAuthToken(bool autoRefreshToken = true)
        {
            return Task.FromResult(this.token);
        }

        private async Task<bool> ConnectWebSocket()
        {
            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            return await this.websocket.Connect($"{WebsocketAddress}?authorization={this.token.accessToken}");
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.InfiniteAlbum);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.InitializeInternal();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.InfiniteAlbum);
        }
    }
}
