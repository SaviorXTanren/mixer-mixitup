using MixItUp.Base.Services;
using MixItUp.Base.Util;
using OBSWebsocketDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.OBS
{
    public class OBSService : IOBSService
    {
        public event EventHandler Disconnected;

        private OBSWebsocket OBSWebsocket;

        public async Task<bool> Initialize(string serverIP, string password)
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
                        this.OBSWebsocket.Connect(serverIP, password);
                        this.OBSWebsocket.Disconnected += OBSWebsocket_Disconnected;
                        connected = true;
                    }
                    catch (Exception ex) { Logger.Log(ex); }
                }, tokenSource.Token);

                await Task.Delay(2000);
                tokenSource.Cancel();

                if (!connected)
                {
                    this.OBSWebsocket = null;
                }
                return connected;
            }
            return false;
        }

        public OBSSourceDimensions GetSourceDimensions(string source)
        {
            try
            {
                OBSScene scene = this.OBSWebsocket.GetCurrentScene();
                foreach (SceneItem item in scene.Items)
                {
                    if (item.SourceName.Equals(source))
                    {
                        return new OBSSourceDimensions() { X = (int)item.XPos, Y = (int)item.YPos, XScale = (item.Width / item.SourceWidth), YScale = (item.Height / item.SourceHeight) };
                    }
                }
            }
            catch (Exception ex) { Logger.Log(ex); }
            return null;
        }

        public void SetCurrentSceneCollection(string sceneCollection)
        {
            try
            {
                this.OBSWebsocket.SetCurrentSceneCollection(sceneCollection);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void SetCurrentScene(string scene)
        {
            try
            {
                this.OBSWebsocket.SetCurrentScene(scene);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void SetSourceRender(string source, bool isVisible)
        {
            try
            {
                this.OBSWebsocket.SetSourceRender(source, isVisible);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void SetWebBrowserSource(string source, string url)
        {
            try
            {
                this.OBSWebsocket.SetSourceRender(source, false);

                BrowserSourceProperties properties = this.OBSWebsocket.GetBrowserSourceProperties(source);
                properties.IsLocalFile = false;
                properties.URL = url;
                this.OBSWebsocket.SetBrowserSourceProperties(source, properties);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public void SetSourceDimensions(string source, OBSSourceDimensions dimensions)
        {
            try
            {
                this.OBSWebsocket.SetSceneItemPosition(source, dimensions.X, dimensions.Y);
                this.OBSWebsocket.SetSceneItemTransform(source, dimensions.Rotation, dimensions.XScale, dimensions.YScale);
            }
            catch (Exception ex) { Logger.Log(ex); }
        }

        public Task Close()
        {
            if (this.OBSWebsocket != null)
            {
                this.OBSWebsocket.Disconnect();
                this.OBSWebsocket = null;
            }
            return Task.FromResult(0);
        }

        private void OBSWebsocket_Disconnected(object sender, EventArgs e)
        {
            this.Close();
            if (this.Disconnected != null)
            {
                this.Disconnected(this, new EventArgs());
            }
        }
    }
}
