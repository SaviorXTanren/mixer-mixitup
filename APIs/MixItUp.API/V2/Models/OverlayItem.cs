using System;

namespace MixItUp.API.V2.Models
{
    public class OverlayItem
    {
        public string OverlayItemType { get; set; }
        public Guid ID { get; set; }
        public string ItemType { get; set; }
        public OverlayItemPosition Position { get; set; }
        public OverlayItemEffects Effects { get; set; }
    }
}
