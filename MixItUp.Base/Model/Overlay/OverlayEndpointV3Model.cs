using MixItUp.Base.Services;
using System;
using System.Runtime.Serialization;

namespace MixItUp.Base.Model.Overlay
{
    [DataContract]
    public class OverlayEndpointV3Model
    {
        public static readonly string DefaultOverlayName = MixItUp.Base.Resources.Default;

        [DataMember]
        public Guid ID { get; set; } = Guid.NewGuid();
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Head { get; set; } = OverlayResources.OverlayEndpointDefaultHead;
        [DataMember]
        public string HTML { get; set; } = OverlayResources.OverlayEndpointDefaultHTML;
        [DataMember]
        public string CSS { get; set; } = OverlayResources.OverlayEndpointDefaultCSS;
        [DataMember]
        public string Javascript { get; set; } = OverlayResources.OverlayEndpointDefaultJavascript;

        [Obsolete]
        public OverlayEndpointV3Model() { }

        public OverlayEndpointV3Model(string name)
        {
            this.Name = name;
        }

        public string Address
        {
            get
            {
                OverlayEndpointV3Service endpointService = ServiceManager.Get<OverlayV3Service>().GetOverlayEndpointService(this.ID);
                if (endpointService != null)
                {
                    return endpointService.HttpAddress;
                }
                return string.Empty;
            }
        }
    }
}
