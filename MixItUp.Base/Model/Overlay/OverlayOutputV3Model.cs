using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayOutputV3Model
    {
        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;
    }
}
