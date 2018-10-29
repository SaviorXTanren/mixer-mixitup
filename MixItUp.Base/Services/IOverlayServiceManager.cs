using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    public interface IOverlayServiceManager
    {
        event EventHandler OnOverlayConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnOverlayDisconnectedOccurred;

        string DefaultOverlayName { get; }
        int DefaultOverlayPort { get; }

        Task<bool> AddOverlay(string name, int port);

        Task RemoveOverlay(string name);

        IOverlayService GetOverlay(string name);

        IEnumerable<string> GetOverlayNames();

        void StartBatching();

        Task EndBatching();

        Task RemoveAllOverlays();
    }
}
