using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public const int AudioTrackGainMinimum = -60;
        public const int AudioTrackGainMaximum = 0;

        private const string versionPropertyName = "version";
        private const string isStreamingPropertyName = "isStreaming";
        private const string isRecordingPropertyName = "isRecording";
        private const string sessionPropertyName = "session";

        private const int propertiesIndexNumberIndex = 0;
        private const int propertiesNameIndex = 1;
        private const int propertiesValueIndex = 3;

        private const string sceneItemType = "scene";
        private const string layerItemType = "layer";
        private const string effectItemType = "effect";
        private const string trackItemType = "track";

        public override string Name { get { return Resources.MeldStudio; } }

        public override bool IsEnabled { get { return !string.IsNullOrWhiteSpace(ChannelSession.Settings.MeldStudioWebSocketAddress); } }

        public override bool IsConnected { get; protected set; }

        private QtClientWebSocket websocket = new QtClientWebSocket();

        private int versionIndex = 0;
        private int isStreamingIndex = 0;
        private int isRecordingIndex = 0;
        private int sessionIndex = 0;

        private int version;
        private bool? isStreaming;
        private bool? isRecording;

        private Dictionary<string, MeldStudioSceneItem> scenes = new Dictionary<string, MeldStudioSceneItem>();
        private Dictionary<string, MeldStudioLayerItem> layers = new Dictionary<string, MeldStudioLayerItem>();
        private Dictionary<string, MeldStudioEffectItem> effects = new Dictionary<string, MeldStudioEffectItem>();
        private Dictionary<string, MeldStudioAudioTrackItem> audioTracks = new Dictionary<string, MeldStudioAudioTrackItem>();

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

                JArray version = this.GetPropertyJArrayValues(properties, versionPropertyName);
                if (version != null)
                {
                    this.versionIndex = version[propertiesIndexNumberIndex].ToObject<int>();
                    this.version = version[propertiesValueIndex].ToObject<int>();
                }

                JArray isStreaming = this.GetPropertyJArrayValues(properties, isStreamingPropertyName);
                if (isStreaming != null)
                {
                    this.isStreamingIndex = isStreaming[propertiesIndexNumberIndex].ToObject<int>();
                    this.isStreaming = isStreaming[propertiesValueIndex].ToObject<bool>();
                }

                JArray isRecording = this.GetPropertyJArrayValues(properties, isRecordingPropertyName);
                if (isRecording != null)
                {
                    this.isRecordingIndex = isRecording[propertiesIndexNumberIndex].ToObject<int>();
                    this.isRecording = isRecording[propertiesValueIndex].ToObject<bool>();
                }

                JArray session = this.GetPropertyJArrayValues(properties, sessionPropertyName);
                if (session != null)
                {
                    this.sessionIndex = session[propertiesIndexNumberIndex].ToObject<int>();
                    this.RebuildItemCache(session[propertiesValueIndex]["items"] as JObject);
                }

                if (this.scenes.Count == 0)
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

            if (state == null)
            {
                this.isStreaming = !this.isStreaming;
            }

            await this.websocket.InvokeMethod("meld", "toggleStream", new List<object>());
        }

        public async Task ChangeRecordState(bool? state)
        {
            if (state != null && state == this.isRecording)
            {
                return;
            }

            if (state == null)
            {
                this.isRecording = !this.isRecording;
            }

            await this.websocket.InvokeMethod("meld", "toggleRecord", new List<object>());
        }

        public async Task TakeScreenshot()
        {
            await this.websocket.InvokeMethod("meld", "sendCommand", new List<object>() { "meld.screenshot" });
        }

        public async Task RecordClip()
        {
            await this.websocket.InvokeMethod("meld", "sendCommand", new List<object>() { "meld.recordClip" });
        }

        public async Task ShowScene(string sceneName)
        {
            MeldStudioSceneItem scene = this.GetScene(sceneName);
            if (scene != null)
            {
                await this.websocket.InvokeMethod("meld", "showScene", new List<object>() { scene.ID });
            }
        }

        public async Task ChangeLayerState(string sceneName, string layerName, bool? state)
        {
            MeldStudioSceneItem scene = this.GetScene(sceneName);
            if (scene != null)
            {
                MeldStudioLayerItem layer = this.GetLayer(scene, layerName);
                if (layer != null)
                {
                    if (state != null && state == layer.visible)
                    {
                        return;
                    }

                    if (state == null)
                    {
                        layer.visible = !layer.visible;
                    }
                    else
                    {
                        layer.visible = state.GetValueOrDefault();
                    }

                    await this.websocket.InvokeMethod("meld", "toggleLayer", new List<object>() { scene.ID, layer.ID });
                }
            }
        }

        public async Task ChangeEffectState(string sceneName, string layerName, string effectName, bool? state)
        {
            MeldStudioSceneItem scene = this.GetScene(sceneName);
            if (scene != null)
            {
                MeldStudioLayerItem layer = this.GetLayer(scene, layerName);
                if (layer != null)
                {
                    MeldStudioEffectItem effect = this.GetEffect(layer, effectName);
                    if (effect != null)
                    {
                        if (state != null && state == effect.enabled)
                        {
                            return;
                        }

                        if (state == null)
                        {
                            effect.enabled = !effect.enabled;
                        }
                        else
                        {
                            effect.enabled = state.GetValueOrDefault();
                        }

                        await this.websocket.InvokeMethod("meld", "toggleEffect", new List<object>() { scene.ID, layer.ID, effect.ID });
                    }
                }
            }
        }

        public async Task ChangeMuteState(string audioTrackName, bool? state)
        {
            MeldStudioAudioTrackItem audioTrack = this.GetAudioTrack(audioTrackName);
            if (audioTrack != null)
            {
                if (state != null && state == audioTrack.muted)
                {
                    return;
                }

                if (state == null)
                {
                    audioTrack.muted = !audioTrack.muted;
                }
                else
                {
                    audioTrack.muted = state.GetValueOrDefault();
                }

                await this.websocket.InvokeMethod("meld", "toggleMute", new List<object>() { audioTrack.ID });
            }
        }

        public async Task ChangeMonitorState(string audioTrackName, bool? state)
        {
            MeldStudioAudioTrackItem audioTrack = this.GetAudioTrack(audioTrackName);
            if (audioTrack != null)
            {
                if (state != null && state == audioTrack.monitoring)
                {
                    return;
                }

                if (state == null)
                {
                    audioTrack.monitoring = !audioTrack.monitoring;
                }
                else
                {
                    audioTrack.monitoring |= state.GetValueOrDefault();
                }

                await this.websocket.InvokeMethod("meld", "toggleMonitor", new List<object>() { audioTrack.ID });
            }
        }

        public async Task SetGain(string audioTrackName, int dB)
        {
            MeldStudioAudioTrackItem audioTrack = this.GetAudioTrack(audioTrackName);
            if (audioTrack != null)
            {
                dB = MathHelper.Clamp(dB, AudioTrackGainMinimum, AudioTrackGainMaximum);
                double gain = this.DBToGain(dB);
                await this.websocket.InvokeMethod("meld", "setGain", new List<object>() { audioTrack.ID, gain });
            }
        }

        private MeldStudioSceneItem GetScene(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return this.scenes.FirstOrDefault(s => s.Value.current).Value;
            }
            else
            {
                this.scenes.TryGetValue(name, out MeldStudioSceneItem result);
                return result;
            }
        }

        private MeldStudioLayerItem GetLayer(MeldStudioSceneItem scene, string name)
        {
            this.layers.TryGetValue(this.GenerateParentIDNameKey(scene.ID, name), out MeldStudioLayerItem result);
            return result;
        }

        private MeldStudioEffectItem GetEffect(MeldStudioLayerItem layer, string name)
        {
            this.effects.TryGetValue(this.GenerateParentIDNameKey(layer.ID, name), out MeldStudioEffectItem result);
            return result;
        }

        private MeldStudioAudioTrackItem GetAudioTrack(string name)
        {
            this.audioTracks.TryGetValue(name, out MeldStudioAudioTrackItem result);
            return result;
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
                            if (kvp.Key == this.isStreamingIndex.ToString())
                            {
                                this.isStreaming = kvp.Value.ToObject<bool>();
                            }
                            else if (kvp.Key == this.isRecordingIndex.ToString())
                            {
                                this.isRecording = kvp.Value.ToObject<bool>();
                            }
                            else if (kvp.Key == this.sessionIndex.ToString())
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
            Dictionary<string, MeldStudioSceneItem> scenes = new Dictionary<string, MeldStudioSceneItem>();
            Dictionary<string, MeldStudioLayerItem> layers = new Dictionary<string, MeldStudioLayerItem>();
            Dictionary<string, MeldStudioEffectItem> effects = new Dictionary<string, MeldStudioEffectItem>();
            Dictionary<string, MeldStudioAudioTrackItem> audioTracks = new Dictionary<string, MeldStudioAudioTrackItem>();

            foreach (var kvp in itemsJObj)
            {
                MeldStudioItem item = null;
                switch (kvp.Value["type"].ToObject<string>())
                {
                    case sceneItemType:
                        MeldStudioSceneItem scene = kvp.Value.ToObject<MeldStudioSceneItem>();
                        scenes[scene.name] = scene;
                        item = scene;
                        break;
                    case layerItemType:
                        MeldStudioLayerItem layer = kvp.Value.ToObject<MeldStudioLayerItem>();
                        layers[this.GenerateParentIDNameKey(layer)] = layer;
                        item = layer;
                        break;
                    case effectItemType:
                        MeldStudioEffectItem effect = kvp.Value.ToObject<MeldStudioEffectItem>();
                        effects[this.GenerateParentIDNameKey(effect)] = effect;
                        item = effect;
                        break;
                    case trackItemType:
                        MeldStudioAudioTrackItem audioTrack = kvp.Value.ToObject<MeldStudioAudioTrackItem>();
                        audioTracks[audioTrack.name] = audioTrack;
                        item = audioTrack;
                        break;
                }

                if (item != null)
                {
                    item.ID = kvp.Key;
                }
            }

            this.scenes = scenes;
            this.layers = layers;
            this.effects = effects;
            this.audioTracks = audioTracks;
        }

        private void ClearItemCache()
        {
            this.isStreaming = null;
            this.isRecording = null;

            this.scenes.Clear();
            this.layers.Clear();
            this.effects.Clear();
            this.audioTracks.Clear();
        }

        private string GenerateParentIDNameKey(MeldStudioItem item)
        {
            return this.GenerateParentIDNameKey(item.parent, item.name);
        }

        private string GenerateParentIDNameKey(string parentID, string name)
        {
            return $"{parentID}|{name}";
        }

        private double DBToGain(double dB)
        {
            if (double.IsInfinity(dB) || dB < -60) dB = -60;
            double gain = Math.Pow(10, dB / 20);

            gain = gain <= 0.001 ? 0 : gain;
            gain = gain > 1 ? 1 : gain;
            return gain;
        }

        private JArray GetPropertyJArrayValues(JArray properties, string name)
        {
            foreach (JArray prop in properties)
            {
                if (prop.Count() > propertiesValueIndex)
                {
                    if (string.Equals(prop[propertiesNameIndex].ToString(), name, StringComparison.OrdinalIgnoreCase))
                    {
                        return prop;
                    }
                }
            }
            return null;
        }
    }
}
