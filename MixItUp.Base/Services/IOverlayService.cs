using System;
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

    public interface IOverlayService
    {
        event EventHandler Disconnected;

        Task<bool> Initialize();

        Task<bool> TestConnection();

        Task SetImage(OverlayImage image);

        Task SetText(OverlayText text);

        Task SetHTMLText(OverlayHTML htmlText);

        Task Disconnect();
    }
}
