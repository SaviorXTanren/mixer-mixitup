using System;
using System.Net.WebSockets;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Services
{
    [DataContract]
    public class OverlayImage : OverlayBase
    {
        [DataMember]
        public string imagePath;
        [DataMember]
        public int width;
        [DataMember]
        public int height;
        [DataMember]
        public string imageData;
    }

    [DataContract]
    public class OverlayText : OverlayBase
    {
        [DataMember]
        public string text;
        [DataMember]
        public string color;
        [DataMember]
        public int fontSize;
    }

    [DataContract]
    public class OverlayLocalVideo : OverlayBase
    {
        [DataMember]
        public string filepath;
        [DataMember]
        public int width;
        [DataMember]
        public int height;

        [DataMember]
        public string videoID;
        [DataMember]
        public string videoType;
    }

    [DataContract]
    public class OverlayYoutubeVideo : OverlayBase
    {
        [DataMember]
        public string videoID;
        [DataMember]
        public int startTime;
        [DataMember]
        public int width;
        [DataMember]
        public int height;
    }

    [DataContract]
    public class OverlayHTML : OverlayBase
    {
        [DataMember]
        public string htmlText;
    }

    [DataContract]
    public class OverlayRouletteWheel
    {
        [DataMember]
        public uint userID;
        [DataMember]
        public int bet;
    }

    [DataContract]
    public abstract class OverlayBase
    {
        [DataMember]
        public double duration;
        [DataMember]
        public int horizontal;
        [DataMember]
        public int vertical;
        [DataMember]
        public int fadeDuration;
    }

    [DataContract]
    public class OverlayTextToSpeech
    {
        [DataMember]
        public string text;
        [DataMember]
        public string voice;
        [DataMember]
        public double volume;
        [DataMember]
        public double pitch;
        [DataMember]
        public double rate;
    }

    public interface IOverlayService
    {
        event EventHandler<WebSocketCloseStatus> OnWebSocketDisconnectOccurred;

        Task<bool> Initialize();

        Task<bool> TestConnection();

        void StartBatching();

        Task EndBatching();

        Task SendImage(OverlayImage image);

        Task SendText(OverlayText text);

        Task SendYoutubeVideo(OverlayYoutubeVideo youtubeVideo);

        Task SendLocalVideo(OverlayLocalVideo localVideo);

        Task SendHTMLText(OverlayHTML htmlText);

        Task SendTextToSpeech(OverlayTextToSpeech textToSpeech);

        Task Disconnect();
    }
}
