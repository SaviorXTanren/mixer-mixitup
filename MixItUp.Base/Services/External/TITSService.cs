using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class TITSItem
    {
        public string ID { get; set; }
        public string name { get; set; }
    }

    public class TITSTrigger
    {
        public string ID { get; set; }
        public string name { get; set; }
    }

    public class TITSWebSocketRequestPacket
    {
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public string requestID { get; set; }
        public string messageType { get; set; }
        public JObject data { get; set; }

        [Obsolete]
        // Only for use with TITSItemListRequest request
        public bool sendImage { get; set; }

        public TITSWebSocketRequestPacket() { }

        public TITSWebSocketRequestPacket(string messageType)
        {
            this.apiName = "TITSPublicApi";
            this.apiVersion = "1.0";
            this.requestID = Guid.NewGuid().ToString();
            this.messageType = messageType;
        }

        public TITSWebSocketRequestPacket(string messageType, JObject data)
            : this(messageType)
        {
            this.data = data;
        }

        public int GetErrorID()
        {
            if (this.data != null && this.data.ContainsKey("errorID"))
            {
                return (int)this.data["errorID"];
            }
            return -1;
        }
    }

    public class TITSWebSocketResponsePacket : TITSWebSocketRequestPacket
    {
        public long timestamp { get; set; }
    }

    public class TITSWebSocket : ClientWebSocketBase
    {
        public event EventHandler<TITSWebSocketResponsePacket> ResponseReceived = delegate { };

        public async Task Send(TITSWebSocketRequestPacket packet)
        {
            Logger.Log(LogLevel.Debug, "TITS Packet Sent - " + JSONSerializerHelper.SerializeToString(packet));

            await this.Send(JSONSerializerHelper.SerializeToString(packet));
        }

        protected override Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "TITS Packet Received - " + packet);

                TITSWebSocketResponsePacket response = JSONSerializerHelper.DeserializeFromString<TITSWebSocketResponsePacket>(packet);
                if (response != null && !string.IsNullOrEmpty(response.requestID))
                {
                    this.ResponseReceived(this, response);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// https://github.com/Remasuri/TITSAPI
    /// </summary>
    public class TITSService : OAuthExternalServiceBase
    {
        public const int DefaultPortNumber = 42069;
        public const int MaxCacheDuration = 30;

        private const string websocketAddress = "ws://localhost:";

        private TITSWebSocket websocket = new TITSWebSocket();

        public DateTimeOffset ItemsLastUpdated { get; private set; } = DateTimeOffset.MinValue;
        private IEnumerable<TITSItem> allItemsCache;

        public DateTimeOffset TriggersLastUpdated { get; private set; } = DateTimeOffset.MinValue;
        private IEnumerable<TITSTrigger> allTriggersCache;

        private CancellationTokenSource backgroundRefreshCancellationTokenSource;

        public TITSService()
            : base(string.Empty)
        {
            this.websocket.ResponseReceived += Websocket_ResponseReceived;
        }

        public override string Name { get { return MixItUp.Base.Resources.TwitchIntegratedThrowingSystem; } }

        public bool IsEnabled { get { return ChannelSession.Settings.TITSOAuthToken != null; } }

        public bool IsWebSocketConnected { get { return this.websocket.IsOpen(); } }

        public override async Task<Result> Connect()
        {
            try
            {
                if (await this.ConnectWebSocket())
                {
                    this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
                    this.websocket.OnDisconnectOccurred += Websocket_OnDisconnectOccurred;
                    return await this.InitializeInternal();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(MixItUp.Base.Resources.TITSConnectionFailed);
        }

        public override async Task<Result> Connect(OAuthTokenModel token)
        {
            this.token = token;
            return await this.Connect();
        }

        public override async Task Disconnect()
        {
            this.token = null;

            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;

            if (this.backgroundRefreshCancellationTokenSource != null)
            {
                this.backgroundRefreshCancellationTokenSource.Cancel();
                this.backgroundRefreshCancellationTokenSource = null;
            }

            await this.websocket.Disconnect();
        }

        public async Task<bool> RequestAllItems()
        {
            try
            {
                if (this.websocket.IsOpen())
                {
                    JObject data = new JObject();
                    data["sendImage"] = false;

#pragma warning disable CS0612 // Type or member is obsolete
                    await this.websocket.Send(new TITSWebSocketRequestPacket("TITSItemListRequest", data)
                    {
                        sendImage = false
                    });
#pragma warning restore CS0612 // Type or member is obsolete
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public IEnumerable<TITSItem> GetAllItems()
        {
            if (this.allItemsCache != null)
            {
                return this.allItemsCache;
            }
            return new List<TITSItem>();
        }

        public async Task<bool> ThrowItem(string itemID, double delayTime, int amount)
        {
            JObject data = new JObject();
            data["items"] = new JArray() { itemID };
            data["delayTime"] = delayTime;
            data["amountOfThrows"] = amount;

            try
            {
                if (this.websocket.IsOpen())
                {
                    await this.websocket.Send(new TITSWebSocketRequestPacket("TITSThrowItemsRequest", data));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        public async Task<bool> RequestAllTriggers()
        {
            try
            {
                if (this.websocket.IsOpen())
                {
                    await this.websocket.Send(new TITSWebSocketRequestPacket("TITSTriggerListRequest"));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public IEnumerable<TITSTrigger> GetAllTriggers()
        {
            if (this.allTriggersCache != null)
            {
                return this.allTriggersCache;
            }
            return new List<TITSTrigger>();
        }

        public async Task ActivateTrigger(string triggerID)
        {
            JObject data = new JObject();
            data["triggerID"] = triggerID;

            try
            {
                if (this.websocket.IsOpen())
                {
                    await this.websocket.Send(new TITSWebSocketRequestPacket("TITSTriggerActivateRequest", data));
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        protected override Task<Result> InitializeInternal()
        {
            this.token = new OAuthTokenModel();

            this.backgroundRefreshCancellationTokenSource = new CancellationTokenSource();
            AsyncRunner.RunAsyncBackground(async (cancellationToken) =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await this.RequestAllItems();
                    await this.RequestAllTriggers();
                }
            }, this.backgroundRefreshCancellationTokenSource.Token, 600000);

            return Task.FromResult(new Result());
        }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }

        private async Task<bool> ConnectWebSocket()
        {
            return await this.websocket.Connect(websocketAddress + ChannelSession.Settings.TITSPortNumber + "/websocket");
        }

        private void Websocket_ResponseReceived(object sender, TITSWebSocketResponsePacket response)
        {
            if (response != null)
            {
                if (string.Equals(response.messageType, "TITSItemListResponse"))
                {
                    if (response.data != null && response.data.TryGetValue("items", out JToken items) && items is JArray)
                    {
                        List<TITSItem> results = new List<TITSItem>();
                        foreach (TITSItem item in ((JArray)items).ToTypedArray<TITSItem>())
                        {
                            if (item != null)
                            {
                                results.Add(item);
                            }
                        }
                        this.allItemsCache = results;
                        this.ItemsLastUpdated = DateTimeOffset.Now;
                    }
                }
                else if (string.Equals(response.messageType, "TITSTriggerListResponse"))
                {
                    if (response.data != null && response.data.TryGetValue("triggers", out JToken triggers) && triggers is JArray)
                    {
                        List<TITSTrigger> results = new List<TITSTrigger>();
                        foreach (TITSTrigger trigger in ((JArray)triggers).ToTypedArray<TITSTrigger>())
                        {
                            if (trigger != null)
                            {
                                results.Add(trigger);
                            }
                        }
                        this.allTriggersCache = results;
                        this.TriggersLastUpdated = DateTimeOffset.Now;
                    }
                }
            }
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TwitchIntegratedThrowingSystem);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TwitchIntegratedThrowingSystem);
        }
    }
}
