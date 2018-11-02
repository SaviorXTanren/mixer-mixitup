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

    [DataContract]
    public class OverlaySongRequest
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string Action { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public int Volume { get; set; }
    }

    public interface IOverlayService
    {
        event EventHandler OnWebSocketConnectedOccurred;
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectedOccurred;

        Task<bool> Initialize();

        Task<bool> TestConnection();

        void StartBatching();

        Task EndBatching();

        Task SendImage(OverlayImageItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendText(OverlayTextItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendYouTubeVideo(OverlayYouTubeItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendLocalVideo(OverlayVideoItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendHTML(OverlayHTMLItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendWebPage(OverlayWebPageItem item, OverlayItemPosition position, OverlayItemEffects effects);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        Task SendSongRequest(OverlaySongRequest songRequest);

        Task Disconnect();
    }
}
