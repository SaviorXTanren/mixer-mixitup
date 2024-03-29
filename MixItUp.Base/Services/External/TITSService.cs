﻿using MixItUp.Base.Util;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Model.OAuth;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Collections.Generic;
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
        private Dictionary<string, TITSWebSocketResponsePacket> responses = new Dictionary<string, TITSWebSocketResponsePacket>();

        public async Task<TITSWebSocketResponsePacket> SendAndReceive(TITSWebSocketRequestPacket packet, int delaySeconds = 5)
        {
            Logger.Log(LogLevel.Debug, "TITS Packet Sent - " + JSONSerializerHelper.SerializeToString(packet));

            this.responses.Remove(packet.requestID);

            await this.Send(JSONSerializerHelper.SerializeToString(packet));

            int cycles = delaySeconds * 10;
            for (int i = 0; i < cycles && !this.responses.ContainsKey(packet.requestID); i++)
            {
                await Task.Delay(100);
            }

            this.responses.TryGetValue(packet.requestID, out TITSWebSocketResponsePacket response);
            return response;
        }

        protected override Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "TITS Packet Received - " + packet);

                TITSWebSocketResponsePacket response = JSONSerializerHelper.DeserializeFromString<TITSWebSocketResponsePacket>(packet);
                if (response != null && !string.IsNullOrEmpty(response.requestID))
                {
                    this.responses[response.requestID] = response;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return Task.CompletedTask;
        }
    }

    public class TITSService : OAuthExternalServiceBase
    {
        public const int DefaultPortNumber = 42069;
        public const int MaxCacheDuration = 30;

        private const string websocketAddress = "ws://localhost:";

        public bool WebSocketConnected { get; private set; }

        private TITSWebSocket websocket = new TITSWebSocket();

        private IEnumerable<TITSItem> allItemsCache;
        private DateTimeOffset allItemsCacheExpiration = DateTimeOffset.MinValue;

        private IEnumerable<TITSTrigger> allTriggersCache;
        private DateTimeOffset allTriggersCacheExpiration = DateTimeOffset.MinValue;

        public TITSService() : base(string.Empty) { }

        public override string Name { get { return MixItUp.Base.Resources.TwitchIntegratedThrowingSystem; } }

        public bool IsEnabled { get { return ChannelSession.Settings.TITSOAuthToken != null; } }

        public override async Task<Result> Connect()
        {
            try
            {
                if (await this.ConnectWebSocket())
                {
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
            this.WebSocketConnected = false;

            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            await this.websocket.Disconnect();
        }

        public async Task<IEnumerable<TITSItem>> GetAllItems()
        {
            try
            {
                if (this.allItemsCacheExpiration <= DateTimeOffset.Now || this.allItemsCache == null)
                {
                    TITSWebSocketResponsePacket response = await this.websocket.SendAndReceive(new TITSWebSocketRequestPacket("TITSItemListRequest"));
                    if (response != null && response.data != null && response.data.TryGetValue("items", out JToken items) && items is JArray)
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
                        this.allItemsCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "TITS - No Response Packet Received - GetAllItems");
                    }
                }

                if (this.allItemsCache != null)
                {
                    return this.allItemsCache;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
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
                TITSWebSocketResponsePacket response = await this.websocket.SendAndReceive(new TITSWebSocketRequestPacket("TITSThrowItemsRequest", data));
                if (response != null && response.data != null && response.data.ContainsKey("numberOfThrownItems"))
                {
                    return true;
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"TITS - No Response Packet Received - ThrowItem - {itemID}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        public async Task<IEnumerable<TITSTrigger>> GetAllTriggers()
        {
            try
            {
                if (this.allTriggersCacheExpiration <= DateTimeOffset.Now || this.allTriggersCache == null)
                {
                    TITSWebSocketResponsePacket response = await this.websocket.SendAndReceive(new TITSWebSocketRequestPacket("TITSTriggerListRequest"));
                    if (response != null && response.data != null && response.data.TryGetValue("triggers", out JToken triggers) && triggers is JArray)
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
                        this.allTriggersCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, $"TITS - No Response Packet Received - GetAllTriggers");
                    }
                }

                if (this.allTriggersCache != null)
                {
                    return this.allTriggersCache;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<TITSTrigger>();
        }

        public async Task<bool> ActivateTrigger(string triggerID)
        {
            JObject data = new JObject();
            data["triggerID"] = triggerID;

            try
            {
                TITSWebSocketResponsePacket response = await this.websocket.SendAndReceive(new TITSWebSocketRequestPacket("TITSTriggerActivateRequest", data));
                if (response != null && response.data != null && response.messageType.Equals("TITSTriggerActivateResponse"))
                {
                    return true;
                }
                else
                {
                    Logger.Log(LogLevel.Error, $"TITS - No Response Packet Received - ActivateTrigger - {triggerID}");
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        public void ClearCaches()
        {
            this.allItemsCache = null;
            this.allItemsCacheExpiration = DateTimeOffset.MinValue;

            this.allTriggersCache = null;
            this.allTriggersCacheExpiration = DateTimeOffset.MinValue;
        }

        protected override Task<Result> InitializeInternal()
        {
            this.token = new OAuthTokenModel();
            return Task.FromResult(new Result());
        }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }

        private async Task<bool> ConnectWebSocket()
        {
            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            return await this.websocket.Connect(websocketAddress + ChannelSession.Settings.TITSPortNumber + "/websocket");
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.TwitchIntegratedThrowingSystem);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.InitializeInternal();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.TwitchIntegratedThrowingSystem);
        }
    }
}
