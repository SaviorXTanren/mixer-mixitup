using MixItUp.Base.Services;
using MixItUp.Overlay;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Desktop.Services
{
    public class OverlayServiceManager : IOverlayServiceManager
    {
        public event EventHandler OnOverlayConnectedOccurred = delegate { };
        public event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred = delegate { };

        private Dictionary<string, IOverlayService> overlays = new Dictionary<string, IOverlayService>();

        public string DefaultOverlayName { get { return "Default"; } }
        public int DefaultOverlayPort { get { return 8111; } }

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
    }
}
