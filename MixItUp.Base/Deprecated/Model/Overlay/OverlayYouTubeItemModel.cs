using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [Obsolete]
    public class OverlayYouTubeItemModel : OverlayFileItemModelBase
    {
        [DataMember]
        public int StartTime { get; set; }
        [DataMember]
        public int Volume { get; set; }

        [DataMember]
        public override string FileType { get { return "youtube"; } set { } }

        [DataMember]
        public double VolumeDecimal { get { return ((double)this.Volume / 100.0); } }

        public OverlayYouTubeItemModel() : base() { }

        public OverlayYouTubeItemModel(string filepath, int width, int height, int startTime, int volume)
            : base(OverlayItemModelTypeEnum.YouTube, filepath, width, height)
        {
            this.StartTime = startTime;
            this.Volume = volume;
        }

        [JsonIgnore]
        public override bool SupportsRefreshUpdating { get { return true; } }
    }
}
