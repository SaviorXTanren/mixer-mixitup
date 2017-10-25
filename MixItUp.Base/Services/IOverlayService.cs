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
    public abstract class OverlayBase
    {
        [DataMember]
        public double duration;
        [DataMember]
        public int horizontal;
        [DataMember]
        public int vertical;
    }

    public interface IOverlayService
    {
        Task<bool> Initialize();

        Task TestConnection();

        void SetImage(OverlayImage image);

        void SetText(OverlayText text);

        Task Close();
    }
}
