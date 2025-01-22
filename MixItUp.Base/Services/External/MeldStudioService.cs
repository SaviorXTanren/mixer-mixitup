using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services.External
{
    public class MeldStudioItem
    {
        public string ID { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string parent { get; set; }
    }

    public class MeldStudioSceneItem : MeldStudioItem
    {
        public bool current { get; set; }
        public bool staged { get; set; }
    }

    public class MeldStudioLayerItem : MeldStudioItem
    {
        public int index { get; set; }
        public bool visible { get; set; }
    }

    public class MeldStudioEffectItem : MeldStudioItem
    {
        public bool enabled { get; set; }
    }

    public class MeldStudioAudioTrackItem : MeldStudioItem
    {
        public bool monitoring { get; set; }
        public bool muted { get; set; }
    }

    public class MeldStudioService : ServiceBase
    {
        private int isStreamingPropertiesIndex = 1;
        private int isRecordingPropertiesIndex = 2;
        private int sessionPropertiesIndex = 3;

        private int propertiesValueIndex = 3;

        public override string Name { get { return Resources.MeldStudio; } }

        public override bool IsEnabled { get { return !string.IsNullOrWhiteSpace(ChannelSession.Settings.MeldStudioWebSocketAddress); } }

        public override bool IsConnected { get; protected set; }

        private QtClientWebSocket websocket = new QtClientWebSocket();

        private bool? isStreaming;
        private bool? isRecording;

        private Dictionary<string, MeldStudioItem> items = new Dictionary<string, MeldStudioItem>();

        public MeldStudioService()
        {
            this.websocket.QtPacketReceived += Websocket_QtPacketReceived;
        }

        public override async Task<Result> ManualConnect(CancellationToken cancellationToken)
        {
            try
            {
                this.ClearItemCache();

                if (string.IsNullOrWhiteSpace(ChannelSession.Settings.MeldStudioWebSocketAddress))
                {
                    return new Result(Resources.MeldStudioInvalidWebSocketAddress);
                }

                if (!await this.websocket.Connect(ChannelSession.Settings.MeldStudioWebSocketAddress, cancellationToken))
                {
                    return new Result(Resources.MeldStudioUnableToConnect);
                }

                QtWebSocketPacket initPacket = await this.websocket.Init();
                if (initPacket == null || initPacket.data == null)
                {
                    return new Result(Resources.MeldStudioUnableToConnect);
                }

                JArray properties = (JArray)initPacket.data["meld"]["properties"];

                this.isStreaming = properties[isStreamingPropertiesIndex][propertiesValueIndex].ToObject<bool>();
                this.isStreaming = properties[isRecordingPropertiesIndex][propertiesValueIndex].ToObject<bool>();

                this.RebuildItemCache(properties[sessionPropertiesIndex][propertiesValueIndex]["items"] as JObject);

                if (this.items.Count == 0)
                {
                    return new Result(Resources.MeldStudioUnableToConnect);
                }

                this.IsConnected = true;
                return new Result();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex);
            }
        }

        public override async Task Disconnect()
        {
            await this.websocket.Disconnect();

            this.ClearItemCache();

            this.IsConnected = false;
        }

        public override async Task Disable()
        {
            await this.Disconnect();

            ChannelSession.Settings.MeldStudioWebSocketAddress = null;
        }

        public async Task ChangeStreamState(bool? state)
        {
            if (state != null && state == this.isStreaming)
            {
                return;
            }

            await this.websocket.InvokeMethod("meld", "toggleStream", new List<object>());
        }

        public async Task ChangeRecordState(bool? state)
        {
            if (state != null && state == this.isRecording)
            {
                return;
            }

            await this.websocket.InvokeMethod("meld", "toggleRecord", new List<object>());
        }

        public async Task TakeScreenshot()
        {
            await this.websocket.InvokeMethod("meld", "sendEvent", new List<object>() { "co.meldstudio.events.screenshot" });
        }

        public async Task ShowScene(string name)
        {
            await this.websocket.InvokeMethod("meld", "showScene", new List<object>() { "C568C423306FB6F9FEFF53CAA89B5A56" });
        }

        public async Task ChangeLayerState(string scene, string layer, bool? state)
        {
            await this.websocket.InvokeMethod("meld", "toggleLayer", new List<object>() { "C79026936A15A74961420C0C54B290BA", "5E57FDBE2492647329B4C1B4E1A0E3B4" });
        }

        public async Task ChangeEffectState(string scene, string layer, string effect, bool? state)
        {
            await this.websocket.InvokeMethod("meld", "toggleEffect", new List<object>() { "C79026936A15A74961420C0C54B290BA", "5E57FDBE2492647329B4C1B4E1A0E3B4", "BF7A4E4C28C500D68C29D77B4A7EA964" });
        }

        public async Task ChangeMuteState(string name, bool? state)
        {
            await this.websocket.InvokeMethod("meld", "toggleMute", new List<object>() { "C568C423306FB6F9FEFF53CAA89B5A56" });
        }

        public async Task ChangeMonitorState(string name, bool? state)
        {
            await this.websocket.InvokeMethod("meld", "toggleMonitor", new List<object>() { "C568C423306FB6F9FEFF53CAA89B5A56" });
        }

        public async Task SetGain(string name, double gain)
        {
            await this.websocket.InvokeMethod("meld", "setGain", new List<object>() { "C568C423306FB6F9FEFF53CAA89B5A56", gain });
        }

        private void Websocket_QtPacketReceived(object sender, QtWebSocketPacket packet)
        {
            try
            {
                if (packet.id < 0)
                {
                    if (packet.type == QtWebSocketPacketType.propertyUpdate)
                    {
                        JArray updates = packet.data as JArray;
                        foreach (var kvp in updates[0]["properties"] as JObject)
                        {
                            if (kvp.Key == isStreamingPropertiesIndex.ToString())
                            {
                                this.isStreaming = kvp.Value.ToObject<bool>();
                            }
                            else if (kvp.Key == isRecordingPropertiesIndex.ToString())
                            {
                                this.isRecording = kvp.Value.ToObject<bool>();
                            }
                            else if (kvp.Key == sessionPropertiesIndex.ToString())
                            {
                                this.RebuildItemCache(kvp.Value["items"] as JObject);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private void RebuildItemCache(JObject itemsJObj)
        {
            Dictionary<string, MeldStudioItem> items = new Dictionary<string, MeldStudioItem>();
            foreach (var kvp in itemsJObj)
            {
                MeldStudioItem item = null;
                switch (kvp.Value["type"].ToObject<string>())
                {
                    case "scene": item = kvp.Value.ToObject<MeldStudioSceneItem>(); break;
                    case "layer": item = kvp.Value.ToObject<MeldStudioLayerItem>(); break;
                    case "effect": item = kvp.Value.ToObject<MeldStudioEffectItem>(); break;
                    case "track": item = kvp.Value.ToObject<MeldStudioAudioTrackItem>(); break;
                }

                if (item != null)
                {
                    item.ID = kvp.Key;
                    items[kvp.Key] = item;
                }
            }
            this.items = items;
        }

        private void ClearItemCache()
        {
            this.isStreaming = null;
            this.isRecording = null;
            this.items.Clear();
        }
    }
}
