using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayYouTubeItemV3Model : OverlayItemV3ModelBase
    {
        public const string DefaultHTML = "<div id=\"{ID}\" />";

        [DataMember]
        public string VideoID { get; set; }

        [DataMember]
        public int StartTime { get; set; }

        [DataMember]
        public int Volume { get; set; }

        public OverlayYouTubeItemV3Model() : base(OverlayItemV3Type.YouTube) { }
    }
}