using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayWidget
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsEnabled { get; set; }

        [DataMember]
        public string OverlayName { get; set; }

        [DataMember]
        public OverlayItemBase Item { get; set; }

        [DataMember]
        public OverlayItemPosition Position { get; set; }

        [DataMember]
        public bool DontRefresh { get; set; }

        public OverlayWidget()
        {
            this.IsEnabled = true;
        }

        public OverlayWidget(string name, string overlayName, OverlayItemBase item, OverlayItemPosition position, bool dontRefresh)
            : this()
        {
            this.Name = name;
            this.OverlayName = overlayName;
            this.Item = item;
            this.Position = position;
            this.DontRefresh = dontRefresh;
        }

        [JsonIgnore]
        public virtual bool SupportsTestButton { get { return (this.Item != null) ? this.Item.SupportsTestButton : false; } }

        public async Task LoadTestData()
        {
            if (this.SupportsTestButton)
            {
                await this.Item.LoadTestData();
            }
        }
    }
}
