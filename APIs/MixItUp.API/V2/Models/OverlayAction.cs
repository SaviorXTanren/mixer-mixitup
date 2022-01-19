using System;

namespace MixItUp.API.V2.Models
{
    public class OverlayAction : ActionBase
    {
        public string OverlayName { get; set; }
        public OverlayItem OverlayItem { get; set; }
        public Guid WidgetID { get; set; }
        public bool ShowWidget { get; set; }
    }
}
