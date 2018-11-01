using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayYouTubeItem : OverlayItemBase
    {
        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public int StartTime { get; set; }
        [DataMember]
        public int Width { get; set; }
        [DataMember]
        public int Height { get; set; }
        [DataMember]
        public int Volume { get; set; }

        public OverlayYouTubeItem() { this.Volume = 100; }

        public OverlayYouTubeItem(string id, int startTime, int width, int height, int volume)
        {
            this.ID = id;
            this.StartTime = startTime;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
        }
    }
}
