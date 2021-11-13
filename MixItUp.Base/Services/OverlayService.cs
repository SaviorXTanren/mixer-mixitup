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
    public class OverlayService : IExternalService, IDisposable
    {
        public event EventHandler OnOverlayConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred = delegate { };

        private Dictionary<string, OverlayEndpointService> overlays = new Dictionary<string, OverlayEndpointService>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public string DefaultOverlayName { get { return MixItUp.Base.Resources.Default; } }
        public int DefaultOverlayPort { get { return 8111; } }

        public string Name { get { return "Overlay"; } }

        public bool IsConnected { get; private set; }

        private long updateSeconds = 0;

        public IDictionary<string, int> AllOverlayNameAndPorts
        {
            get
            {
                Dictionary<string, int> results = new Dictionary<string, int>(ChannelSession.Settings.OverlayCustomNameAndPorts);
                results.Add(ServiceManager.Get<OverlayService>().DefaultOverlayName, ServiceManager.Get<OverlayService>().DefaultOverlayPort);
                return results;
            }
        }

        public async Task<Result> Connect()
        {
            try
            {
                this.IsConnected = false;
                ChannelSession.Settings.EnableOverlay = false;

                foreach (var kvp in this.AllOverlayNameAndPorts)
                {
                    if (!await this.AddOverlay(kvp.Key, kvp.Value))
                    {
                        await this.Disconnect();
                        return new Result(string.Format(Resources.OverlayAddFailed, kvp.Key));
                    }
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                AsyncRunner.RunAsyncBackground(this.WidgetsBackgroundUpdate, this.backgroundThreadCancellationTokenSource.Token, 1000);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                this.IsConnected = true;
                ChannelSession.Settings.EnableOverlay = true;
                ServiceManager.Get<ITelemetryService>().TrackService("Overlay");
                return new Result();
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                return new Result(ex.ToString());
            }
        }

        public async Task Disconnect()
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
            foreach (string overlayName in this.GetOverlayNames())
            {
                await this.RemoveOverlay(overlayName);
            }
            this.IsConnected = false;
            ChannelSession.Settings.EnableOverlay = false;
        }

        public async Task<bool> AddOverlay(string name, int port)
        {
            OverlayEndpointService overlay = new OverlayEndpointService(name, port);
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
            OverlayEndpointService overlay = this.GetOverlay(name);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(name);
            }
        }

        public OverlayEndpointService GetOverlay(string name)
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
            foreach (OverlayEndpointService overlay in this.overlays.Values)
            {
                count += await overlay.TestConnection();
            }
            return count;
        }

        public void StartBatching()
        {
            foreach (OverlayEndpointService overlay in this.overlays.Values)
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (OverlayEndpointService overlay in this.overlays.Values)
            {
                await overlay.EndBatching();
            }
        }

        private async Task WidgetsBackgroundUpdate(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            UserV2ViewModel user = ChannelSession.User;

            foreach (var widgetGroup in ChannelSession.Settings.OverlayWidgets.GroupBy(ow => ow.OverlayName))
            {
                OverlayEndpointService overlay = this.GetOverlay(widgetGroup.Key);
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
                                else if (widget.SupportsRefreshUpdating && widget.RefreshTime > 0 && (this.updateSeconds % widget.RefreshTime) == 0)
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

            this.updateSeconds++;
        }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            OverlayEndpointService overlay = (OverlayEndpointService)sender;
            this.OnOverlayConnectedOccurred(overlay, new EventArgs());

            Logger.Log("Client connected to Overlay Endpoint - " + overlay.Name);

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
            OverlayEndpointService overlay = (OverlayEndpointService)sender;
            this.OnOverlayDisconnectedOccurred(overlay, closeStatus);

            Logger.Log("Client disconnect from Overlay Endpoint - " + overlay.Name);
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