using MixItUp.Base.Model.Overlay;
using System;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class OverlayTextToSpeech
    {
        [DataMember]
        public string Text { get; set; }
        [DataMember]
        public string Voice { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public double Pitch { get; set; }
        [DataMember]
        public double Rate { get; set; }
    }

    public interface IOverlayService
    {
        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred;

        Task<bool> Initialize();

        Task<bool> TestConnection();

        void StartBatching();

        Task EndBatching();

        Task SendItem(OverlayItemBase item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        Task RemoveItem(OverlayItemBase item);

        Task Disconnect();
    }
}
