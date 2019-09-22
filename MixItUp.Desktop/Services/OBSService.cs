using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using OBSWebsocketDotNet;
using StreamingClient.Base.Util;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.OBS
{
    public class OBSService : IStreamingSoftwareService
    {
        public event EventHandler Connected = delegate { };
        public event EventHandler Disconnected = delegate { };

        private string serverIP;
        private string password;
        private OBSWebsocket OBSWebsocket;

        public OBSService(string serverIP, string password)
        {
            this.serverIP = serverIP;
            this.password = password;
        }

        public async Task<bool> Connect()
        {
            if (this.OBSWebsocket == null)
            {
                this.OBSWebsocket = new OBSWebsocket();

                bool connected = false;

                try
                {
                    this.OBSWebsocket.Connect(this.serverIP, this.password);
                    if (this.OBSWebsocket.IsConnected)
                    {
                        this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;
                        connected = true;
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }

                if (connected)
                {
                    await this.StartReplayBuffer();
                    this.Connected(this, new EventArgs());
                }
                else
                {
                    this.OBSWebsocket = null;
                }
                return connected;
            }
            return false;
        }

        public Task Disconnect()
        {
            if (this.OBSWebsocket != null)
            {
                this.OBSWebsocket.Disconnected -= OBSWebsocket_Disconnected;
                this.OBSWebsocket.Disconnect();
                this.Disconnected(this, new EventArgs());
                this.OBSWebsocket = null;
            }
            return Task.FromResult(0);
        }

        public Task<bool> TestConnection() { return Task.FromResult(true); }

        public Task ShowScene(string sceneName)
        {
            try
            {
                this.OBSWebsocket.SetCurrentScene(sceneName);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task SetSourceVisibility(string sceneName, string sourceName, bool visibility)
        {
            try
            {
                this.OBSWebsocket.SetSourceRender(sourceName, visibility, sceneName);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task SetWebBrowserSourceURL(string sceneName, string sourceName, string url)
        {
            try
            {
                this.SetSourceVisibility(sceneName, sourceName, visibility: false);

                BrowserSourceProperties properties = this.OBSWebsocket.GetBrowserSourceProperties(sourceName, sceneName);
                properties.IsLocalFile = false;
                properties.URL = url;
                this.OBSWebsocket.SetBrowserSourceProperties(sourceName, properties);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task SetSourceDimensions(string sceneName, string sourceName, StreamingSourceDimensions dimensions)
        {
            try
            {
                this.OBSWebsocket.SetSceneItemPosition(sourceName, dimensions.X, dimensions.Y, sceneName);
                this.OBSWebsocket.SetSceneItemTransform(sourceName, dimensions.Rotation, dimensions.XScale, dimensions.YScale, sceneName);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task<StreamingSourceDimensions> GetSourceDimensions(string sceneName, string sourceName)
        {
            try
            {
                OBSScene scene = this.OBSWebsocket.GetCurrentScene();
                if (!string.IsNullOrEmpty(sceneName))
                {
                    scene = this.OBSWebsocket.ListScenes().FirstOrDefault(s => s.Name.Equals(sceneName));
                }

                foreach (SceneItem item in scene.Items)
                {
                    if (item.SourceName.Equals(sourceName))
                    {
                        return Task.FromResult(new StreamingSourceDimensions() { X = (int)item.XPos, Y = (int)item.YPos, XScale = (item.Width / item.SourceWidth), YScale = (item.Height / item.SourceHeight) });
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult<StreamingSourceDimensions>(null);
        }

        public Task StartStopStream()
        {
            try
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
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public async Task<bool> StartReplayBuffer()
        {
            try
            {
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                Task t = Task.Run(() => { this.OBSWebsocket.StartReplayBuffer(); }, cancellationToken.Token);
                await Task.Delay(2000);
                if (!t.IsCompleted)
                {
                    cancellationToken.Cancel();
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals("replay buffer already active"))
                {
                    Logger.Log(ex);
                    return false;
                }
            }
            return true;
        }

        public async Task SaveReplayBuffer()
        {
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task t = Task.Run(() => { this.OBSWebsocket.SaveReplayBuffer(); }, cancellationToken.Token);
            await Task.Delay(2000);
            if (!t.IsCompleted)
            {
                cancellationToken.Cancel();
            }
        }

        public Task SetSceneCollection(string sceneCollectionName)
        {
            try
            {
                this.OBSWebsocket.SetCurrentSceneCollection(sceneCollectionName);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        private async void OBSWebsocket_Disconnected(object sender, EventArgs e)
        {
            await this.Disconnect();

            do
            {
                await Task.Delay(2500);
            }
            while (!await this.Connect());
        }
    }
}
