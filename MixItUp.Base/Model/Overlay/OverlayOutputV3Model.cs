using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayOutputV3Model
    {
        [DataMember]
        public Guid ID { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;

        [DataMember]
        public string Duration { get; set; }

        [DataMember]
        public OverlayItemAnimationV3Model EntranceAnimation { get; set; } = new OverlayItemAnimationV3Model();
        [DataMember]
        public OverlayItemAnimationV3Model VisibleAnimation { get; set; } = new OverlayItemAnimationV3Model();
        [DataMember]
        public OverlayItemAnimationV3Model ExitAnimation { get; set; } = new OverlayItemAnimationV3Model();

        public string TextID { get { return "X" + this.ID.ToString().Replace('-', 'X'); } }
    }
}
