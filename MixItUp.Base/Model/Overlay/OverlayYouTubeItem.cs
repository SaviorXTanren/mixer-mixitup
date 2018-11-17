using MixItUp.Base.ViewModel.User;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayYouTubeItem : OverlayItemBase
    {
        [DataMember]
        public override string ItemType { get { return "youtube"; } }

        [DataMember]
        public string VideoID { get; set; }
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
            this.VideoID = id;
            this.StartTime = startTime;
            this.Width = width;
            this.Height = height;
            this.Volume = volume;
        }

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayYouTubeItem item = this.Copy<OverlayYouTubeItem>();
            item.VideoID = await this.ReplaceStringWithSpecialModifiers(item.VideoID, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }
}
