using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class VoicemodWebSocketRequestPacket
    {
        public string actionID { get; set; } = Guid.NewGuid().ToString();
        public string appVersion { get; set; }
        public string pluginVersion { get; set; } = "2.0.0";

        public string actionType { get; set; }
        public JObject actionObject { get; set; } = new JObject();
        public string context { get; set; } = string.Empty;

        private VoicemodWebSocketRequestPacket() { }

        public VoicemodWebSocketRequestPacket(string actionType)
        {
            this.actionType = actionType;
        }

        public VoicemodWebSocketRequestPacket(string actionType, JObject actionObject)
            : this(actionType)
        {
            this.actionObject = actionObject;
        }
    }

    public class VoicemodWebSocket : ClientWebSocketBase
    {
        private Dictionary<string, VoicemodWebSocketRequestPacket> responses = new Dictionary<string, VoicemodWebSocketRequestPacket>();

        public override Task<bool> Connect(string endpoint)
        {
            this.responses.Clear();

            return base.Connect(endpoint);
        }

        public async Task<VoicemodWebSocketRequestPacket> SendAndReceive(VoicemodWebSocketRequestPacket packet, int delaySeconds = 5)
        {
            Logger.Log(LogLevel.Debug, "Voicemod Packet Sent - " + JSONSerializerHelper.SerializeToString(packet));

            this.responses[packet.actionID] = null;

            await this.Send(JSONSerializerHelper.SerializeToString(packet));

            int cycles = delaySeconds * 10;
            VoicemodWebSocketRequestPacket response = null;
            for (int i = 0; i < cycles && response == null; i++)
            {
                this.responses.TryGetValue(packet.actionID, out response);
                await Task.Delay(100);
            }

            this.responses.Remove(packet.actionID);

            return response;
        }

        protected override Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "Voicemod Packet Received - " + packet);

                VoicemodWebSocketRequestPacket response = JSONSerializerHelper.DeserializeFromString<VoicemodWebSocketRequestPacket>(packet);
                if (response != null)
                {
                    if (!string.IsNullOrEmpty(response.actionID) && this.responses.ContainsKey(response.actionID))
                    {
                        this.responses[response.actionID] = response;
                    }
                    else if (this.responses.Keys.Count > 0)
                    {
                        this.responses[this.responses.Keys.First()] = response;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return Task.FromResult(0);
        }
    }

    /// <summary>
    /// Discord: https://discord.com/invite/vm-dev-community
    /// 
    /// v3 Documentation: https://control-api.voicemod.net
    /// </summary>
    public class VoicemodService : IVoicemodService
    {
        private static readonly List<int> AvailablePorts = new List<int>() { 59129, 20000, 39273, 42152, 43782, 46667, 35679, 37170, 38501, 33952, 30546 };

        public string Name { get { return MixItUp.Base.Resources.Voicemod; } }

        public bool IsConnected { get { return this.WebSocketConnected; } }

        public bool WebSocketConnected { get; private set; }

        private VoicemodWebSocket websocket = new VoicemodWebSocket();

        public VoicemodService() { }

        public async Task<Result> Connect()
        {
            // Force use with V3 control API
            string clientKey = ServiceManager.Get<SecretsService>().GetSecret("VoicemodV3ClientKey");

            try
            {
                return await this.ConnectWebSocket();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return new Result(MixItUp.Base.Resources.VoicemodConnectionFailed);
        }

        public async Task Disconnect()
        {
            this.WebSocketConnected = false;

            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
            await this.websocket.Disconnect();
        }

        public async Task<IEnumerable<VoicemodVoiceModel>> GetVoices()
        {
            Dictionary<string, VoicemodVoiceModel> results = new Dictionary<string, VoicemodVoiceModel>();

            VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("getVoices"));
            if (response != null && response.actionObject != null)
            {
                JToken voiceGroup;
                if (response.actionObject.TryGetValue("allVoices", out voiceGroup))
                {
                    foreach (VoicemodVoiceModel voice in ((JArray)voiceGroup).ToTypedArray<VoicemodVoiceModel>())
                    {
                        results[voice.voiceID] = voice;
                    }
                }

                if (response.actionObject.TryGetValue("favoriteVoices", out voiceGroup))
                {
                    foreach (VoicemodVoiceModel voice in ((JArray)voiceGroup).ToTypedArray<VoicemodVoiceModel>())
                    {
                        results[voice.voiceID] = voice;
                    }
                }

                if (response.actionObject.TryGetValue("customVoices", out voiceGroup))
                {
                    foreach (VoicemodVoiceModel voice in ((JArray)voiceGroup).ToTypedArray<VoicemodVoiceModel>())
                    {
                        results[voice.voiceID] = voice;
                    }
                }
            }

            return results.Values.ToList();
        }

        public async Task VoiceChangerOnOff(bool state)
        {
            VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("getVoiceChangerStatus"));
            if (response != null && response.actionObject != null && response.actionObject.TryGetValue("value", out JToken value))
            {
                bool current = value.ToObject<bool>();
                if (current != state)
                {
                    response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("voiceChanger_OnOff"));
                }
            }
        }

        public async Task SelectVoice(string voiceID)
        {
            await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("selectVoice", new JObject()
            {
                { "voiceID", voiceID }
            }));
        }

        public async Task RandomVoice(VoicemodRandomVoiceType voiceType)
        {
            await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("selectRandomVoice", new JObject()
            {
                { "mode", voiceType.ToString() }
            }));
        }

        public async Task BeepSoundOnOff(bool state)
        {
            await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("beepSound_OnOff", new JObject()
            {
                { "badLanguage", state ? 1 : 0 }
            }));
        }

        public async Task HearMyselfOnOff(bool state)
        {
            VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("getHearMyselfStatus"));
            if (response != null && response.actionObject != null && response.actionObject.TryGetValue("value", out JToken value))
            {
                bool current = value.ToObject<bool>();
                if (current != state)
                {
                    response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("hearMyVoice_OnOff"));
                }
            }
        }

        public async Task MuteOnOff(bool state)
        {
            VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("getMuteMicStatus"));
            if (response != null && response.actionObject != null && response.actionObject.TryGetValue("value", out JToken value))
            {
                bool current = value.ToObject<bool>();
                if (current != state)
                {
                    await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("mute_OnOff", new JObject()));
                }
            }
        }

        public async Task<IEnumerable<VoicemodMemeModel>> GetMemeSounds()
        {
            List<VoicemodMemeModel> results = new List<VoicemodMemeModel>();

            VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("getMemes"));
            if (response != null && response.actionObject != null && response.actionObject.TryGetValue("listOfMemes", out JToken memeSounds) && memeSounds is JArray)
            {
                JArray memeSoundsArray = (JArray)memeSounds;
                foreach (VoicemodMemeModel memeSound in memeSoundsArray.ToTypedArray<VoicemodMemeModel>())
                {
                    results.Add(memeSound);
                }
            }

            return results;
        }

        public async Task PlayMemeSound(string fileName)
        {
            await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("playMeme", new JObject()
            {
                { "FileName", fileName },
                { "IsKeyDown", true }
            }));
        }

        public async Task StopAllMemeSounds()
        {
            await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("stopAllMemeSounds"));
        }

        private async Task<Result> ConnectWebSocket()
        {
            this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;

            foreach (int port in VoicemodService.AvailablePorts)
            {
                try
                {
                    if (await this.websocket.Connect(string.Format("ws://localhost:{0}/vmsd/", port)))
                    {
                        VoicemodWebSocketRequestPacket response = await this.websocket.SendAndReceive(new VoicemodWebSocketRequestPacket("registerPlugin"));
                        if (response != null && response.actionObject != null && response.actionObject.TryGetValue("result", out JToken result) && result != null)
                        {
                            // Websocket currently always returns false, so ignore result for now.
                            //if (bool.Equals(true, result))
                            //{
                            //    return new Result();
                            //}

                            this.WebSocketConnected = true;
                            this.websocket.OnDisconnectOccurred += Websocket_OnDisconnectOccurred;

                            ServiceManager.Get<ITelemetryService>().TrackService("Voicemod");

                            return new Result();
                        }
                    }
                    await this.websocket.Disconnect(WebSocketCloseStatus.NormalClosure);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                }
            }

            return new Result(MixItUp.Base.Resources.VoicemodConnectionFailed);
        }

        private async void Websocket_OnDisconnectOccurred(object sender, System.Net.WebSockets.WebSocketCloseStatus e)
        {
            ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.Voicemod);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.ConnectWebSocket();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.Voicemod);
        }
    }
}
