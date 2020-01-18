using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOverlayService : IExternalService
    {
        event EventHandler OnOverlayConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred;

        string DefaultOverlayName { get; }
        int DefaultOverlayPort { get; }

        Task<bool> AddOverlay(string name, int port);

        Task RemoveOverlay(string name);

        IOverlayEndpointService GetOverlay(string name);

        IEnumerable<string> GetOverlayNames();

        Task<int> TestConnections();

        void StartBatching();

        Task EndBatching();
    }

    public class OverlayService : IOverlayService, IDisposable
    {
        public event EventHandler OnOverlayConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred = delegate { };

        private Dictionary<string, IOverlayEndpointService> overlays = new Dictionary<string, IOverlayEndpointService>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public string DefaultOverlayName { get { return "Default"; } }
        public int DefaultOverlayPort { get { return 8111; } }

        public string Name { get { return "Overlay"; } }

        public bool IsConnected { get; private set; }

        public async Task<ExternalServiceResult> Connect()
        {
            this.IsConnected = false;
            foreach (var kvp in ChannelSession.AllOverlayNameAndPorts)
            {
                if (!await this.AddOverlay(kvp.Key, kvp.Value))
                {
                    await this.Disconnect();
                    return new ExternalServiceResult("Failed to add " + kvp.Key + " overlay");
                }
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () => { await this.WidgetsBackgroundUpdate(); }, this.backgroundThreadCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            this.IsConnected = true;
            return new ExternalServiceResult();
        }

        public async Task Disconnect()
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
            foreach (string overlayName in this.GetOverlayNames())
            {
                await this.RemoveOverlay(overlayName);
            }
            this.IsConnected = false;
        }

        public async Task<bool> AddOverlay(string name, int port)
        {
            IOverlayEndpointService overlay = new OverlayEndpointService(name, port);
            if (await overlay.Initialize())
            {
                overlay.OnWebSocketConnectedOccurred += Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred += Overlay_OnWebSocketDisconnectedOccurred;
                this.overlays[name] = overlay;
                return true;
            }
            await this.RemoveOverlay(name);
            return false;
        }

        public async Task RemoveOverlay(string name)
        {
            IOverlayEndpointService overlay = this.GetOverlay(name);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(name);
            }
        }

        public IOverlayEndpointService GetOverlay(string name)
        {
            if (this.overlays.ContainsKey(name))
            {
                return this.overlays[name];
            }
            return null;
        }

        public IEnumerable<string> GetOverlayNames()
        {
            return this.overlays.Keys.ToList();
        }

        public async Task<int> TestConnections()
        {
            int count = 0;
            foreach (IOverlayEndpointService overlay in this.overlays.Values)
            {
                count += await overlay.TestConnection();
            }
            return count;
        }

        public void StartBatching()
        {
            foreach (IOverlayEndpointService overlay in this.overlays.Values)
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (IOverlayEndpointService overlay in this.overlays.Values)
            {
                await overlay.EndBatching();
            }
        }

        private async Task WidgetsBackgroundUpdate()
        {
            long updateSeconds = 0;
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                UserViewModel user = await ChannelSession.GetCurrentUser();

                foreach (var widgetGroup in ChannelSession.Settings.OverlayWidgets.GroupBy(ow => ow.OverlayName))
                {
                    IOverlayEndpointService overlay = this.GetOverlay(widgetGroup.Key);
                    if (overlay != null)
                    {
                        overlay.StartBatching();
                        foreach (OverlayWidgetModel widget in widgetGroup)
                        {
                            try
                            {
                                if (!widget.Item.IsInitialized)
                                {
                                    await widget.Initialize();
                                }

                                if (widget.IsEnabled)
                                {
                                    if (!widget.Item.IsEnabled)
                                    {
                                        await widget.Enable();
                                    }
                                    else if (widget.SupportsRefreshUpdating && widget.RefreshTime > 0 && (updateSeconds % widget.RefreshTime) == 0)
                                    {
                                        await widget.UpdateItem();
                                    }
                                }
                                else
                                {
                                    if (widget.Item.IsEnabled)
                                    {
                                        await widget.Disable();
                                    }
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                        await overlay.EndBatching();
                    }
                }

                await Task.Delay(1000);
                updateSeconds++;
            });
        }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            IOverlayEndpointService overlay = (IOverlayEndpointService)sender;
            this.OnOverlayConnectedOccurred(overlay, new EventArgs());

            overlay.StartBatching();
            foreach (OverlayWidgetModel widget in ChannelSession.Settings.OverlayWidgets.Where(ow => ow.OverlayName.Equals(overlay.Name)))
            {
                try
                {
                    if (widget.IsEnabled)
                    {
                        await widget.ShowItem();
                        await widget.LoadCachedData();
                        await widget.UpdateItem();
                    }
                }
                catch (Exception ex) { Logger.Log(ex); }
            }
            await overlay.EndBatching();
        }

        private void Overlay_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            IOverlayEndpointService overlay = (IOverlayEndpointService)sender;
            this.OnOverlayDisconnectedOccurred(overlay, closeStatus);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).
                    this.backgroundThreadCancellationTokenSource.Dispose();
                }

                // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                // Set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
