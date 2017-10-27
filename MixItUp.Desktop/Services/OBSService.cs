using MixItUp.Base.Services;
using OBSWebsocketDotNet;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.OBS
{
    public class OBSService : IOBSService
    {
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
                        connected = true;
                    }
                    catch (Exception) { }
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

        public void SetCurrentSceneCollection(string sceneCollection)
        {
            try
            {
                this.OBSWebsocket.SetCurrentSceneCollection(sceneCollection);
            }
            catch (Exception) { }
        }

        public void SetCurrentScene(string scene)
        {
            try
            {
                this.OBSWebsocket.SetCurrentScene(scene);
            }
            catch (Exception) { }
        }

        public void SetSourceRender(string source, bool isVisible)
        {
            try
            {
                this.OBSWebsocket.SetSourceRender(source, isVisible);
            }
            catch (Exception) { }
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
    }
}
