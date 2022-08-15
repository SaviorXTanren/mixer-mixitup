using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayEndpointV3Model
    {
        public static readonly string DefaultOverlayName = MixItUp.Base.Resources.Default;
        public const int DefaultOverlayPort = 8111;

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();
        [DataMember]
        public int PortNumber { get; set; }
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string HTML { get; set; } = string.Empty;
        [DataMember]
        public string CSS { get; set; } = string.Empty;
        [DataMember]
        public string Javascript { get; set; } = string.Empty;

        [Obsolete]
        public OverlayEndpointV3Model() { }

        public OverlayEndpointV3Model(int portNumber, string name)
        {
            this.PortNumber = portNumber;
            this.Name = name;
        }
    }
}
