using MixItUp.Base.Actions;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using OBSWebsocketDotNet;
using System;
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

                CancellationTokenSource tokenSource = new CancellationTokenSource();
                bool connected = false;

                Task t = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        this.OBSWebsocket.Connect(this.serverIP, this.password);
                        this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;

                        connected = true;
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }, tokenSource.Token);

                await Task.Delay(2000);
                tokenSource.Cancel();

                await this.StartReplayBuffer();

                if (connected)
                {
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

        public Task SetSourceVisibility(string sourceName, bool visibility)
        {
            try
            {
                this.OBSWebsocket.SetSourceRender(sourceName, visibility);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task SetWebBrowserSourceURL(string sourceName, string url)
        {
            try
            {
                this.SetSourceVisibility(sourceName, visibility: false);

                BrowserSourceProperties properties = this.OBSWebsocket.GetBrowserSourceProperties(sourceName);
                properties.IsLocalFile = false;
                properties.URL = url;
                this.OBSWebsocket.SetBrowserSourceProperties(sourceName, properties);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task SetSourceDimensions(string sourceName, StreamingSourceDimensions dimensions)
        {
            try
            {
                this.OBSWebsocket.SetSceneItemPosition(sourceName, dimensions.X, dimensions.Y);
                this.OBSWebsocket.SetSceneItemTransform(sourceName, dimensions.Rotation, dimensions.XScale, dimensions.YScale);
            }
            catch (Exception ex) { Logger.Log(ex); }
            return Task.FromResult(0);
        }

        public Task<StreamingSourceDimensions> GetSourceDimensions(string sourceName)
        {
            try
            {
                OBSScene scene = this.OBSWebsocket.GetCurrentScene();
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
            Task t = Task.Run(() => { this.OBSWebsocket.StartReplayBuffer(); }, cancellationToken.Token);
            await Task.Delay(2000);
            if (!t.IsCompleted)
            {
                cancellationToken.Cancel();
            }
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
