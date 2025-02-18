using MixItUp.Base;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.WPF.Services
{

    public class WindowsOBSService : IOBSStudioService
    {
        private const int CommandTimeoutInMilliseconds = 2500;
        private const int ConnectTimeoutInMilliseconds = 5000;

        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        private OBSWebsocket OBSWebsocket = new OBSWebsocket();
        private OBSWebsocketV5 OBSWebsocketV5 = new OBSWebsocketV5();

        public WindowsOBSService() { }

        public string Name { get { return "OBS Studio"; } }

        public bool IsEnabled { get { return !string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP); } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;

            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                return Task.Run(async () =>
                {
                    try
                    {
                        this.OBSWebsocketV5.Disconnected -= OBSWebsocket_Disconnected;
                        var success = await this.OBSWebsocketV5.Connect(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword, cancellationToken);
                        if (success && this.OBSWebsocketV5.IsConnected)
                        {
                            this.OBSWebsocketV5.Disconnected += OBSWebsocket_Disconnected;
                            this.IsConnected = true;
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex);
                    }
                    return false;
                });
            }, ConnectTimeoutInMilliseconds);

            if (!this.IsConnected)
            {
                await this.OBSCommandTimeoutWrapper((cancellationToken) =>
                {
                    return Task.Run(() =>
                    {
                        try
                        {
                            this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                            this.OBSWebsocket.Connect(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                            if (this.OBSWebsocket.IsConnected)
                            {
                                this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;
                                this.IsConnected = true;
                                return Task.FromResult(true);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex);
                        }
                        return Task.FromResult(false);
                    });
                }, ConnectTimeoutInMilliseconds);
            }

            if (this.IsConnected)
            {
                await this.StartReplayBuffer();
                this.Connected(this, new EventArgs());
                ChannelSession.ReconnectionOccurred(MixItUp.Base.Resources.OBSStudio);
                ServiceManager.Get<ITelemetryService>().TrackService("OBS Studio");
                return new Result();
            }
            return new Result(Resources.OBSWebSocketFailed);
        }

        public async Task Disconnect()
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                this.IsConnected = false;
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                    this.OBSWebsocket.Disconnect();
                    this.Disconnected(this, new EventArgs());
                    ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.OBSStudio);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    this.OBSWebsocketV5.Disconnected -= OBSWebsocket_Disconnected;
                    await this.OBSWebsocketV5.Disconnect();
                    this.Disconnected(this, new EventArgs());
                    ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.OBSStudio);
                }
                return true;
            }, ConnectTimeoutInMilliseconds);
        }

        public Task<bool> TestConnection() { return Task.FromResult(true); }

        public async Task ShowScene(string sceneName)
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Showing OBS Scene - " + sceneName);

                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.SetCurrentScene(sceneName);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.SetCurrentScene(sceneName);
                }

                return Task.FromResult(true);
            });
        }

        public async Task<string> GetCurrentScene()
        {
            return await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Getting Current OBS Scene");

                string sceneName = "Unknown";

                if (this.OBSWebsocket.IsConnected)
                {
                    OBSScene scene = this.OBSWebsocket.GetCurrentScene();
                    if (scene != null)
                    {
                        sceneName = scene.Name;
                    }
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    sceneName = await this.OBSWebsocketV5.GetCurrentSceneName();
                }

                return sceneName;
            });
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source visibility - " + sourceName);

                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.SetSourceRender(sourceName, visibility, sceneName);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        sceneName = await this.OBSWebsocketV5.GetCurrentSceneName();
                    }

                    await this.OBSWebsocketV5.SetSourceRender(sourceName, visibility, sceneName);
                }

                return Task.FromResult(true);
            });
        }

        public async Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility)
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source filter visibility - " + sourceName + " - " + filterName);

                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.SetSourceFilterVisibility(sourceName, filterName, visibility);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.SetSourceFilterVisibility(sourceName, filterName, visibility);
                }

                return Task.FromResult(true);
            });
        }

        public async Task SetImageSourceFilePath(string sceneName, string sourceName, string filePath)
        {
            Logger.Log(LogLevel.Debug, "Setting image source file path - " + sourceName);

            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    SourceSettings properties = this.OBSWebsocket.GetSourceSettings(sourceName);
                    properties.Settings["file"] = filePath;
                    this.OBSWebsocket.SetSourceSettings(sourceName, properties.Settings);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    JObject settings = await this.OBSWebsocketV5.GetSourceSettings(sourceName);
                    if (settings != null)
                    {
                        settings["file"] = filePath;
                        await this.OBSWebsocketV5.SetSourceSettings(sourceName, settings);
                    }
                }

                return true;
            });
        }

        public async Task SetMediaSourceFilePath(string sceneName, string sourceName, string filePath)
        {
            Logger.Log(LogLevel.Debug, "Setting media source file path - " + sourceName);

            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    SourceSettings properties = this.OBSWebsocket.GetSourceSettings(sourceName);
                    properties.Settings["local_file"] = filePath;
                    this.OBSWebsocket.SetSourceSettings(sourceName, properties.Settings);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    JObject settings = await this.OBSWebsocketV5.GetSourceSettings(sourceName);
                    if (settings != null)
                    {
                        settings["local_file"] = filePath;
                        await this.OBSWebsocketV5.SetSourceSettings(sourceName, settings);
                    }
                }

                return true;
            });
        }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            Logger.Log(LogLevel.Debug, "Setting web browser URL - " + sourceName);

            await this.SetSourceVisibility(sceneName, sourceName, visibility: false);

            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    SourceSettings properties = this.OBSWebsocket.GetSourceSettings(sourceName);
                    properties.Settings["is_local_file"] = false;
                    properties.Settings["url"] = url;
                    this.OBSWebsocket.SetSourceSettings(sourceName, properties.Settings);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    JObject settings = new JObject
                    {
                        ["url"] = url,
                    };
                    await this.OBSWebsocketV5.SetSourceSettings(sourceName, settings);
                }

                return Task.FromResult(true);
            });
        }

        public async Task SetSourceDimensions(string sceneName, string sourceName, StreamingSoftwareSourceDimensionsModel dimensions)
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source dimensions - " + sourceName);

                if (this.OBSWebsocket.IsConnected)
                {
                    SceneItemProperties properties = this.OBSWebsocket.GetSceneItemProperties(sourceName, sceneName);

                    properties.Position.X = dimensions.X;
                    properties.Position.Y = dimensions.Y;
                    properties.Scale.X = dimensions.XScale;
                    properties.Scale.Y = dimensions.YScale;
                    properties.Rotation = dimensions.Rotation;

                    this.OBSWebsocket.SetSceneItemProperties(properties, sceneName);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        sceneName = await this.OBSWebsocketV5.GetCurrentSceneName();
                    }

                    await this.OBSWebsocketV5.SetSceneItemProperties(sceneName, sourceName, dimensions.X, dimensions.Y, dimensions.XScale, dimensions.YScale, dimensions.Rotation);
                }

                return Task.FromResult(false);
            });
        }

        public async Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(string sceneName, string sourceName)
        {
            return await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                StreamingSoftwareSourceDimensionsModel result = null;

                if (this.OBSWebsocket.IsConnected)
                {
                    OBSScene scene;
                    if (!string.IsNullOrEmpty(sceneName))
                    {
                        scene = this.OBSWebsocket.ListScenes().FirstOrDefault(s => s.Name.Equals(sceneName));
                    }
                    else
                    {
                        scene = this.OBSWebsocket.GetCurrentScene();
                    }

                    foreach (SceneItem item in scene.Items)
                    {
                        if (item.SourceName.Equals(sourceName))
                        {
                            result = new StreamingSoftwareSourceDimensionsModel() { X = (int)item.XPos, Y = (int)item.YPos, XScale = (item.Width / item.SourceWidth), YScale = (item.Height / item.SourceHeight) };
                            break;
                        }
                    }
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        sceneName = await this.OBSWebsocketV5.GetCurrentSceneName();
                    }

                    var response = await this.OBSWebsocketV5.GetSceneItemTransform(sceneName, sourceName);
                    if (response.HasValue)
                    {
                        result = new StreamingSoftwareSourceDimensionsModel() { X = (int)response.Value.X, Y = (int)response.Value.Y, XScale = (response.Value.Width / response.Value.SourceWidth), YScale = (response.Value.Height / response.Value.SourceHeight) };
                    }
                }

                return result;
            });
        }

        public async Task StartStopStream()
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.StartStopStreaming();
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.StartStopStreaming();
                }

                return Task.FromResult(true);
            });
        }

        public async Task StartStopRecording()
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.StartStopRecording();
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.StartStopRecording();
                }

                return Task.FromResult(true);
            });
        }

        public async Task<bool> StartReplayBuffer()
        {
            return await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                try
                {
                    if (this.OBSWebsocket.IsConnected)
                    {
                        this.OBSWebsocket.StartReplayBuffer();
                    }

                    if (this.OBSWebsocketV5.IsConnected)
                    {
                        await this.OBSWebsocketV5.StartReplayBuffer();
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("replay buffer already active") || ex.Message.Equals("replay buffer disabled in settings"))
                    {
                        return true;
                    }
                    Logger.Log(ex);
                }
                return false;
            });
        }

        public async Task SaveReplayBuffer()
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.SaveReplayBuffer();
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.SaveReplayBuffer();
                }

                return Task.FromResult(true);
            });
        }

        public async Task SetSceneCollection(string sceneCollectionName)
        {
            await this.OBSCommandTimeoutWrapper(async (cancellationToken) =>
            {
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.SetCurrentSceneCollection(sceneCollectionName);
                }

                if (this.OBSWebsocketV5.IsConnected)
                {
                    await this.OBSWebsocketV5.SetCurrentSceneCollection(sceneCollectionName);
                }

                return Task.FromResult(true);
            });
        }

        private async void OBSWebsocket_Disconnected(object sender, EventArgs e)
        {
            Result result;
            do
            {
                await this.Disconnect();

                await Task.Delay(5000);

                result = await this.Connect();
            }
            while (!result.Success);
        }

        private async Task<T> OBSCommandTimeoutWrapper<T>(Func<CancellationToken, Task<T>> function, int timeout = CommandTimeoutInMilliseconds)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            var task = function(cancellationTokenSource.Token);
            Task delay = Task.Delay(timeout);
            await Task.WhenAny(new Task[] { task, delay });

            if (task.IsCompleted)
            {
                return task.Result;
            }
            else
            {
                cancellationTokenSource.Cancel();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground((cancellationToken) =>
                {
                    if (this.IsConnected)
                    {
                        this.OBSWebsocket_Disconnected(this, new EventArgs());
                    }
                    return true;
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return default(T);
            }
        }
    }

    public class OBSWebsocketV5 : ClientWebSocketBase
    {
        private const string SceneNameChangedEvent = "SceneNameChanged";
        private const string SceneItemRemovedEvent = "SceneItemRemoved";
        private const string InputRemovedEvent = "InputRemoved";
        private const string InputNameChangedEvent = "InputNameChanged";

        private string password;
        private bool identified = false;
        private ConcurrentDictionary<Guid, string> responses = new ConcurrentDictionary<Guid, string>();

        private SemaphoreSlim sendSemaphore = new SemaphoreSlim(1);

        private ConcurrentDictionary<string, ConcurrentDictionary<string, SceneItem>> sceneSourceNameToSceneItemDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, SceneItem>>(StringComparer.OrdinalIgnoreCase);

        public event EventHandler Disconnected;

        public bool IsConnected
        {
            get => this.identified;
        }

        public OBSWebsocketV5()
        {
            base.OnDisconnectOccurred += OBSWebsocketV5_OnDisconnectOccurred;
        }

        private void OBSWebsocketV5_OnDisconnectOccurred(object sender, WebSocketCloseStatus e)
        {
            this.Disconnected?.Invoke(sender, new EventArgs());
        }

        public async Task<bool> Connect(string endpoint, string password, CancellationToken cancellationToken)
        {
            this.password = password;
            this.identified = false;
            if (await base.Connect(endpoint))
            {
                while (!this.identified && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(100);
                }
            }

            if (cancellationToken.IsCancellationRequested || !this.identified)
            {
                await Disconnect();
            }

            return this.identified;
        }

        public override async Task Disconnect(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure)
        {
            this.identified = false;
            await base.Disconnect(closeStatus);
        }

        public async Task SetCurrentScene(string sceneName)
        {
            OBSMessageSetCurrentProgramSceneRequest request = new OBSMessageSetCurrentProgramSceneRequest(sceneName);
            await Send(request);
        }

        public async Task SetCurrentSceneCollection(string sceneCollectionName)
        {
            OBSMessageSetCurrentSceneCollectionRequest request = new OBSMessageSetCurrentSceneCollectionRequest(sceneCollectionName);
            await Send(request);
        }

        public async Task SaveReplayBuffer()
        {
            OBSMessageSaveReplayBufferRequest request = new OBSMessageSaveReplayBufferRequest();
            await Send(request);
        }

        public async Task StartReplayBuffer()
        {
            OBSMessageStartReplayBufferRequest request = new OBSMessageStartReplayBufferRequest();
            await Send(request);
        }

        public async Task StartStopRecording()
        {
            OBSMessageToggleRecordRequest request = new OBSMessageToggleRecordRequest();
            await Send(request);
        }

        public async Task StartStopStreaming()
        {
            OBSMessageToggleStreamRequest request = new OBSMessageToggleStreamRequest();
            await Send(request);
        }

        public async Task<string> GetCurrentSceneName()
        {
            OBSMessageGetCurrentProgramSceneRequest request = new OBSMessageGetCurrentProgramSceneRequest();

            string packet = await SendAndWait(request);
            if (!string.IsNullOrEmpty(packet))
            {
                OBSMessageGetCurrentProgramSceneResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageGetCurrentProgramSceneResponse>(packet);
                return response?.Data?.Data?.CurrentProgramSceneName ?? "Unknown";
            }

            return "Unknown";
        }

        public async Task<(float X, float Y, float Width, float Height, float SourceWidth, float SourceHeight)?> GetSceneItemTransform(string sceneName, string sourceName)
        {
            OBSMessageGetSceneItemListRequest request = new OBSMessageGetSceneItemListRequest(sceneName);

            string packet = await SendAndWait(request);
            if (!string.IsNullOrEmpty(packet))
            {
                OBSMessageGetSceneItemListResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageGetSceneItemListResponse>(packet);
                if (response?.Data?.Data != null)
                {
                    foreach (SceneItem scene in response?.Data?.Data.SceneItems)
                    {
                        if (string.Equals(scene.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            return (
                                scene.SceneItemTransform.PositionX,
                                scene.SceneItemTransform.PositionY,
                                scene.SceneItemTransform.Width,
                                scene.SceneItemTransform.Height,
                                scene.SceneItemTransform.SourceWidth,
                                scene.SceneItemTransform.SourceHeight
                            );
                        }
                    }
                }
            }

            return null;
        }


        public async Task SetSceneItemProperties(string sceneName, string sourceName, int x, int y, float xScale, float yScale, float rotation)
        {
            OBSMessageGetSceneItemListRequest request = new OBSMessageGetSceneItemListRequest(sceneName);

            string packet = await SendAndWait(request);
            if (!string.IsNullOrEmpty(packet))
            {
                OBSMessageGetSceneItemListResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageGetSceneItemListResponse>(packet);
                if (response?.Data?.Data != null)
                {
                    foreach (SceneItem scene in response?.Data?.Data.SceneItems)
                    {
                        if (string.Equals(scene.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            JObject newTransform = new JObject
                            {
                                ["positionX"] = x,
                                ["positionY"] = y,
                                ["scaleX"] = xScale,
                                ["scaleY"] = yScale,
                                ["rotation"] = rotation
                            };

                            OBSMessageSetSceneItemTransformRequest transformRequest = new OBSMessageSetSceneItemTransformRequest(sceneName, scene.SceneItemId, newTransform);
                            await Send(transformRequest);
                            return;
                        }
                    }
                }
            }
        }

        public async Task<JObject> GetSourceSettings(string sourceName)
        {
            OBSMessageGetInputSettingsRequest request = new OBSMessageGetInputSettingsRequest(sourceName);

            string packet = await SendAndWait(request);
            if (!string.IsNullOrEmpty(packet))
            {
                OBSMessageGetInputSettingsResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageGetInputSettingsResponse>(packet);
                return response?.Data?.Data?.InputSettings;
            }

            return null;
        }

        public async Task SetSourceSettings(string sourceName, JObject settings)
        {
            OBSMessageSetInputSettingsRequest request = new OBSMessageSetInputSettingsRequest(sourceName, settings);
            await Send(request);
        }

        public async Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility)
        {
            OBSMessageSetSourceFilterEnabledRequest request = new OBSMessageSetSourceFilterEnabledRequest(sourceName, filterName, visibility);
            await Send(request);
        }

        public async Task SetSourceRender(string sourceName, bool visibility, string sceneName)
        {
            SceneItem sceneItem = await this.SearchForSceneItem(sourceName, sceneName);
            if (sceneItem != null)
            {
                OBSMessageSetSceneItemEnabledRequest setRequest = new OBSMessageSetSceneItemEnabledRequest(sceneItem.GroupName ?? sceneName, sceneItem.SceneItemId, visibility);
                await Send(setRequest);
            }
        }

        private async Task<SceneItem> SearchForSceneItem(string sourceName, string sceneName)
        {
            // Check our scene cache first
            if (sceneSourceNameToSceneItemDictionary.TryGetValue(sceneName, out var sceneItems))
            {
                if (sceneItems.TryGetValue(sourceName, out var sceneItem))
                {
                    return sceneItem;
                }
            }
            else
            {
                sceneSourceNameToSceneItemDictionary[sceneName] = new ConcurrentDictionary<string, SceneItem>(StringComparer.OrdinalIgnoreCase);
            }

            // Failed hit, invalid the scene's cache
            sceneSourceNameToSceneItemDictionary[sceneName].Clear();

            OBSMessageGetSceneItemListRequest request = new OBSMessageGetSceneItemListRequest(sceneName);
            string packet = await SendAndWait(request);
            if (!string.IsNullOrEmpty(packet))
            {
                OBSMessageGetSceneItemListResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageGetSceneItemListResponse>(packet);
                if (response?.Data?.Data != null)
                {
                    // Cache all items first
                    foreach (SceneItem sceneItem in response?.Data?.Data.SceneItems)
                    {
                        sceneSourceNameToSceneItemDictionary[sceneName][sceneItem.SourceName] = sceneItem;
                    }
                    
                    foreach (SceneItem sceneItem in response?.Data?.Data.SceneItems)
                    {
                        if (string.Equals(sceneItem.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
                        {
                            return sceneItem;
                        }
                    }

                    // If we got here, then the item is not found, search groups (this is slow)
                    foreach (SceneItem sceneItem in response?.Data?.Data.SceneItems)
                    {
                        if (sceneItem.IsGroup.GetValueOrDefault())
                        {
                            OBSMessageGetGroupSceneItemListRequest groupRequest = new OBSMessageGetGroupSceneItemListRequest(sceneItem.SourceName);
                            string groupPacket = await SendAndWait(groupRequest);
                            if (!string.IsNullOrEmpty(groupPacket))
                            {
                                OBSMessageGetGroupSceneItemListResponse groupResponse = JSONSerializerHelper.DeserializeFromString<OBSMessageGetGroupSceneItemListResponse>(groupPacket);
                                if (groupResponse?.Data?.Data != null)
                                {
                                    // Cache all items first
                                    foreach (SceneItem groupSceneItem in groupResponse?.Data?.Data.SceneItems)
                                    {
                                        groupSceneItem.GroupName = sceneItem.SourceName;
                                        sceneSourceNameToSceneItemDictionary[sceneName][groupSceneItem.SourceName] = groupSceneItem;
                                    }

                                    foreach (SceneItem groupSceneItem in groupResponse?.Data?.Data.SceneItems)
                                    {
                                        if (string.Equals(groupSceneItem.SourceName, sourceName, StringComparison.OrdinalIgnoreCase))
                                        {
                                            return groupSceneItem;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected override async Task ProcessReceivedPacket(string packet)
        {
            try
            {
                Logger.Log(LogLevel.Debug, $"OBS Studio packet received: " + packet);

                OBSMessage message = JSONSerializerHelper.DeserializeFromString<OBSMessage>(packet);
                switch (message.OpCode)
                {
                    case 0: // Hello
                        await HandleHello(JSONSerializerHelper.DeserializeFromString<OBSMessageHello>(packet));
                        break;
                    case 2: // Identified
                        await HandleIdentified(JSONSerializerHelper.DeserializeFromString<OBSMessageIdentified>(packet));
                        break;
                    case 5: // Event
                        await HandleEvent(JSONSerializerHelper.DeserializeFromString<OBSMessageEvent>(packet));
                        break;
                    case 7: // Response
                        await HandleResponse(packet);
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine(packet);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private Task HandleResponse(string packet)
        {
            OBSMessageResponse response = JSONSerializerHelper.DeserializeFromString<OBSMessageResponse>(packet);
            if (this.responses.TryRemove(response.Data.RequestId, out string value))
            {
                this.responses.TryAdd(response.Data.RequestId, packet);
            }
            return Task.CompletedTask;
        }

        private Task HandleEvent(OBSMessageEvent message)
        {
            switch (message.Data.EventType)
            {
                case SceneNameChangedEvent:
                    if (message.Data.Data.TryGetValue("oldSceneName", out var oldSceneName))
                    {
                        sceneSourceNameToSceneItemDictionary.TryRemove(oldSceneName?.ToString(), out var _);
                    }
                    break;
                case SceneItemRemovedEvent:
                    if (message.Data.Data.TryGetValue("sceneName", out var removedSceneName) && message.Data.Data.TryGetValue("sourceName", out var removedSourceName))
                    {
                        if (sceneSourceNameToSceneItemDictionary.TryGetValue(removedSceneName?.ToString(), out var sceneItems))
                        {
                            sceneItems.TryRemove(removedSourceName?.ToString(), out var _);
                        }
                    }
                    break;
                case InputRemovedEvent:
                    if (message.Data.Data.TryGetValue("inputName", out var inputName))
                    {
                        foreach (var scene in sceneSourceNameToSceneItemDictionary.Keys.ToList())
                        {
                            if (sceneSourceNameToSceneItemDictionary.TryGetValue(scene, out var sceneItems))
                            {
                                sceneItems.TryRemove(inputName?.ToString(), out var _);
                            }
                        }
                    }
                    break;
                case InputNameChangedEvent:
                    if (message.Data.Data.TryGetValue("oldInputName", out var oldInputName) && message.Data.Data.TryGetValue("inputName", out var newInputName))
                    {
                        foreach (var scene in sceneSourceNameToSceneItemDictionary.Keys.ToList())
                        {
                            if (sceneSourceNameToSceneItemDictionary.TryGetValue(scene, out var sceneItems))
                            {
                                sceneItems.TryRemove(oldInputName?.ToString(), out var _);
                            }
                        }
                    }
                    break;
            }
            return Task.CompletedTask;
        }

        private Task HandleIdentified(OBSMessageIdentified message)
        {
            this.identified = true;
            return Task.CompletedTask;
        }

        private async Task Send(OBSMessage message)
        {
            try
            {
                await this.sendSemaphore.WaitAsync();

                await base.Send(JsonConvert.SerializeObject(message));
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            finally
            {
                this.sendSemaphore.Release();
            }
        }

        private async Task<string> SendAndWait<T>(OBSMessageRequest<T> request)
        {
            this.responses.TryAdd(request.Data.RequestId, null);
            await Send(request);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                while (!cts.IsCancellationRequested)
                {
                    if (this.responses.TryGetValue(request.Data.RequestId, out string packet) && !string.IsNullOrEmpty(packet))
                    {
                        return packet;
                    }

                    await Task.Delay(100);
                }
            }

            return null;
        }

        private async Task<string> SendAndWait(OBSMessageRequest request)
        {
            this.responses.TryAdd(request.Data.RequestId, null);
            await Send(request);

            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                while (!cts.IsCancellationRequested)
                {
                    if (this.responses.TryGetValue(request.Data.RequestId, out string packet) && !string.IsNullOrEmpty(packet))
                    {
                        return packet;
                    }

                    await Task.Delay(100);
                }
            }

            return null;
        }

        private async Task HandleHello(OBSMessageHello message)
        {
            OBSMessageIdentify identify = new OBSMessageIdentify();
            if (message != null && message.Data != null && message.Data.Authentication != null)
            {
                // To generate the authentication string, follow these steps:
                // Concatenate the websocket password with the salt provided by the server(password + salt)
                string passwordAndSalt = this.password + message.Data.Authentication.Salt;

                // Generate an SHA256 binary hash of the result and base64 encode it, known as a base64 secret.
                using (SHA256Managed sha256Hash = new SHA256Managed())
                {
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(passwordAndSalt));
                    string base64Secret = Convert.ToBase64String(bytes);

                    // Concatenate the base64 secret with the challenge sent by the server(base64_secret + challenge)
                    string base64SecretAndChallenge = base64Secret + message.Data.Authentication.Challenge;

                    // Generate a binary SHA256 hash of that result and base64 encode it. You now have your authentication string.
                    bytes = sha256Hash.ComputeHash(Encoding.ASCII.GetBytes(base64SecretAndChallenge));
                    identify.Data.Authentication = Convert.ToBase64String(bytes);
                }
            }

            await Send(identify);
        }

        private class OBSMessage
        {
            [JsonProperty("op")]
            public int OpCode { get; protected set; }
        }

        private class OBSMessage<T> : OBSMessage
        {
            [JsonProperty("d")]
            public T Data { get; set; }
        }

        private class OBSMessageHello : OBSMessage<HelloData>
        {
        }

        private class HelloData
        {
            [JsonProperty("obsWebSocketVersion")]
            public string OBSWebSocketVersion { get; set; }

            [JsonProperty("rpcVersion")]
            public int RPCVersion { get; set; }

            [JsonProperty("authentication")]
            public HelloDataAuthentication Authentication { get; set; }
        }

        private class HelloDataAuthentication
        {
            [JsonProperty("challenge")]
            public string Challenge { get; set; }

            [JsonProperty("salt")]
            public string Salt { get; set; }
        }

        private class OBSMessageIdentify : OBSMessage<IdentifyData>
        {
            public OBSMessageIdentify()
            {
                OpCode = 1;
                Data = new IdentifyData
                {
                    RPCVersion = 1,
                    EventSubscriptions = (1 << 2) | (1 << 3) | (1 << 7),   // Scene, Input, Scene Items events
                };
            }
        }

        private class IdentifyData
        {
            [JsonProperty("rpcVersion")]
            public int RPCVersion { get; set; }

            [JsonProperty("authentication")]
            public string Authentication { get; set; }

            [JsonProperty("eventSubscriptions")]
            public ulong EventSubscriptions { get; set; }
        }

        private class OBSMessageIdentified : OBSMessage<IdentifiedData>
        {
        }

        private class IdentifiedData
        {
            [JsonProperty("negotiatedRpcVersion")]
            public string NegotiatedRpcVersion { get; set; }
        }

        private class OBSMessageEvent : OBSMessage<EventData>
        {
        }

        private class EventData
        {
            [JsonProperty("eventType")]
            public string EventType { get; set; }
            [JsonProperty("eventIntent")]
            public int EventIntent { get; set; }
            [JsonProperty("eventData")]
            public JObject Data { get; set; }
        }

        private class OBSMessageToggleStreamRequest : OBSMessageRequest
        {
            public OBSMessageToggleStreamRequest() : base()
            {
                this.Data.RequestType = "ToggleStream";
            }
        }

        private class OBSMessageToggleRecordRequest : OBSMessageRequest
        {
            public OBSMessageToggleRecordRequest() : base()
            {
                this.Data.RequestType = "ToggleRecord";
            }
        }

        private class OBSMessageStartReplayBufferRequest : OBSMessageRequest
        {
            public OBSMessageStartReplayBufferRequest() : base()
            {
                this.Data.RequestType = "StartReplayBuffer";
            }
        }

        private class OBSMessageSaveReplayBufferRequest : OBSMessageRequest
        {
            public OBSMessageSaveReplayBufferRequest() : base()
            {
                this.Data.RequestType = "SaveReplayBuffer";
            }
        }

        private class OBSMessageGetCurrentProgramSceneResponse : OBSMessageResponse<GetCurrentProgramSceneData>
        {
        }

        private class GetCurrentProgramSceneData
        {
            [JsonProperty("currentProgramSceneName")]
            public string CurrentProgramSceneName { get; set; }
        }

        private class OBSMessageSetSceneItemTransformRequest : OBSMessageRequest<SetSceneItemTransformRequestData>
        {
            public OBSMessageSetSceneItemTransformRequest(string sceneName, int sceneItemId, JObject sceneItemTransform) : base()
            {
                this.Data.RequestType = "SetSceneItemTransform";
                this.Data.Data = new SetSceneItemTransformRequestData
                {
                    SceneName = sceneName,
                    SceneItemId = sceneItemId,
                    SceneItemTransform = sceneItemTransform,
                };
            }
        }

        private class SetSceneItemTransformRequestData
        {
            [JsonProperty("sceneName")]
            public string SceneName { get; set; }

            [JsonProperty("sceneItemId")]
            public int SceneItemId { get; set; }

            [JsonProperty("sceneItemTransform")]
            public JObject SceneItemTransform { get; set; }
        }

        private class OBSMessageGetSceneItemListRequest : OBSMessageRequest<GetSceneItemListRequestData>
        {
            public OBSMessageGetSceneItemListRequest(string sceneName) : base()
            {
                this.Data.RequestType = "GetSceneItemList";
                this.Data.Data = new GetSceneItemListRequestData
                {
                    SceneName = sceneName,
                };
            }
        }

        private class GetSceneItemListRequestData
        {
            [JsonProperty("sceneName")]
            public string SceneName { get; set; }
        }

        private class OBSMessageGetSceneItemListResponse : OBSMessageResponse<GetSceneItemListResponseData>
        {
        }

        private class GetSceneItemListResponseData
        {
            [JsonProperty("sceneItems")]
            public SceneItem[] SceneItems { get; set; }
        }

        private class OBSMessageGetGroupSceneItemListRequest : OBSMessageRequest<GetGroupSceneItemListRequestData>
        {
            public OBSMessageGetGroupSceneItemListRequest(string groupName) : base()
            {
                this.Data.RequestType = "GetGroupSceneItemList";
                this.Data.Data = new GetGroupSceneItemListRequestData
                {
                    SceneName = groupName,
                };
            }
        }

        private class GetGroupSceneItemListRequestData
        {
            [JsonProperty("sceneName")]
            public string SceneName { get; set; }
        }

        private class OBSMessageGetGroupSceneItemListResponse : OBSMessageResponse<GetGroupSceneItemListResponseData>
        {
        }

        private class GetGroupSceneItemListResponseData
        {
            [JsonProperty("sceneItems")]
            public SceneItem[] SceneItems { get; set; }
        }

        private class SceneItem
        {
            [JsonProperty("sceneItemId")]
            public int SceneItemId { get; set; }

            [JsonProperty("sourceName")]
            public string SourceName { get; set; }

            [JsonProperty("sceneItemTransform")]
            public SceneItemTransform SceneItemTransform { get; set; }

            [JsonProperty("isGroup")]
            public bool? IsGroup { get; set; }

            public string GroupName { get; set; }
        }

        private class SceneItemTransform
        {
            [JsonProperty("alignment")]
            public int Alignment { get; set; }

            [JsonProperty("boundsAlignment")]
            public int BoundsAlignment { get; set; }

            [JsonProperty("boundsHeight")]
            public float BoundsHeight { get; set; }

            [JsonProperty("boundsType")]
            public string BoundsType { get; set; }

            [JsonProperty("boundsWidth")]
            public float BoundsWidth { get; set; }

            [JsonProperty("cropBottom")]
            public int CropBottom { get; set; }

            [JsonProperty("cropLeft")]
            public int CropLeft { get; set; }

            [JsonProperty("cropRight")]
            public int CropRight { get; set; }

            [JsonProperty("cropTop")]
            public int CropTop { get; set; }

            [JsonProperty("height")]
            public float Height { get; set; }

            [JsonProperty("positionX")]
            public float PositionX { get; set; }

            [JsonProperty("positionY")]
            public float PositionY { get; set; }

            [JsonProperty("rotation")]
            public float Rotation { get; set; }

            [JsonProperty("scaleX")]
            public float ScaleX { get; set; }

            [JsonProperty("scaleY")]
            public float ScaleY { get; set; }

            [JsonProperty("sourceHeight")]
            public float SourceHeight { get; set; }

            [JsonProperty("sourceWidth")]
            public float SourceWidth { get; set; }

            [JsonProperty("width")]
            public float Width { get; set; }
        }

        private class OBSMessageGetCurrentProgramSceneRequest : OBSMessageRequest
        {
            public OBSMessageGetCurrentProgramSceneRequest() : base()
            {
                this.Data.RequestType = "GetCurrentProgramScene";
            }
        }

        private class OBSMessageSetSceneItemEnabledRequest : OBSMessageRequest<SetSceneItemEnabledData>
        {
            public OBSMessageSetSceneItemEnabledRequest(string sceneName, int sceneItemId, bool sceneItemEnabled) : base()
            {
                this.Data.RequestType = "SetSceneItemEnabled";
                this.Data.Data = new SetSceneItemEnabledData
                {
                    SceneName = sceneName,
                    SceneItemId = sceneItemId,
                    SceneItemEnabled = sceneItemEnabled,
                };
            }
        }

        private class SetSceneItemEnabledData
        {
            [JsonProperty("sceneName")]
            public string SceneName { get; set; }

            [JsonProperty("sceneItemId")]
            public int SceneItemId { get; set; }

            [JsonProperty("sceneItemEnabled")]
            public bool SceneItemEnabled { get; set; }
        }

        private class OBSMessageGetInputSettingsRequest : OBSMessageRequest<GetInputSettingsData>
        {
            public OBSMessageGetInputSettingsRequest(string sourceName) : base()
            {
                this.Data.RequestType = "GetInputSettings";
                this.Data.Data = new GetInputSettingsData
                {
                    InputName = sourceName
                };
            }
        }

        private class GetInputSettingsData
        {
            [JsonProperty("inputName")]
            public string InputName { get; set; }
        }

        private class OBSMessageGetInputSettingsResponse : OBSMessageResponse<GetInputSettingsResponseData>
        {
        }

        private class GetInputSettingsResponseData
        {
            [JsonProperty("inputSettings")]
            public JObject InputSettings { get; set; }
            [JsonProperty("inputKind")]
            public string InputKind { get; set; }
        }

        private class OBSMessageSetInputSettingsRequest : OBSMessageRequest<SetInputSettingsData>
        {
            public OBSMessageSetInputSettingsRequest(string sourceName, JObject settings) : base()
            {
                this.Data.RequestType = "SetInputSettings";
                this.Data.Data = new SetInputSettingsData
                {
                    InputName = sourceName,
                    InputSettings = settings,
                };
            }
        }

        private class SetInputSettingsData
        {
            [JsonProperty("inputName")]
            public string InputName { get; set; }

            [JsonProperty("inputSettings")]
            public JObject InputSettings { get; set; }
        }

        private class OBSMessageSetSourceFilterEnabledRequest : OBSMessageRequest<SetSourceFilterEnabledData>
        {
            public OBSMessageSetSourceFilterEnabledRequest(string sourceName, string filterName, bool filterEnabled) : base()
            {
                this.Data.RequestType = "SetSourceFilterEnabled";
                this.Data.Data = new SetSourceFilterEnabledData
                {
                    SourceName = sourceName,
                    FilterName = filterName,
                    FilterEnabled = filterEnabled,
                };
            }
        }

        private class SetSourceFilterEnabledData
        {
            [JsonProperty("sourceName")]
            public string SourceName { get; set; }

            [JsonProperty("filterName")]
            public string FilterName { get; set; }

            [JsonProperty("filterEnabled")]
            public bool FilterEnabled { get; set; }
        }

        private class OBSMessageSetCurrentSceneCollectionRequest : OBSMessageRequest<SetCurrentSceneCollectionData>
        {
            public OBSMessageSetCurrentSceneCollectionRequest(string sceneCollectionName) : base()
            {
                this.Data.RequestType = "SetCurrentProgramScene";
                this.Data.Data = new SetCurrentSceneCollectionData
                {
                    SceneCollectionName = sceneCollectionName,
                };
            }
        }

        private class SetCurrentSceneCollectionData
        {
            [JsonProperty("sceneCollectionName")]
            public string SceneCollectionName { get; set; }
        }

        private class OBSMessageSetCurrentProgramSceneRequest : OBSMessageRequest<SetCurrentProgramSceneRequestData>
        {
            public OBSMessageSetCurrentProgramSceneRequest(string sceneName) : base()
            {
                this.Data.RequestType = "SetCurrentProgramScene";
                this.Data.Data = new SetCurrentProgramSceneRequestData
                {
                    SceneName = sceneName,
                };
            }
        }

        private class SetCurrentProgramSceneRequestData
        {
            [JsonProperty("sceneName")]
            public string SceneName { get; set; }
        }

        private class OBSMessageRequest : OBSMessage<RequestData>
        {
            public OBSMessageRequest()
            {
                OpCode = 6;
                this.Data = new RequestData
                {
                    RequestId = Guid.NewGuid(),
                };
            }
        }

        private class OBSMessageRequest<T> : OBSMessage<RequestData<T>>
        {
            public OBSMessageRequest()
            {
                OpCode = 6;
                this.Data = new RequestData<T>
                {
                    RequestId = Guid.NewGuid(),
                };
            }
        }

        private class RequestData
        {
            [JsonProperty("requestType")]
            public string RequestType { get; set; }

            [JsonProperty("requestId")]
            public Guid RequestId { get; set; }
        }

        private class RequestData<T> : RequestData
        {
            [JsonProperty("requestData")]
            public T Data { get; set; }
        }

        private class OBSMessageResponse : OBSMessage<ResponseData>
        {
        }

        private class OBSMessageResponse<T> : OBSMessage<ResponseData<T>>
        {
        }

        private class ResponseData
        {
            [JsonProperty("requestType")]
            public string RequestType { get; set; }

            [JsonProperty("requestId")]
            public Guid RequestId { get; set; }

            [JsonProperty("requestStatus")]
            public RequestStatus Status { get; set; }
        }

        private class ResponseData<T> : ResponseData
        {
            [JsonProperty("responseData")]
            public T Data { get; set; }
        }

        private class RequestStatus
        {
            [JsonProperty("result")]
            public bool Result { get; set; }

            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("comment")]
            public string Comment { get; set; }
        }
    }
}
