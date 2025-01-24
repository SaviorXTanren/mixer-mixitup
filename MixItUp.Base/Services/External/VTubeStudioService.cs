using MixItUp.Base.Model.Web;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class VTubeStudioModel
    {
        public bool modelLoaded { get; set; }
        public string modelName { get; set; }
        public string modelID { get; set; }
        public VTubeStudioModelPosition modelPosition { get; set; }
    }

    public class VTubeStudioModelPosition
    {
        public double positionX { get; set; }
        public double positionY { get; set; }
        public double rotation { get; set; }
        public double size { get; set; }
    }

    public class VTubeStudioHotKey
    {
        public string name { get; set; }
        public string type { get; set; }
        public string file { get; set; }
        public string hotkeyID { get; set; }

        public string DisplayName
        {
            get
            {
                return !string.IsNullOrEmpty(this.name) ? this.name : this.file;
            }
        }
    }

    public class VTubeStudioWebSocketRequestPacket
    {
        public string apiName { get; set; }
        public string apiVersion { get; set; }
        public string requestID { get; set; }
        public string messageType { get; set; }
        public JObject data { get; set; }

        public VTubeStudioWebSocketRequestPacket() { }

        public VTubeStudioWebSocketRequestPacket(string messageType)
        {
            this.apiName = "VTubeStudioPublicAPI";
            this.apiVersion = "1.0";
            this.requestID = Guid.NewGuid().ToString();
            this.messageType = messageType;
        }

        public VTubeStudioWebSocketRequestPacket(string messageType, JObject data)
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

    public class VTubeStudioWebSocketResponsePacket : VTubeStudioWebSocketRequestPacket
    {
        public long timestamp { get; set; }
    }

    public class VTubeStudioWebSocket : ClientWebSocketBase
    {
        private Dictionary<string, VTubeStudioWebSocketResponsePacket> responses = new Dictionary<string, VTubeStudioWebSocketResponsePacket>();

        public async Task<VTubeStudioWebSocketResponsePacket> SendAndReceive(VTubeStudioWebSocketRequestPacket packet, int delaySeconds = 5)
        {
            Logger.Log(LogLevel.Debug, "VTube Studio Packet Sent - " + JSONSerializerHelper.SerializeToString(packet));

            try
            {
                this.responses.Remove(packet.requestID);

                await this.Send(JSONSerializerHelper.SerializeToString(packet));

                int cycles = delaySeconds * 10;
                for (int i = 0; i < cycles && !this.responses.ContainsKey(packet.requestID); i++)
                {
                    await Task.Delay(100);
                }

                this.responses.TryGetValue(packet.requestID, out VTubeStudioWebSocketResponsePacket response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        protected override Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "VTube Studio Packet Received - " + packet);

                VTubeStudioWebSocketResponsePacket response = JSONSerializerHelper.DeserializeFromString<VTubeStudioWebSocketResponsePacket>(packet);
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

    public class VTubeStudioService : OAuthExternalServiceBase
    {
        public const int DefaultPortNumber = 8001;
        public const int MaxCacheDuration = 30;

        private const string websocketAddress = "ws://localhost:";

        private const string websocketPluginName = "Mix It Up";
        private const string websocketPluginDeveloper = "https://mixitupapp.com/";
        private const string websocketPluginIcon = "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";

        public bool WebSocketConnected { get; private set; }

        private VTubeStudioWebSocket websocket = new VTubeStudioWebSocket();

        private IEnumerable<VTubeStudioModel> allModelsCache;
        private DateTimeOffset allModelsCacheExpiration = DateTimeOffset.MinValue;

        private Dictionary<string, IEnumerable<VTubeStudioHotKey>> modelHotKeyCache = new Dictionary<string, IEnumerable<VTubeStudioHotKey>>();
        private DateTimeOffset modelHotKeyCacheExpiration = DateTimeOffset.MinValue;

        public VTubeStudioService() : base(string.Empty) { }

        public override string Name { get { return MixItUp.Base.Resources.VTubeStudio; } }

        public override async Task<Result> Connect()
        {
            try
            {
                this.token = null;
                if (await this.ConnectWebSocket())
                {
                    VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("APIStateRequest"));
                    if (response != null && response.data != null && response.data.ContainsKey("active"))
                    {
                        JObject data = new JObject();
                        data["pluginName"] = websocketPluginName;
                        data["pluginDeveloper"] = websocketPluginDeveloper;
                        data["pluginIcon"] = websocketPluginIcon;

                        response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("AuthenticationTokenRequest", data), 60);
                        if (response != null && response.data != null && response.data.ContainsKey("authenticationToken"))
                        {
                            this.token = new OAuthTokenModel()
                            {
                                accessToken = response.data["authenticationToken"].ToString()
                            };

                            return await this.InitializeInternal();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(MixItUp.Base.Resources.VTubeStudioConnectionFailed);
        }

        public override async Task<Result> Connect(OAuthTokenModel token)
        {
            try
            {
                this.token = token;
                if (await this.ConnectWebSocket())
                {
                    VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("APIStateRequest"));
                    if (response != null && response.data != null && response.data.ContainsKey("active"))
                    {
                        return await this.InitializeInternal();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(MixItUp.Base.Resources.VTubeStudioConnectionFailed);
        }

        public override async Task Disconnect()
        {
            this.ClearCaches();

            this.token = null;
            this.WebSocketConnected = false;

            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            await this.websocket.Disconnect();
        }

        public async Task<VTubeStudioModel> GetCurrentModel()
        {
            try
            {
                if (this.IsConnected)
                {
                    VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("CurrentModelRequest"));
                    if (response != null && response.data != null)
                    {
                        VTubeStudioModel result = response.data.ToObject<VTubeStudioModel>();
                        if (result != null && result.modelLoaded)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return null;
        }

        public async Task<IEnumerable<VTubeStudioModel>> GetAllModels()
        {
            try
            {
                if (this.IsConnected)
                {
                    if (this.allModelsCacheExpiration <= DateTimeOffset.Now || this.allModelsCache == null)
                    {
                        VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("AvailableModelsRequest"));
                        if (response != null && response.data != null && response.data.TryGetValue("availableModels", out JToken models) && models is JArray)
                        {
                            List<VTubeStudioModel> results = new List<VTubeStudioModel>();
                            foreach (VTubeStudioModel model in ((JArray)models).ToTypedArray<VTubeStudioModel>())
                            {
                                if (model != null)
                                {
                                    results.Add(model);
                                }
                            }
                            this.allModelsCache = results;
                            this.allModelsCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
                        }
                    }
                }

                if (this.allModelsCache != null)
                {
                    return this.allModelsCache;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new List<VTubeStudioModel>();
        }

        public async Task<bool> LoadModel(string modelID)
        {
            JObject data = new JObject();
            data["modelID"] = modelID;

            try
            {
                VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("ModelLoadRequest", data));
                if (response != null && response.data != null && response.data.ContainsKey("modelID"))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        public async Task<bool> MoveModel(double timeInSeconds, bool relative, double? x, double? y, double? rotation, double? size)
        {
            JObject data = new JObject();
            data["timeInSeconds"] = timeInSeconds;
            data["valuesAreRelativeToModel"] = relative;
            if (x.HasValue) { data["positionX"] = x; }
            if (y.HasValue) { data["positionY"] = y; }
            if (rotation.HasValue) { data["rotation"] = rotation; }
            if (size.HasValue) { data["size"] = size; }

            try
            {
                VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("MoveModelRequest", data));
                if (response != null && response.data != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }

            return false;
        }

        public async Task<IEnumerable<VTubeStudioHotKey>> GetHotKeys(string modelID = null)
        {
            try
            {
                if (this.IsConnected)
                {
                    if (this.modelHotKeyCacheExpiration <= DateTimeOffset.Now || !this.modelHotKeyCache.ContainsKey(modelID))
                    {
                        VTubeStudioWebSocketRequestPacket packet = new VTubeStudioWebSocketRequestPacket("HotkeysInCurrentModelRequest");
                        if (!string.IsNullOrEmpty(modelID))
                        {
                            packet.data = new JObject();
                            packet.data["modelID"] = modelID;
                        }

                        VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(packet);
                        if (response != null && response.data != null && response.data.TryGetValue("availableHotkeys", out JToken hotKeys) && hotKeys is JArray)
                        {
                            List<VTubeStudioHotKey> results = new List<VTubeStudioHotKey>();
                            foreach (VTubeStudioHotKey hotKey in ((JArray)hotKeys).ToTypedArray<VTubeStudioHotKey>())
                            {
                                if (hotKey != null)
                                {
                                    results.Add(hotKey);
                                }
                            }
                            this.modelHotKeyCache[modelID] = results;
                            this.modelHotKeyCacheExpiration = DateTimeOffset.Now.AddMinutes(MaxCacheDuration);
                        }
                    }
                }

                if (this.modelHotKeyCache.ContainsKey(modelID))
                {
                    return this.modelHotKeyCache[modelID];
                }
            }
            catch (Exception ex)
            { 
                Logger.Log(ex);
            }

            return new List<VTubeStudioHotKey>();
        }

        public async Task<bool> RunHotKey(string hotKeyID)
        {
            JObject data = new JObject();
            data["hotkeyID"] = hotKeyID;

            try
            {
                VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("HotkeyTriggerRequest", data));
                if (response != null && response.data != null && response.data.ContainsKey("hotkeyID"))
                {
                    return true;
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
            this.allModelsCache = null;
            this.allModelsCacheExpiration = DateTimeOffset.MinValue;

            this.modelHotKeyCache.Clear();
            this.modelHotKeyCacheExpiration = DateTimeOffset.MinValue;
        }

        protected override async Task<Result> InitializeInternal()
        {
            try
            {
                if (!this.websocket.IsOpen())
                {
                    if (!await this.ConnectWebSocket())
                    {
                        return new Result(MixItUp.Base.Resources.VTubeStudioConnectionFailed);
                    }
                }

                JObject data = new JObject();
                data["pluginName"] = websocketPluginName;
                data["pluginDeveloper"] = websocketPluginDeveloper;
                data["authenticationToken"] = this.token?.accessToken;

                VTubeStudioWebSocketResponsePacket response = await this.websocket.SendAndReceive(new VTubeStudioWebSocketRequestPacket("AuthenticationRequest", data));
                if (response != null && response.data != null && response.data.TryGetValue("authenticated", out JToken authenticated) && authenticated.Value<bool>() == true)
                {
                    this.WebSocketConnected = true;
                    this.websocket.OnDisconnectOccurred += Websocket_OnDisconnectOccurred;

                    this.TrackServiceTelemetry("VTube Studio");
                    return new Result();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(MixItUp.Base.Resources.VTubeStudioConnectionFailed);
        }

        protected override Task RefreshOAuthToken() { return Task.CompletedTask; }

        private async Task<bool> ConnectWebSocket()
        {
            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            return await this.websocket.Connect(websocketAddress + ChannelSession.Settings.VTubeStudioPortNumber);
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.VTubeStudio);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.InitializeInternal();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.VTubeStudio);
        }
    }
}
