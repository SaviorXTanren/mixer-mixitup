using MixItUp.Base.Actions;
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

        Task SendImage(OverlayImageEffect effect);

        Task SendText(OverlayTextEffect effect);

        Task SendYoutubeVideo(OverlayYoutubeEffect effect);

        Task SendLocalVideo(OverlayVideoEffect effect);

        Task SendHTMLText(OverlayHTMLEffect effect);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        Task Disconnect();
    }
}
