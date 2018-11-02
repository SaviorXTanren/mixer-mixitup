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

        public override async Task<OverlayItemBase> GetProcessedItem(UserViewModel user, IEnumerable<string> arguments, Dictionary<string, string> extraSpecialIdentifiers)
        {
            OverlayYouTubeItem item = this.Copy<OverlayYouTubeItem>();
            item.ID = await this.ReplaceStringWithSpecialModifiers(item.ID, user, arguments, extraSpecialIdentifiers, encode: true);
            return item;
        }
    }
}
