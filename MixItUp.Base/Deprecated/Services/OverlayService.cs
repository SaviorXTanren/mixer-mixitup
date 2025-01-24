using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [Obsolete]
    public class OverlayService : IExternalService, IDisposable
    {
        public event EventHandler OnOverlayConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred = delegate { };

        private Dictionary<Guid, OverlayEndpointService> overlays = new Dictionary<Guid, OverlayEndpointService>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public string Name { get { return "Overlay"; } }

        public bool IsConnected { get; private set; }

        public async Task<Result> Connect()
        {
            try
            {
                this.IsConnected = false;
                ChannelSession.Settings.EnableOverlay = false;

                foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
                {
                    if (!await this.AddOverlayEndpoint(overlayEndpoint))
                    {
                        await this.Disconnect();
                        return new Result(string.Format(Resources.OverlayAddFailed, overlayEndpoint.Name));
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
            foreach (OverlayEndpointV3Model overlayEndpoint in this.GetOverlayEndpoints())
            {
                await this.RemoveOverlayEndpoint(overlayEndpoint.ID);
            }
            this.IsConnected = false;
            ChannelSession.Settings.EnableOverlay = false;
        }

        public async Task<bool> AddOverlayEndpoint(OverlayEndpointV3Model overlayEndpoint)
        {
            OverlayEndpointService overlay = new OverlayEndpointService();
            if (await overlay.Initialize())
            {
                overlay.OnWebSocketConnectedOccurred += Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred += Overlay_OnWebSocketDisconnectedOccurred;
                this.overlays[overlayEndpoint.ID] = overlay;
                return true;
            }
            await this.RemoveOverlayEndpoint(overlayEndpoint.ID);
            return false;
        }

        public async Task RemoveOverlayEndpoint(Guid id)
        {
            OverlayEndpointService overlay = this.GetOverlayEndpointService(id);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(id);
            }
        }

        public IEnumerable<OverlayEndpointV3Model> GetOverlayEndpoints()
        {
            return ChannelSession.Settings.OverlayEndpointsV3;
        }

        public OverlayEndpointV3Model GetOverlayEndpoint(Guid id)
        {
            return this.GetOverlayEndpoints().FirstOrDefault(oe => oe.ID == id) ?? this.GetDefaultOverlayEndpoint();
        }

        public OverlayEndpointV3Model GetDefaultOverlayEndpoint()
        {
            return this.GetOverlayEndpoint(Guid.Empty);
        }

        public OverlayEndpointService GetDefaultOverlayEndpointService()
        {
            OverlayEndpointV3Model overlayEndpoint = this.GetDefaultOverlayEndpoint();
            if (overlayEndpoint != null)
            {
                return this.GetOverlayEndpointService(overlayEndpoint.ID);
            }
            return null;
        }

        public OverlayEndpointService GetOverlayEndpointService(Guid id)
        {
            if (this.overlays.ContainsKey(id))
            {
                return this.overlays[id];
            }
            return null;
        }

        public IEnumerable<OverlayItemV3ModelBase> GetOverlayWidgets()
        {
            return null;//return ChannelSession.Settings.OverlayWidgetsV3;
        }

        public OverlayItemV3ModelBase GetOverlayWidget(Guid id)
        {
            return this.GetOverlayWidgets().FirstOrDefault(w => w.ID.Equals(id));
        }

        public Task AddOverlayWidget(OverlayItemV3ModelBase widget)
        {
            //ChannelSession.Settings.OverlayWidgetsV3.Add(widget);
            //await widget.EnableAsWidget();
            return Task.CompletedTask;
        }

        public Task RemoveOverlayWidget(OverlayItemV3ModelBase widget)
        {
            //await widget.DisableAsWidget();
            //ChannelSession.Settings.OverlayWidgetsV3.Remove(widget);
            return Task.CompletedTask;
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

        private Task WidgetsBackgroundUpdate(CancellationToken token)
        {
            return Task.CompletedTask;

            //token.ThrowIfCancellationRequested();

            //UserV2ViewModel user = ChannelSession.User;

            //foreach (var widgetGroup in ChannelSession.Settings.OverlayWidgets.GroupBy(ow => ow.OverlayName))
            //{
            //    OverlayEndpointService overlay = this.GetOverlay(widgetGroup.Key);
            //    if (overlay != null)
            //    {
            //        overlay.StartBatching();
            //        foreach (OverlayWidgetModel widget in widgetGroup)
            //        {
            //            try
            //            {
            //                if (!widget.Item.IsInitialized)
            //                {
            //                    await widget.Initialize();
            //                }

            //                if (widget.IsEnabled)
            //                {
            //                    if (!widget.Item.IsEnabled)
            //                    {
            //                        await widget.Enable();
            //                    }
            //                    else if (widget.SupportsRefreshUpdating && widget.RefreshTime > 0 && (this.updateSeconds % widget.RefreshTime) == 0)
            //                    {
            //                        await widget.UpdateItem();
            //                    }
            //                }
            //                else
            //                {
            //                    if (widget.Item.IsEnabled)
            //                    {
            //                        await widget.Disable();
            //                    }
            //                }
            //            }
            //            catch (Exception ex) { Logger.Log(ex); }
            //        }
            //        await overlay.EndBatching();
            //    }
            //}

            //this.updateSeconds++;
        }

        private async void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            OverlayEndpointService overlay = (OverlayEndpointService)sender;
            this.OnOverlayConnectedOccurred(overlay, new EventArgs());

            Logger.Log("Client connected to Overlay Endpoint - " + overlay.Name);

            overlay.StartBatching();
            //foreach (OverlayWidgetModel widget in ChannelSession.Settings.OverlayWidgets.Where(ow => ow.OverlayName.Equals(overlay.Name)))
            //{
            //    try
            //    {
            //        if (widget.IsEnabled)
            //        {
            //            await widget.ShowItem();
            //            await widget.LoadCachedData();
            //            await widget.UpdateItem();
            //        }
            //    }
            //    catch (Exception ex) { Logger.Log(ex); }
            //}
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