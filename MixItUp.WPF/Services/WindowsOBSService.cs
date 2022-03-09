using MixItUp.Base;
using MixItUp.Base.Model.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using OBSWebsocketDotNet;
using OBSWebsocketDotNet.Types;
using StreamingClient.Base.Util;
using System;
using System.Linq;
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

        public WindowsOBSService() { }

        public string Name { get { return "OBS Studio"; } }

        public bool IsEnabled { get { return !string.IsNullOrEmpty(ChannelSession.Settings.OBSStudioServerIP); } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            this.IsConnected = false;

            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                this.OBSWebsocket.Connect(ChannelSession.Settings.OBSStudioServerIP, ChannelSession.Settings.OBSStudioServerPassword);
                if (this.OBSWebsocket.IsConnected)
                {
                    this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;
                    this.IsConnected = true;
                    return true;
                }
                return false;
            }, ConnectTimeoutInMilliseconds);

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
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.IsConnected = false;
                if (this.OBSWebsocket != null)
                {
                    this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                    this.OBSWebsocket.Disconnect();
                    this.Disconnected(this, new EventArgs());
                    ChannelSession.DisconnectionOccurred(MixItUp.Base.Resources.OBSStudio);
                }
                return true;
            }, ConnectTimeoutInMilliseconds);
        }

        public Task<bool> TestConnection() { return Task.FromResult(true); }

        public async Task ShowScene(string sceneName)
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Showing OBS Scene - " + sceneName);

                this.OBSWebsocket.SetCurrentScene(sceneName);

                return true;
            });
        }

        public async Task<string> GetCurrentScene()
        {
            return await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Getting Current OBS Scene");

                var scene = this.OBSWebsocket.GetCurrentScene();
                if (scene == null)
                {
                    return "Unknown";
                }

                return scene.Name;
            });
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source visibility - " + sourceName);

                this.OBSWebsocket.SetSourceRender(sourceName, visibility, sceneName);

                return true;
            });
        }

        public async Task SetSourceFilterVisibility(string sourceName, string filterName, bool visibility)
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source filter visibility - " + sourceName + " - " + filterName);

                this.OBSWebsocket.SetSourceFilterVisibility(sourceName, filterName, visibility);

                return true;
            });
        }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            Logger.Log(LogLevel.Debug, "Setting web browser URL - " + sourceName);

            await this.SetSourceVisibility(sceneName, sourceName, visibility: false);

            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                SourceSettings properties = this.OBSWebsocket.GetSourceSettings(sourceName);
                properties.Settings["is_local_file"] = false;
                properties.Settings["url"] = url;
                this.OBSWebsocket.SetSourceSettings(sourceName, properties.Settings);

                return true;
            });
        }

        public async Task SetSourceDimensions(string sceneName, string sourceName, StreamingSoftwareSourceDimensionsModel dimensions)
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                Logger.Log(LogLevel.Debug, "Setting source dimensions - " + sourceName);

                SceneItemProperties properties = this.OBSWebsocket.GetSceneItemProperties(sourceName, sceneName);

                properties.Position.X = dimensions.X;
                properties.Position.Y = dimensions.Y;
                properties.Scale.X = dimensions.XScale;
                properties.Scale.Y = dimensions.YScale;
                properties.Rotation = dimensions.Rotation;

                this.OBSWebsocket.SetSceneItemProperties(properties, sceneName);

                return false;
            });
        }

        public async Task<StreamingSoftwareSourceDimensionsModel> GetSourceDimensions(string sceneName, string sourceName)
        {
            return await this.OBSCommandTimeoutWrapper((cancellationToken) =>
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
                        return new StreamingSoftwareSourceDimensionsModel() { X = (int)item.XPos, Y = (int)item.YPos, XScale = (item.Width / item.SourceWidth), YScale = (item.Height / item.SourceHeight) };
                    }
                }
                return null;
            });
        }

        public async Task StartStopStream()
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.OBSWebsocket.StartStopStreaming();
                return true;
            });
        }

        public async Task StartStopRecording()
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.OBSWebsocket.StartStopRecording();
                return true;
            });
        }

        public async Task<bool> StartReplayBuffer()
        {
            return await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                try
                {
                    this.OBSWebsocket.StartReplayBuffer();
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
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.OBSWebsocket.SaveReplayBuffer();
                return true;
            });
        }

        public async Task SetSceneCollection(string sceneCollectionName)
        {
            await this.OBSCommandTimeoutWrapper((cancellationToken) =>
            {
                this.OBSWebsocket.SetCurrentSceneCollection(sceneCollectionName);
                return true;
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

        private async Task<T> OBSCommandTimeoutWrapper<T>(Func<CancellationToken, T> function, int timeout = CommandTimeoutInMilliseconds)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task<T> task = AsyncRunner.RunAsyncBackground(function, cancellationTokenSource.Token);
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
                    this.OBSWebsocket_Disconnected(this, new EventArgs());
                    return true;
                }, new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return default(T);
            }
        }
    }
}
