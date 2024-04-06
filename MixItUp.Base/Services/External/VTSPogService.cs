using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using Newtonsoft.Json.Linq;
using StreamingClient.Base.Util;
using StreamingClient.Base.Web;
using System;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class VTSPogWebSocketPacket
    {
        public string type { get; set; }
        public JToken data { get; set; }
    }

    [DataContract]
    public class VTSPogWebSocketTTSStartData
    {
        [DataMember]
        public long id { get; set; }
        [DataMember]
        public string user { get; set; }
        [DataMember]
        public string source { get; set; }
        [DataMember]
        public string text { get; set; }
        [DataMember]
        public int priority { get; set; }
        [DataMember]
        public long charLimit { get; set; }
        [DataMember]
        public string tts { get; set; }
        [DataMember]
        public string voice { get; set; }
        [DataMember]
        public string curatedText { get; set; }
        [DataMember]
        public int volume { get; set; }
        [DataMember]
        public string pet { get; set; }
    }

    public class VTSPogWebSocket : ClientWebSocketBase
    {
        private const string ConnectionShakeType = "shake";
        private const string TTSStartType = "startTTS";
        private const string TTSStopType = "stopTTS";

        public event EventHandler<VTSPogWebSocketTTSStartData> OnTTSStart = delegate { };
        public event EventHandler OnTTSStop = delegate { };

        protected override async Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, "VTS POG Packet Received - " + packet);

                VTSPogWebSocketPacket response = JSONSerializerHelper.DeserializeFromString<VTSPogWebSocketPacket>(packet);
                if (response != null && !string.IsNullOrEmpty(response.type))
                {
                    if (string.Equals(response.type, ConnectionShakeType))
                    {
                        VTSPogWebSocketPacket shakePacket = new VTSPogWebSocketPacket()
                        {
                            type = "client",
                            data = new JValue("shake")
                        };
                        await this.Send(JSONSerializerHelper.SerializeToString(shakePacket));
                    }
                    else if (string.Equals(response.type, TTSStartType))
                    {
                        this.OnTTSStart(this, response.data.ToObject<VTSPogWebSocketTTSStartData>());
                    }
                    else if (string.Equals(response.type, TTSStopType))
                    {
                        this.OnTTSStop(this, new EventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }
    }

    public class VTSPogService : IExternalService
    {
        private const string BaseAddress = "http://localhost:3800/";
        private const string WebSocketAddress = "ws://localhost:3800/api";

        public event EventHandler<VTSPogWebSocketTTSStartData> TTSStarted = delegate { };
        public event EventHandler<long> TTSCompleted = delegate { };

        private VTSPogWebSocket websocket = null;

        private long lastTTSID = 0;

        public string Name { get { return Resources.VTSPog; } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            if (await this.Status())
            {
                this.websocket = new VTSPogWebSocket();
                this.websocket.OnTTSStart += Websocket_OnTTSStart;
                this.websocket.OnTTSStop += Websocket_OnTTSStop;
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
                this.websocket.OnTTSStart -= Websocket_OnTTSStart;
                this.websocket.OnTTSStop -= Websocket_OnTTSStop;
                this.websocket.OnDisconnectOccurred -= Websocket_OnDisconnectOccurred;
                await this.websocket.Disconnect();
            }
            this.websocket = null;

            this.IsConnected = false;
        }

        public async Task<bool> TextToSpeech(string text, UserV2ViewModel user, int characterLimit = 0, string ttsProvider = null, string voice = null)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    string url = $"pog?text={Uri.EscapeDataString(text)}&user={user.DisplayName}";
                    if (characterLimit > 0)
                    {
                        url += $"&limit={characterLimit}";
                    }
                    if (!string.IsNullOrEmpty(ttsProvider))
                    {
                        url += $"&tts={ttsProvider}";
                    }
                    if (!string.IsNullOrEmpty(voice))
                    {
                        url += $"&voice={Uri.EscapeDataString(voice)}";
                    }

                    await client.GetAsync(url);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task<bool> AITextToSpeech(string text, UserV2ViewModel user, int characterLimit = 0, string presentation = null, int prompt = 0)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    string url = $"gpt?text={Uri.EscapeDataString(text)}&user={user.DisplayName}";
                    if (!string.IsNullOrEmpty(presentation))
                    {
                        url += $"&presentation={Uri.EscapeDataString(presentation)}";
                    }
                    if (prompt != 0)
                    {
                        url += $"&prompt={prompt}";
                    }

                    await client.GetAsync(url);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
        }

        public async Task<bool> PlayAudioFile(string filePath, string pet = null)
        {
            try
            {
                using (AdvancedHttpClient client = new AdvancedHttpClient(VTSPogService.BaseAddress))
                {
                    string url = $"pogu?text={Uri.EscapeDataString(filePath)}";
                    if (!string.IsNullOrEmpty(pet))
                    {
                        url += $"&pet={pet}";
                    }

                    await client.GetAsync(url);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return false;
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

        private void Websocket_OnTTSStart(object sender, VTSPogWebSocketTTSStartData data)
        {
            this.lastTTSID = data.id;
            this.TTSStarted(this, data);
        }

        private void Websocket_OnTTSStop(object sender, EventArgs e)
        {
            this.TTSCompleted(this, this.lastTTSID);
        }

        private async void Websocket_OnDisconnectOccurred(object sender, WebSocketCloseStatus status)
        {
            ChannelSession.DisconnectionOccurred(Resources.VTSPog);

            Result result = new Result();
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.Connect();
            }
            while (!result.Success);

            ChannelSession.ReconnectionOccurred(Resources.VTSPog);
        }
    }
}
