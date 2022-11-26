using System;
using System.Collections.Generic;
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
        public Dictionary<string, OverlayAnimationV3Model> Animations { get; set; } = new Dictionary<string, OverlayAnimationV3Model>();

        [DataMember]
        public string TextID { get { return "X" + this.ID.ToString().Replace('-', 'X'); } set { } }
    }
}
