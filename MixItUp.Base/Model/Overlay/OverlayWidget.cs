using Mixer.Base.Util;
using System.Runtime.Serialization;

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

        public OverlayWidget()
        {
            this.IsEnabled = true;
        }

        public OverlayWidget(string name, string overlayName, OverlayItemBase item, OverlayItemPosition position)
            : this()
        {
            this.Name = name;
            this.OverlayName = overlayName;
            this.Item = item;
            this.Position = position;
        }
    }
}
