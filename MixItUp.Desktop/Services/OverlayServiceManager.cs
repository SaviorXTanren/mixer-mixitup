using MixItUp.Base;
using MixItUp.Base.Model.Overlay;
using MixItUp.Base.Services;
using MixItUp.Base.Util;
using MixItUp.Base.ViewModel.User;
using MixItUp.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class OverlayServiceManager : IOverlayServiceManager, IDisposable
    {
        public event EventHandler OnOverlayConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred = delegate { };

        private Dictionary<string, IOverlayService> overlays = new Dictionary<string, IOverlayService>();

        private CancellationTokenSource backgroundThreadCancellationTokenSource = new CancellationTokenSource();

        public string DefaultOverlayName { get { return "Default"; } }
        public int DefaultOverlayPort { get { return 8111; } }

        public void Initialize()
        {
            Task.Run(async () => { await this.WidgetsBackgroundUpdate(); }, this.backgroundThreadCancellationTokenSource.Token);
        }

        public void Disable()
        {
            this.backgroundThreadCancellationTokenSource.Cancel();
        }

        public async Task<bool> AddOverlay(string name, int port)
        {
            IOverlayService overlay = new OverlayService(name, port);
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
            IOverlayService overlay = this.GetOverlay(name);
            if (overlay != null)
            {
                overlay.OnWebSocketConnectedOccurred -= Overlay_OnWebSocketConnectedOccurred;
                overlay.OnWebSocketDisconnectedOccurred -= Overlay_OnWebSocketDisconnectedOccurred;

                await overlay.Disconnect();
                this.overlays.Remove(name);
            }
        }

        public IOverlayService GetOverlay(string name)
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

        public async Task RemoveAllOverlays()
        {
            foreach (string overlayName in this.GetOverlayNames())
            {
                await this.RemoveOverlay(overlayName);
            }
        }

        public void StartBatching()
        {
            foreach (IOverlayService overlay in this.overlays.Values)
            {
                overlay.StartBatching();
            }
        }

        public async Task EndBatching()
        {
            foreach (IOverlayService overlay in this.overlays.Values)
            {
                await overlay.EndBatching();
            }
        }

        private async Task WidgetsBackgroundUpdate()
        {
            await BackgroundTaskWrapper.RunBackgroundTask(this.backgroundThreadCancellationTokenSource, async (tokenSource) =>
            {
                tokenSource.Token.ThrowIfCancellationRequested();

                UserViewModel user = await ChannelSession.GetCurrentUser();

                foreach (var widgetGroup in ChannelSession.Settings.OverlayWidgets.GroupBy(ow => ow.OverlayName))
                {
                    IOverlayService overlay = this.GetOverlay(widgetGroup.Key);
                    if (overlay != null)
                    {
                        overlay.StartBatching();
                        foreach (OverlayWidget widget in widgetGroup)
                        {
                            try
                            {
                                if (widget.IsEnabled)
                                {
                                    bool isInitialized = widget.Item.IsInitialized;

                                    if (!isInitialized)
                                    {
                                        await widget.Item.Initialize();
                                    }

                                    if (!isInitialized || !widget.DontRefresh)
                                    {
                                        OverlayItemBase item = await widget.Item.GetProcessedItem(user, new List<string>(), new Dictionary<string, string>());
                                        if (item != null)
                                        {
                                            await overlay.SendItem(item, widget.Position, new OverlayItemEffects());
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { Logger.Log(ex); }
                        }
                        await overlay.EndBatching();
                    }
                }

                await Task.Delay(ChannelSession.Settings.OverlayWidgetRefreshTime * 1000);
            });
        }

        private void Overlay_OnWebSocketConnectedOccurred(object sender, EventArgs e)
        {
            IOverlayService overlay = (IOverlayService)sender;
            this.OnOverlayConnectedOccurred(overlay, new EventArgs());
        }

        private void Overlay_OnWebSocketDisconnectedOccurred(object sender, WebSocketCloseStatus closeStatus)
        {
            IOverlayService overlay = (IOverlayService)sender;
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
