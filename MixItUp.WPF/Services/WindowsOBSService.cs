using MixItUp.Base;
using MixItUp.Base.Actions;
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
    public class WindowsOBSService : IStreamingSoftwareService
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

            await this.OBSCommandTimeoutWrapper(() =>
            {
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
                ChannelSession.ReconnectionOccurred("OBS");
                return new Result();
            }
            return new Result("Failed to connect to OBS Studio web socket.");
        }

        public async Task Disconnect()
        {
            await this.OBSCommandTimeoutWrapper(() =>
            {
                this.IsConnected = false;
                if (this.OBSWebsocket != null)
                {
                    this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                    this.OBSWebsocket.Disconnect();
                    this.Disconnected(this, new EventArgs());
                    ChannelSession.DisconnectionOccurred("OBS");
                }
                return true;
            }, ConnectTimeoutInMilliseconds);
        }

        public Task<bool> TestConnection() { return Task.FromResult(true); }

        public async Task ShowScene(string sceneName)
        {
            await this.OBSCommandTimeoutWrapper(() =>
            {
                Logger.Log(LogLevel.Debug, "Showing OBS Scene - " + sceneName);

                this.OBSWebsocket.SetCurrentScene(sceneName);

                return true;
            });
        }

        public async Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            await this.OBSCommandTimeoutWrapper(() =>
            {
                Logger.Log(LogLevel.Debug, "Setting source visibility - " + sourceName);

                this.OBSWebsocket.SetSourceRender(sourceName, visibility, sceneName);

                return true;
            });
        }

        public async Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            Logger.Log(LogLevel.Debug, "Setting web browser URL - " + sourceName);

            await this.SetSourceVisibility(sceneName, sourceName, visibility: false);

            await this.OBSCommandTimeoutWrapper(() =>
            {
                BrowserSourceProperties properties = this.OBSWebsocket.GetBrowserSourceProperties(sourceName, sceneName);
                properties.IsLocalFile = false;
                properties.URL = url;
                this.OBSWebsocket.SetBrowserSourceProperties(sourceName, properties);

                return true;
            });
        }

        public async Task SetSourceDimensions(string sceneName, string sourceName, StreamingSourceDimensions dimensions)
        {
            await this.OBSCommandTimeoutWrapper(() =>
            {
                Logger.Log(LogLevel.Debug, "Setting source dimensions - " + sourceName);

                this.OBSWebsocket.SetSceneItemPosition(sourceName, dimensions.X, dimensions.Y, sceneName);
                this.OBSWebsocket.SetSceneItemTransform(sourceName, dimensions.Rotation, dimensions.XScale, dimensions.YScale, sceneName);

                return false;
            });
        }

        public async Task<StreamingSourceDimensions> GetSourceDimensions(string sceneName, string sourceName)
        {
            return await this.OBSCommandTimeoutWrapper(() =>
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
                        return new StreamingSourceDimensions() { X = (int)item.XPos, Y = (int)item.YPos, XScale = (item.Width / item.SourceWidth), YScale = (item.Height / item.SourceHeight) };
                    }
                }
                return null;
            });
        }

        public async Task StartStopStream()
        {
            await this.OBSCommandTimeoutWrapper(() =>
            {
                OutputStatus status = this.OBSWebsocket.GetStreamingStatus();
                if (status.IsStreaming)
                {
                    this.OBSWebsocket.StopStreaming();
                }
                else
                {
                    this.OBSWebsocket.StartStreaming();
                }
                return true;
            });
        }

        public async Task<bool> StartReplayBuffer()
        {
            return await this.OBSCommandTimeoutWrapper(() =>
            {
                try
                {
                    this.OBSWebsocket.StartReplayBuffer();
                    return true;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Equals("replay buffer already active"))
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
            await this.OBSCommandTimeoutWrapper(() =>
            {
                this.OBSWebsocket.SaveReplayBuffer();
                return true;
            });
        }

        public async Task SetSceneCollection(string sceneCollectionName)
        {
            await this.OBSCommandTimeoutWrapper(() =>
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

                await Task.Delay(2500);

                result = await this.Connect();
            }
            while (!result.Success);
        }

        private async Task<T> OBSCommandTimeoutWrapper<T>(Func<T> function, int timeout = CommandTimeoutInMilliseconds)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Task<T> task = AsyncRunner.RunSyncAsAsync(function, cancellationTokenSource.Token);
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
                AsyncRunner.RunSyncAsAsync(() => this.OBSWebsocket_Disconnected(this, new EventArgs()), new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                return default(T);
            }
        }
    }
}
